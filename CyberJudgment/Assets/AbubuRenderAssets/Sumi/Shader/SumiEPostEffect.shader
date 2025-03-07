
Shader "SumiEPostEffectPlus_BrushOutline_Stronger"
{
    Properties
    {
        _Threshold        ("Edge Threshold", Range(0,1)) = 0.6
        _EdgeSoftness     ("Edge Softness", Range(0,1))  = 0.15
        _InkSpread        ("Ink Spread (Blur)", Range(0,1)) = 0.2
        _PosterizeLevels  ("Posterize Levels", Range(1,8)) = 4.0
        _InkDarkness      ("Ink Darkness", Range(0.0,2.0)) = 1.2

        _OutlineThickness ("Brush Outline Thickness", Range(0,5)) = 1.0
        _BrushTex         ("Brush Texture (for outline)", 2D) = "white" {}
        _BrushScale       ("Brush Scale", Float) = 1.0
        _BrushIntensity   ("Brush Intensity", Range(0,1)) = 0.8

        _PaperTex         ("Paper Texture", 2D) = "white" {}
        _PaperScale       ("Paper Scale", Float) = 1.0
        _PaperIntensity   ("Paper Intensity", Range(0,1)) = 0.6 

        _Brightness       ("Overall Brightness", Range(0,2.0)) = 1.2

        _MinTone          ("Minimum Tone", Range(0,1)) = 0.2
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        Pass
        {
            ZTest Always Cull Off ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;            float4 _MainTex_TexelSize;
            sampler2D _PaperTex;           float4 _PaperTex_ST;
            sampler2D _BrushTex;           float4 _BrushTex_ST;
            sampler2D _OutlineMask;

            float _Threshold;
            float _EdgeSoftness;
            float _InkSpread;
            float _PosterizeLevels;
            float _InkDarkness;

            float _OutlineThickness;
            float _BrushScale;
            float _BrushIntensity;

            float _PaperScale;
            float _PaperIntensity;

            float _Brightness;
            float _MinTone;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            v2f vert(appdata_img v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.texcoord;
                return o;
            }

            float3 WeightedSampleBlurColor(float2 uv)
            {
                float2 off = _InkSpread * _MainTex_TexelSize.xy;
                float kernel[9] = {1, 2, 1, 2, 4, 2, 1, 2, 1};
                float totalWeight = 16.0;
                float3 sum = 0;
                int idx = 0;
                for (int y = -1; y <= 1; y++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        float2 sampleUV = uv + float2(x, y) * off;
                        float3 col = tex2D(_MainTex, sampleUV).rgb;
                        sum += col * kernel[idx];
                        idx++;
                    }
                }
                return sum / totalWeight;
            }

            float Posterize(float val, float levels)
            {
                return floor(val * levels) / levels;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 blurCol = WeightedSampleBlurColor(i.uv);
                float gray = dot(blurCol, float3(0.299, 0.587, 0.114));

                float gx = -tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * float2(-1, -1)).r
                           -2.0 * tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * float2(-1, 0)).r
                           -tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * float2(-1, 1)).r
                           + tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * float2(1, -1)).r
                           +2.0 * tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * float2(1, 0)).r
                           +tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * float2(1, 1)).r;
                float gy = -tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * float2(-1, -1)).r
                           -2.0 * tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * float2(0, -1)).r
                           -tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * float2(1, -1)).r
                           + tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * float2(-1, 1)).r
                           +2.0 * tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * float2(0, 1)).r
                           +tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * float2(1, 1)).r;
                float edge = sqrt(gx * gx + gy * gy);

                float edgeFactor = 1.0 - smoothstep(_Threshold, _Threshold + _EdgeSoftness, edge);
                float maskVal = tex2D(_OutlineMask, i.uv).r;
                edgeFactor *= maskVal;

                float poster = Posterize(gray, _PosterizeLevels);
                float baseColor = poster;

                float2 brushUV = i.uv * _BrushScale;
                float brushSample = tex2D(_BrushTex, brushUV).r;
                float brushMask = saturate(1.0 - brushSample * _BrushIntensity);
                float outlineStrength = saturate((edgeFactor - 0.3) / 0.7) * brushMask;
                float sumiColor = lerp(baseColor, _MinTone, outlineStrength);

                float2 paperUV = i.uv * _PaperScale;
                float paperSample = tex2D(_PaperTex, paperUV).r;
                float paperEffect = 1.0 - paperSample * _PaperIntensity;
                sumiColor *= paperEffect;

                sumiColor *= _Brightness;

                return fixed4(sumiColor, sumiColor, sumiColor, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
