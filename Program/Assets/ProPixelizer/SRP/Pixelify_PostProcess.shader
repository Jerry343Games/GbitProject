// Copyright Elliot Bentine, 2018-
// 
// A shader used to pixelise render targets.
//
// DEPRECATED
Shader "Hidden/ProPixelizer/Deprecated/SRP/Pixelization Post Process" {
	Properties{
	}

	SubShader{
	Tags{
		"RenderType" = "TransparentCutout"
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
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		#include "PixelUtils.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"		
		#pragma shader_feature DEPTH_BUFFER_OUTPUT_ON
		#pragma shader_feature ORTHO_PROJECTION

		uniform sampler2D _MainTex;
		float4 _MainTex_TexelSize;

		struct v2f {
			float4 pos : SV_POSITION;
			float4 scrPos:TEXCOORD1;
		};

		struct appdata_base
		{
			float4 vertex   : POSITION;  // The vertex position in model space.
			float3 normal   : NORMAL;    // The vertex normal in model space.
			float4 texcoord : TEXCOORD0; // The first UV coordinate.
		};

		v2f vert(appdata_base v) {
			v2f o;
			o.pos = TransformObjectToHClip(v.vertex.xyz);
			o.scrPos = ComputeScreenPos(o.pos);
			return o;
		}

		#ifdef DEPTH_BUFFER_OUTPUT_ON
		void frag(v2f i, out float nearestRawDepth : SV_DEPTH) {
		#else
		void frag(v2f i, out float4 nearestColor: COLOR, out float nearestRawDepth : SV_DEPTH) {
		#endif

			#if UNITY_UV_STARTS_AT_TOP
				//i.scrPos.y = 1 - i.scrPos.y;
			#else

			#endif

			float depth, nearestDepth;
			float2 ppos;
			float raw_depth;

			#ifdef DEPTH_BUFFER_OUTPUT_ON
			half4 nearestColor;
			#endif

			// shift of one pixel
			float2 pShift = float2(_MainTex_TexelSize.x, _MainTex_TexelSize.y);

			nearestDepth = 1;
			nearestRawDepth = SampleSceneDepth(i.scrPos.xy);
			nearestColor = tex2D(_MainTex, i.scrPos.xy);
			 
			[unroll]
			for (int u = -2; u <= 2; u++)
			{
				[unroll]
				for (int v = -2; v <= 2; v++)
				{
					//Get coord of neighbouring pixel for sampling
					float shiftx = u * pShift.x;
					float shifty = v * pShift.y;
					float2 ppos = i.scrPos.xy + float2(shiftx, shifty);
					float4 neighbour = tex2D(_MainTex, ppos);
					float pixelSize = AlphaToPixelSize(neighbour.a);
					float pos = floor(pixelSize / 1.99);
					float neg = -floor((pixelSize - 1) / 1.99);
					bool pixelate = pixelSize > 0.5 && u >= neg && v >= neg && u <= pos && v <= pos;
					raw_depth = SampleSceneDepth(ppos);
#ifdef ORTHO_PROJECTION
	#if UNITY_REVERSED_Z
					depth = -raw_depth;
	#else
					depth = raw_depth;
	#endif
#else
					depth = Linear01Depth(raw_depth.r, _ZBufferParams);
#endif
					bool nearer = (depth < nearestDepth);
					nearestDepth = nearer && pixelate ? depth : nearestDepth;
					nearestRawDepth = nearer && pixelate ? raw_depth : nearestRawDepth;
					nearestColor = nearer && pixelate ? neighbour : nearestColor;
				}
			}
			#ifdef DEPTH_BUFFER_OUTPUT_ON
				//return nearestRawDepth;
			#endif
		}
		ENDHLSL
	}
	}
	FallBack "Diffuse"
	}