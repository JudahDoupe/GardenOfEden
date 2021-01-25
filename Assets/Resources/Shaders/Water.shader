Shader "Custom/Water" 
{
	Properties{
		[HideInInspector] _HeightMap ("Height Map", 2DArray) = "black" {}
		[HideInInspector] _HeightChannel ("Height Channel", Int) = 3
		_Tessellation ("Water Geometry Detail", Range(1,64)) = 40
        _SeaLevel ("Sea Level", Int) = 999.8
		
	    [HideInInspector] _FocusPosition("Focus Position",  Vector) = (0,0,0,0)
	    [HideInInspector] _FocusRadius("Focus Radius",  Range(0,1000)) = 0

		_ShallowWaterColor("Shallow Water Color", color) = (0.7,0.7,0.7,1)
		_DeepWaterColor("Deep Water Color", color) = (0.7,0.7,0.7,1)
		_DeepWaterDepth("Deep Water Depth", Float) = 150
		_Clarity("Clarity", Float) = 75
		_Smoothness("Smoothness", Range(0,1)) = 0.9
	}

	SubShader
    {
        Tags { "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" "ShaderModel"="4.5"}
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            
			#pragma vertex TessellationVertexProgram 
			#pragma hull HullProgram
			#pragma domain LitPassDomainProgram
			#pragma fragment LitPassFragmentProgram 
            
			#include "Water.hlsl"
            
            ENDHLSL
        }

        Pass
        {
            Name "GBuffer"
            Tags{"LightMode" = "UniversalGBuffer"}

            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT

			#pragma vertex TessellationVertexProgram 
			#pragma hull HullProgram
			#pragma domain LitPassDomainProgram
			#pragma fragment GBufferFragmentProgram 
            
			#include "Water.hlsl"

            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            #pragma vertex TessellationVertexProgram 
			#pragma hull HullProgram
			#pragma domain ClipSpaceDomainProgram
			#pragma fragment ClipSpaceFragmentProgram 
            
			#include "Water.hlsl"

            ENDHLSL
        }

        // This pass is used when drawing to a _CameraNormalsTexture texture
        Pass
        {
            Name "DepthNormals"
            Tags{"LightMode" = "DepthNormals"}

            ZWrite On

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            #pragma vertex TessellationVertexProgram 
			#pragma hull HullProgram
			#pragma domain ClipSpaceDomainProgram
			#pragma fragment DepthNormalsFragmentProgram 
            
			#include "Water.hlsl"

            ENDHLSL
        }
    }
}