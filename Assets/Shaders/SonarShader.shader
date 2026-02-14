Shader "Unlit/TestShader"
{
    Properties
    {
        _Depth("Depth", Float) = 0.15
        _Color("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { 
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "UniversalMaterialType" = "Lit"
            "Queue"="Transparent"
            "IgnoreProjector"="True"
        }
        ZWrite Off
        Blend One One, One One
        ZTest LEqual
        Cull Off
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            
            #define _RECEIVE_SHADOWS_OFF 1

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 screenPos : TEXCOORD1;   
                float4 vertex : SV_POSITION;
            };

            sampler2D_float _CameraDepthTexture;
            float _Depth;
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Find the depth and screen position
                float depth = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)).r);
                float screenPosAlph = i.screenPos.w;
                screenPosAlph = screenPosAlph - _Depth;

                // Clamp and apply color
                float comparisonVal = 1 - depth + screenPosAlph;
                comparisonVal = clamp(comparisonVal, 0, 1);
                
                float4 outColor = comparisonVal * _Color;
                return outColor;
            }
            ENDCG
        }
    }
}
