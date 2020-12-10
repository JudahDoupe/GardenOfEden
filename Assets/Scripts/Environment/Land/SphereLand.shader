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
	    _SoilColor("Live Soil Color", color) = (1,1,1,0)
        _TopographyFrequency ("Topography Line Frequency", Range(5,50)) = 10
        _TopographyWidth ("Topography Line Width", Range(0,1)) = 0.5
        _TopographyDarkening ("Topography Line Darkening", Range(0,0.5)) = 0.2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
		#pragma surface surf Standard alphatest:_Cutoff addshadow fullforwardshadows vertex:disp tessellate:tess nolightmap
		#pragma target 4.6 //TODO: try 3.0
		#include "Tessellation.cginc"
		#include "UnityShaderVariables.cginc"








        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
