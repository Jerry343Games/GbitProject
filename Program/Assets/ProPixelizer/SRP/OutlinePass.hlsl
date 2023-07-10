// Copyright Elliot Bentine, 2018-
//
// vertex and fragment shaders for outline pass.
#ifndef OUTLINE_PASS_INCLUDED
#define OUTLINE_PASS_INCLUDED

SAMPLER(sampler_Albedo_point_repeat); // this matches that used for the shadergraph, so that alpha values in both passes match.

struct appdata
{
	float4 vertex : POSITION; // vertex position
	float2 uv : TEXCOORD0; // texture coordinate
#if NORMAL_EDGE_DETECTION_ON
	float3 normalOS : NORMAL; // object space normals 
#endif
};

struct Varyings {
	float4 pos : SV_POSITION; // clip space position
	float4 posNDC : TEXCOORD1;
	float2 uv : TEXCOORD0; //texture coordinate
#if NORMAL_EDGE_DETECTION_ON
	float3 normalCS : NORMAL; // outline pass normals
#endif
};

Varyings outline_vert(
	appdata data
)
{
	Varyings output = (Varyings)0;
	VertexPositionInputs vertexInput = GetVertexPositionInputs(data.vertex.xyz);
	output.pos = float4(vertexInput.positionCS);
	output.posNDC = vertexInput.positionNDC;
#if NORMAL_EDGE_DETECTION_ON
	float4x4 viewMat = GetWorldToViewMatrix();
	output.normalCS = TransformWorldToViewDir(TransformObjectToWorldNormal(data.normalOS));
#endif
	output.uv = TRANSFORM_TEX(data.uv, _Albedo);
	return output;
}

void outline_frag(Varyings i, out float4 color : COLOR)
{
	float alpha = SAMPLE_TEXTURE2D(_Albedo, sampler_Albedo_point_repeat, i.uv).a * _BaseColor.a;

	float alpha_out;
	float2 dither_uv;
	float4 screenParams;

	float4 ScreenPosition = i.posNDC;
	GetScaledScreenParameters_float(screenParams);
	float4 pixelPos = float4(ScreenPosition.xy / ScreenPosition.w, 0, 0) * float4(screenParams.xy, 0, 0);

	//clip(alpha - _AlphaClipThreshold);

#if defined(USE_OBJECT_POSITION_ON)
	// This is equivalent to SHADERGRAPH_OBJECT_POSITION defined in Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl
	float3 objectPixelPos = UNITY_MATRIX_M._m03_m13_m23;
	PixelClipAlpha_float(UNITY_MATRIX_VP, objectPixelPos, screenParams, pixelPos, _PixelSize, alpha, _AlphaClipThreshold, alpha_out, dither_uv);
#else
	PixelClipAlpha_float(UNITY_MATRIX_VP, _PixelGridOrigin.xyz, screenParams, pixelPos, _PixelSize, alpha, _AlphaClipThreshold, alpha_out, dither_uv);
#endif 
	//clip(alpha_out - 0.0001);
	clip(alpha_out - 0.1);
	PackOutline(_OutlineColor, _ID, round(_PixelSize), color);
#if NORMAL_EDGE_DETECTION_ON
	color.rb = i.normalCS.rg * 0.5 + 0.5;
#endif
}

#endif