Shader "JapaneceGarden/JapaneceGarden_FoliageSheder"
{
    Properties
    {
        [NoScaleOffset] _AlphaMap("AlphaMap",2D) = "White"{}
        [NoScaleOffset] _BaseColorMap("BaseColorMap",2D) = "White"{}
        [NoscaleOffset] _RoughnessMap("RoughnessMap",2D) = "White"{}
        [NoScaleOffset] _MetallicMap("MetallicMap",2D) = "Black"{}
        [NoscaleOffset] _NormalMap("Normal",2D) = "bump"{}

        [Space(20)]

        _Color("Color",Color) = (1,1,1,1)
        _Roughness("RoughnessValue",Range(0,1)) = 0.8
        _Metallic("MetallicValue", Range(0, 1)) = 0

        [Space(20)]

        [MaterialToggle] _UseRoughnessMap("UseRoughnessMap",int) = 1
        [MaterialToggle] _UseMetallicMap("UsematerialMap", int) = 0

        _Cutoff("Cutoff", Range(0, 1)) = 0.9
    }

        SubShader

    {
        Tags {
            "Queue" = "AlphaTest"
            "RenderType" = "TransparentCutout"
        }

        LOD 200

        CGPROGRAM
        
        #pragma surface surf Standard fullforwardshadows alphatest:_Cutoff

        #pragma target 3.0


        struct Input
        {
            float2 uv_BaseColorMap;
        };
        
        sampler2D _AlphaMap;
        sampler2D _BaseColorMap;
        sampler2D _RoughnessMap;
        sampler2D _MetallicMap;
        sampler2D _NormalMap;

        float4 _Color;
        float _Roughness;
        float _Metallic;

        int  _UseRoughnessMap;
        int  _UseMetallicMap;


        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float2 uv = IN.uv_BaseColorMap;

            float3 normal = UnpackNormal(tex2D(_NormalMap, uv));

            
            o.Albedo = tex2D(_BaseColorMap, uv) * _Color;
            o.Alpha = tex2D(_AlphaMap, uv);
            o.Smoothness = (_UseRoughnessMap) ? 1 - tex2D(_RoughnessMap, uv) : _Roughness;
            o.Metallic = (_UseMetallicMap) ? tex2D(_MetallicMap, uv) : _Metallic;
            o.Normal = normal;
        }
        ENDCG
    }

    FallBack "Transparent/Cutout/Diffuse"
}
