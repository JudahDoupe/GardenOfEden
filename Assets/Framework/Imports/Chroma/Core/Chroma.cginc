#ifndef DUSTYROOM_CHROMA_INCLUDED
#define DUSTYROOM_CHROMA_INCLUDED

// ----------------------------------------------------------------------------
float3 SRGBToLinear(float3 c)
{
    const float3 linearRGBLo  = c / 12.92;
    const float3 linearRGBHi  = abs(pow((c + 0.055) / 1.055, float3(2.4, 2.4, 2.4)));
    float3 linearRGB    = (c <= 0.04045) ? linearRGBLo : linearRGBHi;
    return linearRGB;
}
 
float4 SRGBToLinear(float4 c)
{
    return float4(SRGBToLinear(c.rgb), c.a);
}

#define GRADIENT(textureName) UNITY_DECLARE_TEX2D(textureName)

#define SAMPLE_GRADIENT_X(textureName, sampler, coord) UNITY_SAMPLE_TEX2D_SAMPLER(textureName, sampler, float2(coord, 0.5))
#define SAMPLE_GRADIENT(textureName, coord) SAMPLE_GRADIENT_X(textureName, textureName, coord)

#if defined(UNITY_COLORSPACE_GAMMA)
#define SAMPLE_GRADIENT_HDR_X(textureName, sampler, coord) SAMPLE_GRADIENT_X(textureName, sampler, coord)
#else
#define SAMPLE_GRADIENT_HDR_X(textureName, sampler, coord) SRGBToLinear(SAMPLE_GRADIENT_X(textureName, sampler, coord))
#endif

#define SAMPLE_GRADIENT_HDR(textureName, coord) SAMPLE_GRADIENT_HDR_X(textureName, sampler##textureName, coord)

/*
void SampleGradient_float(Texture2D Gradient, SamplerState Sampler, float Coord, out float4 Color)
{
    Color = SAMPLE_GRADIENT_X(Gradient, Sampler, Coord);
}

void SampleGradientHDR_float(Texture2D Gradient, SamplerState Sampler, float Coord, out float4 Color)
{
    Color = SAMPLE_GRADIENT_HDR_X(Gradient, Sampler, Coord);
}
*/

// ----------------------------------------------------------------------------
#define CURVE(textureName) TEXTURE2D(textureName); SAMPLER(sampler##textureName)
#define SAMPLE_CURVE(textureName, coord) SAMPLE_TEXTURE2D(textureName, sampler##textureName, float2(coord, 0.5)).r
#define SAMPLE_CURVE_X(textureName, sampler, coord) SAMPLE_TEXTURE2D(textureName, sampler, float2(coord, 0.5)).r

#endif  // DUSTYROOM_CHROMA_INCLUDED
