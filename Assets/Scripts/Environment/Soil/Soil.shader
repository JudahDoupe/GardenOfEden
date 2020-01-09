Shader "Custom/Soil"
{
    Properties
    {
		_DeadSoilHue("Dead Soil Hue", Range(0.0, 1.0)) = 0.115
		_LiveSoilHue("Live Soil Hue", Range(0.0, 1.0)) = 0.325
        _SoilMap ("Soil Map", 2D) = "white" {}
        _SoilWaterMap ("Soil Water Map", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		_CameraPosition("CameraPosition", Vector) = (.0, .0, .0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.0

		sampler2D _SoilMap;
		sampler2D _SoilWaterMap;

		struct appdata 
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float2 worldPos : TEXCOORD0;
		};
        struct Input
        {
			float3 worldPos : TEXCOORD0;
        };

        half _Glossiness;
        half _Metallic;
		float3 _CameraPosition;

		float _DeadSoilHue;
		float _LiveSoilHue;


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

		Input vert(inout appdata v)
		{
			Input o;
			o.worldPos = mul(unity_ObjectToWorld, v.vertex);
			return o;
		}

        void surf (Input i, inout SurfaceOutputStandard o)
        {
			float2 relativePosition = i.worldPos.xz - _CameraPosition.xz;
			float2 uv = relativePosition / 200;
			float2 normUv = (uv + 1) / 2;
			uint2 xy = floor(normUv * 512) % 511;

			float4 soil = tex2D(_SoilMap, normUv);
			float4 soilWater = tex2D(_SoilWaterMap, normUv);
			float soilDepth = soil.r + 0.0000000001;
			float soilHeight = soil.a;
			float rootDepth = soil.g;
			float waterDepth = soilWater.b;

			float H = lerp(_DeadSoilHue, _LiveSoilHue, rootDepth / soilDepth);
			float S = lerp(0.25, 0.75, soilDepth / 15);
			float L = lerp(0.5,0.25, waterDepth / (soilDepth));

			if(soilHeight % 5 < 0.1){
				L = 0.2;
			}

            o.Albedo = HSLtoRGB(float3(H, S, L));
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 255;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
