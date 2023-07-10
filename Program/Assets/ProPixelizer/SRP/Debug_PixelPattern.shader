// Copyright Elliot Bentine, 2018-
Shader "ProPixelizer/Debug/Shader1"
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
                // This shader produces a test pattern to help me debug your graphics issues!
				//
				// The test pattern consists of a series of rows which each output a different function.

				float v = i.pos.y / _ScreenParams.y;
				float pixelY = i.pos.y;
				
				// First patterns - check output of pixel position at different resolutions
				if (pixelY < 2) {
					return float4(i.pos.x / 10, mod(i.pos.x,10) / 10, 0.0, 1.0);
				}
				if (pixelY < 4) {
					return float4(i.pos.x / 100, mod(i.pos.x,100) / 100, 0.0, 1.0);
				}
				if (pixelY < 6) {
					return float4(i.pos.x / 256, mod(i.pos.x,256) / 256, 0.0, 1.0);
				}
				if (pixelY < 8) {
					return float4(0.0, 1.0, 0.0, 1.0);
				}

				// Results of different pixel patterns
				// First: step/mod
				if (pixelY < 10) {
					return float4(step(mod(i.pos.x, 2), 0.9), 0.0, 0.0, 1.0);
				}
				if (pixelY < 12) {
					return float4(step(mod(i.pos.x, 3), 0.9), 0.0, 0.0, 1.0);
				}
				if (pixelY < 14) {
					return float4(step(mod(i.pos.x, 4), 0.9), 0.0, 0.0, 1.0);
				}
				if (pixelY < 16) {
					return float4(step(mod(i.pos.x, 5), 0.9), 0.0, 0.0, 1.0);
				}
				if (pixelY < 18) {
					return float4(0.0, 1.0, 0.0, 1.0);
				}

				// Second: floor pixel pos, then mod. 
				if (pixelY < 20) {
					return float4(step(mod(floor(i.pos.x), 2), 0.4), 0.0, 0.0, 1.0);
				}
				if (pixelY < 22) {
					return float4(step(mod(floor(i.pos.x), 3), 0.4), 0.0, 0.0, 1.0);
				}
				if (pixelY < 24) {
					return float4(step(mod(floor(i.pos.x), 4), 0.4), 0.0, 0.0, 1.0);
				}
				if (pixelY < 26) {
					return float4(step(mod(floor(i.pos.x), 5), 0.4), 0.0, 0.0, 1.0);
				}
				if (pixelY < 28) {
					return float4(0.0, 1.0, 0.0, 1.0);
				}

				// Third: Debug first with shift of object position, 0.5 Screen res.
				if (pixelY < 30) {
					return float4(step(mod(i.pos.x - _ScreenParams.x * 0.5, 2), 0.9), 0.0, 0.0, 1.0);
				}
				if (pixelY < 32) {
					return float4(step(mod(i.pos.x - _ScreenParams.x * 0.5, 3), 0.9), 0.0, 0.0, 1.0);
				}
				if (pixelY < 34) {
					return float4(step(mod(i.pos.x - _ScreenParams.x * 0.5, 4), 0.9), 0.0, 0.0, 1.0);
				}
				if (pixelY < 36) {
					return float4(step(mod(i.pos.x - _ScreenParams.x * 0.5, 5), 0.9), 0.0, 0.0, 1.0);
				}
				if (pixelY < 38) {
					return float4(0.0, 1.0, 0.0, 1.0);
				}

				// Fourth: Debug second with shift of object position, 0.5 Screen res.
				if (pixelY < 40) {
					return float4(step(mod(floor(i.pos.x - _ScreenParams.x * 0.5), 2), 0.4), 0.0, 0.0, 1.0);
				}
				if (pixelY < 42) {
					return float4(step(mod(floor(i.pos.x - _ScreenParams.x * 0.5), 3), 0.4), 0.0, 0.0, 1.0);
				}
				if (pixelY < 44) {
					return float4(step(mod(floor(i.pos.x - _ScreenParams.x * 0.5), 4), 0.4), 0.0, 0.0, 1.0);
				}
				if (pixelY < 46) {
					return float4(step(mod(floor(i.pos.x - _ScreenParams.x * 0.5), 5), 0.4), 0.0, 0.0, 1.0);
				}
				if (pixelY < 48) {
					return float4(0.0, 1.0, 0.0, 1.0);
				}

				// Fifth: Debug second with shift of object position, 0.5 Screen res.
				if (pixelY < 50) {
					return float4(step(mod(i.pos.x+0.1, 2), 0.9), 0.0, 0.0, 1.0);
				}
				if (pixelY < 52) {
					return float4(step(mod(i.pos.x + 0.1, 3), 0.9), 0.0, 0.0, 1.0);
				}
				if (pixelY < 54) {
					return float4(step(mod(i.pos.x + 0.1, 4), 0.9), 0.0, 0.0, 1.0);
				}
				if (pixelY < 56) {
					return float4(step(mod(i.pos.x + 0.1, 5), 0.9), 0.0, 0.0, 1.0);
				}
				if (pixelY < 58) {
					return float4(0.0, 1.0, 0.0, 1.0);
				}


				return float4(0.0, 0.0, 0.0, 1.0);

            }
            ENDHLSL
        }
    }
}
