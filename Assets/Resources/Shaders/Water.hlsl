#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"
#include "Planet.hlsl"

float4 _ShallowWaterColor;
float4 _DeepWaterColor;
float _DeepWaterDepth;
float _Clarity;
float _Smoothness;
float _Occlusion;
			 
half4 LitPassFragmentProgram(FragmentData input) : SV_Target
{
    float opticalTerrainDepth = LinearEyeDepth(LOAD_TEXTURE2D_X(_CameraDepthTexture, input.positionCS.xy).x, _ZBufferParams);
    float opticalWaterDepth = opticalTerrainDepth - input.positionCS.w;

    float3 uvw = xyz_to_uvw(input.positionOS);
    float4 waterMap = SAMPLE_TEXTURE2D_ARRAY_LOD(_HeightMap, sampler_HeightMap, uvw.xy, uvw.z, 0);

    InputData inputData = InitializeInputData(input);

    float4 color = lerp(_ShallowWaterColor, _DeepWaterColor, saturate(opticalWaterDepth / _DeepWaterDepth));
    color = addFocusRing(color, inputData.positionWS);
    color.a = clamp(min(opticalWaterDepth / _Clarity, waterMap.b), 0, 0.9);
    
    SurfaceData surfaceData = InitializeSurfaceData(color, inputData);
    surfaceData.smoothness = _Smoothness;
    color = UniversalFragmentPBR(inputData, surfaceData);

    return color;
}

FragmentOutput GBufferFragmentProgram(FragmentData input)
{
    InputData inputData = InitializeInputData(input);
    SurfaceData surfaceData = InitializeSurfaceData(_ShallowWaterColor, inputData);

    Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, inputData.shadowMask);
    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, inputData.shadowMask);
    half4 color = half4(inputData.bakedGI * surfaceData.albedo + surfaceData.emission, surfaceData.alpha);

    return SurfaceDataToGbuffer(surfaceData, inputData, color.rgb, kLightingSimpleLit);
};