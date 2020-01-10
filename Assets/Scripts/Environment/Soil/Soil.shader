Shader "Custom/Soil"
{
    Properties
    {
		_DeadSoilColor("Dead Soil Color", color) = (1,1,1,0)
		_LiveSoilColor("Live Soil Color", color) = (1,1,1,0)
		_BedRockColor("Bedrock Color", color) = (1,1,1,0)
        _SoilMap ("Soil Map", 2D) = "white" {}
        _SoilWaterMap ("Soil Water Map", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
		_EdgeLength("Edge length", Range(2,50)) = 15
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

		CGPROGRAM
		#pragma surface surf Standard addshadow fullforwardshadows vertex:disp tessellate:tess
		#pragma target 4.6
		#include "Tessellation.cginc"
		#include "UnityShaderVariables.cginc"

		struct appdata 
		{
			float4 vertex : POSITION;
			float4 tangent : TANGENT;
			float3 normal : NORMAL;
			float2 texcoord : TEXCOORD0;
		};
		struct Input 
		{
			float2 uv_SoilMap : TEXCOORD0;
			float4 screenPos : TEXCOORD1;
		};

        half _Glossiness;
		float _EdgeLength;
		float Epsilon = 1e-10;

		float4 _DeadSoilColor;
		float4 _LiveSoilColor;
		float4 _BedRockColor;

		sampler2D _SoilMap;
		sampler2D _SoilWaterMap;

		float3 HUEtoRGB(in float H)
		{
			float R = abs(H * 6 - 3) - 1;
			float G = 2 - abs(H * 6 - 2);
			float B = 2 - abs(H * 6 - 4);
			return saturate(float3(R, G, B));
		}
		float3 HSLtoRGB(in float3 HSL)
		{
			float3 RGB = HUEtoRGB(HSL.x);
			float C = (1 - abs(2 * HSL.z - 1)) * HSL.y;
			return (RGB - 0.5) * C + HSL.z;
		}
		float3 RGBtoHCV(in float3 RGB)
		{
			float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0/3.0) : float4(RGB.gb, 0.0, -1.0/3.0);
			float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
			float C = Q.x - min(Q.w, Q.y);
			float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
			return float3(H, C, Q.x);
		}
		float3 RGBtoHSL(in float3 RGB)
		{
			float3 HCV = RGBtoHCV(RGB);
			float L = HCV.z - HCV.y * 0.5;
			float S = HCV.y / (1 - abs(L * 2 - 1) + Epsilon);
			return float3(HCV.x, S, L);
		}

		float3 FindNormal(float4 uv)
		{
			float4 h;
			float u = 1.0 / 512.0;
			h[0] = tex2Dlod(_SoilMap, uv + float4(u * float2(0, -1), 0, 0)).a;
			h[1] = tex2Dlod(_SoilMap, uv + float4(u * float2(-1, 0), 0, 0)).a;
			h[2] = tex2Dlod(_SoilMap, uv + float4(u * float2(1, 0), 0, 0)).a;
			h[3] = tex2Dlod(_SoilMap, uv + float4(u * float2(0, 1), 0, 0)).a;
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
			float4 heightMap = tex2Dlod(_SoilMap, float4(v.texcoord, 0, 0));

			v.normal = FindNormal(float4(v.texcoord,0,0));
			v.vertex.y = heightMap.a;
		}

        void surf(Input i, inout SurfaceOutputStandard o) {
			float4 soil = tex2D(_SoilMap, i.uv_SoilMap);
			float4 soilWater = tex2D(_SoilWaterMap, i.uv_SoilMap);

			float soilDepth = max(soil.r, Epsilon);
			float soilHeight = soil.a;
			float rootDepth = soil.g;
			float waterDepth = soilWater.b;

			float3 bedrockHSL = RGBtoHSL(_BedRockColor.xyz);
			float3 liveSoilHSL = RGBtoHSL(_LiveSoilColor.xyz);
			float3 deadSoilHSL = RGBtoHSL(_DeadSoilColor.xyz);
			float3 soilHSL = lerp(deadSoilHSL, liveSoilHSL, rootDepth / soilDepth);
			soilHSL.z = lerp(0.5,0.25, waterDepth / (soilDepth));

			if(soilHeight % 5 < 0.1){
				soilHSL.z = 0.2;
				bedrockHSL.z = 0.2;
			}

			float4 soilColor = float4(HSLtoRGB(soilHSL),1);
			float4 bedrockColor = float4(HSLtoRGB(bedrockHSL),1);

            o.Albedo = lerp(bedrockColor, soilColor, clamp(soilDepth * 10,0,1));
			o.Normal = FindNormal(float4(i.uv_SoilMap,0,0));
        }
        ENDCG
    }
    FallBack "Diffuse"
}
