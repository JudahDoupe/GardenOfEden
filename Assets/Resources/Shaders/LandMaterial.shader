Shader "Custom/Land"
{
    Properties
    {        
        _LandMap ("Land Map", 2DArray) = "black" {}
        _Tess ("Terrain Detail", Range(10,50)) = 40
        _SeaLevel ("Sea Level", Int) = 1000

		_BedRockColor("Bedrock Color", color) = (0.7,0.7,0.7,1)
	    _SoilColor("Soil Color", color) = (0.55,0.27,0.12,1)
        
	    _FocusPosition("Focus Position",  Vector) = (0,0,0,0)
	    _FocusRadius("Focus Radius",  Range(0,1000)) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" "ShaderModel"="2.0"}
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            // Use same blending / depth states as Standard shader
            Blend One Zero
            ZWrite On

            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION

		    #include "TerrainHelpers.hlsl"

			#pragma vertex TessellationVertexProgram 
			#pragma hull HullProgram
			#pragma domain DomainProgram
			#pragma fragment FragmentProgram 

            TEXTURE2D_ARRAY(_LandMap);
            SAMPLER(sampler_LandMap);
            float _SeaLevel;
			float _Tess;

            float4 _BedRockColor;
            float4 _SoilColor;

            float4 _FocusPosition;
            float _FocusRadius;

            ControlPoint TessellationVertexProgram(VertexData v)
            {
                ControlPoint p;
                p.positionOS = v.positionOS;
                p.normalOS = v.normalOS;
                p.tangentOS = v.tangentOS;
                return p;
            }

            [domain("tri")]
            [outputcontrolpoints(3)]
            [outputtopology("triangle_cw")]
            [partitioning("integer")]
            [patchconstantfunc("PatchConstantFunction")]
			ControlPoint HullProgram(InputPatch<ControlPoint, 3> patch, uint id : SV_OutputControlPointID)
            {
                return patch[id];
            }
            TessellationFactors PatchConstantFunction(InputPatch<ControlPoint, 3> patch)
            {
                TessellationFactors f;
                f.edge[0] = _Tess;
                f.edge[1] = _Tess;
                f.edge[2] = _Tess;
                f.inside = _Tess;
                return f;
            }

            [domain("tri")]
			FragmentData DomainProgram(TessellationFactors factors, OutputPatch<ControlPoint, 3> patch, float3 barycentricCoordinates : SV_DomainLocation)
			{
			    VertexData data;
 
				#define PROGRAM_INTERPOLATE(fieldName) data.fieldName = \
					patch[0].fieldName * barycentricCoordinates.x + \
					patch[1].fieldName * barycentricCoordinates.y + \
					patch[2].fieldName * barycentricCoordinates.z;

				PROGRAM_INTERPOLATE(positionOS)
				PROGRAM_INTERPOLATE(normalOS)
				PROGRAM_INTERPOLATE(tangentOS)
				data.normalOS = normalize(data.normalOS);
				data.tangentOS = normalize(data.tangentOS);
 
			   return DisplaceVertexProgram(_LandMap, sampler_LandMap, 0, data, _SeaLevel);
			}

            half4 FragmentProgram(FragmentData input) : SV_Target
            {
                InputData inputData = InitializeInputData(input);

                float4 color = _BedRockColor;
                color = UniversalFragmentBlinnPhong(inputData, color.xyz, 0, 0, 0, 1);
	            color = addTopographyLines(color, inputData.positionWS, inputData.normalWS);
	            color = addFocusRing(color, inputData.positionWS, _FocusPosition, _FocusRadius);

                return color; 
            }

            ENDHLSL
        }
    }
}
