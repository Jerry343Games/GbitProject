// Copyright Elliot Bentine, 2018-
// 
// Applies a pixelization map to _MainTex.
Shader "Hidden/ProPixelizer/SRP/ApplyPixelizationMap" {
	Properties{
	}

	SubShader{
	Tags{
		"RenderType" = "Opaque"
		"PreviewType" = "Plane"
		"RenderPipeline" = "UniversalPipeline"
	}

	Pass{
		Cull Off
		ZWrite On
		ZTest Off
		Blend Off

		HLSLPROGRAM 
		#pragma vertex vert
		#pragma fragment frag
		// I don't like that this has to be global, but setting local keywords from buffer is currently broken on 2022.2
		#pragma multi_compile PIXELMAP_DEPTH_OUTPUT_ON _
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		#include "PixelUtils.hlsl"
		#include "PackingUtils.hlsl"
		//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

		TEXTURE2D(_PixelizationMap);
		TEXTURE2D(_MainTex);
		SAMPLER(sampler_point_clamp);
		float4 _MainTex_TexelSize;
		TEXTURE2D_X_FLOAT(_SourceDepthTexture);
		TEXTURE2D_X_FLOAT(_SceneDepthTexture);

		struct ProPVaryings {
			float4 pos : SV_POSITION;
			float4 scrPos:TEXCOORD1;
		};

		// 2022.2 & URP14+
		#define BLIT_API UNITY_VERSION >= 202220
		#if BLIT_API
			#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

			ProPVaryings vert(Attributes v) {
				Varyings vars;
				vars = Vert(v);
				ProPVaryings o;
				//o.pos = TransformObjectToHClip(v.vertex.xyz);
				o.pos = vars.positionCS;
				o.scrPos = float4(ComputeNormalizedDeviceCoordinatesWithZ(o.pos.xyz).xyz, 0);
				return o;
			}
		#else
			struct Attributes
			{
				float4 vertex   : POSITION;  // The vertex position in model space.
				float3 normal   : NORMAL;    // The vertex normal in model space.
				float4 texcoord : TEXCOORD0; // The first UV coordinate.
			};

			ProPVaryings vert(Attributes v) {
				ProPVaryings o;
				o.pos = TransformObjectToHClip(v.vertex.xyz);
				o.scrPos = float4(ComputeNormalizedDeviceCoordinatesWithZ(o.pos.xyz).xyz, 0);
				return o;
			}
		#endif

		// For more recent URP versions, Metal cannot reliably bind both color and depth output.
		// Instead use the DEPTH_OUTPUT_ON multi_compile_local to toggle whether output is depth or not. 
		#if UNITY_VERSION >= 202220
			#if PIXELMAP_DEPTH_OUTPUT_ON
				float frag(ProPVaryings i) : SV_Depth {
					float depth;
					float4 color;
			#else
				void frag(ProPVaryings i, out float4 color : SV_Target) {
					float depth;
			#endif
		#else
			void frag(ProPVaryings i, out float4 color: COLOR, out float depth : SV_DEPTH) {
		#endif
			float4 packed = SAMPLE_TEXTURE2D(_PixelizationMap, sampler_point_clamp, i.scrPos.xy);
			float2 uvs = UnpackPixelMapUV(packed, _MainTex_TexelSize); 
			color = SAMPLE_TEXTURE2D(_MainTex, sampler_point_clamp, uvs.xy); // scene color at pixelised coordinate
			depth = SAMPLE_TEXTURE2D_X(_SceneDepthTexture, sampler_point_clamp, uvs.xy).r; // scene depth at pixelised coordinate
			float4 original_color = SAMPLE_TEXTURE2D(_MainTex, sampler_point_clamp, i.scrPos.xy); // scene color at unpixelised coordinate
			float original_depth = SAMPLE_TEXTURE2D_X(_SceneDepthTexture, sampler_point_clamp, i.scrPos.xy).r; // scene depth at unpixelised coordinate
			float pixelated_depth = SAMPLE_TEXTURE2D_X(_SourceDepthTexture, sampler_point_clamp, uvs.xy).r; // depth at pixelised coordinate, of the pixelised object.

			#if UNITY_REVERSED_Z
				float delta = original_depth - pixelated_depth;
			#else
				float delta = pixelated_depth - original_depth;
			#endif

			if (delta > 0.0)
			{
				color = original_color;
				depth = original_depth;
			}
			#if PIXELMAP_DEPTH_OUTPUT_ON && UNITY_VERSION >= 202220
				return depth;
			#endif
		}
		ENDHLSL
		}
	}
	FallBack "Diffuse"
}