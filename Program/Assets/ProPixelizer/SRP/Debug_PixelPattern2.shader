// Copyright Elliot Bentine, 2018-
Shader "ProPixelizer/Debug/Shader2"
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
				float v = i.pos.y / _ScreenParams.y;
				return float4(mod(i.pos.x, 2), mod(i.pos.y, 2), 0.0, 1.0);
            }
            ENDHLSL
        }
    }
}
