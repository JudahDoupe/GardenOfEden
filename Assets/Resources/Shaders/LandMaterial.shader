Shader "Custom/SphereLand"
{
    Properties
    {        
        _LandMap ("Land Map", 2DArray) = "black" {}
        _Tess ("Terrain Detail", Range(10,50)) = 40
        _SeaLevel ("Sea Level", Int) = 1000

		_BedRockColor("Bedrock Color", color) = (0.7,0.7,0.7,1)
	    _SoilColor("Soil Color", color) = (0.55,0.27,0.12,1)
        
	    _FocusPosition("Focus Position",  Vector) = (0,0,0,0)
	    _FocusRadius("Focus Radius",  Range(0,1000)) = 0
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
		#include "TerrainHelpers.hlsl"

        /* --- DATA --- */

        UNITY_DECLARE_TEX2DARRAY(_LandMap);
        float _Tess;
        int _SeaLevel;
        sampler2D _Tex;

		float4 _SoilColor;
		float4 _BedRockColor;

        float3 _FocusPosition;
        float _FocusRadius;

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

		float3 getDisplacedNormal(float3 normal, float3 tangent, int channel)
        {
            float3 up = normal;
            float3 forward = cross(tangent, normal);
            float offset = 0.004;
     
            float4 uvw0 = float4(xyz_to_uvw(normalize(normal + (forward * -offset))),0);
            float4 uvw1 = float4(xyz_to_uvw(normalize(normal + (tangent * -offset))),0);
            float4 uvw2 = float4(xyz_to_uvw(normalize(normal + (tangent * offset))),0);
            float4 uvw3 = float4(xyz_to_uvw(normalize(normal + (forward * offset))),0);

            float4 h;
            float u = 1.0 / 512.0;
            h[0] = UNITY_SAMPLE_TEX2DARRAY_LOD(_LandMap, uvw0, 0)[channel];
            h[1] = UNITY_SAMPLE_TEX2DARRAY_LOD(_LandMap, uvw1, 0)[channel];
            h[2] = UNITY_SAMPLE_TEX2DARRAY_LOD(_LandMap, uvw2, 0)[channel];
            h[3] = UNITY_SAMPLE_TEX2DARRAY_LOD(_LandMap, uvw3, 0)[channel];
            float3 n;
            n.z = h[0] - h[3];
            n.x = h[1] - h[2];
            n.y = 2;

            return normalize(n.x * tangent + n.y * normal + n.z * forward);
        }

		float4 tess (appdata v0, appdata v1, appdata v2) {
            return _Tess;
        }

		void disp(inout appdata v)
		{	
			int channel = 0;
            float4 uvw = float4(xyz_to_uvw(v.vertex),0);
			float height = UNITY_SAMPLE_TEX2DARRAY_LOD(_LandMap, uvw, 0)[channel] + _SeaLevel;
			v.vertex.xyz = normalize(v.normal) * height;
			v.normal = getDisplacedNormal(normalize(v.normal), normalize(v.tangent), channel);
		}

        void surf (Input i, inout SurfaceOutputStandardSpecular o)
        {
			float4 color = _BedRockColor;
			color = addTopographyLines(color, i.worldPos, i.worldNormal);
			color = addFocusRing(color, i.worldPos, _FocusPosition, _FocusRadius);

            o.Albedo = color;
			o.Specular = 0.0;
			o.Smoothness = 0.0;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
