// Copyright Elliot Bentine, 2018-
Shader "ProPixelizer/Debug/ObjectPosition"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma target 2.5

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "PixelUtils.hlsl"

			struct appdata
			{
				float4 vertex : POSITION; // vertex position
			};

			struct Varyings {
				float4 pos : SV_POSITION; // clip space position
			};

			Varyings vert(
				appdata data
			)
			{
				Varyings output = (Varyings)0;
				VertexPositionInputs vertexInput = GetVertexPositionInputs(data.vertex.xyz);
				output.pos = float4(vertexInput.positionCS);
				return output;
			}

            float4 frag (Varyings i) : SV_Target
            {
				float4 object_pixel_pos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1));
                float4 objClipPos = mul(unity_MatrixVP, float4(object_pixel_pos.xyz, 1));
				float2 objViewPos = objClipPos.xy / objClipPos.w;
				float2 test = (0.5 * (objViewPos.xy + 1));
				float2 objPixelPos = (0.5 * (objViewPos.xy + 1) * (_ScreenParams.xy));
				float2 testPos = objPixelPos.xy;
				testPos = floor(testPos + 0.1);
				testPos = float2(mod(testPos.x, 2.0), mod(testPos.y, 2.0));
				return float4(testPos, 0, 1);
            }
            ENDHLSL
        }
    }
}
