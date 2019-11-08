Shader "Custom/Water"
{
	Properties{
		_EdgeLength("Edge length", Range(0,50)) = 3
		[PerRendererData] _WaterMap("Water Map", 2D) = "gray" {}
		_Color("Color", color) = (1,1,1,0)
	}
	SubShader{
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }

			ZWrite Off
			Cull Off
			Blend SrcAlpha OneMinusSrcAlpha
			LOD 200

			CGPROGRAM
			#pragma surface surf BlinnPhong addshadow fullforwardshadows vertex:disp tessellate:tessEdge nolightmap
			#pragma target 4.6
			#include "Tessellation.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
			};

			float _EdgeLength;

			float4 tessEdge(appdata v0, appdata v1, appdata v2)
			{
				return UnityEdgeLengthBasedTess(v0.vertex, v1.vertex, v2.vertex, _EdgeLength);
			}

			sampler2D _WaterMap;

			void disp(inout appdata v)
			{
				float d = tex2Dlod(_WaterMap, float4(v.texcoord.xy,0,0)).a;
				v.vertex.xyz += v.normal * d;
			}

			struct Input {
				float2 uv_MainTex;
			};

			fixed4 _Color;

			void surf(Input IN, inout SurfaceOutput o) {
				o.Albedo = _Color.rgb;
				o.Alpha = _Color.a;
			}
			ENDCG
	}
	FallBack "Diffuse"
}