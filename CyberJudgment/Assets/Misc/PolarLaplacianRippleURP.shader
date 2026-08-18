Shader "Custom/HollowPurpleEffect_URP_WithGridRandomColor"
{
 Properties
    {
        // メインテクスチャ
        _MainTex ("Main Texture", 2D) = "white" {}
    
        // マスクテクスチャ
        _MaskTex ("Ripple Mask", 2D) = "white" {}
    
        // --- 時間係数 ---
        _TimeScale ("Time Scale", Float) = 1.0
    
        // --- 蒼(青波)パラメータ ---
        _BlueWaveSpeed ("Blue Wave Speed", Float) = 0.8
        _BlueWaveFrequency ("Blue Wave Frequency", Float) = 6.0
        _BlueWaveAmplitude ("Blue Wave Amplitude", Float) = 0.03
        _BlueColor ("Blue Color (蒼)", Color) = (0.0, 0.7, 1.0, 1.0)
    
        // --- 赫(赤波)パラメータ ---
        _RedWaveSpeed ("Red Wave Speed", Float) = 1.2
        _RedWaveFrequency ("Red Wave Frequency", Float) = 8.0
        _RedWaveAmplitude ("Red Wave Amplitude", Float) = 0.03
        _RedColor ("Red Color (赫)", Color) = (1.0, 0.2, 0.2, 1.0)
    
        // --- 衝突(紫)演出パラメータ ---
        _PurpleIntensity ("Purple Intensity", Range(0, 3)) = 1.0
        _PurpleColor ("Purple Color (茈)", Color) = (1.0, 0.0, 1.0, 1.0)
    
        // ノイズ振動
        _NoiseVibration ("Noise Vibration", Float) = 0.02
    
        // 波の中心
        _Center ("Center", Vector) = (0.5, 0.5, 0, 0)
    
        // マスクのチャンネル指定
        _MaskChannel ("Mask Channel (0=R,1=G,2=B,3=A)", Float) = 0.0
    
        // --- ベース色・反射 ---
        _Color ("Tint Color", Color) = (1,1,1,1)
        _ReflectionStrength ("Reflection Strength", Range(0,1)) = 0.5
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
        _EnvReflectionColor ("Environment Reflection Color", Color) = (0.6,0.7,0.8,1)
    
        // --- グリッド分割 ---
        _GridCountX("Grid Count X", Float) = 4.0
        _GridCountY("Grid Count Y", Float) = 4.0
    
        // --- グリッド蛍光色のフェード設定 ---
        _ColorCycleSpeed("Fluorescent Color Cycle Speed", Float) = 1.0 // フェード速度を少し速く
        _GridColorStrength("Grid Random Color Strength", Range(0,1)) = 1.5 // デフォルトを1.5に増加
    
        // --- グリッドのふち ---
        _GridLineThickness("Grid Line Thickness", Range(0,0.1)) = 0.02
        _GridLineColor("Grid Line Color", Color) = (0,0,0,1)
    
        // --- エミッション ---
        _EmissionColor ("Emission Color", Color) = (0,0,0,1)
        _EmissionMap ("Emission Map", 2D) = "black" {}
        _BloomStrength ("Bloom Strength", Float) = 1.0 // 追加
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            //----------------------------------------------------
            // 構造体定義
            //----------------------------------------------------
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            //----------------------------------------------------
            // テクスチャ & プロパティ宣言
            //----------------------------------------------------
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);

            // --- エミッション ---
            TEXTURE2D(_EmissionMap);
            SAMPLER(sampler_EmissionMap);
            float4 _EmissionColor;
            float _BloomStrength; // 追加

            float _TimeScale;

            // 蒼(青波)
            float _BlueWaveSpeed;
            float _BlueWaveFrequency;
            float _BlueWaveAmplitude;
            float4 _BlueColor;

            // 赫(赤波)
            float _RedWaveSpeed;
            float _RedWaveFrequency;
            float _RedWaveAmplitude;
            float4 _RedColor;

            // 衝突(紫)
            float _PurpleIntensity;
            float4 _PurpleColor;

            float _NoiseVibration;
            float4 _Center;
            float _MaskChannel;

            // ベース色
            float4 _Color;
            float _ReflectionStrength;
            float _Smoothness;
            float4 _EnvReflectionColor;

            // グリッド
            float _GridCountX;
            float _GridCountY;

            // グリッド蛍光フェード
            float _ColorCycleSpeed;      // 色が切り替わる速さ
            float _GridColorStrength;    // 波紋色とグリッド色のブレンド比率

            // グリッドライン
            float _GridLineThickness;
            float4 _GridLineColor;

            //----------------------------------------------------
            // ランダム & ノイズ用ユーティリティ
            //----------------------------------------------------
            float Rand2D(float2 co)
            {
                // 0～1の乱数を返す
                return frac(sin(dot(co, float2(12.9898, 78.233))) * 43758.5453);
            }

            // 簡易3Dノイズ
            float Noise3D(float3 p)
            {
                float2 xy = frac(p.xy);
                float2 xz = frac(p.xz);
                float2 yz = frac(p.yz);
                float r1 = Rand2D(xy);
                float r2 = Rand2D(xz);
                float r3 = Rand2D(yz);
                return frac(r1 + r2 + r3);
            }

            // HSVからRGBへの変換関数
            float3 HSVtoRGB(float3 hsv)
            {
                float3 rgb = float3(0.0, 0.0, 0.0);
                float h = hsv.x;
                float s = hsv.y;
                float v = hsv.z;

                float c = v * s;
                float x = c * (1.0 - abs(fmod(h * 6.0, 2.0) - 1.0));
                float m = v - c;

                if (h < 1.0 / 6.0)
                {
                    rgb = float3(c, x, 0.0);
                }
                else if (h < 2.0 / 6.0)
                {
                    rgb = float3(x, c, 0.0);
                }
                else if (h < 3.0 / 6.0)
                {
                    rgb = float3(0.0, c, x);
                }
                else if (h < 4.0 / 6.0)
                {
                    rgb = float3(0.0, x, c);
                }
                else if (h < 5.0 / 6.0)
                {
                    rgb = float3(x, 0.0, c);
                }
                else
                {
                    rgb = float3(c, 0.0, x);
                }

                return rgb + float3(m, m, m);
            }

            // 「蛍光色」を生成するための関数例（拡張版）
            // HSV色空間を利用して、より多様な明るい色を生成
            float3 RandomFluorescentColor(float2 seed)
            {
                float rand = Rand2D(seed);
                float hue = rand; // 0.0〜1.0の範囲で色相をランダムに
                float saturation = 1.0; // 最大彩度
                float value = 1.0; // 最大明度
                return HSVtoRGB(float3(hue, saturation, value));
            }

            //----------------------------------------------------
            // Vertex シェーダー
            //----------------------------------------------------
            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS);
                o.uv = v.uv;
                o.worldPos = TransformObjectToWorld(v.positionOS);
                return o;
            }

            //----------------------------------------------------
            // Fragment シェーダー
            //----------------------------------------------------
            half4 frag(Varyings i) : SV_Target
            {
                // 1) ベース設定
                float2 uv = i.uv;
                float time = _Time.y * _TimeScale;

                //==================================================
                // 2) 波紋エフェクト計算（従来通り）
                //==================================================
                float2 centeredUV = uv - _Center.xy;
                float r = length(centeredUV) + 1e-5;

                // 蒼(青) & 赫(赤)
                float waveBlue = sin(_BlueWaveFrequency * r - _BlueWaveSpeed * time) * _BlueWaveAmplitude;
                float waveRed  = sin(_RedWaveFrequency  * r - _RedWaveSpeed  * time) * _RedWaveAmplitude;

                // ノイズ
                float noiseVal = Noise3D(float3(uv * 10.0, time * 0.5)) - 0.5;
                float waveNoise = noiseVal * _NoiseVibration;

                // マスク
                float4 maskRGBA = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uv);
                int ch = (int)floor(_MaskChannel + 0.5);
                ch = clamp(ch, 0, 3);
                float maskValue = 0.0;
                if (ch == 0) maskValue = maskRGBA.r;
                else if(ch == 1) maskValue = maskRGBA.g;
                else if(ch == 2) maskValue = maskRGBA.b;
                else             maskValue = maskRGBA.a;

                float rippleHeight = (waveBlue + waveRed + waveNoise) * maskValue;

                // 法線計算(簡易)
                float2 eps = float2(0.001, 0);
                float baseHeight = rippleHeight;

                // rippleHeight(uv + eps.xx)
                float2 uvX = (uv + eps.xx) - _Center.xy;
                float rX = length(uvX) + 1e-5;
                float waveBlueX = sin(_BlueWaveFrequency * rX - _BlueWaveSpeed * time) * _BlueWaveAmplitude;
                float waveRedX  = sin(_RedWaveFrequency  * rX - _RedWaveSpeed  * time) * _RedWaveAmplitude;
                float noiseValX = Noise3D(float3((uv + eps.xx)*10.0, time*0.5)) - 0.5;
                float waveNoiseX = noiseValX * _NoiseVibration;
                float maskX = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uv + eps.xx).r;
                float rippleHeightX = (waveBlueX + waveRedX + waveNoiseX) * maskX;

                // rippleHeight(uv + eps.yy)
                float2 uvY = (uv + eps.yy) - _Center.xy;
                float rY = length(uvY) + 1e-5;
                float waveBlueY = sin(_BlueWaveFrequency * rY - _BlueWaveSpeed * time) * _BlueWaveAmplitude;
                float waveRedY  = sin(_RedWaveFrequency  * rY - _RedWaveSpeed  * time) * _RedWaveAmplitude;
                float noiseValY = Noise3D(float3((uv + eps.yy)*10.0, time*0.5)) - 0.5;
                float waveNoiseY = noiseValY * _NoiseVibration;
                float maskY = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uv + eps.yy).r;
                float rippleHeightY = (waveBlueY + waveRedY + waveNoiseY) * maskY;

                float dx = (rippleHeightX - baseHeight) / eps.x;
                float dy = (rippleHeightY - baseHeight) / eps.x;
                float3 normal = normalize(float3(-dx, 1.0, -dy));

                // Fresnel & 反射
                float3 viewDir = normalize(GetCameraPositionWS() - i.worldPos);
                float fresnel = pow(1.0 - saturate(dot(normal, viewDir)), 3.0);
                float reflectionFactor = _ReflectionStrength * fresnel * _Smoothness;

                // ベース色
                float4 baseSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                float3 baseColor = baseSample.rgb * _Color.rgb;
                float alpha = baseSample.a * _Color.a;

                // 波色（蒼＋赫）と衝突(紫)
                float blueAmount = abs(waveBlue) * maskValue;
                float redAmount  = abs(waveRed)  * maskValue;
                float3 colorFromBlue = _BlueColor.rgb * blueAmount;
                float3 colorFromRed  = _RedColor.rgb  * redAmount;

                float collide = abs(waveBlue * waveRed) * _PurpleIntensity;
                float purpleBlend = saturate(collide);
                float3 colorPurple = _PurpleColor.rgb * purpleBlend;

                // 波紋の合成色
                float3 combinedColor = baseColor + colorFromBlue + colorFromRed + colorPurple;

                //==================================================
                // 3) グリッドごとの「蛍光色フェード」計算
                //==================================================
                float2 gridUV = float2(_GridCountX, _GridCountY) * uv;
                float2 cellID = floor(gridUV); // グリッドID (整数)

                // サイクル管理
                //   time * _ColorCycleSpeed が1進むごとに、色が一巡するイメージ
                float cycleIndex = floor(time * _ColorCycleSpeed);
                float cycleFrac  = frac(time * _ColorCycleSpeed);

                // シードを作るため、cellID.x, cellID.y, cycleIndexをまとめて使う
                //   colorA: サイクルIndex (継続中の色)
                //   colorB: サイクルIndex + 1 (次にフェードする色)
                float baseSeed = cellID.x + cellID.y * 1000.0; // 適当に大きい値
                float2 seedA = float2(baseSeed + cycleIndex, baseSeed - cycleIndex);
                float2 seedB = float2(baseSeed + (cycleIndex + 1.0), baseSeed - (cycleIndex + 1.0));

                // それぞれ蛍光色を生成（拡張版）
                float3 colorA = RandomFluorescentColor(seedA);
                float3 colorB = RandomFluorescentColor(seedB);

                // フェード: 0〜1 で colorA→colorB を補間
                float3 colorFade = lerp(colorA, colorB, cycleFrac);

                // グリッドの線を描画（端なら線色、それ以外はフェード色）
                float2 fractUV = frac(gridUV);
                bool isEdgeX = (fractUV.x < _GridLineThickness) || (fractUV.x > 1.0 - _GridLineThickness);
                bool isEdgeY = (fractUV.y < _GridLineThickness) || (fractUV.y > 1.0 - _GridLineThickness);
                bool isInLine = isEdgeX || isEdgeY;

                float3 colorGrid = isInLine ? _GridLineColor.rgb : colorFade;

                // 波紋の色 `combinedColor` と グリッド蛍光色 `colorGrid` をブレンド
                float3 finalColor = lerp(combinedColor, colorGrid, _GridColorStrength);

                //==================================================
                // 4) 環境反射や微調整
                //==================================================
                float3 envColor = _EnvReflectionColor.rgb;
                finalColor = lerp(finalColor, envColor, reflectionFactor);

                // エミッションの計算
                float3 emission = _EmissionColor.rgb;
                
                // エミッションマップが設定されている場合はサンプリング
                #if defined(_EmissionMap)
                    emission += SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, uv).rgb;
                #endif

                // Bloomの強さを適用 // ここを追加
                emission *= _BloomStrength;

                // 最終カラーにエミッションを加算
                finalColor += emission;

                // 波の高さによる明暗変化（お好みで）
                finalColor += rippleHeight * 0.05;

                // 明るさを保つために色をクランプ
                finalColor = saturate(finalColor);

                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}
