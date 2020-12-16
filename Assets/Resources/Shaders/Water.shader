Shader "Custom/Water" 
{
	Properties{
		_EdgeLength("Edge length", Range(2,50)) = 15
		_MainTex("Texture (RGB)", 2D) = "white" {}
		_WaterMap("Water Map", 2D) = "gray" {}
		_ShallowColor("Shallow Color", color) = (1,1,1,0)
		_DeepColor("Deep Color", color) = (1,1,1,0)
		_SpecColor("Spec color", color) = (0.5,0.5,0.5,0.5)
		_Clarity("Clarity", Range(0.1,5)) = 0.5
	}
		SubShader{
			Tags { "RenderType" = "Transparent" }
			LOD 300

			CGPROGRAM
			#pragma surface surf Lambert addshadow fullforwardshadows vertex:disp tessellate:tess nolightmap alpha
			#pragma target 4.6
			#include "Tessellation.cginc"
			#include "UnityShaderVariables.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
			};
			struct Input {
				float2 uv_WaterMap : TEXCOORD0;
				float4 screenPos : TEXCOORD1;
			};

			float _EdgeLength;
			sampler2D _WaterMap;
			sampler2D _MainTex;
			sampler2D _CameraDepthTexture;
			fixed4 _ShallowColor;
			fixed4 _DeepColor;
			float _Clarity;

			float3 FindNormal(float4 uv)
			{
				float4 h;
				float u = 1.0 / 512.0;
				h[0] = tex2Dlod(_WaterMap, uv + float4(u * float2(0, -1), 0, 0)).a;
				h[1] = tex2Dlod(_WaterMap, uv + float4(u * float2(-1, 0), 0, 0)).a;
				h[2] = tex2Dlod(_WaterMap, uv + float4(u * float2(1, 0), 0, 0)).a;
				h[3] = tex2Dlod(_WaterMap, uv + float4(u * float2(0, 1), 0, 0)).a;
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
				float4 heightMap = tex2Dlod(_WaterMap, float4(v.texcoord.xy, 0, 0));

				v.normal = FindNormal(float4(v.texcoord.xy,0,0));
				v.vertex.y = heightMap.a;
			}

			void surf(Input input, inout SurfaceOutput o) {
				float4 depthSample = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, input.screenPos);
				float depth = LinearEyeDepth(depthSample).r;
				float edgeDepth = 1 - saturate((depth - input.screenPos.w) / _Clarity);

				float4 w = tex2D(_WaterMap, input.uv_WaterMap);
				half4 c = lerp(_DeepColor, _ShallowColor, edgeDepth);
				o.Albedo = c.rgb;
				o.Specular = 0.9;
				o.Gloss = 0.1;
				o.Alpha = c.a * (w.b > 0);
				o.Normal = FindNormal(float4(input.uv_WaterMap,0,0)).xzy;
			}
			ENDCG
		}
			FallBack "Diffuse"
}