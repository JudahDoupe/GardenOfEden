#include "Colors.hlsl"
#include "CoordinateTransforms.hlsl"

float4 addDiffuseLighting(float4 color, float3 normal, float3 sunDir, float diffuse)
{
    float diffuseLighting = (saturate(dot(sunDir, normal)) * (diffuse / 1)) + ((1 - diffuse) / 1);
    return color * diffuseLighting;
}

float4 addSpecularHighlight(float4 color, float3 normal, float3 sunDir, float3 cameraDir, float smoothness)
{
    float specularAngle = acos(dot(normalize(sunDir - cameraDir), normal));
    float specularExponent = specularAngle / (1 - smoothness);
    float specularHighlight = exp(-specularExponent * specularExponent);
    return color + specularHighlight;
}

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
    return color;
}

float4 addFocusRing(float4 color, float3 xyz, float3 focusXyz, float radius)
{
    half lightening = 1.8;
    
    float d = distance(xyz, focusXyz);
    if (radius - (radius / 10) < d && d < radius)
    {
        float3 hsl = rgb_to_hsl(color.xyz);
        hsl.z = saturate(hsl.z * lightening);
        color = float4(hsl_to_rgb(hsl), color.a);
    }
    return color;
}