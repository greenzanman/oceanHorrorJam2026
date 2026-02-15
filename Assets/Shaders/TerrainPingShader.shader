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

                    float dist = distance(input.worldPos.xz, source.xz);

                    // We only draw inside the circle
                    if (dist < radius) {
                        
                        // OLD MATH (Solid Circle):
                        // float falloff = 1.0 - (dist / radius); 
                        // Result: Center is Bright (1), Edge is Dark (0)

                        // NEW MATH (Hollow Ring):
                        // We want the edge (dist ~ radius) to be 1.
                        // We want the center (dist ~ 0) to be 0.
                        float hollowNormalized = dist / radius;

                        // We use 'pow' to make the ring thinner. 
                        // Higher power = Thinner ring at the edge.
                        // We multiply by _SonarFadeStrength * 4 to give you more control.
                        float ringEdge = pow(hollowNormalized, _SonarFadeStrength * 4);
                        
                        // Multiply by global intensity (source.w) so it still fades over time
                        float val = source.w * ringEdge;
                        
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
                fixed4 pingColor = _SonarBaseColor * h * pattern;
                
                return pingColor; 
            }
            ENDCG
        }
    } 
    Fallback "Diffuse"
}