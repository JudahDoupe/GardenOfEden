Shader "Custom/SphereLand"
{
    Properties
    {        
        _LandMap ("Land Map", 2DArray) = "white" {}

		_BedRockColor("Bedrock Color", color) = (0.7,0.7,0.7,1)
	    _SoilColor("Soil Color", color) = (0.55,0.27,0.12,1)

        _TopographyFrequency ("Topography Line Frequency", Range(5,50)) = 10
        _TopographyWidth ("Topography Line Width", Range(0,1)) = 0.5
        _TopographyDarkening ("Topography Line Darkening", Range(0,0.5)) = 0.2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
		#pragma surface surf Standard alphatest:_Cutoff addshadow fullforwardshadows vertex:disp nolightmap
		#pragma target 4.6 //TODO: try 3.0
		#include "Tessellation.cginc"
		#include "UnityShaderVariables.cginc"
		#include "Colors.hlsl"
		#include "CoordinateTransforms.hlsl"

		float4 _SoilColor;
		float4 _BedRockColor;

        half _TopographyWidth;
        half _TopographyFrequency;
		half _TopographyDarkening;

		struct appdata 
		{
			float4 vertex : POSITION;
			float4 tangent : TANGENT;
			float3 normal : NORMAL;
		};

		struct Input 
		{
			float4 coord : TEXCOORD0;
		};

        UNITY_DECLARE_TEX2DARRAY(_LandMap);

        float3 FindNormal(float4 uvw, int channel)
		{
			float4 h;
			float u = 1.0 / 512.0;
			h[0] = UNITY_SAMPLE_TEX2DARRAY_LOD(_LandMap, uvw + float4(u * float2(0, -1), 0, 0), 0)[channel];
			h[1] = UNITY_SAMPLE_TEX2DARRAY_LOD(_LandMap, uvw + float4(u * float2(-1, 0), 0, 0), 0)[channel];
			h[2] = UNITY_SAMPLE_TEX2DARRAY_LOD(_LandMap, uvw + float4(u * float2(1, 0), 0, 0), 0)[channel];
			h[3] = UNITY_SAMPLE_TEX2DARRAY_LOD(_LandMap, uvw + float4(u * float2(0, 1), 0, 0), 0)[channel];
			float3 n;
			n.z = h[0] - h[3];
			n.x = h[1] - h[2];
			n.y = 2;
			return normalize(n);
		}

		void disp(inout appdata v, out Input o)
		{	
            UNITY_INITIALIZE_OUTPUT(Input,o);
			int channel = 3;
            float4 uvw = float4(xyz_to_uvw(v.vertex),0);
			float height = UNITY_SAMPLE_TEX2DARRAY_LOD(_LandMap, o.coord, 0)[channel];
            float3 textureNormal = FindNormal(o.coord, channel);
			o.coord = uvw;
			v.vertex += float4(v.normal,0) * height;
			v.normal = normalize(textureNormal.x * v.tangent + textureNormal.y * v.normal + textureNormal.z * cross(v.tangent, v.normal));
		}

        void surf (Input i, inout SurfaceOutputStandard o)
        {
            float4 soil = UNITY_SAMPLE_TEX2DARRAY(_LandMap, i.coord); 
            
			float soilDepth = max(soil.r, Epsilon);
			float landHeight = soil.a;

			float3 bedrockHSL = rgb_to_hsl(_BedRockColor.xyz);
			float3 soilHSL = rgb_to_hsl(_SoilColor.xyz);

			float3 angle = float3(0,1,0);
			float angleDist = length(angle - o.Normal);
			if (landHeight % _TopographyFrequency < angleDist * _TopographyWidth){
				bedrockHSL.z -= _TopographyDarkening;
				soilHSL.z -= _TopographyDarkening;
			}

			float4 bedrockColor = float4(hsl_to_rgb(bedrockHSL),1);
			float4 soilColor = float4(hsl_to_rgb(soilHSL),1);

            o.Albedo = lerp(bedrockColor, soilColor, saturate(soilDepth * 10));
            float2 id = floor(i.coord.xy * 16);
            o.Albedo = float4(id / 16 ,0.5,0);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
