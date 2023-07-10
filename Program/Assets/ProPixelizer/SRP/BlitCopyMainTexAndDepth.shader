// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
// Copies depth from depth target and color from MainTex.

Shader "Hidden/ProPixelizer/SRP/BlitCopyMainTexAndDepth" {
	Properties{ _MainTex("Texture", any) = "" {} }
		SubShader{
			Pass {
				ZTest Always Cull Off ZWrite On

				HLSLPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0

				
				//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
				//#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
				//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

				// 2022.2 & URP14+
				#define BLIT_API UNITY_VERSION >= 202220
				#if BLIT_API
					#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
					#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

					TEXTURE2D_X_FLOAT(_Depth);
					SAMPLER(sampler_Depth_point_clamp);
					uniform float4 _Depth_ST;
					sampler2D _MainTex;

					struct BCMTADVaryings {
						float4 vertex : SV_POSITION;
						float2 texcoord : TEXCOORD0;
						UNITY_VERTEX_OUTPUT_STEREO
					};

					BCMTADVaryings vert(Attributes v) {
						Varyings vars;
						vars = Vert(v);
						BCMTADVaryings o;
						o.vertex = vars.positionCS;
						o.texcoord = vars.texcoord;
						return o;
					}
				#else
					#include "UnityCG.cginc"

					UNITY_DECLARE_DEPTH_TEXTURE(_Depth);
					uniform float4 _Depth_ST;
					sampler2D _MainTex;

					struct BCMTADVaryings {
						float4 vertex : SV_POSITION;
						float2 texcoord : TEXCOORD0;
						UNITY_VERTEX_OUTPUT_STEREO
					};

					struct Attributes
					{
						float4 vertex : POSITION;
						float2 texcoord : TEXCOORD0;
						UNITY_VERTEX_INPUT_INSTANCE_ID
					};

					BCMTADVaryings vert(Attributes v) {
						BCMTADVaryings o;
						UNITY_SETUP_INSTANCE_ID(v);
						UNITY_INITIALIZE_OUTPUT(BCMTADVaryings, o);
						UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
						o.vertex = UnityObjectToClipPos(v.vertex);
						o.texcoord = TRANSFORM_TEX(v.texcoord.xy, _Depth);
						return o;
					}
				#endif

				float4 frag(BCMTADVaryings i, out float outDepth : SV_Depth) : SV_Target
				{
					#if UNITY_UV_STARTS_AT_TOP
						//i.texcoord.y = 1 - i.texcoord.y;
					#endif
					#if BLIT_API
						outDepth = SAMPLE_TEXTURE2D_X(_Depth, sampler_Depth_point_clamp, i.texcoord);
					#else
						outDepth = SAMPLE_RAW_DEPTH_TEXTURE(_Depth, i.texcoord);
					#endif
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
					return tex2D(_MainTex, i.texcoord);
				}
				ENDHLSL

			}
		}
	Fallback "Hidden/Universal Render Pipeline/Blit"
}