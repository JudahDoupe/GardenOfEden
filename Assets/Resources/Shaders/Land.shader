Shader "Custom/Land"
{
    Properties
    {        
		[HideInInspector] _HeightMap ("Height Map", 2DArray) = "black" {}
		[HideInInspector] _HeightChannel ("Height Channel", Int) = 0
        _Tessellation ("Terrain Detail", Range(10,50)) = 40
        _SeaLevel ("Sea Level", Int) = 1000
        
	    [HideInInspector] _FocusPosition("Focus Position",  Vector) = (0,0,0,0)
	    [HideInInspector] _FocusRadius("Focus Radius",  Range(0,1000)) = 0

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

			#pragma vertex TessellationVertexProgram 
			#pragma hull HullProgram
			#pragma domain DomainProgram
			#pragma fragment FragmentProgram 

		    #include "Land.hlsl"

            ENDHLSL
        }
    }
}
