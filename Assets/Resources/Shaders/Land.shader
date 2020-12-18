Shader "Custom/SphereLand"
{
    Properties
    {        
        _LandMap ("Land Map", 2DArray) = "black" {}
        _Tess ("Terrain Detail", Range(10,30)) = 15
		_BedRockColor("Bedrock Color", color) = (0.7,0.7,0.7,1)
	    _SoilColor("Soil Color", color) = (0.55,0.27,0.12,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
		#pragma surface surf StandardSpecular addshadow fullforwardshadows vertex:disp tessellate:tess nolightmap
		#pragma target 4.6
		#include "Tessellation.cginc"
		#include "UnityShaderVariables.cginc"
		#include "LandHelpers.hlsl"

        /* --- DATA --- */

        UNITY_DECLARE_TEX2DARRAY(_LandMap);
		float4 _SoilColor;
		float4 _BedRockColor;
        float _Tess;

		struct appdata 
		{
			float4 vertex : POSITION;
			float4 tangent : TANGENT;
			float3 normal : NORMAL;
		};

		struct Input 
		{
			float3 worldPos;
			float3 worldNormal;
		};

        /* --- METHODS --- */

		float3 getNormal(float4 uvw, int channel)
        {
            // xyz to pta
            // pta += epsilon
            // pta to xyz
            // xyz to uvw
     
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

		float4 tess (appdata v0, appdata v1, appdata v2) {
            return _Tess;
        }

		void disp(inout appdata v)
		{	
			int channel = 0;
            float4 uvw = float4(xyz_to_uvw(v.vertex),0);
			float height = UNITY_SAMPLE_TEX2DARRAY_LOD(_LandMap, uvw, 0)[channel] + 1000;
            float3 textureNormal = getNormal(uvw, channel);
			v.vertex = normalize(float4(v.normal,0)) * height;
			v.normal = normalize(textureNormal.x * v.tangent + textureNormal.y * v.normal + textureNormal.z * cross(v.tangent, v.normal));
		}

        void surf (Input i, inout SurfaceOutputStandardSpecular o)
        {
			float4 color = _BedRockColor;
			color = addTopographyLines(color, i.worldPos, i.worldNormal);

            o.Albedo = color;
			o.Specular = 0.0;
			o.Smoothness = 0.0;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
