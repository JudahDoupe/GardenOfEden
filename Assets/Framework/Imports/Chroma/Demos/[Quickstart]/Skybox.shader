Shader "Chroma/Demos/Skybox"
{
    Properties
    {
        [Gradient(1024, hdr)] _Gradient("Gradient", 2D) = "white" {}

        _FoldoutDirection("[Space][Foldout(2)] Direction", Float) = 0
        _DirectionYaw ("X angle", Range(0, 1)) = 0
        _Tooltip1 ("[Tooltip] `Yaw` or horizon rotation.", Float) = 0
        _DirectionPitch ("Y angle", Range(0, 1)) = 0
        _Tooltip2 ("[Tooltip] `Pitch` or azimuth rotation.", Float) = 0
        
        _Line1 ("[Space][Line]", Float) = 0
    }

    CGINCLUDE
    #include "UnityCG.cginc"

    struct appdata
    {
        float4 position : POSITION;
        float3 texcoord : TEXCOORD0;
    };

    struct v2f
    {
        float4 position : SV_POSITION;
        float3 texcoord : TEXCOORD0;
    };

    UNITY_DECLARE_TEX2D_FLOAT(_Gradient);
    float _DirectionYaw, _DirectionPitch;

    v2f vert(appdata v) {
        v2f o;
        o.position = UnityObjectToClipPos(v.position);
        o.texcoord = v.texcoord;
        return o;
    }

    fixed4 frag(v2f i) : COLOR {
        const float pitch = _DirectionPitch * UNITY_PI;
        const float yaw = _DirectionYaw * UNITY_PI;
        const float3 direction = float3(sin(pitch) * sin(yaw), cos(pitch), sin(pitch) * cos(yaw));
        const float d = dot(normalize(i.texcoord), direction) * 0.5f + 0.5f;
        float4 color = UNITY_SAMPLE_TEX2D(_Gradient, float2(d, 0.5f));

        // Add random noise to the skybox to avoid banding.
        color.rgb += frac(sin(dot(i.texcoord, float3(12.9898, 78.233, 45.164))) * 43758.5453) * 0.01f;

        return color;
    }
    ENDCG

    SubShader
    {
        Tags
        {
            "RenderType"="Background" "Queue"="Background"
        }

        Pass
        {
            ZWrite Off
            Cull Off
            Fog
            {
                Mode Off
            }
            CGPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }

    CustomEditor "Chroma"
}