#include "Colors.hlsl"
#include "CoordinateTransforms.hlsl"

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
        color = float4(hsl_to_rgb(hsl), 0);
    }
    return color;
}