Shader "Custom/InteractableHighlight"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1,1,1,1)
        _GlowColor("Glow Color", Color) = (0,1,1,1)
        _HighlightLevel("Highlight Level", Range(0, 1)) = 0
        _NearbyRadius("Nearby Radius", Float) = 5.0
        _GlowIntensity("Glow Intensity", Float) = 2.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

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
                float3 positionWS : TEXCOORD0;
            };

            float4 _BaseColor;
            float4 _GlowColor;
            float _HighlightLevel;
            float _NearbyRadius;
            float _GlowIntensity;

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Calculate distance to camera (the player)
                float d = distance(input.positionWS, _WorldSpaceCameraPos);
                
                // Nearby Effect: Subtle glow that fades in as you get closer
                float nearbyMask = 1.0 - saturate(d / _NearbyRadius);
                
                // Focused Effect: Driven by the script (_HighlightLevel)
                // We combine them so Focused is always stronger than Nearby
                float finalHighlight = max(nearbyMask * 0.3, _HighlightLevel);
                
                // Mix colors based on the highlight
                half3 finalRGB = lerp(_BaseColor.rgb, _GlowColor.rgb * _GlowIntensity, finalHighlight);
                
                return half4(finalRGB, 1.0);
            }
            ENDHLSL
        }
    }
}