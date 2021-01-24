#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "CoordinateTransforms.hlsl"

TEXTURE2D(_OcclusionMap);       SAMPLER(sampler_OcclusionMap);

TEXTURE2D_ARRAY(_HeightMap);    SAMPLER(linear_clamp_sampler_HeightMap);
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

/***** Tessellation Program *****/

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

float4 sampleHeightMap(float3 uvw)
{
    return SAMPLE_TEXTURE2D_ARRAY_LOD(_HeightMap, linear_clamp_sampler_HeightMap, uvw.xy, uvw.z, 0);
}
float sampleHeight(float3 uvw)
{
    return sampleHeightMap(uvw)[_HeightChannel] + _SeaLevel;
}

float3 getDisplacedNormal(float3 normal, float3 tangent)
{
    float3 forward = cross(tangent, normal);
    float offset = 0.004;
     
    float3 uvw0 = xyz_to_uvw(normalize(normal + (forward * -offset)));
    float3 uvw1 = xyz_to_uvw(normalize(normal + (tangent * -offset)));
    float3 uvw2 = xyz_to_uvw(normalize(normal + (tangent * offset)));
    float3 uvw3 = xyz_to_uvw(normalize(normal + (forward * offset)));

    float3 n;
    n.z = sampleHeight(uvw0) - sampleHeight(uvw3);
    n.x = sampleHeight(uvw1) - sampleHeight(uvw2);
    n.y = 2;

    return normalize(n.x * tangent + n.y * normal + n.z * forward);
}

/***** Full Vertex Programs *****/

FragmentData LitPassVertexProgram(VertexData input)
{
    float height = sampleHeight(xyz_to_uvw(input.positionOS.xyz));
    input.positionOS.xyz = input.normalOS * height;
    input.normalOS = getDisplacedNormal(input.normalOS, input.tangentOS);

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
FragmentData LitPassDomainProgram(TessellationFactors factors, OutputPatch<ControlPoint, 3> patch, float3 barycentricCoordinates : SV_DomainLocation)
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
 
    return LitPassVertexProgram(data);
}


/***** Clip Space Vertex *****/


// Shadow Casting Light geometric parameters. These variables are used when applying the shadow Normal Bias and are set by UnityEngine.Rendering.Universal.ShadowUtils.SetupShadowCasterConstantBuffer in com.unity.render-pipelines.universal/Runtime/ShadowUtils.cs
// For Directional lights, _LightDirection is used when applying shadow Normal Bias.
// For Spot lights and Point lights, _LightPosition is used to compute the actual light direction because it is different at each shadow caster geometry vertex.
float3 _LightDirection;
float3 _LightPosition;

FragmentData ClipSpaceVertexProgram(VertexData input)
{
    float height = sampleHeight(xyz_to_uvw(input.positionOS.xyz));
    input.positionOS.xyz = input.normalOS * height;
    input.normalOS = getDisplacedNormal(input.normalOS, input.tangentOS);

    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

    FragmentData output;
    
#if _CASTING_PUNCTUAL_LIGHT_SHADOW
    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
#else
    float3 lightDirectionWS = _LightDirection;
#endif

    output.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

#if UNITY_REVERSED_Z
    output.positionCS.z = min(output.positionCS.z, UNITY_NEAR_CLIP_VALUE);
#else
    output.positionCS.z = max(output.positionCS.z, UNITY_NEAR_CLIP_VALUE);
#endif
    
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
    output.normalWS = NormalizeNormalPerVertex(normalInput.normalWS);
    
    return output;
}

[domain("tri")]
FragmentData ClipSpaceDomainProgram(TessellationFactors factors, OutputPatch<ControlPoint, 3> patch, float3 barycentricCoordinates : SV_DomainLocation)
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
 
    return ClipSpaceVertexProgram(data);
}

/***** Fragemnt Helpers *****/

half4 ClipSpaceFragmentProgram(FragmentData input) : SV_TARGET
{
    return 0;
}

float4 DepthNormalsFragmentProgram(FragmentData input) : SV_TARGET
{
    return float4(PackNormalOctRectEncode(TransformWorldToViewDir(input.normalWS, true)), 0.0, 0.0);
}

InputData InitializeInputData(FragmentData input)
{
    InputData inputData = (InputData) 0;
    inputData.positionWS = input.positionWS;
    inputData.normalWS = NormalizeNormalPerPixel(input.normalWS);
    inputData.viewDirectionWS = SafeNormalize(input.viewDirectionWS);
    inputData.vertexLighting = input.vertexLighting;
    inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, inputData.normalWS);
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);
    inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);

    return inputData;
}

SurfaceData InitializeSurfaceData(float4 color, InputData input)
{
    SurfaceData outSurfaceData = (SurfaceData) 0;

    color = saturate(color);
    
    outSurfaceData.alpha = color.a;
    outSurfaceData.albedo = color.rgb;
    outSurfaceData.metallic = 0.0;
    outSurfaceData.specular = float3(1,1,1);
    outSurfaceData.smoothness = 0;
    outSurfaceData.normalTS = float3(0,0,1);
    outSurfaceData.occlusion = SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, input.normalizedScreenSpaceUV).g;
    outSurfaceData.emission = 0;
    
    return outSurfaceData;
}

float4 addFocusRing(float4 color, float3 xyz)
{
    half lightening = 1.8;
    
    float d = distance(xyz, _FocusPosition);
    if (_FocusRadius - (_FocusRadius / 10) < d && d < _FocusRadius)
    {
        float3 hsv = RgbToHsv(color.xyz);
        hsv.z *= lightening;
        color = float4(HsvToRgb(hsv), color.a);
    }
    return color;
}