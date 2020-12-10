Shader "Custom/SphereLand"
{
    Properties
    {        
        _LandMapXPos ("Land Map X+", 2D) = "white" {}
        _LandMapXNeg ("Land Map X-", 2D) = "white" {}
        _LandMapYPos ("Land Map Y+", 2D) = "white" {}
        _LandMapYNeg ("Land Map Y-", 2D) = "white" {}
        _LandMapZPos ("Land Map Z+", 2D) = "white" {}
        _LandMapZNeg ("Land Map Z-", 2D) = "white" {}

		_BedRockColor("Bedrock Color", color) = (1,1,1,0)
	    _SoilColor("Live Soil Color", color) = (1,1,1,0)

		_EdgeLength("Tesselation Edge length", Range(2,50)) = 15

        _TopographyFrequency ("Topography Line Frequency", Range(5,50)) = 10
        _TopographyWidth ("Topography Line Width", Range(0,1)) = 0.5
        _TopographyDarkening ("Topography Line Darkening", Range(0,0.5)) = 0.2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
		#pragma surface surf Standard alphatest:_Cutoff addshadow fullforwardshadows vertex:disp tessellate:tess nolightmap
		#pragma target 4.6 //TODO: try 3.0
		#include "Tessellation.cginc"
		#include "UnityShaderVariables.cginc"

        sampler2D _LandMapXPos;
        sampler2D _LandMapXNeg;
        sampler2D _LandMapYPos;
        sampler2D _LandMapYNeg;
        sampler2D _LandMapZPos;
        sampler2D _LandMapZNeg;

        half _TopographyWidth;
        half _TopographyFrequency;
		half _TopographyDarkening;
		float _EdgeLength;
		float Epsilon = 1e-10;

		struct appdata 
		{
			float4 vertex : POSITION;
			float4 tangent : TANGENT;
			float3 normal : NORMAL;
			float2 texcoord : TEXCOORD0;
			float4 color : COLOR;
		};

		struct Input 
		{
			float2 uv_LandMap : TEXCOORD0;
			float4 screenPos : TEXCOORD1;
			float4 color : COLOR;
		};

        float3 FindNormal(float4 uv, sampler2D map, int channel)
		{
			float4 h;
			float u = 1.0 / 512.0;
			h[0] = tex2Dlod(map, uv + float4(u * float2(0, -1), 0, 0))[channel];
			h[1] = tex2Dlod(map, uv + float4(u * float2(-1, 0), 0, 0))[channel];
			h[2] = tex2Dlod(map, uv + float4(u * float2(1, 0), 0, 0))[channel];
			h[3] = tex2Dlod(map, uv + float4(u * float2(0, 1), 0, 0))[channel];
			float3 n;
			n.z = h[0] - h[3];
			n.x = h[1] - h[2];
			n.y = 2;
			return normalize(n);
		}
        
        float4 tess(appdata v0, appdata v1, appdata v2)
		{
			return UnityEdgeLengthBasedTess(v0.vertex, v1.vertex, v2.vertex, _EdgeLength);
		} 

		void disp(inout appdata v)
		{	
			sampler2D map = _LandMapXPos;
			float4 soil = tex2Dlod(map, float4(v.texcoord, 0, 0));
			int channel = 3;

			float3 textureNormal = FindNormal(float4(v.texcoord,0,0), map, channel);

			v.vertex += float4(v.normal * soil[channel], 0);
			v.normal = normalize(textureNormal.x * v.tangent + textureNormal.y * v.normal + textureNormal.z * cross(v.tangent, v.normal));
			v.color = float4(0,0,0,0);
		}

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = 0.5;
            o.Metallic = 0;
            o.Smoothness = 0;
            o.Alpha = 0;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
