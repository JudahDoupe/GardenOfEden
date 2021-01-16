#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Colors.hlsl"
#include "CoordinateTransforms.hlsl"

TEXTURE2D_ARRAY(_HeightMap);
SAMPLER(sampler_HeightMap);
int _HeightChannel;
float _Tessellation;
float _SeaLevel;

float3 _FocusPosition;
float _FocusRadius;

struct VertexData
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float2 texcoord : TEXCOORD0;
    float2 lightmapUV : TEXCOORD1;
};

struct FragmentData
{
    float4 positionCS : SV_POSITION;
    float3 positionOS : TEXCOORD6;
    float3 positionWS : TEXCOORD2;
    float3 normalWS : TEXCOORD3;
    float3 viewDirectionWS : TEXCOORD4;
    float3 vertexLighting : TEXCOORD5;
    DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);
};

struct ControlPoint
{
    float4 positionOS : INTERNALTESSPOS;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
};

struct TessellationFactors
{
    float edge[3] : SV_TessFactor;
    float inside : SV_InsideTessFactor;
};

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

float4 addFocusRing(float4 color, float3 xyz)
{
    half lightening = 1.8;
    
    float d = distance(xyz, _FocusPosition);
    if (_FocusRadius - (_FocusRadius / 10) < d && d < _FocusRadius)
    {
        float3 hsl = rgb_to_hsl(color.xyz);
        hsl.z = saturate(hsl.z * lightening);
        color = float4(hsl_to_rgb(hsl), color.a);
    }
    return color;
}

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
    f.edge[0] = _Tessellation;
    f.edge[1] = _Tessellation;
    f.edge[2] = _Tessellation;
    f.inside = _Tessellation;
    return f;
}

float3 getDisplacedNormal(Texture2DArray tex, SamplerState sample, float3 normal, float3 tangent, int channel)
{
    float3 up = normal;
    float3 forward = cross(tangent, normal);
    float offset = 0.004;
     
    float4 uvw0 = float4(xyz_to_uvw(normalize(normal + (forward * -offset))), 0);
    float4 uvw1 = float4(xyz_to_uvw(normalize(normal + (tangent * -offset))), 0);
    float4 uvw2 = float4(xyz_to_uvw(normalize(normal + (tangent * offset))), 0);
    float4 uvw3 = float4(xyz_to_uvw(normalize(normal + (forward * offset))), 0);

    float4 h;
    float u = 1.0 / 512.0;
    h[0] = SAMPLE_TEXTURE2D_ARRAY_LOD(tex, sample, uvw0.xy, uvw0.z, 0)[channel];
    h[1] = SAMPLE_TEXTURE2D_ARRAY_LOD(tex, sample, uvw1.xy, uvw1.z, 0)[channel];
    h[2] = SAMPLE_TEXTURE2D_ARRAY_LOD(tex, sample, uvw2.xy, uvw2.z, 0)[channel];
    h[3] = SAMPLE_TEXTURE2D_ARRAY_LOD(tex, sample, uvw3.xy, uvw3.z, 0)[channel];
    float3 n;
    n.z = h[0] - h[3];
    n.x = h[1] - h[2];
    n.y = 2;

    return normalize(n.x * tangent + n.y * normal + n.z * forward);
}

FragmentData DisplaceVertexProgram(VertexData input)
{
    float4 uvw = float4(xyz_to_uvw(input.positionOS), 0);
    float height = SAMPLE_TEXTURE2D_ARRAY_LOD(_HeightMap, sampler_HeightMap, uvw.xy, uvw.z, 0)[_HeightChannel] + _SeaLevel;
    input.positionOS.xyz = input.normalOS * height;
    input.normalOS = getDisplacedNormal(_HeightMap, sampler_HeightMap, input.normalOS, input.tangentOS, _HeightChannel);

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS);
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

    FragmentData output;
    output.positionWS = vertexInput.positionWS;
    output.positionOS = input.positionOS;
    output.positionCS = vertexInput.positionCS;
    output.normalWS = NormalizeNormalPerVertex(normalInput.normalWS);
    output.viewDirectionWS = GetWorldSpaceViewDir(output.positionWS);
    output.vertexLighting = VertexLighting(vertexInput.positionWS, output.normalWS);

    OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
    OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

    return output;
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
 
    return DisplaceVertexProgram(data);
}

InputData InitializeInputData(FragmentData input)
{
    InputData inputData = (InputData) 0;
    inputData.positionWS = input.positionWS;
    inputData.normalWS = SafeNormalize(input.normalWS);
    inputData.viewDirectionWS = NormalizeNormalPerPixel(input.viewDirectionWS);
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    inputData.vertexLighting = input.vertexLighting;
    inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, inputData.normalWS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);

    return inputData;
}