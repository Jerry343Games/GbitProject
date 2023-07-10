// Copyright Elliot Bentine, 2018-
#ifndef PROPIXELIZER_SHADERGRAPH_UTILS_INCLUDED
	#define PROPIXELIZER_SHADERGRAPH_UTILS_INCLUDED

	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Version.hlsl"

	// Defines SHADERGRAPH_PREVIEW_TEST, which can be used to test if we are in a shadergraph preview.
	#if VERSION_GREATER_EQUAL(10, 0)
		// URP10: SHADERGRAPH_PREVIEW refers to individual node previews.
		#define SHADERGRAPH_PREVIEW_TEST defined(SHADERGRAPH_PREVIEW)
	#else
		#define SHADERGRAPH_PREVIEW_TEST SHADERGRAPH_PREVIEW
	#endif

	// Disable for safety, for now.
	//#if VERSION_GREATER_EQUAL(13, 0)
	//	// SHADERPASS_FORWARD_PREVIEW refers to the preview window. It can't be defined in previous version though -
	//	// If you do, the value is zero so this test will pass TRUE on the forward rendering pass!
	//	// It was an exceptionally poor design decision to use '0' for a pass...
	//	#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
	//	#define SHADERGRAPH_PREVIEW_WINDOW ( SHADERPASS == SHADERPASS_FORWARD_PREVIEW )
	//#endif

#endif