//// By Hayashi 
Shader "UI/BloomImage"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BloomIntensity("Bloom Intensity", Range(1, 10)) = 1.0
        _ScrollSpeed("Scroll Speed", Range(0, 10)) = 1.0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION; // Vertex position
                float2 uv : TEXCOORD0;    // Texture UV
                float4 color : COLOR;     // Vertex color from UI
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR; // Will carry the UI color to fragment
            };

            sampler2D _MainTex;
            float _BloomIntensity;
            float _ScrollSpeed;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color; // Pass the vertex color through
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float xOffset = _Time.x * _ScrollSpeed;

                float intPart;
                float fracPart = modf(xOffset, intPart);

                float2 wrappedUV = i.uv;
                wrappedUV.x += fracPart;
                if (wrappedUV.x > 1.0)
                {
                    wrappedUV.x -= 1.0;
                }

                fixed4 col = tex2D(_MainTex, wrappedUV);

                // Apply bloom based on alpha threshold
                // (If you have a bloom threshold property, define it)
                // For now, assume a threshold or remove bloom threshold usage
                // float bloom = saturate(col.a - _BloomThreshold); // Uncomment if using _BloomThreshold
                // col *= _BloomIntensity * bloom; // Adjust per your bloom logic

                // Without a defined _BloomThreshold in properties, just apply intensity:
                col *= _BloomIntensity;

                // Multiply by the UI vertex color
                col *= i.color;

                // CutOff
                if (col.a <= 0.5)
                {
                    discard;
                }

                return col;
            }
            ENDCG
        }
    }
}
