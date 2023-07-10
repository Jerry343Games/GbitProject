// Copyright Elliot Bentine, 2018-
//
// Utility functions for outlining.
#ifndef OUTLINE_UTILS_INCLUDED
#define OUTLINE_UTILS_INCLUDED

TEXTURE2D(_ProPixelizerOutlines);
SAMPLER(sampler_ProPixelizerOutlines);

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Version.hlsl"
#include "ShaderGraphUtils.hlsl"

/// <summary>
/// Gets the value from the outline buffer.
/// </summary>
inline void GetOutline_float(float2 texel, out float IDOutline, out float EdgeOutline) {
#if SHADERGRAPH_PREVIEW_TEST
	//disable outlines in shadergraph preview - we don't have the passes to make them work anyway.
	IDOutline = 0;
	EdgeOutline = 0;
#else
	//#if VERSION_GREATER_EQUAL(13,0)
		// URP 13 added functionality to render a subregion of a larger render target. I thought we would need
		// to account for this explicitly, e.g.
		// `float2 uv = texel * _ProPixelizer_RenderTargetInfo.zw;`
		// but we do not. It's possibly that the screen position node already accounts for this.
	//#else
	float4 outlineResult = SAMPLE_TEXTURE2D(_ProPixelizerOutlines, sampler_ProPixelizerOutlines, texel);
	//#endif
	IDOutline = outlineResult.r;
	EdgeOutline = outlineResult.b;
#endif
}
#endif