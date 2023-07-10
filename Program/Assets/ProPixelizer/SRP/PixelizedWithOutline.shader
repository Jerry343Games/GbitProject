// Copyright Elliot Bentine, 2018-
Shader "ProPixelizer/SRP/PixelizedWithOutline"
{
	//A shader that renders outline buffer data and color appearance for pixelated objects.
	//
	// If you want to add your own properties to the PixelizedWithOutline shader, or modify
	// the appearance of it, you can make changes to the ProPixelizerBase shader graph.
	// Make sure you follow these steps:
	//   1. Make your changes to the ProPixelizerBase graph, e.g. adding new properties,
	//      changing connections. Your changes will not be visible until Unity reloads this
	//      shader. The easiest way to trigger recompilation is to modify and re-saving
	//      this file.
	//   2. Make sure the UnityPerMaterial CBUFFER in the ProPixelizerPass below matches that
	//      in the generated ProPixelizerBase shader (you can view the generated shader from
	//      the inspector window).
	//   3. If you want to edit your new properties in editor, it might help to disable the
	//      CustomEditor at the bottom of this file.

    Properties
    {
		_LightingRamp("LightingRamp", 2D) = "white" {}
		_PaletteLUT("PaletteLUT", 2D) = "white" {}
		[MainTex][NoScaleOffset]_Albedo("Albedo", 2D) = "white" {}
		_Albedo_ST("Albedo_ST", Vector) = (1, 1, 0, 0)
		[MainColor]_BaseColor("Color", Color) = (1, 1, 1, 1)
		_AmbientLight("AmbientLight", Color) = (0.2, 0.2, 0.2, 1.0)
		[IntRange] _PixelSize("PixelSize", Range(1, 5)) = 3
		_PixelGridOrigin("PixelGridOrigin", Vector) = (0, 0, 0, 0)
		[Normal][NoScaleOffset]_NormalMap("Normal Map", 2D) = "bump" {}
		_NormalMap_ST("Normal Map_ST", Vector) = (1, 1, 0, 0)
		[NoScaleOffset]_Emission("Emission", 2D) = "white" {}
		_Emission_ST("Emission_ST", Vector) = (1, 1, 0, 0)
		_EmissionColor("EmissionColor", Color) = (1, 1, 1, 0)
		_AlphaClipThreshold("Alpha Clip Threshold", Range(0, 1)) = 0.5
		[IntRange] _ID("ID", Range(0, 255)) = 1 // A unique ID used to differentiate objects for purposes of outlines.
		_OutlineColor("OutlineColor", Color) = (0.0, 0.0, 0.0, 0.5)
		_EdgeHighlightColor("Edge Highlight Color", Color) = (0.5, 0.5, 0.5, 0)
		_DiffuseVertexColorWeight("DiffuseVertexColorWeight", Range(0, 1)) = 1
		_EmissiveVertexColorWeight("EmissiveVertexColorWeight", Range(0, 1)) = 0
		[HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
		[HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
		[HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
		[Toggle]COLOR_GRADING("Use Color Grading", Float) = 0
		[Toggle]USE_OBJECT_POSITION("Use Object Position", Float) = 1
		[Toggle]RECEIVE_SHADOWS("ReceiveShadows", Float) = 1
		[Toggle]PROPIXELIZER_DITHERING("Use Dithering", Float) = 1
	}

		SubShader
		{
			Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

			UsePass "ProPixelizer/Hidden/ProPixelizerBase/UNIVERSAL FORWARD"
			UsePass "ProPixelizer/Hidden/ProPixelizerBase/SHADOWCASTER"
			UsePass "ProPixelizer/Hidden/ProPixelizerBase/DEPTHONLY"
			UsePass "ProPixelizer/Hidden/ProPixelizerBase/DEPTHNORMALS"

		Pass
		{
			Name "ProPixelizerPass"
			Tags {
				"RenderPipeline" = "UniversalRenderPipeline"
				"LightMode" = "ProPixelizer"
				"DisableBatching" = "True"
			}

			ZWrite On
			Cull Off
			Blend Off

			HLSLPROGRAM
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "PixelUtils.hlsl"
			#include "PackingUtils.hlsl"
			#include "ScreenUtils.hlsl" 
			#pragma vertex outline_vert
			#pragma fragment outline_frag
			#pragma target 2.5
			#pragma multi_compile_local USE_OBJECT_POSITION_ON _
			#pragma multi_compile USE_ALPHA_ON _
			#pragma multi_compile NORMAL_EDGE_DETECTION_ON _
			#pragma multi_compile_local PROPIXELIZER_DITHERING_ON _
			
			// If you want to use the SRP Batcher:
			// The CBUFFER has to match that generated from ShaderGraph - otherwise all hell breaks loose.
			// In some cases, it might be easier to just break SRP Batching support for your outline shader.
			// Graph Properties
			CBUFFER_START(UnityPerMaterial)
			float4 _LightingRamp_TexelSize;
			float4 _PaletteLUT_TexelSize;
			float4 _Albedo_TexelSize;
			float4 _Albedo_ST;
			float4 _BaseColor;
			float4 _AmbientLight;
			float _PixelSize;
			float4 _PixelGridOrigin;
			float4 _NormalMap_TexelSize;
			float4 _NormalMap_ST;
			float4 _Emission_TexelSize;
			float4 _Emission_ST;
			float _AlphaClipThreshold;
			float _ID;
			float4 _OutlineColor;
			float4 _EdgeHighlightColor;
			float4 _EmissionColor;
			float _DiffuseVertexColorWeight;
			float _EmissiveVertexColorWeight;
			CBUFFER_END
			
			// Object and Global properties
			SAMPLER(SamplerState_Linear_Repeat);
			SAMPLER(SamplerState_Point_Clamp);
			SAMPLER(SamplerState_Point_Repeat);
			TEXTURE2D(_LightingRamp);
			SAMPLER(sampler_LightingRamp);
			TEXTURE2D(_PaletteLUT);
			SAMPLER(sampler_PaletteLUT);
			TEXTURE2D(_Albedo);
			SAMPLER(sampler_Albedo);
			TEXTURE2D(_NormalMap);
			SAMPLER(sampler_NormalMap);
			TEXTURE2D(_Emission);
			SAMPLER(sampler_Emission);

			#include "OutlinePass.hlsl"
			ENDHLSL
		}
     }
	CustomEditor "PixelizedWithOutlineShaderGUI"
	FallBack "ProPixelizer/Hidden/ProPixelizerBase"
}
