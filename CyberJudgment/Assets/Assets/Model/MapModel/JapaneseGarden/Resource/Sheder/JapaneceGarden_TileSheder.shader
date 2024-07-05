Shader "JapaneceGarden/JapaneceGarden_TileSheder"
{
    Properties
    {
        [NoScaleOffset] _BaseColorMap("BaseColorMap",2D) = "White"{}
        [NoscaleOffset] _RoughnessMap("RoughnessMap",2D) = "White"{}
        [NoScaleOffset] _MetallicMap("MetallicMap",2D) = "Black"{}
        [NoscaleOffset] _NormalMap("Normal",2D) = "bump"{}

        [Space(20)]

        _Roughness("RoughnessValue",Range(0,1)) = 0.8
        _Metallic("MetallicValue", Range(0, 1)) = 0

        [Space(20)]

        [MaterialToggle] _UseRoughnessMap("UseRoughnessMap",int) = 1
        [MaterialToggle] _UseMetallicMap("UsematerialMap", int) = 0

        [Space(20)]

        _TileScale("TileScale",float) = 1
        _TileX("TileNumber X",float) = 1
        _TileY("TileNumber Y",float) = 1
    }

        SubShader

    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        
        #pragma surface surf Standard fullforwardshadows

        #pragma target 3.0


        struct Input
        {
            float2 uv_BaseColorMap;
        };
        
        sampler2D _BaseColorMap;
        sampler2D _RoughnessMap;
        sampler2D _MetallicMap;
        sampler2D _NormalMap;

        float _Roughness;
        float _Metallic;

        int  _UseRoughnessMap;
        int  _UseMetallicMap;

        float _TileScale;
        float _TileX;
        float _TileY;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float2 tile = _TileScale * float2(_TileX, _TileY);
            float2 uv = IN.uv_BaseColorMap * tile;

            float3 normal = UnpackNormal(tex2D(_NormalMap, uv));

            o.Albedo = tex2D(_BaseColorMap, uv);
            o.Smoothness = (_UseRoughnessMap) ? 1 - tex2D(_RoughnessMap, uv) : _Roughness;
            o.Metallic = (_UseMetallicMap) ? tex2D(_MetallicMap, uv) : _Metallic;
            o.Normal = normal;
        }
        ENDCG
    }

    FallBack "Diffuse"
}
