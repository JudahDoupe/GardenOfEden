#include "Planet.hlsl"

float4 _ShallowWaterColor;
float4 _DeepWaterColor;
float _DeepWaterDepth;
float _Clarity;
float _Smoothness;
			 
half4 FragmentProgram(FragmentData input) : SV_Target
{
				//float opticalTerrainDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, input.screenPos)).r;
    float opticalWaterDepth = 0.5; //saturate((opticalTerrainDepth - input.screenPos.w) / _DeepWaterDepth);
    float alpha = 0.5; //clamp((opticalTerrainDepth - input.screenPos.w) / _Clarity, 0.1, 0.9);

    float3 uvw = xyz_to_uvw(input.positionOS);
    float4 waterMap = SAMPLE_TEXTURE2D_ARRAY_LOD(_HeightMap, sampler_HeightMap, uvw.xy, uvw.z, 0);

    InputData inputData = InitializeInputData(input);

    float4 color = lerp(_ShallowWaterColor, _DeepWaterColor, opticalWaterDepth);
    color = addFocusRing(color, inputData.positionWS);
    color.a = alpha * (waterMap.b > 0.1);
    color = UniversalFragmentBlinnPhong(inputData, color.xyz, float4(1, 1, 1, 1), _Smoothness, 0, color.a);

    return color;
}
