Shader "Custom/Land"
{
    Properties
    {        
		[HideInInspector] _HeightMap ("Height Map", 2DArray) = "black" {}
		[HideInInspector] _HeightChannel ("Height Channel", Int) = 0
        _Tessellation ("Terrain Detail", Range(0,50)) = 40
        _SeaLevel ("Sea Level", Int) = 1000
        
	    [HideInInspector] _FocusPosition("Focus Position",  Vector) = (0,0,0,0)
	    [HideInInspector] _FocusRadius("Focus Radius",  Range(0,1000)) = 0

		_BaseColor("Bedrock Color", color) = (0.7,0.7,0.7,1)
		_BedRockColor("Bedrock Color", color) = (0.7,0.7,0.7,1)
	    _SoilColor("Soil Color", color) = (0.55,0.27,0.12,1)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" "ShaderModel"="4.5"}
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend One Zero
            ZWrite On

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

            #include "Land.hlsl"

            /*
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitForwardPass.hlsl"
            */
            
            ENDHLSL
        }
        
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

			#pragma vertex TessellationVertexProgram 
			#pragma hull HullProgram
			#pragma domain ClipSpaceDomainProgram
			#pragma fragment ClipSpaceFragmentProgram 

            #include "Land.hlsl"

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

		    #include "Land.hlsl"

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

            #include "Land.hlsl"

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
            
            #include "Land.hlsl"

            ENDHLSL
        }
    }
}
