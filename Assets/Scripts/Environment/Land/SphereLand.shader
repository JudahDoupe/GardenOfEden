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
	    _SoilColor("Soil Color", color) = (1,1,1,0)

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
		#pragma surface surf Standard alphatest:_Cutoff addshadow fullforwardshadows vertex:disp nolightmap
		#pragma target 4.6 //TODO: try 3.0
		#include "Tessellation.cginc"
		#include "UnityShaderVariables.cginc"

        sampler2D _LandMapXPos;
        sampler2D _LandMapXNeg;
        sampler2D _LandMapYPos;
        sampler2D _LandMapYNeg;
        sampler2D _LandMapZPos;
        sampler2D _LandMapZNeg;

		float4 _SoilColor;
		float4 _BedRockColor;

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
		};

		struct Input 
		{
			float2 uv_LandMapXPos : TEXCOORD0;
            int textureId;
		};

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

		void disp(inout appdata v, out Input o)
		{	
            UNITY_INITIALIZE_OUTPUT(Input,o);
			int channel = 3;
            float3 textureNormal;
			float4 soil;

            float3 absV = abs(v.vertex);
            int greatestIndex = 0;
            for (int i = 1; i < 3; i++){
                if (absV[i] > absV[greatestIndex]) {
                    greatestIndex = i;
                }
            }
            float3 cv = v.vertex / absV[greatestIndex];

            if (greatestIndex == 0) {
                if (cv.x > 0) {
                    textureNormal = FindNormal(float4(v.texcoord,0,0), _LandMapXPos, channel);
                    soil = tex2Dlod(_LandMapXPos, float4(v.texcoord, 0, 0));
                    v.texcoord = float2(cv.z, cv.y);
                    o.textureId = 0;
                }
                else {
                    textureNormal = FindNormal(float4(v.texcoord,0,0), _LandMapXNeg, channel);
                    soil = tex2Dlod(_LandMapXNeg, float4(v.texcoord, 0, 0));
                    v.texcoord = float2(-cv.z, cv.y);
                    o.textureId = 1;
                }
            }
            else if (greatestIndex == 1) {
                if (cv.y > 0) {
                    textureNormal = FindNormal(float4(v.texcoord,0,0), _LandMapYPos, channel);
                    soil = tex2Dlod(_LandMapYPos, float4(v.texcoord, 0, 0));
                    v.texcoord = float2(-cv.x, cv.z);
                    o.textureId = 2;
                }
                else {
                    textureNormal = FindNormal(float4(v.texcoord,0,0), _LandMapYNeg, channel);
                    soil = tex2Dlod(_LandMapYNeg, float4(v.texcoord, 0, 0));
                    v.texcoord = float2(-cv.x, -cv.z);
                    o.textureId = 3;
                }
            }
            else if (greatestIndex == 2) {
                if (cv.z > 0) {
                    textureNormal = FindNormal(float4(v.texcoord,0,0), _LandMapZPos, channel);
                    soil = tex2Dlod(_LandMapZPos, float4(v.texcoord, 0, 0));
                    v.texcoord = float2(-cv.x, cv.y);
                    o.textureId = 4;
                }
                else {
                    textureNormal = FindNormal(float4(v.texcoord,0,0), _LandMapZNeg, channel);
                    soil = tex2Dlod(_LandMapZNeg, float4(v.texcoord, 0, 0));
                    v.texcoord = float2(cv.x, cv.y);
                    o.textureId = 5;
                }
            }
            
			v.vertex += float4(v.normal * soil[channel], 0);
			v.normal = normalize(textureNormal.x * v.tangent + textureNormal.y * v.normal + textureNormal.z * cross(v.tangent, v.normal));
		}

        void surf (Input i, inout SurfaceOutputStandard o)
        {
            float4 soil;

            if (i.textureId == 0) {
                soil = tex2D(_LandMapXPos, i.uv_LandMapXPos);
            } else if (i.textureId == 1){
                soil = tex2D(_LandMapXNeg, i.uv_LandMapXPos);
            } else if (i.textureId == 2){
                soil = tex2D(_LandMapYPos, i.uv_LandMapXPos);
            } else if (i.textureId == 3){
                soil = tex2D(_LandMapYNeg, i.uv_LandMapXPos);
            } else if (i.textureId == 4){
                soil = tex2D(_LandMapZPos, i.uv_LandMapXPos);
            } else{
                soil = tex2D(_LandMapZNeg, i.uv_LandMapXPos);
            }
            
			float soilDepth = max(soil.r, Epsilon);
			float landHeight = soil.a;

			float3 bedrockHSL = RGBtoHSL(_BedRockColor.xyz);
			float3 soilHSL = RGBtoHSL(_SoilColor.xyz);

			float3 angle = float3(0,1,0);
			float angleDist = length(angle - o.Normal);
			if (landHeight % _TopographyFrequency < angleDist * _TopographyWidth){
				bedrockHSL.z -= _TopographyDarkening;
				soilHSL.z -= _TopographyDarkening;
			}

			float4 bedrockColor = float4(HSLtoRGB(bedrockHSL),1);
			float4 soilColor = float4(HSLtoRGB(soilHSL),1);

            o.Albedo = lerp(bedrockColor, soilColor, saturate(soilDepth * 10));
        }
        ENDCG
    }
    FallBack "Diffuse"
}
