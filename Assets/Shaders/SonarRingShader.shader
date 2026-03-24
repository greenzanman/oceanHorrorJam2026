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
                float4 screenPos : TEXCOORD0;   
                float3 worldPos : TEXCOORD1;
                float3 centerPos : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

            sampler2D_float _CameraDepthTexture;
            float _Depth;
            fixed4 _Color;

            // Global properties from SonarManager
            uniform float _WedgeFeather;
            uniform float _MinOmniRadius;

            // Per-instance property set by SonarPingSphere.cs
            uniform float4 _PingDirectionInfo; 
            

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.centerPos = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // --- WEDGE & OMNI CULLING ---
                float3 offset = i.worldPos - i.centerPos;
                
                // 1. Horizontal direction
                float2 flatDirToPixel = normalize(offset.xz);
                float2 flatForward = normalize(_PingDirectionInfo.xz);

                // 2. Dot Product
                float dotProduct = dot(flatDirToPixel, flatForward);

                // 3. Feathering
                float wedgeMask = smoothstep(_PingDirectionInfo.w - _WedgeFeather, _PingDirectionInfo.w + _WedgeFeather, dotProduct);

                // 4. Distance bypass
                float distToCenter = length(offset);
                float omniMask = 1.0 - smoothstep(_MinOmniRadius - 1.0, _MinOmniRadius, distToCenter);

                // 5. Combine and Cull
                float finalVisibility = max(wedgeMask, omniMask);
                if (finalVisibility <= 0) {
                    discard;
                }

                // --- THE X-RAY FIX ---
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

                // Multiply visual output by final visibility mask to feather the edges
                return ringIntensity * finalVisibility * _Color;
            }
            ENDCG
        }
    }
}