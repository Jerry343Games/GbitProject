// Copyright Elliot Bentine, 2018-
Shader "Hidden/ProPixelizer/SRP/OutlineDetection" {
	Properties{
		_OutlineDepthTestThreshold("Threshold used for depth testing outlines.", Float) = 0.0001
	}

		SubShader{
		Tags{
			"RenderPipeline" = "UniversalRenderPipeline"
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
			#pragma multi_compile_local DEPTH_TEST_OUTLINES_ON _
			#pragma multi_compile NORMAL_EDGE_DETECTION_ON _

			#if DEPTH_TEST_OUTLINES_ON
			float _OutlineDepthTestThreshold;
			#endif

			#if NORMAL_EDGE_DETECTION_ON
			float _NormalEdgeDetectionSensitivity;
			#endif

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex_point_clamp);
			float4 _TexelSize;
			TEXTURE2D_X_FLOAT(_MainTex_Depth);
			SAMPLER(sampler_MainTex_Depth_point_clamp);

			struct ProPVaryings {
				float4 pos : SV_POSITION;
				float4 scrPos : TEXCOORD1;
			};

			#define BLIT_API UNITY_VERSION >= 202220

			// 2022.2 & URP14+
			#if BLIT_API
				#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

				ProPVaryings vert(Attributes v) {
					Varyings vars;
					vars = Vert(v);
					ProPVaryings o;
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

			// Tests the outline IDs are the same for the given gradient dir. This is to prevent 'creasing' when objects differ (for which silhouette is more reliable).
			inline float checkMatchingIDsForCrease(float2 mainTexel, float2 gradientDir, float ID, float pixelSize) {
				
				float4 neighbourA = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex_point_clamp, mainTexel + gradientDir * _TexelSize.xy * pixelSize);
				float4 neighbourB = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex_point_clamp, mainTexel - gradientDir * _TexelSize.xy * pixelSize);
				return getUID(neighbourA) == getUID(neighbourB) && getUID(neighbourA) == ID ? 1.0 : 0.0;
			}

			inline float countNeighbourID(float2 mainTexel, float2 neighbour, float pixelSize, float ID, float depth) {
				float2 npos = mainTexel + float2(neighbour.x * _TexelSize.x * pixelSize, neighbour.y * _TexelSize.y * pixelSize);
				float4 neighbourD = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex_point_clamp, npos);

				#if DEPTH_TEST_OUTLINES_ON
				float neighbourDepth = SAMPLE_TEXTURE2D_X(_MainTex_Depth, sampler_MainTex_Depth_point_clamp, npos).r;
					#if UNITY_REVERSED_Z
						bool neighbourInFront = neighbourDepth > depth + _OutlineDepthTestThreshold;
					#else
						bool neighbourInFront = neighbourDepth < depth - _OutlineDepthTestThreshold;
					#endif
						return neighbourInFront || (getUID(neighbourD) == ID && AlphaToPixelSize(neighbourD.a) > 0.5) ? 1 : 0;
				#else
					return getUID(neighbourD) == ID && AlphaToPixelSize(neighbourD.a) > 0.5 ? 1 : 0;
				#endif
			}

			inline float3 getOutlineNormals(float2 uv) {
				float2 rg = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex_point_clamp, uv).rb * 2 - 1;
				float b = sqrt(1 - dot(rg, rg));
				return float3(rg, b);
			}

			void frag(ProPVaryings i, out float4 color: COLOR) {
			
				float2 mainTexel = i.scrPos.xy;
				float2 pShift = float2(_TexelSize.x, _TexelSize.y);

				float4 packedData = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex_point_clamp, mainTexel);
				float4 outline_color;
				float ID;
				float pixelSize;
				UnpackOutline(packedData, outline_color, ID, pixelSize);

				if (pixelSize < 1)
				{
					// if this pixel is not pixelised, just return main texture colour.
					color = float4(0,0,0,1);
					return;
				}

				UnpackOutline(packedData, outline_color, ID, pixelSize);
			 
				#if DEPTH_TEST_OUTLINES_ON
					float depth = SAMPLE_TEXTURE2D_X(_MainTex_Depth, sampler_MainTex_Depth_point_clamp, mainTexel).r;
				#else
					float depth = 0;
				#endif
				
				// Neighbour ID comparison
				// Loop over nearest neighbours.
				float countSimilar = 0;
				countSimilar = countSimilar + countNeighbourID(mainTexel, float2(-1,  1), pixelSize, ID, depth);
				countSimilar = countSimilar + countNeighbourID(mainTexel, float2( 0,  1), pixelSize, ID, depth);
				countSimilar = countSimilar + countNeighbourID(mainTexel, float2( 1,  1), pixelSize, ID, depth);
				countSimilar = countSimilar + countNeighbourID(mainTexel, float2(-1,  0), pixelSize, ID, depth);
				countSimilar = countSimilar + countNeighbourID(mainTexel, float2( 0,  0), pixelSize, ID, depth);
				countSimilar = countSimilar + countNeighbourID(mainTexel, float2( 1,  0), pixelSize, ID, depth);
				countSimilar = countSimilar + countNeighbourID(mainTexel, float2(-1, -1), pixelSize, ID, depth);
				countSimilar = countSimilar + countNeighbourID(mainTexel, float2( 0, -1), pixelSize, ID, depth);
				countSimilar = countSimilar + countNeighbourID(mainTexel, float2( 1, -1), pixelSize, ID, depth);
				float IDfactor = countSimilar > 7 ? 0.0 : 1.0;

				// Edge detection through normals.
				#if NORMAL_EDGE_DETECTION_ON
					float3 normalSamples[4];
					normalSamples[0] = getOutlineNormals(mainTexel + pixelSize * float2(-_TexelSize.x, 0));
					normalSamples[1] = getOutlineNormals(mainTexel + 0*pixelSize * float2(_TexelSize.x, 0));
					normalSamples[2] = getOutlineNormals(mainTexel + pixelSize * float2(0, -_TexelSize.y));
					normalSamples[3] = getOutlineNormals(mainTexel + 0*pixelSize * float2(0, _TexelSize.y));
					float3 dNormalX = normalSamples[1] - normalSamples[0];
					float3 dNormalY = normalSamples[3] - normalSamples[2];
					float xTest = checkMatchingIDsForCrease(mainTexel, float2(1,0), ID, pixelSize);
					float yTest = checkMatchingIDsForCrease(mainTexel, float2(0, 1), ID, pixelSize);
					float edgeNormalSq = dot(dNormalX, dNormalX) * xTest + dot(dNormalY, dNormalY) * yTest;
					float normalFactor = edgeNormalSq > (1 / _NormalEdgeDetectionSensitivity) ? 1 : 0;
				#else
					float normalFactor = 0;
				#endif

				color.r = IDfactor;
				color.b = normalFactor;
				color.g = 0;
				color.a = 1.0;
			
				// Test normal retrieval - matches object outline debug.
				//color.rb = getOutlineNormals(mainTexel).rg * 0.5 + 0.5;
				//color.a = dot(getOutlineNormals(mainTexel), TransformWorldToViewDir(float3(1, 0, 0)));
			}
		
		ENDHLSL
		}
	}
	//FallBack "Diffuse"
}
