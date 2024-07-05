#ifndef UNIVERSAL_BAKEDLIT_INPUT_INCLUDED
#define UNIVERSAL_BAKEDLIT_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    half4 _BaseColor;
    half _Cutoff;
    half _Glossiness;
    half _Metallic;
    half _Surface;



//Beast
float _Beast_TessellationFactor;
float _Beast_TessellationMinDistance;
float _Beast_TessellationMaxDistance;
float _Beast_TessellationEdgeLength;
float _Beast_TessellationPhong;
float4 _Beast_TessellationDisplaceMap_TexelSize;
half4 _Beast_TessellationDisplaceMap_ST;
half _Beast_TessellationDisplaceMapUVSet;
int _Beast_TessellationDisplaceMapChannel;
float _Beast_TessellationDisplaceStrength;
float _Beast_TessellationTriplanarUVScale;
float _Beast_TessellationNormalCoef;
float _Beast_TessellationTangentCoef;
float _Beast_TessellationShadowPassLOD;
float _Beast_TessellationDepthPassLOD;
float _Beast_TessellationUseSmoothNormals;

CBUFFER_END

TEXTURE2D(_Beast_TessellationDisplaceMap); SAMPLER(sampler_Beast_TessellationDisplaceMap);


#ifdef UNITY_DOTS_INSTANCING_ENABLED
    UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
        UNITY_DOTS_INSTANCED_PROP(float4, _BaseColor)
        UNITY_DOTS_INSTANCED_PROP(float , _Cutoff)
        UNITY_DOTS_INSTANCED_PROP(float , _Glossiness)
        UNITY_DOTS_INSTANCED_PROP(float , _Metallic)
        UNITY_DOTS_INSTANCED_PROP(float , _Surface)
    UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

    #define _BaseColor          UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4 , Metadata_BaseColor)
    #define _Cutoff             UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata_Cutoff)
    #define _Glossiness         UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata_Glossiness)
    #define _Metallic           UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata_Metallic)
    #define _Surface            UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata_Surface)
#endif

#endif
