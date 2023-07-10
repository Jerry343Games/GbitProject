// Copyright Elliot Bentine, 2018-
// DEPRECATED
Shader "Hidden/ProPixelizer/Deprecated/SRP/Screen Post Process" {
	Properties{
		_OutlineDepthTestThreshold("Threshold used for depth testing outlines.", Float) = 0.0001
	}

		SubShader{
		Tags{
			"RenderType" = "Opaque"
			"PreviewType" = "Plane"
		}

		Pass{
			Cull Off
			ZWrite On
			ZTest Off

			HLSLINCLUDE
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" 
				#include "PixelUtils.hlsl"
				#include "PackingUtils.hlsl"
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"		
			ENDHLSL

			HLSLPROGRAM
			#pragma target 2.5
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile DEPTH_TEST_OUTLINES_ON

			#if DEPTH_TEST_OUTLINES_ON
			float _OutlineDepthTestThreshold;
			#endif

		uniform sampler2D _MainTex;
		uniform sampler2D _Outlines;
		uniform sampler2D _Pixelised;
		TEXTURE2D(_PixelizationMap);
		SAMPLER(sampler_point_clamp);
		TEXTURE2D_X_FLOAT(_Depth);
		SAMPLER(sampler_Depth);
		float4 _TexelSize;
		float4 _Test;

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
			o.pos = TransformObjectToHClip(v.vertex.rgb);
			o.scrPos = ComputeScreenPos(o.pos);
			return o;
		}

		void frag(v2f i, out float4 color: COLOR, out float depth : SV_Depth) {
			float2 mainTexel = i.scrPos.xy;
			float4 screenColor = tex2D(_MainTex, mainTexel);
			float4 pixelisedColor = tex2D(_Pixelised, i.scrPos.xy);
			float screenColorPixelSize = AlphaToPixelSize(pixelisedColor.a);

			depth = SAMPLE_TEXTURE2D_X(_Depth, sampler_Depth, UnityStereoTransformScreenSpaceTex(i.scrPos.xy)).r;

			if (screenColorPixelSize < 1)
			{
				// if this pixel is not pixelised, just return main texture colour.
				color = screenColor;
				return;
			}
			color = pixelisedColor;
			color.a = 1.0;
		}
		
		ENDHLSL
	}
	}
	FallBack "Diffuse"
}
