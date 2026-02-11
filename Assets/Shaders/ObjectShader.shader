Shader "Unlit/ObjectShader"
{
    Properties
    {
        _Visible("Visible", Float) = 1
        _AlwaysHidden("AlwaysHidden", Float) = 0
        
        _ColorBottom("Bottom Color", Color) = (0, 0.1, 0.3, 1)
        _ColorTop("Top Color", Color) = (0.3, 0.7, 1, 1)
    }
    SubShader
    {
        Tags { 
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "UniversalMaterialType" = "Lit"
            "Queue"="Transparent"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On
        ZTest LEqual
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

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
                float4 vertex : SV_POSITION;
            };

            // Declare the variables so the fragment shader can see them
            float _Visible;
            float _AlwaysHidden;
            fixed4 _ColorTop;
            fixed4 _ColorBottom;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Create a vertical gradient using the properties
                // UV.y goes from 0 (bottom) to 1 (top)
                float t = i.uv.y;
                fixed4 col = lerp(_ColorBottom, _ColorTop, t);
                
                UNITY_APPLY_FOG(i.fogCoord, col);

                if (_AlwaysHidden > 0)
                    return fixed4(0, 0, 0, 1);

                return fixed4(col.rgb * _Visible, col.a);
            }
            ENDCG
        }
    }
}