Shader "Custom/EnemyMarkedGlow"
{
    Properties
    {
        _GlowColor("Glow Color", Color) = (1, 0, 0, 1)
        _GlowIntensity("Glow Intensity", Float) = 2.0
        _Alpha("Alpha", Range(0, 1)) = 1.0
    }

    SubShader
    {
        // Setup for Transparent rendering
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        // Enable Alpha Blending and disable ZWrite so it doesn't occlude itself weirdly
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            float4 _GlowColor;
            float _GlowIntensity;
            float _Alpha;

            Varyings vert(Attributes input)
            {
                Varyings output;
                // Directly convert Object Space to Clip Space
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Multiply base color by intensity for the HDR glow effect
                half3 finalRGB = _GlowColor.rgb * _GlowIntensity;
                return half4(finalRGB, _Alpha);
            }
            ENDHLSL
        }
    }
}