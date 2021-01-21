#include "Planet.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"

float4 _BedRockColor;
float4 _SoilColor;

float4 addTopographyLines(float4 color, float3 xyz, float3 normal)
{
    half lineWidth = 1;
    half lineFrequency = 25;
    half darkening = 0.8;
    
    float height = length(xyz);
    float angleDist = length(normalize(xyz) - normal);
    if (height % lineFrequency < angleDist * lineWidth) 
    {
        float3 hsv = RgbToHsv(color.xyz);
        hsv.z *= darkening;
        color = float4(HsvToRgb(hsv), color.a);
    }
    return saturate(color);
}

half4 LitPassFragmentProgram(FragmentData input) : SV_Target
{
    InputData inputData = InitializeInputData(input);

    float4 color = _BedRockColor;
    color = addTopographyLines(color, inputData.positionWS, inputData.normalWS);
    color = addFocusRing(color, inputData.positionWS);
    
    SurfaceData surfaceData = InitializeSurfaceData(color, inputData);
    color = UniversalFragmentPBR(inputData, surfaceData);
    
    return color;
}

FragmentOutput GBufferFragmentProgram(FragmentData input)
{
    InputData inputData = InitializeInputData(input);
    SurfaceData surfaceData = InitializeSurfaceData(_BedRockColor, inputData);

    Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, inputData.shadowMask);
    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, inputData.shadowMask);
    half4 color = half4(inputData.bakedGI * surfaceData.albedo + surfaceData.emission, surfaceData.alpha);

    return SurfaceDataToGbuffer(surfaceData, inputData, color.rgb, kLightingSimpleLit);
};