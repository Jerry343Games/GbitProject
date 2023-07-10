// Copyright Elliot Bentine, 2018-
#ifndef PACKINGUTILS_INCLUDED
#define PACKINGUTILS_INCLUDED

// Similar functions were previously defined in UnityCG.cginc but have since been dropped in the (nearly equivalent) Core.hlsl.
// Note that the built-in versions did not work perfectly! See the below SO for details:
//	https://stackoverflow.com/questions/41566049/understanding-unitys-rgba-encoding-in-float-encodefloatrgba

inline float2 EncodeFloatRG(float v)
{
	uint vi = (uint)(v * (16.0f * 16.0f));
	int ex = (int)(vi / 16.0001 % 16.0001); // Note that imperfect division is required to prevent compiler replacing with bitExtract (not supported on ES 3.0)
	int ey = (int)(vi % 16);
	float2 e = float2(ex / 15.0f, ey / 15.0f);
	return e;
}

inline float DecodeFloatRG(float2 enc)
{
	uint ex = (uint)(enc.x * 15);
	uint ey = (uint)(enc.y * 15);
	uint v = (ex * 16) + ey;
	return v / (16.0f * 16.0f);
}

// - Used for packing the PixelMap texture into rg,ba channels.
float2 EncodeFloat2(in float value)
{
	value = clamp(value, 0, 1);
	float intVal = floor(value * (256.0 * 256.0 - 1));
	float major = floor(intVal / 256.0) / 255.0;
	float minor = (intVal - major * 256.0 * 255.0) / 255.0;
	return float2(major, minor);
}
float DecodeFloat2(in float2 pack)
{
	return pack.x * (255.0 / 256.0) + pack.y / 256.0 * (255.0 / 256.0);
}

// PackPixelMapUV(uv, texel_params):
//   Pack UV coordinates into the pixelation map, an 8-bit RGBA buffer.
//
//   Input Arguments:
//    -uv: The UV coordinates in the pixelation map.
//    -texel_params: float4 describing texel size (rg) and screen resolution (ba)
inline float4 PackPixelMapUV(in float2 uv, uniform float4 texel_params) {
	return float4(
		EncodeFloat2(clamp(uv.r * texel_params.b / 65535.0, 0, 1)),
		EncodeFloat2(clamp(uv.g * texel_params.a / 65535.0, 0, 1))
		);
}

// UnpackPixelMapUV(uv, texel_params):
//   Unpack UV coordinates from the pixelation map 8-bit RGBA buffer into float2 that can be used to sample a screen texture.
//
//   Input Arguments:
//    -packed: The packed UV coordinates from the pixelation map buffer.
//    -texel_params: float4 describing texel size (rg) and screen resolution (ba)
inline float2 UnpackPixelMapUV(in float4 packed, uniform float4 texel_params) {
	float2 uvs = float2(DecodeFloat2(packed.rg), DecodeFloat2(packed.ba));
	uvs *= 65535.0 / texel_params.ba;
	uvs += texel_params.rg / 2.0; // shift to centre of each pixel.
	return uvs;
}

inline float PixelSizeToAlpha(float pixelSize) {
	return clamp(pixelSize, 0.0, 5.0) / 6.0;
}

inline float AlphaToPixelSize(float alpha) {
	return round(alpha * 6.0 % 6.0);
}

inline void PackOutline(float4 outline_color, float ID, float pixelSize, out float4 output) {
	output = float4(0.0, 0.0, 0.0, 0.0);
	output.r = DecodeFloatRG(outline_color.rg/2);
	output.b = DecodeFloatRG(outline_color.ba/2);
	output.g = ID;
	output.g = fmod(ID, 256.0) / 256.0;
	output.a = PixelSizeToAlpha(pixelSize);
}

inline float getUID(float4 data) {
	return data.g;
}

inline void UnpackOutline(float4 data, out float4 outline_color, out float ID, out float pixelSize) {
	outline_color = float4(2*EncodeFloatRG(data.r), 2*EncodeFloatRG(data.b));
	ID = getUID(data);
	pixelSize = AlphaToPixelSize(data.a);
}

#endif