Shader "Custom/Water" 
{
	Properties{
		_WaterMap ("Water Map", 2DArray) = "black" {}
        _Tess ("Water Geometry Detail", Range(10,50)) = 40
        _SeaLevel ("Sea Level", Int) = 1000

		_SpecColor("Specular Color", color) = (0.5,0.5,0.5,0.5)
		_ShallowWaterColor("Shallow Water Color", color) = (0.7,0.7,0.7,1)
		_DeepWaterColor("Deep Water Color", color) = (0.7,0.7,0.7,1)
		_Clarity("Clarity", Range(0.1,5)) = 0.5
	}
	SubShader{
		Tags { "RenderType" = "Transparent" }
		LOD 300

		CGPROGRAM
		#pragma surface surf Lambert addshadow fullforwardshadows vertex:disp tessellate:tess nolightmap alpha:blend
		#pragma target 4.6
		#include "Tessellation.cginc"
		#include "UnityShaderVariables.cginc"
		#include "Colors.hlsl"
		#include "CoordinateTransforms.hlsl"

		struct appdata {
			float4 vertex : POSITION;
			float4 tangent : TANGENT;
			float3 normal : NORMAL;
		};
		struct Input {
			float3 worldPos;
			float3 worldNormal; 
			float4 screenPos : TEXCOORD1;
		};

		UNITY_DECLARE_TEX2DARRAY(_WaterMap);
		sampler2D _CameraDepthTexture;
		float _Tess;
		float _SeaLevel;

		float4 _ShallowWaterColor;
		float4 _DeepWaterColor;
		float _Clarity;

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
			h[0] = UNITY_SAMPLE_TEX2DARRAY_LOD(_WaterMap, uvw0, 0)[channel];
			h[1] = UNITY_SAMPLE_TEX2DARRAY_LOD(_WaterMap, uvw1, 0)[channel];
			h[2] = UNITY_SAMPLE_TEX2DARRAY_LOD(_WaterMap, uvw2, 0)[channel];
			h[3] = UNITY_SAMPLE_TEX2DARRAY_LOD(_WaterMap, uvw3, 0)[channel];
			float3 n;
			n.z = h[0] - h[3];
			n.x = h[1] - h[2];
			n.y = 2;

			return normalize(n.x * tangent + n.y * normal + n.z * forward);
		}


		float4 tess(appdata v0, appdata v1, appdata v2)
		{
			return _Tess;
		}

		void disp(inout appdata v)
		{
			int channel = 3;
			float4 uvw = float4(xyz_to_uvw(v.vertex),0);
			float height = UNITY_SAMPLE_TEX2DARRAY_LOD(_WaterMap, uvw, 0)[channel] + _SeaLevel;
			v.vertex = normalize(float4(v.normal,0)) * height;
			v.normal = getDisplacedNormal(v.normal, v.tangent, channel);
		}
			 
		void surf(Input input, inout SurfaceOutput o) {
			float4 depthSample = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, input.screenPos);
			float visibleDepth = LinearEyeDepth(depthSample).r;
			float edgeDepth = 1 - saturate((visibleDepth - input.screenPos.w) / _Clarity);

			float3 uvw = xyz_to_uvw(input.worldPos);
			float height = UNITY_SAMPLE_TEX2DARRAY_LOD(_WaterMap, uvw, 0)[3];
			float depth = UNITY_SAMPLE_TEX2DARRAY_LOD(_WaterMap, uvw, 0)[2];
			float4 c = lerp(_ShallowWaterColor, _DeepWaterColor, clamp(depth / (_Clarity * 10), 0, 1));
			o.Albedo = c; 
			o.Specular = _SpecColor;
			o.Gloss = 0.1;
			o.Alpha = c.a;
		}
		ENDCG
	}
		FallBack "Diffuse"
}