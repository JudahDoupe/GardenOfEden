Shader "Unlit/LightReplacementShader"
{
    Properties
    {
		_LightAbsorbtionId ("Light Absorbtion Id", float) = 1000
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            struct v2f
            {
                float4 vertex : SV_POSITION;
            };
			struct fragOutput {
				float id : SV_Depth;
			};            

			uniform float _LightAbsorbtionId;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

			fragOutput frag (v2f i)
			{
				fragOutput o;
				o.id = _LightAbsorbtionId;
				return o;
			}
            ENDCG
        }
    }
}
