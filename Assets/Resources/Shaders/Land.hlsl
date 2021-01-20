﻿#include "Planet.hlsl"
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
        float3 hsl = rgb_to_hsl(color.xyz);
        hsl.z *= darkening;
        color = float4(hsl_to_rgb(hsl), color.a);
    }
    return saturate(color);
}

half4 LitPassFragmentProgram(FragmentData input) : SV_Target
{
    InputData inputData = InitializeInputData(input);
    SurfaceData surfaceData = InitializeSurfaceData(_BedRockColor, 0);

    float4 color = UniversalFragmentPBR(inputData, surfaceData);
    color = addTopographyLines(color, inputData.positionWS, inputData.normalWS);
    color = addFocusRing(color, inputData.positionWS);

    return color;
}

FragmentOutput GBufferFragmentProgram(FragmentData input)
{
    SurfaceData surfaceData = InitializeSurfaceData(_BedRockColor, 0);
    InputData inputData = InitializeInputData(input);

    Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, inputData.shadowMask);
    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, inputData.shadowMask);
    half4 color = half4(inputData.bakedGI * surfaceData.albedo + surfaceData.emission, surfaceData.alpha);

    return SurfaceDataToGbuffer(surfaceData, inputData, color.rgb, kLightingSimpleLit);
};