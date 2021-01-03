Shader "Custom/Water" 
{
	Properties{
		_WaterMap ("Water Map", 2DArray) = "black" {}
        _Tess ("Water Geometry Detail", Range(10,64)) = 40
        _SeaLevel ("Sea Level", Int) = 1000

		_ShallowWaterColor("Shallow Water Color", color) = (0.7,0.7,0.7,1)
		_DeepWaterColor("Deep Water Color", color) = (0.7,0.7,0.7,1)
		_DeepWaterDepth("Deep Water Depth", Float) = 150
		_Clarity("Clarity", Float) = 75
		_Smoothness("Smoothness", Range(0,1)) = 0.9
		_Diffuse("Diffuse", Range(0,1)) = 0.3

		_SunDirection("Sun Direction", Vector) = (0,1,0,0)
		
	    _FocusPosition("Focus Position",  Vector) = (0,0,0,0)
	    _FocusRadius("Focus Radius",  Range(0,1000)) = 0
	}
	SubShader{
		Tags { "Queue" = "Transparent" }
		LOD 300
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off

		Pass {
			CGPROGRAM
			#pragma vertex TessellationVertexProgram 
			#pragma fragment FragmentProgram 
			#pragma hull HullProgram
			#pragma domain DomainProgram
			#pragma target 4.6
            #include "UnityCG.cginc"
			#include "Tessellation.cginc"
			#include "UnityShaderVariables.cginc"
			#include "TerrainHelpers.hlsl"

			struct appdata {
				float4 vertex : POSITION;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
			};
			struct v2f {
				float4 position : SV_POSITION;
				float4 screenPos : TEXCOORD1;
				float3 normal : TEXCOORD2;
				float4 globalPosition : TEXCOORD3;
				float4 waterMap : TEXCOORD4;
			};
			struct ControlPoint
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
			};
			struct TessellationFactors
			{
			   float edge[3] : SV_TessFactor;
			   float inside : SV_InsideTessFactor;
			};

			UNITY_DECLARE_TEX2DARRAY(_WaterMap);
			sampler2D _CameraDepthTexture;
			float _Tess;
			float _SeaLevel;

			float4 _ShallowWaterColor;
			float4 _DeepWaterColor;
			float _DeepWaterDepth;
			float _Clarity;
			float _Smoothness;
			float _Diffuse;

			float3 _SunDirection;

			float3 _FocusPosition;
			float _FocusRadius;

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

			ControlPoint TessellationVertexProgram(appdata v)
			{
			    ControlPoint p;	
				p.vertex = v.vertex;
				p.normal = v.normal;
				p.tangent = v.tangent;
			    return p;
			}

			[UNITY_domain("tri")]
			[UNITY_outputcontrolpoints(3)]
			[UNITY_outputtopology("triangle_cw")]
			[UNITY_partitioning("integer")]
			[UNITY_patchconstantfunc("PatchConstantFunction")]
			ControlPoint HullProgram(InputPatch<ControlPoint, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}
			TessellationFactors PatchConstantFunction(InputPatch<ControlPoint, 3> patch)
			{
			   TessellationFactors f;
			   f.edge[0] = _Tess;
			   f.edge[1] = _Tess;
			   f.edge[2] = _Tess;
			   f.inside = _Tess;
			   return f;
			}

			v2f VertexProgram (appdata v)
			{
				v2f o;
				int channel = 3;
				float4 uvw = float4(xyz_to_uvw(v.vertex),0);
				o.waterMap = UNITY_SAMPLE_TEX2DARRAY_LOD(_WaterMap, uvw, 0);
				float height = o.waterMap[channel] + _SeaLevel;
				v.vertex.xyz = v.normal * height;
				v.normal = getDisplacedNormal(v.normal, v.tangent, channel);

                o.globalPosition = v.vertex;
                o.position = UnityObjectToClipPos(v.vertex);
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.screenPos = ComputeScreenPos(o.position);
				return o;
			}

			[UNITY_domain("tri")]
			v2f DomainProgram(TessellationFactors factors, OutputPatch<ControlPoint, 3> patch, float3 barycentricCoordinates : SV_DomainLocation)
			{
			    appdata data;
 
				#define PROGRAM_INTERPOLATE(fieldName) data.fieldName = \
					patch[0].fieldName * barycentricCoordinates.x + \
					patch[1].fieldName * barycentricCoordinates.y + \
					patch[2].fieldName * barycentricCoordinates.z;

				PROGRAM_INTERPOLATE(vertex)
				PROGRAM_INTERPOLATE(normal)
				PROGRAM_INTERPOLATE(tangent)
				data.normal = normalize(data.normal);
				data.tangent = normalize(data.tangent);
 
			   return VertexProgram(data);
			}
			 
			fixed4 FragmentProgram  (v2f i) : SV_Target
			{
				float3 cameraDirection = normalize(i.globalPosition - _WorldSpaceCameraPos);

				float opticalTerrainDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, i.screenPos)).r;
				float opticalWaterDepth = saturate((opticalTerrainDepth - i.screenPos.w) / _DeepWaterDepth);
				float alpha = clamp((opticalTerrainDepth - i.screenPos.w) / _Clarity, 0.1, 0.9);

				float4 color = lerp(_ShallowWaterColor, _DeepWaterColor, opticalWaterDepth);
				color = addDiffuseLighting(color, i.normal, _SunDirection, _Diffuse);
				color = addSpecularHighlight(color, i.normal, _SunDirection, cameraDirection, _Smoothness);
				color = addFocusRing(color, i.globalPosition, _FocusPosition, _FocusRadius);
				color.a = alpha * (i.waterMap.b > 0.1);

				return saturate(color); 
			}
			ENDCG
		}
	}
}