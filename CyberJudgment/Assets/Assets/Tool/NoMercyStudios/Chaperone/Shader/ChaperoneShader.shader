Shader "Custom/ChaperoneShader" {
    Properties{
        _MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
        [HDR]_Color("Color Multiplier", Color) = (1,1,1,1)
        [HideInInspector]_MaskRadius("Mask Radius", Range(0,1000)) = 250
        [HideInInspector]_UseMask("Mask Amount", Range(0,1)) = 1
        [HideInInspector]_MaskCentersCount("Mask Center Count", Range(0,10)) = 1
    }
    SubShader{
        Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Front
        Pass {
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fog
                #pragma shader_feature LOCAL_KEYWORDS_ON
                #pragma multi_compile_instancing
                #define MAX_MASK_CENTERS 32
                #include "UnityCG.cginc"

                struct appdata_t {
                    float4 vertex : POSITION;
                    float2 texcoord : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f {
                    float4 vertex : SV_POSITION;
                    half2 texcoord : TEXCOORD0;
                    float3 worldPos : TEXCOORD1;
                    UNITY_FOG_COORDS(2)
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                float4 _Color;
                float _MaskRadius;
                float _UseMask;
                int _MaskCentersCount;
                float4 _MaskCentersArray[MAX_MASK_CENTERS];

                v2f vert(appdata_t v)
                {
                    v2f o;
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                    o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                    UNITY_TRANSFER_FOG(o, o.vertex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                    float minDist = 2.0;
                    for (int j = 0; j < _MaskCentersCount; j++)
                    {
                        float dist = (distance(i.worldPos, _MaskCentersArray[j].xyz) / _MaskRadius);
                        minDist = min(minDist, dist);
                    }
                    float mask = smoothstep(1.0, 0.0, minDist);
                    fixed4 col = tex2D(_MainTex, i.texcoord) * _Color;
                    col.a *= lerp(1.0, mask, _UseMask);
                    UNITY_APPLY_FOG(i.fogCoord, col);
                    return col;
                }
            ENDCG
        }
    }
}