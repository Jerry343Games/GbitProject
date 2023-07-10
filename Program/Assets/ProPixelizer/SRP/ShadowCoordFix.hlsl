#ifndef SHADOWCOORDFIX_INCLUDED
	#define SHADOWCOORDFIX_INCLUDED

	// Note that we import this from PixelUtils, and _not_ from LightUtils.
	// This is because we need the import to run for passes like shadowcaster, which
	// don't calculate a color (but do calculate an alpha, hence PixelUtils will be imported)

	// The following undef is from Cyanilux's repo at https://github.com/Cyanilux/URP_ShaderGraphCustomLighting/blob/main/CustomLighting.hlsl:
	/*
	- This undef (un-define) is required to prevent the "invalid subscript 'shadowCoord'" error,
	  which occurs when _MAIN_LIGHT_SHADOWS is used with 1/No Shadow Cascades with the Unlit Graph.
	- It's technically not required for the PBR/Lit graph, so I'm using the SHADERPASS_FORWARD to ignore it for the pass.
	  (But it would probably still remove the interpolator for other passes in the PBR/Lit graph and use a per-pixel version)
	*/
#ifndef SHADERGRAPH_PREVIEW
	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Version.hlsl"
	#if VERSION_GREATER_EQUAL(9, 0)
		#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
		#if (SHADERPASS != SHADERPASS_FORWARD)
			#undef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
		#endif
	#else
		#ifndef SHADERPASS_FORWARD
			#undef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
		#endif
	#endif
#endif
#endif