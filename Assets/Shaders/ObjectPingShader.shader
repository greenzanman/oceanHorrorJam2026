Shader "ObjectPingShader" {
    Properties {
        _HeatTex ("Texture", 2D) = "white" {}
        
    }
    SubShader {

        Tags {
            "Queue"="Transparent"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha // Alpha blend
        
        Pass {
            CGPROGRAM
            #pragma vertex vert             
            #pragma fragment frag

            struct imData {
                float4 pos : POSITION;
            };  
            struct v2f {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 localPos : TEXCOORD1;
            };

            v2f vert(imData input) {
                v2f output;
                output.worldPos = mul (unity_ObjectToWorld, input.pos);
                output.vertex = UnityObjectToClipPos(input.pos);
                output.localPos = mul(unity_ObjectToWorld, input.pos - float4(0,0,0,1)).xyz;
                return output;
            }

            CBUFFER_START(UnityPerMaterial)
            uniform int _PointCount = 0;
            uniform float _Radii [16];
            uniform float _MaxRadius = 0;
            uniform float4 _PointIntensities [16];       
            CBUFFER_END 
            
            fixed4 frag(v2f input) : COLOR {

                float h = 0;
                
                for (int i = 0; i < _PointCount; i ++)
                {
                    float4 pointIntensity = _PointIntensities[i];

                    if (pointIntensity.w <= 0)
                        continue;

                    float di = distance(input.localPos, fixed3(pointIntensity.x, 
                        pointIntensity.y, pointIntensity.z));
                    if (di < _Radii[i])
                        h = max(h, pointIntensity.w * (1 - di / _MaxRadius));
                }

                // Coloration changes
                h = h * (-0.5 + 0.5 * (sin(input.vertex.y) + cos(input.vertex.x)));

                fixed4 color = fixed4(h, 0, 0, 1);
                return color;
            }
            ENDCG
        }
    } 
    Fallback "Diffuse"
}