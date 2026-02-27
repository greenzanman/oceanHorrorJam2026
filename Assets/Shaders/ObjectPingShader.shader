Shader "Custom/ObjectPingShader" {
    Properties {
        // Controlled by SonarManager.cs
    }
    SubShader {
        // Keep "Opaque" queue to ensure proper sorting (prevents seeing through objects)
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }

        
        ZWrite On
        Cull Back 
        
        Pass {
            CGPROGRAM
            #pragma vertex vert             
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            // --- GLOBALS ---
            uniform float4 _SonarBaseColor;
            uniform float _SonarFadeStrength;
            uniform float _SonarGridScale;
            uniform float _SonarDotSize;
            uniform float _SceneViewVisibility; // Added for Scene View visibility
            uniform float4 _ColorLow;
            uniform float4 _ColorHigh;
            uniform float _MinY;
            uniform float _MaxY;

            // --- ARRAYS ---
            uniform int _PointCount;
            uniform float _Radii[16]; 
            uniform float4 _PointIntensities[16]; 

            v2f vert(appdata input) {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.worldPos = mul(unity_ObjectToWorld, input.vertex).xyz;
                output.screenPos = ComputeScreenPos(output.vertex);
                return output;
            }

            fixed4 frag(v2f input) : COLOR {
                float h = 0;
                
                // 1. RING LOGIC (Calculate Intensity)
                for (int i = 0; i < _PointCount; i++) {
                    float4 source = _PointIntensities[i];
                    float radius = _Radii[i];

                    if (source.w <= 0 || radius <= 0.1) continue;

                    float dist = distance(input.worldPos.xz, source.xz);

                    if (dist < radius) {
                        // dist / radius results in:
                        // 0.0 at the center (faded out immediately)
                        // 1.0 at the edge (brightest point)
                        float hollowNormalized = dist / radius; 

                        // Use pow() to push the darkness further out from the center,
                        // making the ring thinner and sharper at the edge.
                        float val = source.w * pow(hollowNormalized, _SonarFadeStrength * 2);
                        
                        h = max(h, val);
                    }
                }

                // 2. PATTERN LOGIC
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                screenUV.x *= _ScreenParams.x / _ScreenParams.y;

                float2 grid = frac(screenUV * _SonarGridScale);
                float pattern = step(_SonarDotSize, grid.x) * step(_SonarDotSize, grid.y);
                
                // render black instead of transparent when 'h' is 0
                // - If 'h' is 0 (no ping) or 'pattern' is 0 (grid line), the Math makes it Black.
                // - But it remains a SOLID PIXEL that writes to Depth.
                
                // Calculate normalized height factor (t)
                float t = saturate((input.worldPos.y - _MinY) / (_MaxY - _MinY));
                
                // Interpolate between low and high colors
                float4 gradientColor = lerp(_ColorLow, _ColorHigh, t);

                // Color calulation:
                fixed4 finalColor = gradientColor * h * pattern; 
                
                // Ensure Alpha is 1.0 so it is treated as a solid object
                finalColor.a = 1.0;

                // --- IMPROVED VISIBILITY LOGIC (Clay Render) ---
                if (_SceneViewVisibility > 0) {
                    // 1. Calculate the "Normal" (facing direction) of this pixel automatically
                    float3 visibleNormal = normalize(cross(ddy(input.worldPos), ddx(input.worldPos)));

                    // 2. Create a fake "Sun" direction (coming from top-left)
                    float3 fakeLightDir = normalize(float3(-1, 2, -1));

                    // 3. Calculate simple brightness (Dot Product)
                    float lightIntensity = saturate(dot(visibleNormal, fakeLightDir));

                    // 4. Add a "Rim Light" effect so edges stand out
                    float3 viewDir = normalize(_WorldSpaceCameraPos - input.worldPos);
                    float rim = 1.0 - saturate(dot(viewDir, visibleNormal));

                    // 5. Combine: Dark Grey Base + Lighting + Rim
                    float3 debugColor = float3(0.1, 0.1, 0.1);
                    debugColor += lightIntensity * 0.3;
                    debugColor += rim * 0.2;

                    // Add this on top of the sonar visuals
                    finalColor.rgb += debugColor * _SceneViewVisibility;
                }

                return finalColor;
            }
            ENDCG
        }
    } 
}