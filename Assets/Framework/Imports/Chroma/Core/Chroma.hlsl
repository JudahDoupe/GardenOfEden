#ifndef DUSTYROOM_CHROMA_INCLUDED
#define DUSTYROOM_CHROMA_INCLUDED

// ----------------------------------------------------------------------------
#define GRADIENT(textureName) TEXTURE2D(textureName); SAMPLER(sampler##textureName)

#define SAMPLE_GRADIENT_X(textureName, sampler, coord) SAMPLE_TEXTURE2D(textureName, sampler, float2(coord, 0.5))
#define SAMPLE_GRADIENT(textureName, coord) SAMPLE_GRADIENT_X(textureName, sampler##textureName, coord)

#if defined(UNITY_COLORSPACE_GAMMA)
#define SAMPLE_GRADIENT_HDR_X(textureName, sampler, coord) SAMPLE_GRADIENT_X(textureName, sampler, coord)
#else
#define SAMPLE_GRADIENT_HDR_X(textureName, sampler, coord) SRGBToLinear(SAMPLE_GRADIENT_X(textureName, sampler, coord))
#endif

#define SAMPLE_GRADIENT_HDR(textureName, coord) SAMPLE_GRADIENT_HDR_X(textureName, sampler##textureName, coord)

// 57e5f804-6d4f-4cb3-94de-2446540651b6
// 95b02117-de66-49f0-91e7-cc5f4291cf90

// ----------------------------------------------------------------------------
#define CURVE(textureName) TEXTURE2D(textureName); SAMPLER(sampler##textureName)
#define SAMPLE_CURVE(textureName, coord) SAMPLE_TEXTURE2D(textureName, sampler##textureName, float2(coord, 0.5)).r
#define SAMPLE_CURVE_X(textureName, sampler, coord) SAMPLE_TEXTURE2D(textureName, sampler, float2(coord, 0.5)).r

#endif  // DUSTYROOM_CHROMA_INCLUDED
