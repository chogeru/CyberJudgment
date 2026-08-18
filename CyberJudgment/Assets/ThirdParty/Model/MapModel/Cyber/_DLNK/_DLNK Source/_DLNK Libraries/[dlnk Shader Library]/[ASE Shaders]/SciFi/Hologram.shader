// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "DLNK Shaders/ASE/SciFi/Hologram"
{
	Properties
	{
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		_MainTex("Albedo", 2D) = "white" {}
		_Opacity("Opacity", Float) = 1
		_EmissiveColor("Emissive Color", Color) = (1,1,1,0)
		_URPEmissive("URP Emissive", Float) = 1
		_EmissionIntensity("Emission Intensity", Float) = 1
		_PannerSpeed("Panner Speed", Float) = 0.1
		_Metallic("Metallic", Float) = 0
		_Gloss("Smoothness", Float) = 0
		_ImageNoiseXY("Image Noise (XY)", Vector) = (0,1,0,0)
		[Toggle(_DISPLACEHORIZONTAL_ON)] _DisplaceHorizontal("Displace Horizontal", Float) = 0
		[Toggle(_DISPLACECONTINUOUS_ON)] _DisplaceContinuous("Displace Continuous", Float) = 0
		_HeightMap("Height Map", 2D) = "white" {}
		_SmoothHeightXY("Smooth Height (XY)", Vector) = (0,1,0,0)
		_DepthIntensityXPowerY("Depth IntensityX PowerY", Vector) = (0,0,0,0)
		_StripesXSpeedYThickZRotW("StripesX SpeedY ThickZ RotW", Vector) = (100,1,0.5,90)
		_GlitchScaleXPowerY("Glitch ScaleX PowerY", Vector) = (0,0,0,0)
		_GlitchSmoothXY("Glitch Smooth (XY)", Vector) = (0,1,0,0)
		_GlitchXSpeedYThickZRotW("GlitchX SpeedY ThickZ RotW", Vector) = (100,1,0.5,90)
		[Toggle(_USEGLITCHCOLOR_ON)] _UseGlitchColor("Use Glitch Color", Float) = 0
		_GlitchColor("GlitchColor", Color) = (1,1,1,0)
		[Toggle(_USEGLITCHINTENSITY_ON)] _UseGlitchIntensity("Use Glitch Intensity", Float) = 0
		_GlitchIntensity("Glitch Intensity", Float) = 1


		//_TransmissionShadow( "Transmission Shadow", Range( 0, 1 ) ) = 0.5
		//_TransStrength( "Trans Strength", Range( 0, 50 ) ) = 1
		//_TransNormal( "Trans Normal Distortion", Range( 0, 1 ) ) = 0.5
		//_TransScattering( "Trans Scattering", Range( 1, 50 ) ) = 2
		//_TransDirect( "Trans Direct", Range( 0, 1 ) ) = 0.9
		//_TransAmbient( "Trans Ambient", Range( 0, 1 ) ) = 0.1
		//_TransShadow( "Trans Shadow", Range( 0, 1 ) ) = 0.5
		//_TessPhongStrength( "Tess Phong Strength", Range( 0, 1 ) ) = 0.5
		//_TessValue( "Tess Max Tessellation", Range( 1, 32 ) ) = 16
		//_TessMin( "Tess Min Distance", Float ) = 10
		//_TessMax( "Tess Max Distance", Float ) = 25
		//_TessEdgeLength ( "Tess Edge length", Range( 2, 50 ) ) = 16
		//_TessMaxDisp( "Tess Max Displacement", Float ) = 25

		[HideInInspector][ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
		[HideInInspector][ToggleOff] _EnvironmentReflections("Environment Reflections", Float) = 1.0
		[HideInInspector][ToggleOff] _ReceiveShadows("Receive Shadows", Float) = 1.0

		[HideInInspector] _QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector] _QueueControl("_QueueControl", Float) = -1

        [HideInInspector][NoScaleOffset] unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
	}

	SubShader
	{
		LOD 0

		

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" "UniversalMaterialType"="Lit" }

		Cull Off
		ZWrite Off
		ZTest LEqual
		Offset 0 , 0
		AlphaToMask Off

		

		HLSLINCLUDE
		#pragma target 3.5
		#pragma prefer_hlslcc gles
		// ensure rendering platforms toggle list is visible

		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"

		#ifndef ASE_TESS_FUNCS
		#define ASE_TESS_FUNCS
		float4 FixedTess( float tessValue )
		{
			return tessValue;
		}

		float CalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess, float4x4 o2w, float3 cameraPos )
		{
			float3 wpos = mul(o2w,vertex).xyz;
			float dist = distance (wpos, cameraPos);
			float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
			return f;
		}

		float4 CalcTriEdgeTessFactors (float3 triVertexFactors)
		{
			float4 tess;
			tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
			tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
			tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
			tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
			return tess;
		}

		float CalcEdgeTessFactor (float3 wpos0, float3 wpos1, float edgeLen, float3 cameraPos, float4 scParams )
		{
			float dist = distance (0.5 * (wpos0+wpos1), cameraPos);
			float len = distance(wpos0, wpos1);
			float f = max(len * scParams.y / (edgeLen * dist), 1.0);
			return f;
		}

		float DistanceFromPlane (float3 pos, float4 plane)
		{
			float d = dot (float4(pos,1.0f), plane);
			return d;
		}

		bool WorldViewFrustumCull (float3 wpos0, float3 wpos1, float3 wpos2, float cullEps, float4 planes[6] )
		{
			float4 planeTest;
			planeTest.x = (( DistanceFromPlane(wpos0, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[0]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.y = (( DistanceFromPlane(wpos0, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[1]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.z = (( DistanceFromPlane(wpos0, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[2]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.w = (( DistanceFromPlane(wpos0, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[3]) > -cullEps) ? 1.0f : 0.0f );
			return !all (planeTest);
		}

		float4 DistanceBasedTess( float4 v0, float4 v1, float4 v2, float tess, float minDist, float maxDist, float4x4 o2w, float3 cameraPos )
		{
			float3 f;
			f.x = CalcDistanceTessFactor (v0,minDist,maxDist,tess,o2w,cameraPos);
			f.y = CalcDistanceTessFactor (v1,minDist,maxDist,tess,o2w,cameraPos);
			f.z = CalcDistanceTessFactor (v2,minDist,maxDist,tess,o2w,cameraPos);

			return CalcTriEdgeTessFactors (f);
		}

		float4 EdgeLengthBasedTess( float4 v0, float4 v1, float4 v2, float edgeLength, float4x4 o2w, float3 cameraPos, float4 scParams )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;
			tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
			tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
			tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
			tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			return tess;
		}

		float4 EdgeLengthBasedTessCull( float4 v0, float4 v1, float4 v2, float edgeLength, float maxDisplacement, float4x4 o2w, float3 cameraPos, float4 scParams, float4 planes[6] )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;

			if (WorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement, planes))
			{
				tess = 0.0f;
			}
			else
			{
				tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
				tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
				tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
				tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			}
			return tess;
		}
		#endif //ASE_TESS_FUNCS
		ENDHLSL

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }

			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZWrite Off
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA

			

			HLSLPROGRAM

			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma instancing_options renderinglayer
			#pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _SURFACE_TYPE_TRANSPARENT 1
			#define _EMISSION
			#define ASE_SRP_VERSION 120112


			#pragma shader_feature_local _RECEIVE_SHADOWS_OFF
			#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
			#pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF

			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
			#pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
			#pragma multi_compile_fragment _ _SHADOWS_SOFT
			#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
			#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
			#pragma multi_compile_fragment _ _LIGHT_LAYERS
			#pragma multi_compile_fragment _ _LIGHT_COOKIES
			#pragma multi_compile _ _CLUSTERED_RENDERING

            #pragma multi_compile _ DOTS_INSTANCING_ON

			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
			#pragma multi_compile _ SHADOWS_SHADOWMASK
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile _ DYNAMICLIGHTMAP_ON
			#pragma multi_compile_fragment _ DEBUG_DISPLAY

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_FORWARD

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#if defined(UNITY_INSTANCING_ENABLED) && defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL)
				#define ENABLE_TERRAIN_PERPIXEL_NORMAL
			#endif

			#pragma shader_feature_local _DISPLACEHORIZONTAL_ON
			#pragma shader_feature_local _DISPLACECONTINUOUS_ON
			#pragma shader_feature_local _USEGLITCHINTENSITY_ON
			#pragma shader_feature_local _USEGLITCHCOLOR_ON


			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float4 clipPosV : TEXCOORD0;
				float4 lightmapUVOrVertexSH : TEXCOORD1;
				half4 fogFactorAndVertexLight : TEXCOORD2;
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					float4 shadowCoord : TEXCOORD6;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON)
					float2 dynamicLightmapUV : TEXCOORD7;
				#endif
				float4 ase_texcoord8 : TEXCOORD8;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _GlitchXSpeedYThickZRotW;
			float4 _EmissiveColor;
			float4 _GlitchColor;
			float4 _StripesXSpeedYThickZRotW;
			float2 _SmoothHeightXY;
			float2 _DepthIntensityXPowerY;
			float2 _GlitchSmoothXY;
			float2 _GlitchScaleXPowerY;
			float2 _ImageNoiseXY;
			float _PannerSpeed;
			float _EmissionIntensity;
			float _GlitchIntensity;
			float _URPEmissive;
			float _Metallic;
			float _Opacity;
			float _Gloss;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			sampler2D _HeightMap;
			sampler2D _MainTex;


			float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }
			float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }
			float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }
			float snoise( float2 v )
			{
				const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
				float2 i = floor( v + dot( v, C.yy ) );
				float2 x0 = v - i + dot( i, C.xx );
				float2 i1;
				i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
				float4 x12 = x0.xyxy + C.xxzz;
				x12.xy -= i1;
				i = mod2D289( i );
				float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
				float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
				m = m * m;
				m = m * m;
				float3 x = 2.0 * frac( p * C.www ) - 1.0;
				float3 h = abs( x ) - 0.5;
				float3 ox = floor( x + 0.5 );
				float3 a0 = x - ox;
				m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
				float3 g;
				g.x = a0.x * x0.x + h.x * x0.y;
				g.yz = a0.yz * x12.xz + h.yz * x12.yw;
				return 130.0 * dot( m, g );
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float2 texCoord81 = v.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				#ifdef _DISPLACECONTINUOUS_ON
				float staticSwitch83 = _TimeParameters.x;
				#else
				float staticSwitch83 = cos( _TimeParameters.x * 0.5 );
				#endif
				float2 temp_cast_0 = (_PannerSpeed).xx;
				float2 temp_cast_1 = (texCoord81.y).xx;
				float2 panner84 = ( staticSwitch83 * temp_cast_0 + temp_cast_1);
				float2 appendResult86 = (float2(texCoord81.x , panner84.x));
				float2 temp_cast_3 = (_PannerSpeed).xx;
				float2 temp_cast_4 = (texCoord81.x).xx;
				float2 panner85 = ( staticSwitch83 * temp_cast_3 + temp_cast_4);
				float2 appendResult87 = (float2(panner85.x , texCoord81.y));
				#ifdef _DISPLACEHORIZONTAL_ON
				float2 staticSwitch88 = appendResult87;
				#else
				float2 staticSwitch88 = appendResult86;
				#endif
				float smoothstepResult21 = smoothstep( _SmoothHeightXY.x , _SmoothHeightXY.y , tex2Dlod( _HeightMap, float4( staticSwitch88, 0, 0.0) ).r);
				float cos10_g11 = cos( radians( _GlitchXSpeedYThickZRotW.w ) );
				float sin10_g11 = sin( radians( _GlitchXSpeedYThickZRotW.w ) );
				float2 rotator10_g11 = mul( v.texcoord.xy - float2( 0.5,0.5 ) , float2x2( cos10_g11 , -sin10_g11 , sin10_g11 , cos10_g11 )) + float2( 0.5,0.5 );
				float2 appendResult8_g11 = (float2(_GlitchXSpeedYThickZRotW.x , 1.0));
				float mulTime66 = _TimeParameters.x * _GlitchXSpeedYThickZRotW.y;
				float2 appendResult9_g11 = (float2(mulTime66 , 0.0));
				float2 appendResult10_g12 = (float2(_GlitchXSpeedYThickZRotW.z , 1.0));
				float2 temp_output_11_0_g12 = ( abs( (frac( (rotator10_g11*appendResult8_g11 + appendResult9_g11) )*2.0 + -1.0) ) - appendResult10_g12 );
				float2 break16_g12 = ( 1.0 - ( temp_output_11_0_g12 / (0).xx ) );
				float smoothstepResult71 = smoothstep( _GlitchSmoothXY.x , _GlitchSmoothXY.y , saturate( min( break16_g12.x , break16_g12.y ) ));
				float2 temp_cast_6 = (_PannerSpeed).xx;
				float2 panner58 = ( cos( _TimeParameters.x * 0.5 ) * temp_cast_6 + texCoord81);
				float simplePerlin2D63 = snoise( panner58*_GlitchScaleXPowerY.x );
				simplePerlin2D63 = simplePerlin2D63*0.5 + 0.5;
				float3 appendResult52 = (float3(0.0 , ( pow( smoothstepResult21 , _DepthIntensityXPowerY.y ) * _DepthIntensityXPowerY.x ) , ( smoothstepResult71 * simplePerlin2D63 * _GlitchScaleXPowerY.y )));
				
				o.ase_texcoord8.xy = v.texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord8.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = appendResult52;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif
				v.normalOS = v.normalOS;
				v.tangentOS = v.tangentOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( v.positionOS.xyz );
				VertexNormalInputs normalInput = GetVertexNormalInputs( v.normalOS, v.tangentOS );

				o.tSpace0 = float4( normalInput.normalWS, vertexInput.positionWS.x );
				o.tSpace1 = float4( normalInput.tangentWS, vertexInput.positionWS.y );
				o.tSpace2 = float4( normalInput.bitangentWS, vertexInput.positionWS.z );

				#if defined(LIGHTMAP_ON)
					OUTPUT_LIGHTMAP_UV( v.texcoord1, unity_LightmapST, o.lightmapUVOrVertexSH.xy );
				#endif

				#if !defined(LIGHTMAP_ON)
					OUTPUT_SH( normalInput.normalWS.xyz, o.lightmapUVOrVertexSH.xyz );
				#endif

				#if defined(DYNAMICLIGHTMAP_ON)
					o.dynamicLightmapUV.xy = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#endif

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					o.lightmapUVOrVertexSH.zw = v.texcoord.xy;
					o.lightmapUVOrVertexSH.xy = v.texcoord.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif

				half3 vertexLight = VertexLighting( vertexInput.positionWS, normalInput.normalWS );

				#ifdef ASE_FOG
					half fogFactor = ComputeFogFactor( vertexInput.positionCS.z );
				#else
					half fogFactor = 0;
				#endif

				o.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.positionCS = vertexInput.positionCS;
				o.clipPosV = vertexInput.positionCS;
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				o.tangentOS = v.tangentOS;
				o.texcoord = v.texcoord;
				o.texcoord1 = v.texcoord1;
				o.texcoord2 = v.texcoord2;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				o.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				o.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				o.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag ( VertexOutput IN
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.positionCS.xyz, unity_LODFade.x );
				#endif

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float2 sampleCoords = (IN.lightmapUVOrVertexSH.zw / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
					float3 WorldNormal = TransformObjectToWorldNormal(normalize(SAMPLE_TEXTURE2D(_TerrainNormalmapTexture, sampler_TerrainNormalmapTexture, sampleCoords).rgb * 2 - 1));
					float3 WorldTangent = -cross(GetObjectToWorldMatrix()._13_23_33, WorldNormal);
					float3 WorldBiTangent = cross(WorldNormal, -WorldTangent);
				#else
					float3 WorldNormal = normalize( IN.tSpace0.xyz );
					float3 WorldTangent = IN.tSpace1.xyz;
					float3 WorldBiTangent = IN.tSpace2.xyz;
				#endif

				float3 WorldPosition = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
				float3 WorldViewDirection = _WorldSpaceCameraPos.xyz  - WorldPosition;
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				float4 ClipPos = IN.clipPosV;
				float4 ScreenPos = ComputeScreenPos( IN.clipPosV );

				float2 NormalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.positionCS);

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					ShadowCoords = IN.shadowCoord;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
				#endif

				WorldViewDirection = SafeNormalize( WorldViewDirection );

				float2 break19_g15 = _ImageNoiseXY;
				float2 texCoord81 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				#ifdef _DISPLACECONTINUOUS_ON
				float staticSwitch83 = _TimeParameters.x;
				#else
				float staticSwitch83 = cos( _TimeParameters.x * 0.5 );
				#endif
				float2 temp_cast_0 = (_PannerSpeed).xx;
				float2 temp_cast_1 = (texCoord81.y).xx;
				float2 panner84 = ( staticSwitch83 * temp_cast_0 + temp_cast_1);
				float2 appendResult86 = (float2(texCoord81.x , panner84.x));
				float2 temp_cast_3 = (_PannerSpeed).xx;
				float2 temp_cast_4 = (texCoord81.x).xx;
				float2 panner85 = ( staticSwitch83 * temp_cast_3 + temp_cast_4);
				float2 appendResult87 = (float2(panner85.x , texCoord81.y));
				#ifdef _DISPLACEHORIZONTAL_ON
				float2 staticSwitch88 = appendResult87;
				#else
				float2 staticSwitch88 = appendResult86;
				#endif
				float4 tex2DNode1 = tex2D( _MainTex, staticSwitch88 );
				float4 temp_output_1_0_g15 = tex2DNode1;
				float4 sinIn7_g15 = sin( temp_output_1_0_g15 );
				float4 sinInOffset6_g15 = sin( ( temp_output_1_0_g15 + 1.0 ) );
				float lerpResult20_g15 = lerp( break19_g15.x , break19_g15.y , frac( ( sin( ( ( sinIn7_g15 - sinInOffset6_g15 ) * 91.2228 ) ) * 43758.55 ) ).r);
				
				float cos10_g11 = cos( radians( _GlitchXSpeedYThickZRotW.w ) );
				float sin10_g11 = sin( radians( _GlitchXSpeedYThickZRotW.w ) );
				float2 rotator10_g11 = mul( IN.ase_texcoord8.xy - float2( 0.5,0.5 ) , float2x2( cos10_g11 , -sin10_g11 , sin10_g11 , cos10_g11 )) + float2( 0.5,0.5 );
				float2 appendResult8_g11 = (float2(_GlitchXSpeedYThickZRotW.x , 1.0));
				float mulTime66 = _TimeParameters.x * _GlitchXSpeedYThickZRotW.y;
				float2 appendResult9_g11 = (float2(mulTime66 , 0.0));
				float2 appendResult10_g12 = (float2(_GlitchXSpeedYThickZRotW.z , 1.0));
				float2 temp_output_11_0_g12 = ( abs( (frac( (rotator10_g11*appendResult8_g11 + appendResult9_g11) )*2.0 + -1.0) ) - appendResult10_g12 );
				float2 break16_g12 = ( 1.0 - ( temp_output_11_0_g12 / fwidth( temp_output_11_0_g12 ) ) );
				float smoothstepResult71 = smoothstep( _GlitchSmoothXY.x , _GlitchSmoothXY.y , saturate( min( break16_g12.x , break16_g12.y ) ));
				float lerpResult77 = lerp( _EmissionIntensity , _GlitchIntensity , smoothstepResult71);
				#ifdef _USEGLITCHINTENSITY_ON
				float staticSwitch78 = lerpResult77;
				#else
				float staticSwitch78 = _EmissionIntensity;
				#endif
				float4 lerpResult73 = lerp( _EmissiveColor , _GlitchColor , smoothstepResult71);
				#ifdef _USEGLITCHCOLOR_ON
				float4 staticSwitch74 = lerpResult73;
				#else
				float4 staticSwitch74 = _EmissiveColor;
				#endif
				
				float cos10_g13 = cos( radians( _StripesXSpeedYThickZRotW.w ) );
				float sin10_g13 = sin( radians( _StripesXSpeedYThickZRotW.w ) );
				float2 rotator10_g13 = mul( IN.ase_texcoord8.xy - float2( 0.5,0.5 ) , float2x2( cos10_g13 , -sin10_g13 , sin10_g13 , cos10_g13 )) + float2( 0.5,0.5 );
				float2 appendResult8_g13 = (float2(_StripesXSpeedYThickZRotW.x , 1.0));
				float mulTime25 = _TimeParameters.x * _StripesXSpeedYThickZRotW.y;
				float2 appendResult9_g13 = (float2(mulTime25 , 0.0));
				float2 appendResult10_g14 = (float2(_StripesXSpeedYThickZRotW.z , 1.0));
				float2 temp_output_11_0_g14 = ( abs( (frac( (rotator10_g13*appendResult8_g13 + appendResult9_g13) )*2.0 + -1.0) ) - appendResult10_g14 );
				float2 break16_g14 = ( 1.0 - ( temp_output_11_0_g14 / fwidth( temp_output_11_0_g14 ) ) );
				float temp_output_20_0 = ( tex2DNode1.a * saturate( min( break16_g14.x , break16_g14.y ) ) * _Opacity );
				

				float3 BaseColor = ( lerpResult20_g15 + sinIn7_g15 ).rgb;
				float3 Normal = float3(0, 0, 1);
				float3 Emission = ( tex2DNode1 * staticSwitch78 * staticSwitch74 * _URPEmissive ).rgb;
				float3 Specular = 0.5;
				float Metallic = _Metallic;
				float Smoothness = ( temp_output_20_0 * _Gloss );
				float Occlusion = 1;
				float Alpha = temp_output_20_0;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;
				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = 1;
				float3 Translucency = 1;

				#ifdef ASE_DEPTH_WRITE_ON
					float DepthValue = IN.positionCS.z;
				#endif

				#ifdef _CLEARCOAT
					float CoatMask = 0;
					float CoatSmoothness = 0;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				InputData inputData = (InputData)0;
				inputData.positionWS = WorldPosition;
				inputData.viewDirectionWS = WorldViewDirection;

				#ifdef _NORMALMAP
						#if _NORMAL_DROPOFF_TS
							inputData.normalWS = TransformTangentToWorld(Normal, half3x3(WorldTangent, WorldBiTangent, WorldNormal));
						#elif _NORMAL_DROPOFF_OS
							inputData.normalWS = TransformObjectToWorldNormal(Normal);
						#elif _NORMAL_DROPOFF_WS
							inputData.normalWS = Normal;
						#endif
					inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
				#else
					inputData.normalWS = WorldNormal;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					inputData.shadowCoord = ShadowCoords;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
				#else
					inputData.shadowCoord = float4(0, 0, 0, 0);
				#endif

				#ifdef ASE_FOG
					inputData.fogCoord = IN.fogFactorAndVertexLight.x;
				#endif
					inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float3 SH = SampleSH(inputData.normalWS.xyz);
				#else
					float3 SH = IN.lightmapUVOrVertexSH.xyz;
				#endif

				#if defined(DYNAMICLIGHTMAP_ON)
					inputData.bakedGI = SAMPLE_GI(IN.lightmapUVOrVertexSH.xy, IN.dynamicLightmapUV.xy, SH, inputData.normalWS);
				#else
					inputData.bakedGI = SAMPLE_GI(IN.lightmapUVOrVertexSH.xy, SH, inputData.normalWS);
				#endif

				#ifdef ASE_BAKEDGI
					inputData.bakedGI = BakedGI;
				#endif

				inputData.normalizedScreenSpaceUV = NormalizedScreenSpaceUV;
				inputData.shadowMask = SAMPLE_SHADOWMASK(IN.lightmapUVOrVertexSH.xy);

				#if defined(DEBUG_DISPLAY)
					#if defined(DYNAMICLIGHTMAP_ON)
						inputData.dynamicLightmapUV = IN.dynamicLightmapUV.xy;
					#endif
					#if defined(LIGHTMAP_ON)
						inputData.staticLightmapUV = IN.lightmapUVOrVertexSH.xy;
					#else
						inputData.vertexSH = SH;
					#endif
				#endif

				SurfaceData surfaceData;
				surfaceData.albedo              = BaseColor;
				surfaceData.metallic            = saturate(Metallic);
				surfaceData.specular            = Specular;
				surfaceData.smoothness          = saturate(Smoothness),
				surfaceData.occlusion           = Occlusion,
				surfaceData.emission            = Emission,
				surfaceData.alpha               = saturate(Alpha);
				surfaceData.normalTS            = Normal;
				surfaceData.clearCoatMask       = 0;
				surfaceData.clearCoatSmoothness = 1;

				#ifdef _CLEARCOAT
					surfaceData.clearCoatMask       = saturate(CoatMask);
					surfaceData.clearCoatSmoothness = saturate(CoatSmoothness);
				#endif

				#ifdef _DBUFFER
					ApplyDecalToSurfaceData(IN.positionCS, surfaceData, inputData);
				#endif

				half4 color = UniversalFragmentPBR( inputData, surfaceData);

				#ifdef ASE_TRANSMISSION
				{
					float shadow = _TransmissionShadow;

					Light mainLight = GetMainLight( inputData.shadowCoord );
					float3 mainAtten = mainLight.color * mainLight.distanceAttenuation;
					mainAtten = lerp( mainAtten, mainAtten * mainLight.shadowAttenuation, shadow );
					half3 mainTransmission = max(0 , -dot(inputData.normalWS, mainLight.direction)) * mainAtten * Transmission;
					color.rgb += BaseColor * mainTransmission;

					#ifdef _ADDITIONAL_LIGHTS
						int transPixelLightCount = GetAdditionalLightsCount();
						for (int i = 0; i < transPixelLightCount; ++i)
						{
							Light light = GetAdditionalLight(i, inputData.positionWS);
							float3 atten = light.color * light.distanceAttenuation;
							atten = lerp( atten, atten * light.shadowAttenuation, shadow );

							half3 transmission = max(0 , -dot(inputData.normalWS, light.direction)) * atten * Transmission;
							color.rgb += BaseColor * transmission;
						}
					#endif
				}
				#endif

				#ifdef ASE_TRANSLUCENCY
				{
					float shadow = _TransShadow;
					float normal = _TransNormal;
					float scattering = _TransScattering;
					float direct = _TransDirect;
					float ambient = _TransAmbient;
					float strength = _TransStrength;

					Light mainLight = GetMainLight( inputData.shadowCoord );
					float3 mainAtten = mainLight.color * mainLight.distanceAttenuation;
					mainAtten = lerp( mainAtten, mainAtten * mainLight.shadowAttenuation, shadow );

					half3 mainLightDir = mainLight.direction + inputData.normalWS * normal;
					half mainVdotL = pow( saturate( dot( inputData.viewDirectionWS, -mainLightDir ) ), scattering );
					half3 mainTranslucency = mainAtten * ( mainVdotL * direct + inputData.bakedGI * ambient ) * Translucency;
					color.rgb += BaseColor * mainTranslucency * strength;

					#ifdef _ADDITIONAL_LIGHTS
						int transPixelLightCount = GetAdditionalLightsCount();
						for (int i = 0; i < transPixelLightCount; ++i)
						{
							Light light = GetAdditionalLight(i, inputData.positionWS);
							float3 atten = light.color * light.distanceAttenuation;
							atten = lerp( atten, atten * light.shadowAttenuation, shadow );

							half3 lightDir = light.direction + inputData.normalWS * normal;
							half VdotL = pow( saturate( dot( inputData.viewDirectionWS, -lightDir ) ), scattering );
							half3 translucency = atten * ( VdotL * direct + inputData.bakedGI * ambient ) * Translucency;
							color.rgb += BaseColor * translucency * strength;
						}
					#endif
				}
				#endif

				#ifdef ASE_REFRACTION
					float4 projScreenPos = ScreenPos / ScreenPos.w;
					float3 refractionOffset = ( RefractionIndex - 1.0 ) * mul( UNITY_MATRIX_V, float4( WorldNormal,0 ) ).xyz * ( 1.0 - dot( WorldNormal, WorldViewDirection ) );
					projScreenPos.xy += refractionOffset.xy;
					float3 refraction = SHADERGRAPH_SAMPLE_SCENE_COLOR( projScreenPos.xy ) * RefractionColor;
					color.rgb = lerp( refraction, color.rgb, color.a );
					color.a = 1;
				#endif

				#ifdef ASE_FINAL_COLOR_ALPHA_MULTIPLY
					color.rgb *= color.a;
				#endif

				#ifdef ASE_FOG
					#ifdef TERRAIN_SPLAT_ADDPASS
						color.rgb = MixFogColor(color.rgb, half3( 0, 0, 0 ), IN.fogFactorAndVertexLight.x );
					#else
						color.rgb = MixFog(color.rgb, IN.fogFactorAndVertexLight.x);
					#endif
				#endif

				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif

				return color;
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ZWrite On
			ColorMask 0
			AlphaToMask Off

			HLSLPROGRAM

            #define _NORMAL_DROPOFF_TS 1
            #pragma multi_compile_instancing
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
            #define ASE_FOG 1
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define _EMISSION
            #define ASE_SRP_VERSION 120112


            #pragma multi_compile _ DOTS_INSTANCING_ON

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_DEPTHONLY

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#pragma shader_feature_local _DISPLACEHORIZONTAL_ON
			#pragma shader_feature_local _DISPLACECONTINUOUS_ON


			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float4 clipPosV : TEXCOORD0;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 positionWS : TEXCOORD1;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD2;
				#endif
				float4 ase_texcoord3 : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _GlitchXSpeedYThickZRotW;
			float4 _EmissiveColor;
			float4 _GlitchColor;
			float4 _StripesXSpeedYThickZRotW;
			float2 _SmoothHeightXY;
			float2 _DepthIntensityXPowerY;
			float2 _GlitchSmoothXY;
			float2 _GlitchScaleXPowerY;
			float2 _ImageNoiseXY;
			float _PannerSpeed;
			float _EmissionIntensity;
			float _GlitchIntensity;
			float _URPEmissive;
			float _Metallic;
			float _Opacity;
			float _Gloss;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			sampler2D _HeightMap;
			sampler2D _MainTex;


			float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }
			float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }
			float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }
			float snoise( float2 v )
			{
				const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
				float2 i = floor( v + dot( v, C.yy ) );
				float2 x0 = v - i + dot( i, C.xx );
				float2 i1;
				i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
				float4 x12 = x0.xyxy + C.xxzz;
				x12.xy -= i1;
				i = mod2D289( i );
				float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
				float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
				m = m * m;
				m = m * m;
				float3 x = 2.0 * frac( p * C.www ) - 1.0;
				float3 h = abs( x ) - 0.5;
				float3 ox = floor( x + 0.5 );
				float3 a0 = x - ox;
				m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
				float3 g;
				g.x = a0.x * x0.x + h.x * x0.y;
				g.yz = a0.yz * x12.xz + h.yz * x12.yw;
				return 130.0 * dot( m, g );
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float2 texCoord81 = v.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				#ifdef _DISPLACECONTINUOUS_ON
				float staticSwitch83 = _TimeParameters.x;
				#else
				float staticSwitch83 = cos( _TimeParameters.x * 0.5 );
				#endif
				float2 temp_cast_0 = (_PannerSpeed).xx;
				float2 temp_cast_1 = (texCoord81.y).xx;
				float2 panner84 = ( staticSwitch83 * temp_cast_0 + temp_cast_1);
				float2 appendResult86 = (float2(texCoord81.x , panner84.x));
				float2 temp_cast_3 = (_PannerSpeed).xx;
				float2 temp_cast_4 = (texCoord81.x).xx;
				float2 panner85 = ( staticSwitch83 * temp_cast_3 + temp_cast_4);
				float2 appendResult87 = (float2(panner85.x , texCoord81.y));
				#ifdef _DISPLACEHORIZONTAL_ON
				float2 staticSwitch88 = appendResult87;
				#else
				float2 staticSwitch88 = appendResult86;
				#endif
				float smoothstepResult21 = smoothstep( _SmoothHeightXY.x , _SmoothHeightXY.y , tex2Dlod( _HeightMap, float4( staticSwitch88, 0, 0.0) ).r);
				float cos10_g11 = cos( radians( _GlitchXSpeedYThickZRotW.w ) );
				float sin10_g11 = sin( radians( _GlitchXSpeedYThickZRotW.w ) );
				float2 rotator10_g11 = mul( v.ase_texcoord.xy - float2( 0.5,0.5 ) , float2x2( cos10_g11 , -sin10_g11 , sin10_g11 , cos10_g11 )) + float2( 0.5,0.5 );
				float2 appendResult8_g11 = (float2(_GlitchXSpeedYThickZRotW.x , 1.0));
				float mulTime66 = _TimeParameters.x * _GlitchXSpeedYThickZRotW.y;
				float2 appendResult9_g11 = (float2(mulTime66 , 0.0));
				float2 appendResult10_g12 = (float2(_GlitchXSpeedYThickZRotW.z , 1.0));
				float2 temp_output_11_0_g12 = ( abs( (frac( (rotator10_g11*appendResult8_g11 + appendResult9_g11) )*2.0 + -1.0) ) - appendResult10_g12 );
				float2 break16_g12 = ( 1.0 - ( temp_output_11_0_g12 / (0).xx ) );
				float smoothstepResult71 = smoothstep( _GlitchSmoothXY.x , _GlitchSmoothXY.y , saturate( min( break16_g12.x , break16_g12.y ) ));
				float2 temp_cast_6 = (_PannerSpeed).xx;
				float2 panner58 = ( cos( _TimeParameters.x * 0.5 ) * temp_cast_6 + texCoord81);
				float simplePerlin2D63 = snoise( panner58*_GlitchScaleXPowerY.x );
				simplePerlin2D63 = simplePerlin2D63*0.5 + 0.5;
				float3 appendResult52 = (float3(0.0 , ( pow( smoothstepResult21 , _DepthIntensityXPowerY.y ) * _DepthIntensityXPowerY.x ) , ( smoothstepResult71 * simplePerlin2D63 * _GlitchScaleXPowerY.y )));
				
				o.ase_texcoord3.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = appendResult52;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif

				v.normalOS = v.normalOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( v.positionOS.xyz );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					o.positionWS = vertexInput.positionWS;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.positionCS = vertexInput.positionCS;
				o.clipPosV = vertexInput.positionCS;
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(	VertexOutput IN
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.positionWS;
				#endif

				float4 ShadowCoords = float4( 0, 0, 0, 0 );
				float4 ClipPos = IN.clipPosV;
				float4 ScreenPos = ComputeScreenPos( IN.clipPosV );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float2 texCoord81 = IN.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				#ifdef _DISPLACECONTINUOUS_ON
				float staticSwitch83 = _TimeParameters.x;
				#else
				float staticSwitch83 = cos( _TimeParameters.x * 0.5 );
				#endif
				float2 temp_cast_0 = (_PannerSpeed).xx;
				float2 temp_cast_1 = (texCoord81.y).xx;
				float2 panner84 = ( staticSwitch83 * temp_cast_0 + temp_cast_1);
				float2 appendResult86 = (float2(texCoord81.x , panner84.x));
				float2 temp_cast_3 = (_PannerSpeed).xx;
				float2 temp_cast_4 = (texCoord81.x).xx;
				float2 panner85 = ( staticSwitch83 * temp_cast_3 + temp_cast_4);
				float2 appendResult87 = (float2(panner85.x , texCoord81.y));
				#ifdef _DISPLACEHORIZONTAL_ON
				float2 staticSwitch88 = appendResult87;
				#else
				float2 staticSwitch88 = appendResult86;
				#endif
				float4 tex2DNode1 = tex2D( _MainTex, staticSwitch88 );
				float cos10_g13 = cos( radians( _StripesXSpeedYThickZRotW.w ) );
				float sin10_g13 = sin( radians( _StripesXSpeedYThickZRotW.w ) );
				float2 rotator10_g13 = mul( IN.ase_texcoord3.xy - float2( 0.5,0.5 ) , float2x2( cos10_g13 , -sin10_g13 , sin10_g13 , cos10_g13 )) + float2( 0.5,0.5 );
				float2 appendResult8_g13 = (float2(_StripesXSpeedYThickZRotW.x , 1.0));
				float mulTime25 = _TimeParameters.x * _StripesXSpeedYThickZRotW.y;
				float2 appendResult9_g13 = (float2(mulTime25 , 0.0));
				float2 appendResult10_g14 = (float2(_StripesXSpeedYThickZRotW.z , 1.0));
				float2 temp_output_11_0_g14 = ( abs( (frac( (rotator10_g13*appendResult8_g13 + appendResult9_g13) )*2.0 + -1.0) ) - appendResult10_g14 );
				float2 break16_g14 = ( 1.0 - ( temp_output_11_0_g14 / fwidth( temp_output_11_0_g14 ) ) );
				float temp_output_20_0 = ( tex2DNode1.a * saturate( min( break16_g14.x , break16_g14.y ) ) * _Opacity );
				

				float Alpha = temp_output_20_0;
				float AlphaClipThreshold = 0.5;

				#ifdef ASE_DEPTH_WRITE_ON
					float DepthValue = IN.positionCS.z;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.positionCS.xyz, unity_LODFade.x );
				#endif

				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif

				return 0;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "Meta"
			Tags { "LightMode"="Meta" }

			Cull Off

			HLSLPROGRAM

			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#define _SURFACE_TYPE_TRANSPARENT 1
			#define _EMISSION
			#define ASE_SRP_VERSION 120112


			#pragma vertex vert
			#pragma fragment frag

			#pragma shader_feature EDITOR_VISUALIZATION

			#define SHADERPASS SHADERPASS_META

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#pragma shader_feature_local _DISPLACEHORIZONTAL_ON
			#pragma shader_feature_local _DISPLACECONTINUOUS_ON
			#pragma shader_feature_local _USEGLITCHINTENSITY_ON
			#pragma shader_feature_local _USEGLITCHCOLOR_ON


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 texcoord0 : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 positionWS : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord : TEXCOORD1;
				#endif
				#ifdef EDITOR_VISUALIZATION
					float4 VizUV : TEXCOORD2;
					float4 LightCoord : TEXCOORD3;
				#endif
				float4 ase_texcoord4 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _GlitchXSpeedYThickZRotW;
			float4 _EmissiveColor;
			float4 _GlitchColor;
			float4 _StripesXSpeedYThickZRotW;
			float2 _SmoothHeightXY;
			float2 _DepthIntensityXPowerY;
			float2 _GlitchSmoothXY;
			float2 _GlitchScaleXPowerY;
			float2 _ImageNoiseXY;
			float _PannerSpeed;
			float _EmissionIntensity;
			float _GlitchIntensity;
			float _URPEmissive;
			float _Metallic;
			float _Opacity;
			float _Gloss;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			sampler2D _HeightMap;
			sampler2D _MainTex;


			float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }
			float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }
			float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }
			float snoise( float2 v )
			{
				const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
				float2 i = floor( v + dot( v, C.yy ) );
				float2 x0 = v - i + dot( i, C.xx );
				float2 i1;
				i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
				float4 x12 = x0.xyxy + C.xxzz;
				x12.xy -= i1;
				i = mod2D289( i );
				float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
				float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
				m = m * m;
				m = m * m;
				float3 x = 2.0 * frac( p * C.www ) - 1.0;
				float3 h = abs( x ) - 0.5;
				float3 ox = floor( x + 0.5 );
				float3 a0 = x - ox;
				m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
				float3 g;
				g.x = a0.x * x0.x + h.x * x0.y;
				g.yz = a0.yz * x12.xz + h.yz * x12.yw;
				return 130.0 * dot( m, g );
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float2 texCoord81 = v.texcoord0.xy * float2( 1,1 ) + float2( 0,0 );
				#ifdef _DISPLACECONTINUOUS_ON
				float staticSwitch83 = _TimeParameters.x;
				#else
				float staticSwitch83 = cos( _TimeParameters.x * 0.5 );
				#endif
				float2 temp_cast_0 = (_PannerSpeed).xx;
				float2 temp_cast_1 = (texCoord81.y).xx;
				float2 panner84 = ( staticSwitch83 * temp_cast_0 + temp_cast_1);
				float2 appendResult86 = (float2(texCoord81.x , panner84.x));
				float2 temp_cast_3 = (_PannerSpeed).xx;
				float2 temp_cast_4 = (texCoord81.x).xx;
				float2 panner85 = ( staticSwitch83 * temp_cast_3 + temp_cast_4);
				float2 appendResult87 = (float2(panner85.x , texCoord81.y));
				#ifdef _DISPLACEHORIZONTAL_ON
				float2 staticSwitch88 = appendResult87;
				#else
				float2 staticSwitch88 = appendResult86;
				#endif
				float smoothstepResult21 = smoothstep( _SmoothHeightXY.x , _SmoothHeightXY.y , tex2Dlod( _HeightMap, float4( staticSwitch88, 0, 0.0) ).r);
				float cos10_g11 = cos( radians( _GlitchXSpeedYThickZRotW.w ) );
				float sin10_g11 = sin( radians( _GlitchXSpeedYThickZRotW.w ) );
				float2 rotator10_g11 = mul( v.texcoord0.xy - float2( 0.5,0.5 ) , float2x2( cos10_g11 , -sin10_g11 , sin10_g11 , cos10_g11 )) + float2( 0.5,0.5 );
				float2 appendResult8_g11 = (float2(_GlitchXSpeedYThickZRotW.x , 1.0));
				float mulTime66 = _TimeParameters.x * _GlitchXSpeedYThickZRotW.y;
				float2 appendResult9_g11 = (float2(mulTime66 , 0.0));
				float2 appendResult10_g12 = (float2(_GlitchXSpeedYThickZRotW.z , 1.0));
				float2 temp_output_11_0_g12 = ( abs( (frac( (rotator10_g11*appendResult8_g11 + appendResult9_g11) )*2.0 + -1.0) ) - appendResult10_g12 );
				float2 break16_g12 = ( 1.0 - ( temp_output_11_0_g12 / (0).xx ) );
				float smoothstepResult71 = smoothstep( _GlitchSmoothXY.x , _GlitchSmoothXY.y , saturate( min( break16_g12.x , break16_g12.y ) ));
				float2 temp_cast_6 = (_PannerSpeed).xx;
				float2 panner58 = ( cos( _TimeParameters.x * 0.5 ) * temp_cast_6 + texCoord81);
				float simplePerlin2D63 = snoise( panner58*_GlitchScaleXPowerY.x );
				simplePerlin2D63 = simplePerlin2D63*0.5 + 0.5;
				float3 appendResult52 = (float3(0.0 , ( pow( smoothstepResult21 , _DepthIntensityXPowerY.y ) * _DepthIntensityXPowerY.x ) , ( smoothstepResult71 * simplePerlin2D63 * _GlitchScaleXPowerY.y )));
				
				o.ase_texcoord4.xy = v.texcoord0.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord4.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = appendResult52;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif

				v.normalOS = v.normalOS;

				float3 positionWS = TransformObjectToWorld( v.positionOS.xyz );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					o.positionWS = positionWS;
				#endif

				o.positionCS = MetaVertexPosition( v.positionOS, v.texcoord1.xy, v.texcoord1.xy, unity_LightmapST, unity_DynamicLightmapST );

				#ifdef EDITOR_VISUALIZATION
					float2 VizUV = 0;
					float4 LightCoord = 0;
					UnityEditorVizData(v.positionOS.xyz, v.texcoord0.xy, v.texcoord1.xy, v.texcoord2.xy, VizUV, LightCoord);
					o.VizUV = float4(VizUV, 0, 0);
					o.LightCoord = LightCoord;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = o.positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 texcoord0 : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				o.texcoord0 = v.texcoord0;
				o.texcoord1 = v.texcoord1;
				o.texcoord2 = v.texcoord2;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				o.texcoord0 = patch[0].texcoord0 * bary.x + patch[1].texcoord0 * bary.y + patch[2].texcoord0 * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				o.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 WorldPosition = IN.positionWS;
				#endif

				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float2 break19_g15 = _ImageNoiseXY;
				float2 texCoord81 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				#ifdef _DISPLACECONTINUOUS_ON
				float staticSwitch83 = _TimeParameters.x;
				#else
				float staticSwitch83 = cos( _TimeParameters.x * 0.5 );
				#endif
				float2 temp_cast_0 = (_PannerSpeed).xx;
				float2 temp_cast_1 = (texCoord81.y).xx;
				float2 panner84 = ( staticSwitch83 * temp_cast_0 + temp_cast_1);
				float2 appendResult86 = (float2(texCoord81.x , panner84.x));
				float2 temp_cast_3 = (_PannerSpeed).xx;
				float2 temp_cast_4 = (texCoord81.x).xx;
				float2 panner85 = ( staticSwitch83 * temp_cast_3 + temp_cast_4);
				float2 appendResult87 = (float2(panner85.x , texCoord81.y));
				#ifdef _DISPLACEHORIZONTAL_ON
				float2 staticSwitch88 = appendResult87;
				#else
				float2 staticSwitch88 = appendResult86;
				#endif
				float4 tex2DNode1 = tex2D( _MainTex, staticSwitch88 );
				float4 temp_output_1_0_g15 = tex2DNode1;
				float4 sinIn7_g15 = sin( temp_output_1_0_g15 );
				float4 sinInOffset6_g15 = sin( ( temp_output_1_0_g15 + 1.0 ) );
				float lerpResult20_g15 = lerp( break19_g15.x , break19_g15.y , frac( ( sin( ( ( sinIn7_g15 - sinInOffset6_g15 ) * 91.2228 ) ) * 43758.55 ) ).r);
				
				float cos10_g11 = cos( radians( _GlitchXSpeedYThickZRotW.w ) );
				float sin10_g11 = sin( radians( _GlitchXSpeedYThickZRotW.w ) );
				float2 rotator10_g11 = mul( IN.ase_texcoord4.xy - float2( 0.5,0.5 ) , float2x2( cos10_g11 , -sin10_g11 , sin10_g11 , cos10_g11 )) + float2( 0.5,0.5 );
				float2 appendResult8_g11 = (float2(_GlitchXSpeedYThickZRotW.x , 1.0));
				float mulTime66 = _TimeParameters.x * _GlitchXSpeedYThickZRotW.y;
				float2 appendResult9_g11 = (float2(mulTime66 , 0.0));
				float2 appendResult10_g12 = (float2(_GlitchXSpeedYThickZRotW.z , 1.0));
				float2 temp_output_11_0_g12 = ( abs( (frac( (rotator10_g11*appendResult8_g11 + appendResult9_g11) )*2.0 + -1.0) ) - appendResult10_g12 );
				float2 break16_g12 = ( 1.0 - ( temp_output_11_0_g12 / fwidth( temp_output_11_0_g12 ) ) );
				float smoothstepResult71 = smoothstep( _GlitchSmoothXY.x , _GlitchSmoothXY.y , saturate( min( break16_g12.x , break16_g12.y ) ));
				float lerpResult77 = lerp( _EmissionIntensity , _GlitchIntensity , smoothstepResult71);
				#ifdef _USEGLITCHINTENSITY_ON
				float staticSwitch78 = lerpResult77;
				#else
				float staticSwitch78 = _EmissionIntensity;
				#endif
				float4 lerpResult73 = lerp( _EmissiveColor , _GlitchColor , smoothstepResult71);
				#ifdef _USEGLITCHCOLOR_ON
				float4 staticSwitch74 = lerpResult73;
				#else
				float4 staticSwitch74 = _EmissiveColor;
				#endif
				
				float cos10_g13 = cos( radians( _StripesXSpeedYThickZRotW.w ) );
				float sin10_g13 = sin( radians( _StripesXSpeedYThickZRotW.w ) );
				float2 rotator10_g13 = mul( IN.ase_texcoord4.xy - float2( 0.5,0.5 ) , float2x2( cos10_g13 , -sin10_g13 , sin10_g13 , cos10_g13 )) + float2( 0.5,0.5 );
				float2 appendResult8_g13 = (float2(_StripesXSpeedYThickZRotW.x , 1.0));
				float mulTime25 = _TimeParameters.x * _StripesXSpeedYThickZRotW.y;
				float2 appendResult9_g13 = (float2(mulTime25 , 0.0));
				float2 appendResult10_g14 = (float2(_StripesXSpeedYThickZRotW.z , 1.0));
				float2 temp_output_11_0_g14 = ( abs( (frac( (rotator10_g13*appendResult8_g13 + appendResult9_g13) )*2.0 + -1.0) ) - appendResult10_g14 );
				float2 break16_g14 = ( 1.0 - ( temp_output_11_0_g14 / fwidth( temp_output_11_0_g14 ) ) );
				float temp_output_20_0 = ( tex2DNode1.a * saturate( min( break16_g14.x , break16_g14.y ) ) * _Opacity );
				

				float3 BaseColor = ( lerpResult20_g15 + sinIn7_g15 ).rgb;
				float3 Emission = ( tex2DNode1 * staticSwitch78 * staticSwitch74 * _URPEmissive ).rgb;
				float Alpha = temp_output_20_0;
				float AlphaClipThreshold = 0.5;

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				MetaInput metaInput = (MetaInput)0;
				metaInput.Albedo = BaseColor;
				metaInput.Emission = Emission;
				#ifdef EDITOR_VISUALIZATION
					metaInput.VizUV = IN.VizUV.xy;
					metaInput.LightCoord = IN.LightCoord;
				#endif

				return UnityMetaFragment(metaInput);
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "Universal2D"
			Tags { "LightMode"="Universal2D" }

			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZWrite Off
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA

			HLSLPROGRAM

			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#define _SURFACE_TYPE_TRANSPARENT 1
			#define _EMISSION
			#define ASE_SRP_VERSION 120112


			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_2D

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#pragma shader_feature_local _DISPLACEHORIZONTAL_ON
			#pragma shader_feature_local _DISPLACECONTINUOUS_ON


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 positionWS : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _GlitchXSpeedYThickZRotW;
			float4 _EmissiveColor;
			float4 _GlitchColor;
			float4 _StripesXSpeedYThickZRotW;
			float2 _SmoothHeightXY;
			float2 _DepthIntensityXPowerY;
			float2 _GlitchSmoothXY;
			float2 _GlitchScaleXPowerY;
			float2 _ImageNoiseXY;
			float _PannerSpeed;
			float _EmissionIntensity;
			float _GlitchIntensity;
			float _URPEmissive;
			float _Metallic;
			float _Opacity;
			float _Gloss;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			sampler2D _HeightMap;
			sampler2D _MainTex;


			float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }
			float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }
			float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }
			float snoise( float2 v )
			{
				const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
				float2 i = floor( v + dot( v, C.yy ) );
				float2 x0 = v - i + dot( i, C.xx );
				float2 i1;
				i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
				float4 x12 = x0.xyxy + C.xxzz;
				x12.xy -= i1;
				i = mod2D289( i );
				float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
				float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
				m = m * m;
				m = m * m;
				float3 x = 2.0 * frac( p * C.www ) - 1.0;
				float3 h = abs( x ) - 0.5;
				float3 ox = floor( x + 0.5 );
				float3 a0 = x - ox;
				m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
				float3 g;
				g.x = a0.x * x0.x + h.x * x0.y;
				g.yz = a0.yz * x12.xz + h.yz * x12.yw;
				return 130.0 * dot( m, g );
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				float2 texCoord81 = v.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				#ifdef _DISPLACECONTINUOUS_ON
				float staticSwitch83 = _TimeParameters.x;
				#else
				float staticSwitch83 = cos( _TimeParameters.x * 0.5 );
				#endif
				float2 temp_cast_0 = (_PannerSpeed).xx;
				float2 temp_cast_1 = (texCoord81.y).xx;
				float2 panner84 = ( staticSwitch83 * temp_cast_0 + temp_cast_1);
				float2 appendResult86 = (float2(texCoord81.x , panner84.x));
				float2 temp_cast_3 = (_PannerSpeed).xx;
				float2 temp_cast_4 = (texCoord81.x).xx;
				float2 panner85 = ( staticSwitch83 * temp_cast_3 + temp_cast_4);
				float2 appendResult87 = (float2(panner85.x , texCoord81.y));
				#ifdef _DISPLACEHORIZONTAL_ON
				float2 staticSwitch88 = appendResult87;
				#else
				float2 staticSwitch88 = appendResult86;
				#endif
				float smoothstepResult21 = smoothstep( _SmoothHeightXY.x , _SmoothHeightXY.y , tex2Dlod( _HeightMap, float4( staticSwitch88, 0, 0.0) ).r);
				float cos10_g11 = cos( radians( _GlitchXSpeedYThickZRotW.w ) );
				float sin10_g11 = sin( radians( _GlitchXSpeedYThickZRotW.w ) );
				float2 rotator10_g11 = mul( v.ase_texcoord.xy - float2( 0.5,0.5 ) , float2x2( cos10_g11 , -sin10_g11 , sin10_g11 , cos10_g11 )) + float2( 0.5,0.5 );
				float2 appendResult8_g11 = (float2(_GlitchXSpeedYThickZRotW.x , 1.0));
				float mulTime66 = _TimeParameters.x * _GlitchXSpeedYThickZRotW.y;
				float2 appendResult9_g11 = (float2(mulTime66 , 0.0));
				float2 appendResult10_g12 = (float2(_GlitchXSpeedYThickZRotW.z , 1.0));
				float2 temp_output_11_0_g12 = ( abs( (frac( (rotator10_g11*appendResult8_g11 + appendResult9_g11) )*2.0 + -1.0) ) - appendResult10_g12 );
				float2 break16_g12 = ( 1.0 - ( temp_output_11_0_g12 / (0).xx ) );
				float smoothstepResult71 = smoothstep( _GlitchSmoothXY.x , _GlitchSmoothXY.y , saturate( min( break16_g12.x , break16_g12.y ) ));
				float2 temp_cast_6 = (_PannerSpeed).xx;
				float2 panner58 = ( cos( _TimeParameters.x * 0.5 ) * temp_cast_6 + texCoord81);
				float simplePerlin2D63 = snoise( panner58*_GlitchScaleXPowerY.x );
				simplePerlin2D63 = simplePerlin2D63*0.5 + 0.5;
				float3 appendResult52 = (float3(0.0 , ( pow( smoothstepResult21 , _DepthIntensityXPowerY.y ) * _DepthIntensityXPowerY.x ) , ( smoothstepResult71 * simplePerlin2D63 * _GlitchScaleXPowerY.y )));
				
				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = appendResult52;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif

				v.normalOS = v.normalOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( v.positionOS.xyz );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					o.positionWS = vertexInput.positionWS;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.positionCS = vertexInput.positionCS;

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 WorldPosition = IN.positionWS;
				#endif

				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float2 break19_g15 = _ImageNoiseXY;
				float2 texCoord81 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				#ifdef _DISPLACECONTINUOUS_ON
				float staticSwitch83 = _TimeParameters.x;
				#else
				float staticSwitch83 = cos( _TimeParameters.x * 0.5 );
				#endif
				float2 temp_cast_0 = (_PannerSpeed).xx;
				float2 temp_cast_1 = (texCoord81.y).xx;
				float2 panner84 = ( staticSwitch83 * temp_cast_0 + temp_cast_1);
				float2 appendResult86 = (float2(texCoord81.x , panner84.x));
				float2 temp_cast_3 = (_PannerSpeed).xx;
				float2 temp_cast_4 = (texCoord81.x).xx;
				float2 panner85 = ( staticSwitch83 * temp_cast_3 + temp_cast_4);
				float2 appendResult87 = (float2(panner85.x , texCoord81.y));
				#ifdef _DISPLACEHORIZONTAL_ON
				float2 staticSwitch88 = appendResult87;
				#else
				float2 staticSwitch88 = appendResult86;
				#endif
				float4 tex2DNode1 = tex2D( _MainTex, staticSwitch88 );
				float4 temp_output_1_0_g15 = tex2DNode1;
				float4 sinIn7_g15 = sin( temp_output_1_0_g15 );
				float4 sinInOffset6_g15 = sin( ( temp_output_1_0_g15 + 1.0 ) );
				float lerpResult20_g15 = lerp( break19_g15.x , break19_g15.y , frac( ( sin( ( ( sinIn7_g15 - sinInOffset6_g15 ) * 91.2228 ) ) * 43758.55 ) ).r);
				
				float cos10_g13 = cos( radians( _StripesXSpeedYThickZRotW.w ) );
				float sin10_g13 = sin( radians( _StripesXSpeedYThickZRotW.w ) );
				float2 rotator10_g13 = mul( IN.ase_texcoord2.xy - float2( 0.5,0.5 ) , float2x2( cos10_g13 , -sin10_g13 , sin10_g13 , cos10_g13 )) + float2( 0.5,0.5 );
				float2 appendResult8_g13 = (float2(_StripesXSpeedYThickZRotW.x , 1.0));
				float mulTime25 = _TimeParameters.x * _StripesXSpeedYThickZRotW.y;
				float2 appendResult9_g13 = (float2(mulTime25 , 0.0));
				float2 appendResult10_g14 = (float2(_StripesXSpeedYThickZRotW.z , 1.0));
				float2 temp_output_11_0_g14 = ( abs( (frac( (rotator10_g13*appendResult8_g13 + appendResult9_g13) )*2.0 + -1.0) ) - appendResult10_g14 );
				float2 break16_g14 = ( 1.0 - ( temp_output_11_0_g14 / fwidth( temp_output_11_0_g14 ) ) );
				float temp_output_20_0 = ( tex2DNode1.a * saturate( min( break16_g14.x , break16_g14.y ) ) * _Opacity );
				

				float3 BaseColor = ( lerpResult20_g15 + sinIn7_g15 ).rgb;
				float Alpha = temp_output_20_0;
				float AlphaClipThreshold = 0.5;

				half4 color = half4(BaseColor, Alpha );

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				return color;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthNormals"
			Tags { "LightMode"="DepthNormals" }

			ZWrite On
			Blend One Zero
			ZTest LEqual
			ZWrite On

			HLSLPROGRAM

			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
			#define ASE_FOG 1
			#define _SURFACE_TYPE_TRANSPARENT 1
			#define _EMISSION
			#define ASE_SRP_VERSION 120112


			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_DEPTHNORMALSONLY

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#pragma shader_feature_local _DISPLACEHORIZONTAL_ON
			#pragma shader_feature_local _DISPLACECONTINUOUS_ON


			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float4 clipPosV : TEXCOORD0;
				float3 worldNormal : TEXCOORD1;
				float4 worldTangent : TEXCOORD2;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 positionWS : TEXCOORD3;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord : TEXCOORD4;
				#endif
				float4 ase_texcoord5 : TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _GlitchXSpeedYThickZRotW;
			float4 _EmissiveColor;
			float4 _GlitchColor;
			float4 _StripesXSpeedYThickZRotW;
			float2 _SmoothHeightXY;
			float2 _DepthIntensityXPowerY;
			float2 _GlitchSmoothXY;
			float2 _GlitchScaleXPowerY;
			float2 _ImageNoiseXY;
			float _PannerSpeed;
			float _EmissionIntensity;
			float _GlitchIntensity;
			float _URPEmissive;
			float _Metallic;
			float _Opacity;
			float _Gloss;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			sampler2D _HeightMap;
			sampler2D _MainTex;


			float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }
			float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }
			float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }
			float snoise( float2 v )
			{
				const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
				float2 i = floor( v + dot( v, C.yy ) );
				float2 x0 = v - i + dot( i, C.xx );
				float2 i1;
				i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
				float4 x12 = x0.xyxy + C.xxzz;
				x12.xy -= i1;
				i = mod2D289( i );
				float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
				float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
				m = m * m;
				m = m * m;
				float3 x = 2.0 * frac( p * C.www ) - 1.0;
				float3 h = abs( x ) - 0.5;
				float3 ox = floor( x + 0.5 );
				float3 a0 = x - ox;
				m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
				float3 g;
				g.x = a0.x * x0.x + h.x * x0.y;
				g.yz = a0.yz * x12.xz + h.yz * x12.yw;
				return 130.0 * dot( m, g );
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float2 texCoord81 = v.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				#ifdef _DISPLACECONTINUOUS_ON
				float staticSwitch83 = _TimeParameters.x;
				#else
				float staticSwitch83 = cos( _TimeParameters.x * 0.5 );
				#endif
				float2 temp_cast_0 = (_PannerSpeed).xx;
				float2 temp_cast_1 = (texCoord81.y).xx;
				float2 panner84 = ( staticSwitch83 * temp_cast_0 + temp_cast_1);
				float2 appendResult86 = (float2(texCoord81.x , panner84.x));
				float2 temp_cast_3 = (_PannerSpeed).xx;
				float2 temp_cast_4 = (texCoord81.x).xx;
				float2 panner85 = ( staticSwitch83 * temp_cast_3 + temp_cast_4);
				float2 appendResult87 = (float2(panner85.x , texCoord81.y));
				#ifdef _DISPLACEHORIZONTAL_ON
				float2 staticSwitch88 = appendResult87;
				#else
				float2 staticSwitch88 = appendResult86;
				#endif
				float smoothstepResult21 = smoothstep( _SmoothHeightXY.x , _SmoothHeightXY.y , tex2Dlod( _HeightMap, float4( staticSwitch88, 0, 0.0) ).r);
				float cos10_g11 = cos( radians( _GlitchXSpeedYThickZRotW.w ) );
				float sin10_g11 = sin( radians( _GlitchXSpeedYThickZRotW.w ) );
				float2 rotator10_g11 = mul( v.ase_texcoord.xy - float2( 0.5,0.5 ) , float2x2( cos10_g11 , -sin10_g11 , sin10_g11 , cos10_g11 )) + float2( 0.5,0.5 );
				float2 appendResult8_g11 = (float2(_GlitchXSpeedYThickZRotW.x , 1.0));
				float mulTime66 = _TimeParameters.x * _GlitchXSpeedYThickZRotW.y;
				float2 appendResult9_g11 = (float2(mulTime66 , 0.0));
				float2 appendResult10_g12 = (float2(_GlitchXSpeedYThickZRotW.z , 1.0));
				float2 temp_output_11_0_g12 = ( abs( (frac( (rotator10_g11*appendResult8_g11 + appendResult9_g11) )*2.0 + -1.0) ) - appendResult10_g12 );
				float2 break16_g12 = ( 1.0 - ( temp_output_11_0_g12 / (0).xx ) );
				float smoothstepResult71 = smoothstep( _GlitchSmoothXY.x , _GlitchSmoothXY.y , saturate( min( break16_g12.x , break16_g12.y ) ));
				float2 temp_cast_6 = (_PannerSpeed).xx;
				float2 panner58 = ( cos( _TimeParameters.x * 0.5 ) * temp_cast_6 + texCoord81);
				float simplePerlin2D63 = snoise( panner58*_GlitchScaleXPowerY.x );
				simplePerlin2D63 = simplePerlin2D63*0.5 + 0.5;
				float3 appendResult52 = (float3(0.0 , ( pow( smoothstepResult21 , _DepthIntensityXPowerY.y ) * _DepthIntensityXPowerY.x ) , ( smoothstepResult71 * simplePerlin2D63 * _GlitchScaleXPowerY.y )));
				
				o.ase_texcoord5.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord5.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = appendResult52;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif

				v.normalOS = v.normalOS;
				v.tangentOS = v.tangentOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( v.positionOS.xyz );

				float3 normalWS = TransformObjectToWorldNormal( v.normalOS );
				float4 tangentWS = float4( TransformObjectToWorldDir( v.tangentOS.xyz ), v.tangentOS.w );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					o.positionWS = vertexInput.positionWS;
				#endif

				o.worldNormal = normalWS;
				o.worldTangent = tangentWS;

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.positionCS = vertexInput.positionCS;
				o.clipPosV = vertexInput.positionCS;
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				o.tangentOS = v.tangentOS;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				o.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(	VertexOutput IN
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 WorldPosition = IN.positionWS;
				#endif

				float4 ShadowCoords = float4( 0, 0, 0, 0 );
				float3 WorldNormal = IN.worldNormal;
				float4 WorldTangent = IN.worldTangent;

				float4 ClipPos = IN.clipPosV;
				float4 ScreenPos = ComputeScreenPos( IN.clipPosV );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float2 texCoord81 = IN.ase_texcoord5.xy * float2( 1,1 ) + float2( 0,0 );
				#ifdef _DISPLACECONTINUOUS_ON
				float staticSwitch83 = _TimeParameters.x;
				#else
				float staticSwitch83 = cos( _TimeParameters.x * 0.5 );
				#endif
				float2 temp_cast_0 = (_PannerSpeed).xx;
				float2 temp_cast_1 = (texCoord81.y).xx;
				float2 panner84 = ( staticSwitch83 * temp_cast_0 + temp_cast_1);
				float2 appendResult86 = (float2(texCoord81.x , panner84.x));
				float2 temp_cast_3 = (_PannerSpeed).xx;
				float2 temp_cast_4 = (texCoord81.x).xx;
				float2 panner85 = ( staticSwitch83 * temp_cast_3 + temp_cast_4);
				float2 appendResult87 = (float2(panner85.x , texCoord81.y));
				#ifdef _DISPLACEHORIZONTAL_ON
				float2 staticSwitch88 = appendResult87;
				#else
				float2 staticSwitch88 = appendResult86;
				#endif
				float4 tex2DNode1 = tex2D( _MainTex, staticSwitch88 );
				float cos10_g13 = cos( radians( _StripesXSpeedYThickZRotW.w ) );
				float sin10_g13 = sin( radians( _StripesXSpeedYThickZRotW.w ) );
				float2 rotator10_g13 = mul( IN.ase_texcoord5.xy - float2( 0.5,0.5 ) , float2x2( cos10_g13 , -sin10_g13 , sin10_g13 , cos10_g13 )) + float2( 0.5,0.5 );
				float2 appendResult8_g13 = (float2(_StripesXSpeedYThickZRotW.x , 1.0));
				float mulTime25 = _TimeParameters.x * _StripesXSpeedYThickZRotW.y;
				float2 appendResult9_g13 = (float2(mulTime25 , 0.0));
				float2 appendResult10_g14 = (float2(_StripesXSpeedYThickZRotW.z , 1.0));
				float2 temp_output_11_0_g14 = ( abs( (frac( (rotator10_g13*appendResult8_g13 + appendResult9_g13) )*2.0 + -1.0) ) - appendResult10_g14 );
				float2 break16_g14 = ( 1.0 - ( temp_output_11_0_g14 / fwidth( temp_output_11_0_g14 ) ) );
				float temp_output_20_0 = ( tex2DNode1.a * saturate( min( break16_g14.x , break16_g14.y ) ) * _Opacity );
				

				float3 Normal = float3(0, 0, 1);
				float Alpha = temp_output_20_0;
				float AlphaClipThreshold = 0.5;

				#ifdef ASE_DEPTH_WRITE_ON
					float DepthValue = IN.positionCS.z;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.positionCS.xyz, unity_LODFade.x );
				#endif

				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif

				#if defined(_GBUFFER_NORMALS_OCT)
					float2 octNormalWS = PackNormalOctQuadEncode(WorldNormal);
					float2 remappedOctNormalWS = saturate(octNormalWS * 0.5 + 0.5);
					half3 packedNormalWS = PackFloat2To888(remappedOctNormalWS);
					return half4(packedNormalWS, 0.0);
				#else
					#if defined(_NORMALMAP)
						#if _NORMAL_DROPOFF_TS
							float crossSign = (WorldTangent.w > 0.0 ? 1.0 : -1.0) * GetOddNegativeScale();
							float3 bitangent = crossSign * cross(WorldNormal.xyz, WorldTangent.xyz);
							float3 normalWS = TransformTangentToWorld(Normal, half3x3(WorldTangent.xyz, bitangent, WorldNormal.xyz));
						#elif _NORMAL_DROPOFF_OS
							float3 normalWS = TransformObjectToWorldNormal(Normal);
						#elif _NORMAL_DROPOFF_WS
							float3 normalWS = Normal;
						#endif
					#else
						float3 normalWS = WorldNormal;
					#endif
					return half4(NormalizeNormalPerPixel(normalWS), 0.0);
				#endif
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "GBuffer"
			Tags { "LightMode"="UniversalGBuffer" }

			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZWrite Off
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM

			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma instancing_options renderinglayer
			#pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _SURFACE_TYPE_TRANSPARENT 1
			#define _EMISSION
			#define ASE_SRP_VERSION 120112


			#pragma shader_feature_local _RECEIVE_SHADOWS_OFF
			#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
			#pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF

			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
			#pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
			#pragma multi_compile_fragment _ _SHADOWS_SOFT
			#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
			#pragma multi_compile_fragment _ _LIGHT_LAYERS
			#pragma multi_compile_fragment _ _RENDER_PASS_ENABLED

			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
			#pragma multi_compile _ SHADOWS_SHADOWMASK
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile _ DYNAMICLIGHTMAP_ON
			#pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_GBUFFER

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#if defined(UNITY_INSTANCING_ENABLED) && defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL)
				#define ENABLE_TERRAIN_PERPIXEL_NORMAL
			#endif

			#pragma shader_feature_local _DISPLACEHORIZONTAL_ON
			#pragma shader_feature_local _DISPLACECONTINUOUS_ON
			#pragma shader_feature_local _USEGLITCHINTENSITY_ON
			#pragma shader_feature_local _USEGLITCHCOLOR_ON


			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float4 clipPosV : TEXCOORD0;
				float4 lightmapUVOrVertexSH : TEXCOORD1;
				half4 fogFactorAndVertexLight : TEXCOORD2;
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				float4 shadowCoord : TEXCOORD6;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON)
				float2 dynamicLightmapUV : TEXCOORD7;
				#endif
				float4 ase_texcoord8 : TEXCOORD8;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _GlitchXSpeedYThickZRotW;
			float4 _EmissiveColor;
			float4 _GlitchColor;
			float4 _StripesXSpeedYThickZRotW;
			float2 _SmoothHeightXY;
			float2 _DepthIntensityXPowerY;
			float2 _GlitchSmoothXY;
			float2 _GlitchScaleXPowerY;
			float2 _ImageNoiseXY;
			float _PannerSpeed;
			float _EmissionIntensity;
			float _GlitchIntensity;
			float _URPEmissive;
			float _Metallic;
			float _Opacity;
			float _Gloss;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			sampler2D _HeightMap;
			sampler2D _MainTex;


			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"

			float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }
			float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }
			float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }
			float snoise( float2 v )
			{
				const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
				float2 i = floor( v + dot( v, C.yy ) );
				float2 x0 = v - i + dot( i, C.xx );
				float2 i1;
				i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
				float4 x12 = x0.xyxy + C.xxzz;
				x12.xy -= i1;
				i = mod2D289( i );
				float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
				float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
				m = m * m;
				m = m * m;
				float3 x = 2.0 * frac( p * C.www ) - 1.0;
				float3 h = abs( x ) - 0.5;
				float3 ox = floor( x + 0.5 );
				float3 a0 = x - ox;
				m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
				float3 g;
				g.x = a0.x * x0.x + h.x * x0.y;
				g.yz = a0.yz * x12.xz + h.yz * x12.yw;
				return 130.0 * dot( m, g );
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float2 texCoord81 = v.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				#ifdef _DISPLACECONTINUOUS_ON
				float staticSwitch83 = _TimeParameters.x;
				#else
				float staticSwitch83 = cos( _TimeParameters.x * 0.5 );
				#endif
				float2 temp_cast_0 = (_PannerSpeed).xx;
				float2 temp_cast_1 = (texCoord81.y).xx;
				float2 panner84 = ( staticSwitch83 * temp_cast_0 + temp_cast_1);
				float2 appendResult86 = (float2(texCoord81.x , panner84.x));
				float2 temp_cast_3 = (_PannerSpeed).xx;
				float2 temp_cast_4 = (texCoord81.x).xx;
				float2 panner85 = ( staticSwitch83 * temp_cast_3 + temp_cast_4);
				float2 appendResult87 = (float2(panner85.x , texCoord81.y));
				#ifdef _DISPLACEHORIZONTAL_ON
				float2 staticSwitch88 = appendResult87;
				#else
				float2 staticSwitch88 = appendResult86;
				#endif
				float smoothstepResult21 = smoothstep( _SmoothHeightXY.x , _SmoothHeightXY.y , tex2Dlod( _HeightMap, float4( staticSwitch88, 0, 0.0) ).r);
				float cos10_g11 = cos( radians( _GlitchXSpeedYThickZRotW.w ) );
				float sin10_g11 = sin( radians( _GlitchXSpeedYThickZRotW.w ) );
				float2 rotator10_g11 = mul( v.texcoord.xy - float2( 0.5,0.5 ) , float2x2( cos10_g11 , -sin10_g11 , sin10_g11 , cos10_g11 )) + float2( 0.5,0.5 );
				float2 appendResult8_g11 = (float2(_GlitchXSpeedYThickZRotW.x , 1.0));
				float mulTime66 = _TimeParameters.x * _GlitchXSpeedYThickZRotW.y;
				float2 appendResult9_g11 = (float2(mulTime66 , 0.0));
				float2 appendResult10_g12 = (float2(_GlitchXSpeedYThickZRotW.z , 1.0));
				float2 temp_output_11_0_g12 = ( abs( (frac( (rotator10_g11*appendResult8_g11 + appendResult9_g11) )*2.0 + -1.0) ) - appendResult10_g12 );
				float2 break16_g12 = ( 1.0 - ( temp_output_11_0_g12 / (0).xx ) );
				float smoothstepResult71 = smoothstep( _GlitchSmoothXY.x , _GlitchSmoothXY.y , saturate( min( break16_g12.x , break16_g12.y ) ));
				float2 temp_cast_6 = (_PannerSpeed).xx;
				float2 panner58 = ( cos( _TimeParameters.x * 0.5 ) * temp_cast_6 + texCoord81);
				float simplePerlin2D63 = snoise( panner58*_GlitchScaleXPowerY.x );
				simplePerlin2D63 = simplePerlin2D63*0.5 + 0.5;
				float3 appendResult52 = (float3(0.0 , ( pow( smoothstepResult21 , _DepthIntensityXPowerY.y ) * _DepthIntensityXPowerY.x ) , ( smoothstepResult71 * simplePerlin2D63 * _GlitchScaleXPowerY.y )));
				
				o.ase_texcoord8.xy = v.texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord8.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = appendResult52;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif

				v.normalOS = v.normalOS;
				v.tangentOS = v.tangentOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( v.positionOS.xyz );
				VertexNormalInputs normalInput = GetVertexNormalInputs( v.normalOS, v.tangentOS );

				o.tSpace0 = float4( normalInput.normalWS, vertexInput.positionWS.x);
				o.tSpace1 = float4( normalInput.tangentWS, vertexInput.positionWS.y);
				o.tSpace2 = float4( normalInput.bitangentWS, vertexInput.positionWS.z);

				#if defined(LIGHTMAP_ON)
					OUTPUT_LIGHTMAP_UV(v.texcoord1, unity_LightmapST, o.lightmapUVOrVertexSH.xy);
				#endif

				#if defined(DYNAMICLIGHTMAP_ON)
					o.dynamicLightmapUV.xy = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#endif

				#if !defined(LIGHTMAP_ON)
					OUTPUT_SH(normalInput.normalWS.xyz, o.lightmapUVOrVertexSH.xyz);
				#endif

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					o.lightmapUVOrVertexSH.zw = v.texcoord.xy;
					o.lightmapUVOrVertexSH.xy = v.texcoord.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif

				half3 vertexLight = VertexLighting( vertexInput.positionWS, normalInput.normalWS );

				o.fogFactorAndVertexLight = half4(0, vertexLight);

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.positionCS = vertexInput.positionCS;
				o.clipPosV = vertexInput.positionCS;
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				o.tangentOS = v.tangentOS;
				o.texcoord = v.texcoord;
				o.texcoord1 = v.texcoord1;
				o.texcoord2 = v.texcoord2;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				o.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				o.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				o.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			FragmentOutput frag ( VertexOutput IN
								#ifdef ASE_DEPTH_WRITE_ON
								,out float outputDepth : ASE_SV_DEPTH
								#endif
								 )
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.positionCS.xyz, unity_LODFade.x );
				#endif

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float2 sampleCoords = (IN.lightmapUVOrVertexSH.zw / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
					float3 WorldNormal = TransformObjectToWorldNormal(normalize(SAMPLE_TEXTURE2D(_TerrainNormalmapTexture, sampler_TerrainNormalmapTexture, sampleCoords).rgb * 2 - 1));
					float3 WorldTangent = -cross(GetObjectToWorldMatrix()._13_23_33, WorldNormal);
					float3 WorldBiTangent = cross(WorldNormal, -WorldTangent);
				#else
					float3 WorldNormal = normalize( IN.tSpace0.xyz );
					float3 WorldTangent = IN.tSpace1.xyz;
					float3 WorldBiTangent = IN.tSpace2.xyz;
				#endif

				float3 WorldPosition = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
				float3 WorldViewDirection = _WorldSpaceCameraPos.xyz  - WorldPosition;
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				float4 ClipPos = IN.clipPosV;
				float4 ScreenPos = ComputeScreenPos( IN.clipPosV );

				float2 NormalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.positionCS);

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					ShadowCoords = IN.shadowCoord;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
				#else
					ShadowCoords = float4(0, 0, 0, 0);
				#endif

				WorldViewDirection = SafeNormalize( WorldViewDirection );

				float2 break19_g15 = _ImageNoiseXY;
				float2 texCoord81 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				#ifdef _DISPLACECONTINUOUS_ON
				float staticSwitch83 = _TimeParameters.x;
				#else
				float staticSwitch83 = cos( _TimeParameters.x * 0.5 );
				#endif
				float2 temp_cast_0 = (_PannerSpeed).xx;
				float2 temp_cast_1 = (texCoord81.y).xx;
				float2 panner84 = ( staticSwitch83 * temp_cast_0 + temp_cast_1);
				float2 appendResult86 = (float2(texCoord81.x , panner84.x));
				float2 temp_cast_3 = (_PannerSpeed).xx;
				float2 temp_cast_4 = (texCoord81.x).xx;
				float2 panner85 = ( staticSwitch83 * temp_cast_3 + temp_cast_4);
				float2 appendResult87 = (float2(panner85.x , texCoord81.y));
				#ifdef _DISPLACEHORIZONTAL_ON
				float2 staticSwitch88 = appendResult87;
				#else
				float2 staticSwitch88 = appendResult86;
				#endif
				float4 tex2DNode1 = tex2D( _MainTex, staticSwitch88 );
				float4 temp_output_1_0_g15 = tex2DNode1;
				float4 sinIn7_g15 = sin( temp_output_1_0_g15 );
				float4 sinInOffset6_g15 = sin( ( temp_output_1_0_g15 + 1.0 ) );
				float lerpResult20_g15 = lerp( break19_g15.x , break19_g15.y , frac( ( sin( ( ( sinIn7_g15 - sinInOffset6_g15 ) * 91.2228 ) ) * 43758.55 ) ).r);
				
				float cos10_g11 = cos( radians( _GlitchXSpeedYThickZRotW.w ) );
				float sin10_g11 = sin( radians( _GlitchXSpeedYThickZRotW.w ) );
				float2 rotator10_g11 = mul( IN.ase_texcoord8.xy - float2( 0.5,0.5 ) , float2x2( cos10_g11 , -sin10_g11 , sin10_g11 , cos10_g11 )) + float2( 0.5,0.5 );
				float2 appendResult8_g11 = (float2(_GlitchXSpeedYThickZRotW.x , 1.0));
				float mulTime66 = _TimeParameters.x * _GlitchXSpeedYThickZRotW.y;
				float2 appendResult9_g11 = (float2(mulTime66 , 0.0));
				float2 appendResult10_g12 = (float2(_GlitchXSpeedYThickZRotW.z , 1.0));
				float2 temp_output_11_0_g12 = ( abs( (frac( (rotator10_g11*appendResult8_g11 + appendResult9_g11) )*2.0 + -1.0) ) - appendResult10_g12 );
				float2 break16_g12 = ( 1.0 - ( temp_output_11_0_g12 / fwidth( temp_output_11_0_g12 ) ) );
				float smoothstepResult71 = smoothstep( _GlitchSmoothXY.x , _GlitchSmoothXY.y , saturate( min( break16_g12.x , break16_g12.y ) ));
				float lerpResult77 = lerp( _EmissionIntensity , _GlitchIntensity , smoothstepResult71);
				#ifdef _USEGLITCHINTENSITY_ON
				float staticSwitch78 = lerpResult77;
				#else
				float staticSwitch78 = _EmissionIntensity;
				#endif
				float4 lerpResult73 = lerp( _EmissiveColor , _GlitchColor , smoothstepResult71);
				#ifdef _USEGLITCHCOLOR_ON
				float4 staticSwitch74 = lerpResult73;
				#else
				float4 staticSwitch74 = _EmissiveColor;
				#endif
				
				float cos10_g13 = cos( radians( _StripesXSpeedYThickZRotW.w ) );
				float sin10_g13 = sin( radians( _StripesXSpeedYThickZRotW.w ) );
				float2 rotator10_g13 = mul( IN.ase_texcoord8.xy - float2( 0.5,0.5 ) , float2x2( cos10_g13 , -sin10_g13 , sin10_g13 , cos10_g13 )) + float2( 0.5,0.5 );
				float2 appendResult8_g13 = (float2(_StripesXSpeedYThickZRotW.x , 1.0));
				float mulTime25 = _TimeParameters.x * _StripesXSpeedYThickZRotW.y;
				float2 appendResult9_g13 = (float2(mulTime25 , 0.0));
				float2 appendResult10_g14 = (float2(_StripesXSpeedYThickZRotW.z , 1.0));
				float2 temp_output_11_0_g14 = ( abs( (frac( (rotator10_g13*appendResult8_g13 + appendResult9_g13) )*2.0 + -1.0) ) - appendResult10_g14 );
				float2 break16_g14 = ( 1.0 - ( temp_output_11_0_g14 / fwidth( temp_output_11_0_g14 ) ) );
				float temp_output_20_0 = ( tex2DNode1.a * saturate( min( break16_g14.x , break16_g14.y ) ) * _Opacity );
				

				float3 BaseColor = ( lerpResult20_g15 + sinIn7_g15 ).rgb;
				float3 Normal = float3(0, 0, 1);
				float3 Emission = ( tex2DNode1 * staticSwitch78 * staticSwitch74 * _URPEmissive ).rgb;
				float3 Specular = 0.5;
				float Metallic = _Metallic;
				float Smoothness = ( temp_output_20_0 * _Gloss );
				float Occlusion = 1;
				float Alpha = temp_output_20_0;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;
				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = 1;
				float3 Translucency = 1;

				#ifdef ASE_DEPTH_WRITE_ON
					float DepthValue = IN.positionCS.z;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				InputData inputData = (InputData)0;
				inputData.positionWS = WorldPosition;
				inputData.positionCS = IN.positionCS;
				inputData.shadowCoord = ShadowCoords;

				#ifdef _NORMALMAP
					#if _NORMAL_DROPOFF_TS
						inputData.normalWS = TransformTangentToWorld(Normal, half3x3( WorldTangent, WorldBiTangent, WorldNormal ));
					#elif _NORMAL_DROPOFF_OS
						inputData.normalWS = TransformObjectToWorldNormal(Normal);
					#elif _NORMAL_DROPOFF_WS
						inputData.normalWS = Normal;
					#endif
				#else
					inputData.normalWS = WorldNormal;
				#endif

				inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
				inputData.viewDirectionWS = SafeNormalize( WorldViewDirection );

				inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float3 SH = SampleSH(inputData.normalWS.xyz);
				#else
					float3 SH = IN.lightmapUVOrVertexSH.xyz;
				#endif

				#ifdef ASE_BAKEDGI
					inputData.bakedGI = BakedGI;
				#else
					#if defined(DYNAMICLIGHTMAP_ON)
						inputData.bakedGI = SAMPLE_GI( IN.lightmapUVOrVertexSH.xy, IN.dynamicLightmapUV.xy, SH, inputData.normalWS);
					#else
						inputData.bakedGI = SAMPLE_GI( IN.lightmapUVOrVertexSH.xy, SH, inputData.normalWS );
					#endif
				#endif

				inputData.normalizedScreenSpaceUV = NormalizedScreenSpaceUV;
				inputData.shadowMask = SAMPLE_SHADOWMASK(IN.lightmapUVOrVertexSH.xy);

				#if defined(DEBUG_DISPLAY)
					#if defined(DYNAMICLIGHTMAP_ON)
						inputData.dynamicLightmapUV = IN.dynamicLightmapUV.xy;
						#endif
					#if defined(LIGHTMAP_ON)
						inputData.staticLightmapUV = IN.lightmapUVOrVertexSH.xy;
					#else
						inputData.vertexSH = SH;
					#endif
				#endif

				#ifdef _DBUFFER
					ApplyDecal(IN.positionCS,
						BaseColor,
						Specular,
						inputData.normalWS,
						Metallic,
						Occlusion,
						Smoothness);
				#endif

				BRDFData brdfData;
				InitializeBRDFData
				(BaseColor, Metallic, Specular, Smoothness, Alpha, brdfData);

				Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, inputData.shadowMask);
				half4 color;
				MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, inputData.shadowMask);
				color.rgb = GlobalIllumination(brdfData, inputData.bakedGI, Occlusion, inputData.positionWS, inputData.normalWS, inputData.viewDirectionWS);
				color.a = Alpha;

				#ifdef ASE_FINAL_COLOR_ALPHA_MULTIPLY
					color.rgb *= color.a;
				#endif

				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif

				return BRDFDataToGbuffer(brdfData, inputData, Smoothness, Emission + color.rgb, Occlusion);
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "SceneSelectionPass"
			Tags { "LightMode"="SceneSelectionPass" }

			Cull Off
			AlphaToMask Off

			HLSLPROGRAM

			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#define _SURFACE_TYPE_TRANSPARENT 1
			#define _EMISSION
			#define ASE_SRP_VERSION 120112


			#pragma vertex vert
			#pragma fragment frag

			#define SCENESELECTIONPASS 1

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define SHADERPASS SHADERPASS_DEPTHONLY

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#pragma shader_feature_local _DISPLACEHORIZONTAL_ON
			#pragma shader_feature_local _DISPLACECONTINUOUS_ON


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _GlitchXSpeedYThickZRotW;
			float4 _EmissiveColor;
			float4 _GlitchColor;
			float4 _StripesXSpeedYThickZRotW;
			float2 _SmoothHeightXY;
			float2 _DepthIntensityXPowerY;
			float2 _GlitchSmoothXY;
			float2 _GlitchScaleXPowerY;
			float2 _ImageNoiseXY;
			float _PannerSpeed;
			float _EmissionIntensity;
			float _GlitchIntensity;
			float _URPEmissive;
			float _Metallic;
			float _Opacity;
			float _Gloss;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			sampler2D _HeightMap;
			sampler2D _MainTex;


			float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }
			float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }
			float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }
			float snoise( float2 v )
			{
				const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
				float2 i = floor( v + dot( v, C.yy ) );
				float2 x0 = v - i + dot( i, C.xx );
				float2 i1;
				i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
				float4 x12 = x0.xyxy + C.xxzz;
				x12.xy -= i1;
				i = mod2D289( i );
				float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
				float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
				m = m * m;
				m = m * m;
				float3 x = 2.0 * frac( p * C.www ) - 1.0;
				float3 h = abs( x ) - 0.5;
				float3 ox = floor( x + 0.5 );
				float3 a0 = x - ox;
				m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
				float3 g;
				g.x = a0.x * x0.x + h.x * x0.y;
				g.yz = a0.yz * x12.xz + h.yz * x12.yw;
				return 130.0 * dot( m, g );
			}
			

			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float2 texCoord81 = v.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				#ifdef _DISPLACECONTINUOUS_ON
				float staticSwitch83 = _TimeParameters.x;
				#else
				float staticSwitch83 = cos( _TimeParameters.x * 0.5 );
				#endif
				float2 temp_cast_0 = (_PannerSpeed).xx;
				float2 temp_cast_1 = (texCoord81.y).xx;
				float2 panner84 = ( staticSwitch83 * temp_cast_0 + temp_cast_1);
				float2 appendResult86 = (float2(texCoord81.x , panner84.x));
				float2 temp_cast_3 = (_PannerSpeed).xx;
				float2 temp_cast_4 = (texCoord81.x).xx;
				float2 panner85 = ( staticSwitch83 * temp_cast_3 + temp_cast_4);
				float2 appendResult87 = (float2(panner85.x , texCoord81.y));
				#ifdef _DISPLACEHORIZONTAL_ON
				float2 staticSwitch88 = appendResult87;
				#else
				float2 staticSwitch88 = appendResult86;
				#endif
				float smoothstepResult21 = smoothstep( _SmoothHeightXY.x , _SmoothHeightXY.y , tex2Dlod( _HeightMap, float4( staticSwitch88, 0, 0.0) ).r);
				float cos10_g11 = cos( radians( _GlitchXSpeedYThickZRotW.w ) );
				float sin10_g11 = sin( radians( _GlitchXSpeedYThickZRotW.w ) );
				float2 rotator10_g11 = mul( v.ase_texcoord.xy - float2( 0.5,0.5 ) , float2x2( cos10_g11 , -sin10_g11 , sin10_g11 , cos10_g11 )) + float2( 0.5,0.5 );
				float2 appendResult8_g11 = (float2(_GlitchXSpeedYThickZRotW.x , 1.0));
				float mulTime66 = _TimeParameters.x * _GlitchXSpeedYThickZRotW.y;
				float2 appendResult9_g11 = (float2(mulTime66 , 0.0));
				float2 appendResult10_g12 = (float2(_GlitchXSpeedYThickZRotW.z , 1.0));
				float2 temp_output_11_0_g12 = ( abs( (frac( (rotator10_g11*appendResult8_g11 + appendResult9_g11) )*2.0 + -1.0) ) - appendResult10_g12 );
				float2 break16_g12 = ( 1.0 - ( temp_output_11_0_g12 / (0).xx ) );
				float smoothstepResult71 = smoothstep( _GlitchSmoothXY.x , _GlitchSmoothXY.y , saturate( min( break16_g12.x , break16_g12.y ) ));
				float2 temp_cast_6 = (_PannerSpeed).xx;
				float2 panner58 = ( cos( _TimeParameters.x * 0.5 ) * temp_cast_6 + texCoord81);
				float simplePerlin2D63 = snoise( panner58*_GlitchScaleXPowerY.x );
				simplePerlin2D63 = simplePerlin2D63*0.5 + 0.5;
				float3 appendResult52 = (float3(0.0 , ( pow( smoothstepResult21 , _DepthIntensityXPowerY.y ) * _DepthIntensityXPowerY.x ) , ( smoothstepResult71 * simplePerlin2D63 * _GlitchScaleXPowerY.y )));
				
				o.ase_texcoord.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = appendResult52;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif

				v.normalOS = v.normalOS;

				float3 positionWS = TransformObjectToWorld( v.positionOS.xyz );

				o.positionCS = TransformWorldToHClip(positionWS);

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				float2 texCoord81 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				#ifdef _DISPLACECONTINUOUS_ON
				float staticSwitch83 = _TimeParameters.x;
				#else
				float staticSwitch83 = cos( _TimeParameters.x * 0.5 );
				#endif
				float2 temp_cast_0 = (_PannerSpeed).xx;
				float2 temp_cast_1 = (texCoord81.y).xx;
				float2 panner84 = ( staticSwitch83 * temp_cast_0 + temp_cast_1);
				float2 appendResult86 = (float2(texCoord81.x , panner84.x));
				float2 temp_cast_3 = (_PannerSpeed).xx;
				float2 temp_cast_4 = (texCoord81.x).xx;
				float2 panner85 = ( staticSwitch83 * temp_cast_3 + temp_cast_4);
				float2 appendResult87 = (float2(panner85.x , texCoord81.y));
				#ifdef _DISPLACEHORIZONTAL_ON
				float2 staticSwitch88 = appendResult87;
				#else
				float2 staticSwitch88 = appendResult86;
				#endif
				float4 tex2DNode1 = tex2D( _MainTex, staticSwitch88 );
				float cos10_g13 = cos( radians( _StripesXSpeedYThickZRotW.w ) );
				float sin10_g13 = sin( radians( _StripesXSpeedYThickZRotW.w ) );
				float2 rotator10_g13 = mul( IN.ase_texcoord.xy - float2( 0.5,0.5 ) , float2x2( cos10_g13 , -sin10_g13 , sin10_g13 , cos10_g13 )) + float2( 0.5,0.5 );
				float2 appendResult8_g13 = (float2(_StripesXSpeedYThickZRotW.x , 1.0));
				float mulTime25 = _TimeParameters.x * _StripesXSpeedYThickZRotW.y;
				float2 appendResult9_g13 = (float2(mulTime25 , 0.0));
				float2 appendResult10_g14 = (float2(_StripesXSpeedYThickZRotW.z , 1.0));
				float2 temp_output_11_0_g14 = ( abs( (frac( (rotator10_g13*appendResult8_g13 + appendResult9_g13) )*2.0 + -1.0) ) - appendResult10_g14 );
				float2 break16_g14 = ( 1.0 - ( temp_output_11_0_g14 / fwidth( temp_output_11_0_g14 ) ) );
				float temp_output_20_0 = ( tex2DNode1.a * saturate( min( break16_g14.x , break16_g14.y ) ) * _Opacity );
				

				surfaceDescription.Alpha = temp_output_20_0;
				surfaceDescription.AlphaClipThreshold = 0.5;

				#if _ALPHATEST_ON
					float alphaClipThreshold = 0.01f;
					#if ALPHA_CLIP_THRESHOLD
						alphaClipThreshold = surfaceDescription.AlphaClipThreshold;
					#endif
					clip(surfaceDescription.Alpha - alphaClipThreshold);
				#endif

				half4 outColor = 0;

				#ifdef SCENESELECTIONPASS
					outColor = half4(_ObjectId, _PassValue, 1.0, 1.0);
				#elif defined(SCENEPICKINGPASS)
					outColor = _SelectionID;
				#endif

				return outColor;
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "ScenePickingPass"
			Tags { "LightMode"="Picking" }

			AlphaToMask Off

			HLSLPROGRAM

			#define _NORMAL_DROPOFF_TS 1
			#define ASE_FOG 1
			#define _SURFACE_TYPE_TRANSPARENT 1
			#define _EMISSION
			#define ASE_SRP_VERSION 120112


			#pragma vertex vert
			#pragma fragment frag

		    #define SCENEPICKINGPASS 1

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define SHADERPASS SHADERPASS_DEPTHONLY

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#pragma shader_feature_local _DISPLACEHORIZONTAL_ON
			#pragma shader_feature_local _DISPLACECONTINUOUS_ON


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _GlitchXSpeedYThickZRotW;
			float4 _EmissiveColor;
			float4 _GlitchColor;
			float4 _StripesXSpeedYThickZRotW;
			float2 _SmoothHeightXY;
			float2 _DepthIntensityXPowerY;
			float2 _GlitchSmoothXY;
			float2 _GlitchScaleXPowerY;
			float2 _ImageNoiseXY;
			float _PannerSpeed;
			float _EmissionIntensity;
			float _GlitchIntensity;
			float _URPEmissive;
			float _Metallic;
			float _Opacity;
			float _Gloss;
			#ifdef ASE_TRANSMISSION
				float _TransmissionShadow;
			#endif
			#ifdef ASE_TRANSLUCENCY
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
				float4 _SelectionID;
			#endif

			#ifdef SCENESELECTIONPASS
				int _ObjectId;
				int _PassValue;
			#endif

			sampler2D _HeightMap;
			sampler2D _MainTex;


			float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }
			float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }
			float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }
			float snoise( float2 v )
			{
				const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
				float2 i = floor( v + dot( v, C.yy ) );
				float2 x0 = v - i + dot( i, C.xx );
				float2 i1;
				i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
				float4 x12 = x0.xyxy + C.xxzz;
				x12.xy -= i1;
				i = mod2D289( i );
				float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
				float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
				m = m * m;
				m = m * m;
				float3 x = 2.0 * frac( p * C.www ) - 1.0;
				float3 h = abs( x ) - 0.5;
				float3 ox = floor( x + 0.5 );
				float3 a0 = x - ox;
				m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
				float3 g;
				g.x = a0.x * x0.x + h.x * x0.y;
				g.yz = a0.yz * x12.xz + h.yz * x12.yw;
				return 130.0 * dot( m, g );
			}
			

			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float2 texCoord81 = v.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				#ifdef _DISPLACECONTINUOUS_ON
				float staticSwitch83 = _TimeParameters.x;
				#else
				float staticSwitch83 = cos( _TimeParameters.x * 0.5 );
				#endif
				float2 temp_cast_0 = (_PannerSpeed).xx;
				float2 temp_cast_1 = (texCoord81.y).xx;
				float2 panner84 = ( staticSwitch83 * temp_cast_0 + temp_cast_1);
				float2 appendResult86 = (float2(texCoord81.x , panner84.x));
				float2 temp_cast_3 = (_PannerSpeed).xx;
				float2 temp_cast_4 = (texCoord81.x).xx;
				float2 panner85 = ( staticSwitch83 * temp_cast_3 + temp_cast_4);
				float2 appendResult87 = (float2(panner85.x , texCoord81.y));
				#ifdef _DISPLACEHORIZONTAL_ON
				float2 staticSwitch88 = appendResult87;
				#else
				float2 staticSwitch88 = appendResult86;
				#endif
				float smoothstepResult21 = smoothstep( _SmoothHeightXY.x , _SmoothHeightXY.y , tex2Dlod( _HeightMap, float4( staticSwitch88, 0, 0.0) ).r);
				float cos10_g11 = cos( radians( _GlitchXSpeedYThickZRotW.w ) );
				float sin10_g11 = sin( radians( _GlitchXSpeedYThickZRotW.w ) );
				float2 rotator10_g11 = mul( v.ase_texcoord.xy - float2( 0.5,0.5 ) , float2x2( cos10_g11 , -sin10_g11 , sin10_g11 , cos10_g11 )) + float2( 0.5,0.5 );
				float2 appendResult8_g11 = (float2(_GlitchXSpeedYThickZRotW.x , 1.0));
				float mulTime66 = _TimeParameters.x * _GlitchXSpeedYThickZRotW.y;
				float2 appendResult9_g11 = (float2(mulTime66 , 0.0));
				float2 appendResult10_g12 = (float2(_GlitchXSpeedYThickZRotW.z , 1.0));
				float2 temp_output_11_0_g12 = ( abs( (frac( (rotator10_g11*appendResult8_g11 + appendResult9_g11) )*2.0 + -1.0) ) - appendResult10_g12 );
				float2 break16_g12 = ( 1.0 - ( temp_output_11_0_g12 / (0).xx ) );
				float smoothstepResult71 = smoothstep( _GlitchSmoothXY.x , _GlitchSmoothXY.y , saturate( min( break16_g12.x , break16_g12.y ) ));
				float2 temp_cast_6 = (_PannerSpeed).xx;
				float2 panner58 = ( cos( _TimeParameters.x * 0.5 ) * temp_cast_6 + texCoord81);
				float simplePerlin2D63 = snoise( panner58*_GlitchScaleXPowerY.x );
				simplePerlin2D63 = simplePerlin2D63*0.5 + 0.5;
				float3 appendResult52 = (float3(0.0 , ( pow( smoothstepResult21 , _DepthIntensityXPowerY.y ) * _DepthIntensityXPowerY.x ) , ( smoothstepResult71 * simplePerlin2D63 * _GlitchScaleXPowerY.y )));
				
				o.ase_texcoord.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = appendResult52;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif

				v.normalOS = v.normalOS;

				float3 positionWS = TransformObjectToWorld( v.positionOS.xyz );
				o.positionCS = TransformWorldToHClip(positionWS);

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				float2 texCoord81 = IN.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				#ifdef _DISPLACECONTINUOUS_ON
				float staticSwitch83 = _TimeParameters.x;
				#else
				float staticSwitch83 = cos( _TimeParameters.x * 0.5 );
				#endif
				float2 temp_cast_0 = (_PannerSpeed).xx;
				float2 temp_cast_1 = (texCoord81.y).xx;
				float2 panner84 = ( staticSwitch83 * temp_cast_0 + temp_cast_1);
				float2 appendResult86 = (float2(texCoord81.x , panner84.x));
				float2 temp_cast_3 = (_PannerSpeed).xx;
				float2 temp_cast_4 = (texCoord81.x).xx;
				float2 panner85 = ( staticSwitch83 * temp_cast_3 + temp_cast_4);
				float2 appendResult87 = (float2(panner85.x , texCoord81.y));
				#ifdef _DISPLACEHORIZONTAL_ON
				float2 staticSwitch88 = appendResult87;
				#else
				float2 staticSwitch88 = appendResult86;
				#endif
				float4 tex2DNode1 = tex2D( _MainTex, staticSwitch88 );
				float cos10_g13 = cos( radians( _StripesXSpeedYThickZRotW.w ) );
				float sin10_g13 = sin( radians( _StripesXSpeedYThickZRotW.w ) );
				float2 rotator10_g13 = mul( IN.ase_texcoord.xy - float2( 0.5,0.5 ) , float2x2( cos10_g13 , -sin10_g13 , sin10_g13 , cos10_g13 )) + float2( 0.5,0.5 );
				float2 appendResult8_g13 = (float2(_StripesXSpeedYThickZRotW.x , 1.0));
				float mulTime25 = _TimeParameters.x * _StripesXSpeedYThickZRotW.y;
				float2 appendResult9_g13 = (float2(mulTime25 , 0.0));
				float2 appendResult10_g14 = (float2(_StripesXSpeedYThickZRotW.z , 1.0));
				float2 temp_output_11_0_g14 = ( abs( (frac( (rotator10_g13*appendResult8_g13 + appendResult9_g13) )*2.0 + -1.0) ) - appendResult10_g14 );
				float2 break16_g14 = ( 1.0 - ( temp_output_11_0_g14 / fwidth( temp_output_11_0_g14 ) ) );
				float temp_output_20_0 = ( tex2DNode1.a * saturate( min( break16_g14.x , break16_g14.y ) ) * _Opacity );
				

				surfaceDescription.Alpha = temp_output_20_0;
				surfaceDescription.AlphaClipThreshold = 0.5;

				#if _ALPHATEST_ON
					float alphaClipThreshold = 0.01f;
					#if ALPHA_CLIP_THRESHOLD
						alphaClipThreshold = surfaceDescription.AlphaClipThreshold;
					#endif
						clip(surfaceDescription.Alpha - alphaClipThreshold);
				#endif

				half4 outColor = 0;

				#ifdef SCENESELECTIONPASS
					outColor = half4(_ObjectId, _PassValue, 1.0, 1.0);
				#elif defined(SCENEPICKINGPASS)
					outColor = _SelectionID;
				#endif

				return outColor;
			}

			ENDHLSL
		}
		
	}
	
	CustomEditor "UnityEditor.ShaderGraphLitGUI"
	FallBack "Hidden/Shader Graph/FallbackError"
	
	Fallback Off
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.CosTime;79;-1741.882,-61.76231;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;80;-1920.958,161.7255;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;81;-1699.044,-213.0616;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;83;-1720.031,201.2026;Inherit;False;Property;_DisplaceContinuous;Displace Continuous;10;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;82;-1800.329,96.1619;Inherit;False;Property;_PannerSpeed;Panner Speed;5;0;Create;True;0;0;0;False;0;False;0.1;0.05;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;84;-1498.882,-28.76231;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;85;-1413.211,178.4743;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;86;-1277,-242.8244;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;87;-1208.211,119.4742;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector4Node;65;-709.8809,816.8859;Inherit;False;Property;_GlitchXSpeedYThickZRotW;GlitchX SpeedY ThickZ RotW;17;0;Create;True;0;0;0;False;0;False;100,1,0.5,90;1,0.4,0.05,90;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;88;-1171.458,-56.02241;Inherit;False;Property;_DisplaceHorizontal;Displace Horizontal;9;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;66;-393.6035,920.0998;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;5;-1003.059,327.5146;Inherit;True;Property;_HeightMap;Height Map;11;0;Create;True;0;0;0;False;0;False;-1;None;1089e2c5988bab14e8663e2f37d5488b;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;22;-1006.993,534.7032;Inherit;False;Property;_SmoothHeightXY;Smooth Height (XY);12;0;Create;True;0;0;0;False;0;False;0,1;0,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector4Node;19;-779.4229,-372.42;Inherit;False;Property;_StripesXSpeedYThickZRotW;StripesX SpeedY ThickZ RotW;14;0;Create;True;0;0;0;False;0;False;100,1,0.5,90;100,5,0.69,90;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;64;-345.815,741.2189;Inherit;False;Stripes;-1;;11;8e73a71cdf24db740864b4c3f3357e7f;0;4;5;FLOAT;6;False;4;FLOAT;0;False;3;FLOAT;0.5;False;12;FLOAT;45;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;72;-131.5236,950.0193;Inherit;False;Property;_GlitchSmoothXY;Glitch Smooth (XY);16;0;Create;True;0;0;0;False;0;False;0,1;0,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SmoothstepOpNode;21;-644.988,387.7604;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;55;-704.5572,605.6772;Inherit;False;Property;_DepthIntensityXPowerY;Depth IntensityX PowerY;13;0;Create;True;0;0;0;False;0;False;0,0;0.5,0.3;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;70;-417.5236,608.0193;Inherit;False;Property;_GlitchScaleXPowerY;Glitch ScaleX PowerY;15;0;Create;True;0;0;0;False;0;False;0,0;1,0.2;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.PannerNode;58;-1282.137,357.7174;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;25;-971.0588,-190.093;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;71;-90.12357,802.5192;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;54;-479.9521,483.8088;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;63;-170.3529,551.3173;Inherit;False;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-585.0148,-151.9142;Inherit;True;Property;_MainTex;Albedo;0;0;Create;False;0;0;0;False;0;False;-1;None;c7cce12c240a262468640ced6f8c0b79;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;15;-458.8758,-317.4812;Inherit;False;Stripes;-1;;13;8e73a71cdf24db740864b4c3f3357e7f;0;4;5;FLOAT;6;False;4;FLOAT;0;False;3;FLOAT;0.5;False;12;FLOAT;45;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;38;-338.198,323.0317;Inherit;False;Property;_Opacity;Opacity;1;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-331.2476,475.4807;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;68;67.47644,572.0193;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;18;-586.4229,38.57996;Inherit;False;Property;_EmissiveColor;Emissive Color;2;0;Create;True;0;0;0;False;0;False;1,1,1,0;0.8687484,0.6921056,0.9716981,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;75;242.602,887.1328;Inherit;False;Property;_GlitchColor;GlitchColor;19;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;4;-545.2476,251.4807;Inherit;False;Property;_EmissionIntensity;Emission Intensity;4;0;Create;True;0;0;0;False;0;False;1;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;76;229.0862,790.0625;Inherit;False;Property;_GlitchIntensity;Glitch Intensity;21;0;Create;True;0;0;0;False;0;False;1;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;73;383.9068,528.3415;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;77;468.69,692.9921;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;47;-139.7801,-41.2599;Inherit;False;Property;_Gloss;Smoothness;7;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;32;-190.5958,-328.794;Inherit;False;Property;_ImageNoiseXY;Image Noise (XY);8;0;Create;True;0;0;0;False;0;False;0,1;-0.1,0.1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.StaticSwitch;74;560.8452,532.0276;Inherit;False;Property;_UseGlitchColor;Use Glitch Color;18;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;78;646.7422,674.29;Inherit;False;Property;_UseGlitchIntensity;Use Glitch Intensity;20;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;3;215.5233,-84.24339;Inherit;False;4;4;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;48;233.7643,52.36887;Inherit;False;Property;_Metallic;Metallic;6;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;49;228.7643,132.3689;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;52;232.4659,380.1337;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;31;165.7036,-179.6917;Inherit;False;Noise Sine Wave;-1;;15;a6eff29f739ced848846e3b648af87bd;0;2;1;COLOR;0,0,0,0;False;2;FLOAT2;-0.5,0.5;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;89;-26.02509,42.45569;Inherit;False;Property;_URPEmissive;URP Emissive;3;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;221.7751,236.7066;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;90;506.0996,-144.7976;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Lit;True;3;True;12;all;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;0;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;91;506.0996,-144.7976;Float;False;True;-1;2;UnityEditor.ShaderGraphLitGUI;0;12;DLNK Shaders/ASE/SciFi/Hologram;94348b07e5e8bab40bd6c8a1e3df54cd;True;Forward;0;1;Forward;21;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;2;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;2;False;;True;3;False;;True;True;0;False;;0;False;;True;4;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;UniversalMaterialType=Lit;True;3;True;12;all;0;False;True;1;5;False;;10;False;;1;1;False;;10;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;True;True;2;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=UniversalForward;False;False;0;;0;0;Standard;39;Workflow;1;0;Surface;1;638460306127983543;  Refraction Model;0;0;  Blend;0;0;Two Sided;0;638460306167172339;Fragment Normal Space,InvertActionOnDeselection;0;0;Forward Only;0;0;Transmission;0;0;  Transmission Shadow;0.5,False,;0;Translucency;0;0;  Translucency Strength;1,False,;0;  Normal Distortion;0.5,False,;0;  Scattering;2,False,;0;  Direct;0.9,False,;0;  Ambient;0.1,False,;0;  Shadow;0.5,False,;0;Cast Shadows;0;638460306180819944;  Use Shadow Threshold;0;0;GPU Instancing;1;0;LOD CrossFade;1;0;Built-in Fog;1;0;_FinalColorxAlpha;0;0;Meta Pass;1;0;Override Baked GI;0;0;Extra Pre Pass;0;0;Tessellation;0;0;  Phong;0;0;  Strength;0.5,False,;0;  Type;0;0;  Tess;16,False,;0;  Min;10,False,;0;  Max;25,False,;0;  Edge Length;16,False,;0;  Max Displacement;25,False,;0;Write Depth;0;0;  Early Z;0;0;Vertex Position,InvertActionOnDeselection;1;0;Debug Display;0;0;Clear Coat;0;0;0;10;False;True;False;True;True;True;True;True;True;True;False;;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;92;506.0996,-144.7976;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Lit;True;3;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;False;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=ShadowCaster;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;93;506.0996,-144.7976;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Lit;True;3;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;False;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;False;False;True;1;LightMode=DepthOnly;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;94;506.0996,-144.7976;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Lit;True;3;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;95;506.0996,-144.7976;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Universal2D;0;5;Universal2D;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Lit;True;3;True;12;all;0;False;True;1;5;False;;10;False;;1;1;False;;10;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;False;False;True;2;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=Universal2D;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;96;506.0996,-144.7976;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthNormals;0;6;DepthNormals;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Lit;True;3;True;12;all;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=DepthNormals;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;97;506.0996,-144.7976;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;GBuffer;0;7;GBuffer;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Lit;True;3;True;12;all;0;False;True;1;5;False;;10;False;;1;1;False;;10;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;2;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=UniversalGBuffer;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;98;506.0996,-144.7976;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;SceneSelectionPass;0;8;SceneSelectionPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Lit;True;3;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=SceneSelectionPass;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;99;506.0996,-144.7976;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ScenePickingPass;0;9;ScenePickingPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Lit;True;3;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Picking;False;False;0;;0;0;Standard;0;False;0
WireConnection;83;1;79;3
WireConnection;83;0;80;0
WireConnection;84;0;81;2
WireConnection;84;2;82;0
WireConnection;84;1;83;0
WireConnection;85;0;81;1
WireConnection;85;2;82;0
WireConnection;85;1;83;0
WireConnection;86;0;81;1
WireConnection;86;1;84;0
WireConnection;87;0;85;0
WireConnection;87;1;81;2
WireConnection;88;1;86;0
WireConnection;88;0;87;0
WireConnection;66;0;65;2
WireConnection;5;1;88;0
WireConnection;64;5;65;1
WireConnection;64;4;66;0
WireConnection;64;3;65;3
WireConnection;64;12;65;4
WireConnection;21;0;5;1
WireConnection;21;1;22;1
WireConnection;21;2;22;2
WireConnection;58;0;81;0
WireConnection;58;2;82;0
WireConnection;58;1;79;3
WireConnection;25;0;19;2
WireConnection;71;0;64;0
WireConnection;71;1;72;1
WireConnection;71;2;72;2
WireConnection;54;0;21;0
WireConnection;54;1;55;2
WireConnection;63;0;58;0
WireConnection;63;1;70;1
WireConnection;1;1;88;0
WireConnection;15;5;19;1
WireConnection;15;4;25;0
WireConnection;15;3;19;3
WireConnection;15;12;19;4
WireConnection;6;0;54;0
WireConnection;6;1;55;1
WireConnection;68;0;71;0
WireConnection;68;1;63;0
WireConnection;68;2;70;2
WireConnection;73;0;18;0
WireConnection;73;1;75;0
WireConnection;73;2;71;0
WireConnection;77;0;4;0
WireConnection;77;1;76;0
WireConnection;77;2;71;0
WireConnection;74;1;18;0
WireConnection;74;0;73;0
WireConnection;78;1;4;0
WireConnection;78;0;77;0
WireConnection;3;0;1;0
WireConnection;3;1;78;0
WireConnection;3;2;74;0
WireConnection;3;3;89;0
WireConnection;49;0;20;0
WireConnection;49;1;47;0
WireConnection;52;1;6;0
WireConnection;52;2;68;0
WireConnection;31;1;1;0
WireConnection;31;2;32;0
WireConnection;20;0;1;4
WireConnection;20;1;15;0
WireConnection;20;2;38;0
WireConnection;91;0;31;0
WireConnection;91;2;3;0
WireConnection;91;3;48;0
WireConnection;91;4;49;0
WireConnection;91;6;20;0
WireConnection;91;8;52;0
ASEEND*/
//CHKSM=18DEA859A05CEA190783C71620797F62BC9FB17E