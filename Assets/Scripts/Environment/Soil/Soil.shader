Shader "Custom/Soil"
{
    Properties
    {
		_DeadSoilColor("Dead Soil Color", color) = (1,1,1,0)
		_LiveSoilColor("Live Soil Color", color) = (1,1,1,0)
		_BedRockColor("Bedrock Color", color) = (1,1,1,0)
        _TopographyWidth ("Topography Line Width", Range(0,1)) = 0.5
        _TopographyDarkening ("Topography Line Darkening", Range(0,0.5)) = 0.2
        _SoilMap ("Soil Map", 2D) = "white" {}
        _SoilWaterMap ("Soil Water Map", 2D) = "white" {}
		_EdgeLength("Edge length", Range(2,50)) = 15
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

		CGPROGRAM
		#pragma surface surf Standard addshadow fullforwardshadows vertex:disp tessellate:tess nolightmap
		#pragma target 4.6
		#include "Tessellation.cginc"
		#include "UnityShaderVariables.cginc"

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
			float2 uv_SoilMap : TEXCOORD0;
			float4 screenPos : TEXCOORD1;
			float4 color : COLOR;
		};

        half _TopographyWidth;
		half _TopographyDarkening;
		float _EdgeLength;
		float Epsilon = 1e-10;

		float4 _DeadSoilColor;
		float4 _LiveSoilColor;
		float4 _BedRockColor;

		sampler2D _SoilMap;
		sampler2D _SoilWaterMap;
		sampler2D _CameraDepthTexture;

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
			float4 soil = tex2Dlod(_SoilMap, float4(v.texcoord, 0, 0));

			v.normal = FindNormal(float4(v.texcoord,0,0));
			v.vertex.y = soil.a;
			v.color = float4(0,0,0,0);
		}

        void surf(Input i, inout SurfaceOutputStandard o) { 
			float4 soil = tex2D(_SoilMap, i.uv_SoilMap);
			float4 soilWater = tex2D(_SoilWaterMap, i.uv_SoilMap);

			float soilDepth = max(soil.r, Epsilon);
			float landHeight = soil.a;
			float rootDepth = soil.g;
			float waterDepth = max(soilWater.b, Epsilon);

			float3 bedrockHSL = RGBtoHSL(_BedRockColor.xyz);
			float4 soilColor = lerp(_DeadSoilColor, _LiveSoilColor, saturate(rootDepth / soilDepth));
			float3 soilHSL = RGBtoHSL(soilColor.xyz);
			soilHSL.z = lerp(0.5,0.25, saturate(waterDepth / soilDepth));

			float3 normal = FindNormal(float4(i.uv_SoilMap,0,0));
			float3 angle = float3(0,1,0);
			float angleDist = length(angle - normal);
			if (landHeight % 5 < angleDist * _TopographyWidth){
				bedrockHSL.z -= _TopographyDarkening;
				soilHSL.z -= _TopographyDarkening;
			}

			float4 bedrockColor = float4(HSLtoRGB(bedrockHSL),1);
			soilColor = float4(HSLtoRGB(soilHSL),1);

            o.Albedo = lerp(bedrockColor, soilColor, saturate(soilDepth * 10));
			o.Normal = normal;
			o.Metallic = 0;
			o.Smoothness = 0;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
