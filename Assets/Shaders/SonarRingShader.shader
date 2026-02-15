Shader "Custom/SonarRing"
{
    Properties
    {
        _Depth("Ring Thickness", Float) = 0.4
        _Color("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { 
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "IgnoreProjector"="True"
        }
        
        ZWrite Off
        Blend One One
        ZTest LEqual
        Cull Front // render inner face of sphere

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
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
                // 1. Sample the depth texture
                float rawDepth = tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)).r;
                
                // 2. Convert to Linear Eye Depth
                // In URP, we use _ZBufferParams to ensure the units are correct
                float sceneDepth = LinearEyeDepth(rawDepth);
                
                // 3. Sphere Surface Depth
                float sphereDepth = i.screenPos.w;

                // --- THE X-RAY FIX ---
                // If the sphere is further than the geometry, stop drawing.
                if (sphereDepth > sceneDepth + 0.05) {
                    discard;
                }

                // --- THE RING LOGIC ---
                float diff = abs(sceneDepth - sphereDepth);
                
                // Calculate intensity based on distance to intersection
                float ringIntensity = saturate(1.0 - (diff / _Depth));
                
                // Sharpen the ring look
                ringIntensity = pow(ringIntensity, 3);

                return ringIntensity * _Color;
            }
            ENDCG
        }
    }
}