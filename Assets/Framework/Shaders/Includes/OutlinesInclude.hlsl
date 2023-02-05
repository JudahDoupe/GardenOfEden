
static float2 sobelSamplePoints[9] = {
    float2(-1, 1), float2(0, 1), float2(1, 1),
    float2(-1, 0), float2(0, 0), float2(1, 0),
    float2(-1, -1), float2(0, -1), float2(1, -1),
};

// Weights for the x component
static float sobelXMatrix[9] = {
    1, 0, -1,
    2, 0, -2,
    1, 0, -1
};

// Weights for the y component
static float sobelYMatrix[9] = {
    1, 2, 1,
    0, 0, 0,
    -1, -2, -1
};

#ifndef SOBELOUTLINES_INCLUDED
#define SOBELOUTLINES_INCLUDED
void OutlineSobel_float(float2 UV, float Thickness, UnityTexture2D OutlineTexture, UnitySamplerState Sampler, out float Out) {
    float2 sobel = 0;
    [unroll] for (int i = 0; i < 9; i++) {
        float2 uv = UV + sobelSamplePoints[i] * Thickness;
        float4 color = SAMPLE_TEXTURE2D(OutlineTexture, Sampler, uv);
        sobel += color.r * float2(sobelXMatrix[i], sobelYMatrix[i]);
    } 
    Out = length(sobel); 
}
#endif

#ifndef SOBELOUTLINESCUBE_INCLUDED
#define SOBELOUTLINESCUBE_INCLUDED
void OutlineSobelCube_float(float2 UV, float Index, float Thickness, UnityTexture2DArray OutlineTexture, UnitySamplerState Sampler, out float Out) {
    float2 sobel = 0;
    [unroll] for (int i = 0; i < 9; i++) {
        float2 uv = UV + sobelSamplePoints[i] * Thickness; 
        float4 color = SAMPLE_TEXTURE2D_ARRAY(OutlineTexture, Sampler, uv, round(Index));
        sobel += color.r * float2(sobelXMatrix[i], sobelYMatrix[i]); 
    }
    Out = length(sobel);
}
#endif