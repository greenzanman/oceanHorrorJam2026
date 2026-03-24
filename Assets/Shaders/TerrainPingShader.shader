Shader "Custom/TerrainPingShader" {
    Properties {
        // Empty Properties block
    }
    SubShader {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+1" }
        
        Pass {
            ZWrite On
            ZTest LEqual
            
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

            // --- GLOBALS (Must match Object Shader exactly) ---
            uniform float4 _SonarBaseColor;
            uniform float _SonarFadeStrength;
            uniform float _SonarGridScale;
            uniform float _SonarDotSize;
            uniform float _SceneViewVisibility; // Added for Scene View visibility
            uniform float4 _ColorLow;
            uniform float4 _ColorHigh;
            uniform float _MinY;
            uniform float _MaxY;
            
            // --- CONE GLOBALS ---
            uniform float4 _PingDirections[16];
            uniform float _WedgeFeather; 
            uniform float _MinOmniRadius;


            // Arrays
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
                
                // 1. RING LOGIC
                for (int i = 0; i < _PointCount; i++) {
                    float4 source = _PointIntensities[i];
                    float radius = _Radii[i];
                    if (source.w <= 0 || radius <= 0.1) continue;

                    float3 pingCenter = source.xyz;
                    float3 offset = input.worldPos - pingCenter;
                    
                    // --- WEDGE & OMNI CULLING ---
                    // 1. Calculate horizontal-only direction
                    float2 flatDirToPixel = normalize(offset.xz);
                    float2 flatForward = normalize(_PingDirections[i].xz);

                    // 2. Horizontal Dot Product
                    float dotProduct = dot(flatDirToPixel, flatForward);

                    // 3. Smoothstep feathering
                    float wedgeMask = smoothstep(_PingDirections[i].w - _WedgeFeather, _PingDirections[i].w + _WedgeFeather, dotProduct);

                    // 4. Distance bypass for close-up omnidirectional ping
                    float distToCenter = length(offset);
                    float omniMask = 1.0 - smoothstep(_MinOmniRadius - 1.0, _MinOmniRadius, distToCenter);

                    // 5. Combine masks
                    float finalVisibility = max(wedgeMask, omniMask);

                    if (finalVisibility <= 0) continue; // Skip rendering if outside visible cone area

                    // --- DISTANCE CULLING ---
                    float dist = distance(input.worldPos.xz, source.xz);

                    // We only draw inside the circle
                    if (dist < radius) {
                        // (Hollow Ring):
                        // We want the edge (dist ~ radius) to be 1.
                        // We want the center (dist ~ 0) to be 0.
                        float hollowNormalized = dist / radius;

                        // We use 'pow' to make the ring thinner. 
                        // Higher power = Thinner ring at the edge.
                        // We multiply by _SonarFadeStrength * 2 to give you more control.
                        float ringEdge = pow(hollowNormalized, _SonarFadeStrength * 2);
                        
                        // Multiply by global intensity (source.w) so it still fades over time
                        float val = source.w * ringEdge;
                        
                        // Apply the visibility mask to the final intensity value
                        val *= finalVisibility; 
                        
                        h = max(h, val);
                    }
                }

                // 2. PATTERN LOGIC
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                screenUV.x *= _ScreenParams.x / _ScreenParams.y;

                float2 grid = frac(screenUV * _SonarGridScale);
                float pattern = step(_SonarDotSize, grid.x) * step(_SonarDotSize, grid.y);
                
                // 3. COMBINE
                // For terrain (Opaque), we multiply color directly
                // fixed4 pingColor = _SonarBaseColor * h * pattern;

                // Calculate normalized height factor (t)
                float t = saturate((input.worldPos.y - _MinY) / (_MaxY - _MinY));
                
                // Interpolate between low and high colors
                float4 gradientColor = lerp(_ColorLow, _ColorHigh, t);
                
                // Calculate final ping color:
                fixed4 pingColor = gradientColor * h * pattern;
                
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
                    pingColor.rgb += debugColor * _SceneViewVisibility;
                }
                return pingColor;
            }
            ENDCG
        }
    } 
    Fallback "Diffuse"
}