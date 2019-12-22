Shader "Custom/Stepped"
{
    Properties {
		_MainTex ("Texture", 2D) = "white" {}
        _PrimaryColor ("Primary Color", Color) = (0, 0, 0, 1)
        _SecondaryColor ("Secondary Color", Color) = (0, 0, 0, 1)
    }
    SubShader {
        Tags{ "RenderType"="Opaque" "Queue"="Geometry"}

        CGPROGRAM

        #pragma surface Surface Stepped fullforwardshadows
        #pragma target 3.0

        fixed4 _PrimaryColor;
        fixed4 _SecondaryColor;
		sampler2D _MainTex;

		float2 RandomPoint(float2 p) {
			float3 a = frac(float3(p,1) * float3(123.34,234.34,345.65));
			a += dot(a, a + 98.75);
			return frac(float2(a.x * a.y, a.y * a.z));
		}
		float3 RandomColor(float2 p) {
			float3 a = frac(float3(p,1) * float3(123.34,234.34,345.65));
			a += dot(a, a + 98.75);
			return frac(float3(a.x * a.y, a.y * a.z, a.z * a.x));
		}

        struct Input {
            float2 uv_MainTex;
        };

        void Surface (Input input, inout SurfaceOutput o) {
			float2 uv = input.uv_MainTex;
			float gridSize = 10;
			float minDist = 1;
			float2 minId;

			float2 grid = uv * gridSize;
			float2 gv = frac(grid) - 0.5;
			float2 id = floor(grid);

			for(float x = -1; x <= 1; x += 1) {
				for(float y = -1; y <= 1; y += 1) {
					float2 offset = float2(x,y);
					float2 offsetId = (id + offset) % gridSize;
					float2 pointUv = offset + RandomPoint(offsetId) * 0.5;
					float dist = distance(gv, pointUv);

					if(dist < minDist){
						minDist = dist;
						minId = offsetId;
					}
				}
			}

			float l = 1/(1+pow(2.718,5-8*minDist));

            o.Albedo =_PrimaryColor - RandomColor(minId) / 7;
        }

        float4 LightingStepped(SurfaceOutput s, float3 lightDir, half3 viewDir, float shadowAttenuation){
            //how much does the normal point towards the light?
            float towardsLight = dot(s.Normal, lightDir);
            // make the lighting a hard cut
            float towardsLightChange = fwidth(towardsLight);
            float lightIntensity = smoothstep(0, towardsLightChange, towardsLight);

        #ifdef USING_DIRECTIONAL_LIGHT
            //for directional lights, get a hard vut in the middle of the shadow attenuation
            float attenuationChange = fwidth(shadowAttenuation) * 0.5;
            float shadow = smoothstep(0.5 - attenuationChange, 0.5 + attenuationChange, shadowAttenuation);
        #else
            //for other light types (point, spot), put the cutoff near black, so the falloff doesn't affect the range
            float attenuationChange = fwidth(shadowAttenuation);
            float shadow = smoothstep(0, attenuationChange, shadowAttenuation);
        #endif
            lightIntensity = clamp(0,0.6, lightIntensity * shadow);

            //calculate shadow color and mix light and shadow based on the light. Then taint it based on the light color
            float4 color;
            color.rgb = s.Albedo * _LightColor0.rgb * lightIntensity;
            color.a = s.Alpha;
            return color;
        }
        ENDCG
    }
    FallBack "Standard"
}