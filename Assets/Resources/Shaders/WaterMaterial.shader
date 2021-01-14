Shader "Custom/Water" 
{
	Properties{
		_WaterMap ("Water Map", 2DArray) = "black" {}
        _Tess ("Water Geometry Detail", Range(10,64)) = 40
        _SeaLevel ("Sea Level", Int) = 1000

		_ShallowWaterColor("Shallow Water Color", color) = (0.7,0.7,0.7,1)
		_DeepWaterColor("Deep Water Color", color) = (0.7,0.7,0.7,1)
		_DeepWaterDepth("Deep Water Depth", Float) = 150
		_Clarity("Clarity", Float) = 75
		_Smoothness("Smoothness", Range(0,1)) = 0.9
		
	    _FocusPosition("Focus Position",  Vector) = (0,0,0,0)
	    _FocusRadius("Focus Radius",  Range(0,1000)) = 0
	}
	SubShader{
		Tags { "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" "ShaderModel"="2.0"}
		LOD 300
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off

		Pass {
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

            TEXTURE2D_ARRAY(_WaterMap);
            SAMPLER(sampler_WaterMap);
			float _Tess;
			float _SeaLevel;

			float4 _ShallowWaterColor;
			float4 _DeepWaterColor;
			float _DeepWaterDepth;
			float _Clarity;
			float _Smoothness;

			float3 _FocusPosition;
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
 
			   return DisplaceVertexProgram(_WaterMap, sampler_WaterMap, 3, data, _SeaLevel);
			}
			 
			half4 FragmentProgram(FragmentData input) : SV_Target
			{
				//float opticalTerrainDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, input.screenPos)).r;
				float opticalWaterDepth = 0.5;//saturate((opticalTerrainDepth - input.screenPos.w) / _DeepWaterDepth);
				float alpha = 0.5; //clamp((opticalTerrainDepth - input.screenPos.w) / _Clarity, 0.1, 0.9);

				float3 uvw = xyz_to_uvw(input.positionOS);
				float4 waterMap = SAMPLE_TEXTURE2D_ARRAY_LOD(_WaterMap, sampler_WaterMap, uvw.xy, uvw.z, 0);

				InputData inputData = InitializeInputData(input);

				float4 color = lerp(_ShallowWaterColor, _DeepWaterColor, opticalWaterDepth);
				color = UniversalFragmentBlinnPhong(inputData, color.xyz, 1, _Smoothness, 0, alpha);
	            color = addFocusRing(color, inputData.positionWS, _FocusPosition, _FocusRadius);
				color.a = alpha * (waterMap.b > 0.1);

				return saturate(color); 
			}
			ENDHLSL
		}
	}
}