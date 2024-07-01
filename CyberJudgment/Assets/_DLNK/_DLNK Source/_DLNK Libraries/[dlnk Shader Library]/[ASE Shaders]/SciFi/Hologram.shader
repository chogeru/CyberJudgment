// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "DLNK Shaders/ASE/SciFi/Hologram"
{
	Properties
	{
		_MainTex("Albedo", 2D) = "white" {}
		_Opacity("Opacity", Float) = 1
		_EmissiveColor("Emissive Color", Color) = (1,1,1,0)
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
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 4.6
		#pragma shader_feature_local _DISPLACEHORIZONTAL_ON
		#pragma shader_feature_local _DISPLACECONTINUOUS_ON
		#pragma shader_feature_local _USEGLITCHINTENSITY_ON
		#pragma shader_feature_local _USEGLITCHCOLOR_ON
		#pragma surface surf Standard alpha:fade keepalpha noshadow dithercrossfade vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float2 _SmoothHeightXY;
		uniform sampler2D _HeightMap;
		uniform float _PannerSpeed;
		uniform float2 _DepthIntensityXPowerY;
		uniform float2 _GlitchSmoothXY;
		uniform float4 _GlitchXSpeedYThickZRotW;
		uniform float2 _GlitchScaleXPowerY;
		uniform float2 _ImageNoiseXY;
		uniform sampler2D _MainTex;
		uniform float _EmissionIntensity;
		uniform float _GlitchIntensity;
		uniform float4 _EmissiveColor;
		uniform float4 _GlitchColor;
		uniform float _Metallic;
		uniform float4 _StripesXSpeedYThickZRotW;
		uniform float _Opacity;
		uniform float _Gloss;


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


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			#ifdef _DISPLACECONTINUOUS_ON
				float staticSwitch83 = _Time.y;
			#else
				float staticSwitch83 = _CosTime.z;
			#endif
			float2 temp_cast_0 = (_PannerSpeed).xx;
			float2 temp_cast_1 = (v.texcoord.xy.y).xx;
			float2 panner84 = ( staticSwitch83 * temp_cast_0 + temp_cast_1);
			float2 appendResult86 = (float2(v.texcoord.xy.x , panner84.x));
			float2 temp_cast_3 = (_PannerSpeed).xx;
			float2 temp_cast_4 = (v.texcoord.xy.x).xx;
			float2 panner85 = ( staticSwitch83 * temp_cast_3 + temp_cast_4);
			float2 appendResult87 = (float2(panner85.x , v.texcoord.xy.y));
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
			float mulTime66 = _Time.y * _GlitchXSpeedYThickZRotW.y;
			float2 appendResult9_g11 = (float2(mulTime66 , 0.0));
			float2 appendResult10_g12 = (float2(_GlitchXSpeedYThickZRotW.z , 1.0));
			float2 temp_output_11_0_g12 = ( abs( (frac( (rotator10_g11*appendResult8_g11 + appendResult9_g11) )*2.0 + -1.0) ) - appendResult10_g12 );
			float2 break16_g12 = ( 1.0 - ( temp_output_11_0_g12 / (0).xx ) );
			float smoothstepResult71 = smoothstep( _GlitchSmoothXY.x , _GlitchSmoothXY.y , saturate( min( break16_g12.x , break16_g12.y ) ));
			float2 temp_cast_6 = (_PannerSpeed).xx;
			float2 panner58 = ( _CosTime.z * temp_cast_6 + v.texcoord.xy);
			float simplePerlin2D63 = snoise( panner58*_GlitchScaleXPowerY.x );
			simplePerlin2D63 = simplePerlin2D63*0.5 + 0.5;
			float3 appendResult52 = (float3(0.0 , ( pow( smoothstepResult21 , _DepthIntensityXPowerY.y ) * _DepthIntensityXPowerY.x ) , ( smoothstepResult71 * simplePerlin2D63 * _GlitchScaleXPowerY.y )));
			v.vertex.xyz += appendResult52;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 break19_g15 = _ImageNoiseXY;
			#ifdef _DISPLACECONTINUOUS_ON
				float staticSwitch83 = _Time.y;
			#else
				float staticSwitch83 = _CosTime.z;
			#endif
			float2 temp_cast_0 = (_PannerSpeed).xx;
			float2 temp_cast_1 = (i.uv_texcoord.y).xx;
			float2 panner84 = ( staticSwitch83 * temp_cast_0 + temp_cast_1);
			float2 appendResult86 = (float2(i.uv_texcoord.x , panner84.x));
			float2 temp_cast_3 = (_PannerSpeed).xx;
			float2 temp_cast_4 = (i.uv_texcoord.x).xx;
			float2 panner85 = ( staticSwitch83 * temp_cast_3 + temp_cast_4);
			float2 appendResult87 = (float2(panner85.x , i.uv_texcoord.y));
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
			o.Albedo = ( lerpResult20_g15 + sinIn7_g15 ).rgb;
			float cos10_g11 = cos( radians( _GlitchXSpeedYThickZRotW.w ) );
			float sin10_g11 = sin( radians( _GlitchXSpeedYThickZRotW.w ) );
			float2 rotator10_g11 = mul( i.uv_texcoord - float2( 0.5,0.5 ) , float2x2( cos10_g11 , -sin10_g11 , sin10_g11 , cos10_g11 )) + float2( 0.5,0.5 );
			float2 appendResult8_g11 = (float2(_GlitchXSpeedYThickZRotW.x , 1.0));
			float mulTime66 = _Time.y * _GlitchXSpeedYThickZRotW.y;
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
			o.Emission = ( tex2DNode1 * staticSwitch78 * staticSwitch74 ).rgb;
			o.Metallic = _Metallic;
			float cos10_g13 = cos( radians( _StripesXSpeedYThickZRotW.w ) );
			float sin10_g13 = sin( radians( _StripesXSpeedYThickZRotW.w ) );
			float2 rotator10_g13 = mul( i.uv_texcoord - float2( 0.5,0.5 ) , float2x2( cos10_g13 , -sin10_g13 , sin10_g13 , cos10_g13 )) + float2( 0.5,0.5 );
			float2 appendResult8_g13 = (float2(_StripesXSpeedYThickZRotW.x , 1.0));
			float mulTime25 = _Time.y * _StripesXSpeedYThickZRotW.y;
			float2 appendResult9_g13 = (float2(mulTime25 , 0.0));
			float2 appendResult10_g14 = (float2(_StripesXSpeedYThickZRotW.z , 1.0));
			float2 temp_output_11_0_g14 = ( abs( (frac( (rotator10_g13*appendResult8_g13 + appendResult9_g13) )*2.0 + -1.0) ) - appendResult10_g14 );
			float2 break16_g14 = ( 1.0 - ( temp_output_11_0_g14 / fwidth( temp_output_11_0_g14 ) ) );
			float temp_output_20_0 = ( tex2DNode1.a * saturate( min( break16_g14.x , break16_g14.y ) ) * _Opacity );
			o.Smoothness = ( temp_output_20_0 * _Gloss );
			o.Alpha = temp_output_20_0;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.CosTime;79;-1741.882,-61.76231;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;80;-1920.958,161.7255;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;81;-1699.044,-213.0616;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;83;-1720.031,201.2026;Inherit;False;Property;_DisplaceContinuous;Displace Continuous;9;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;82;-1800.329,96.1619;Inherit;False;Property;_PannerSpeed;Panner Speed;4;0;Create;True;0;0;0;False;0;False;0.1;0.05;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;84;-1498.882,-28.76231;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;85;-1413.211,178.4743;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector4Node;65;-709.8809,816.8859;Inherit;False;Property;_GlitchXSpeedYThickZRotW;GlitchX SpeedY ThickZ RotW;16;0;Create;True;0;0;0;False;0;False;100,1,0.5,90;1,0.4,0.05,90;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;86;-1277,-242.8244;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;87;-1208.211,119.4742;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;66;-393.6035,920.0998;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;88;-1171.458,-56.02241;Inherit;False;Property;_DisplaceHorizontal;Displace Horizontal;8;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector4Node;19;-779.4229,-372.42;Inherit;False;Property;_StripesXSpeedYThickZRotW;StripesX SpeedY ThickZ RotW;13;0;Create;True;0;0;0;False;0;False;100,1,0.5,90;100,5,0.69,90;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;5;-1003.059,327.5146;Inherit;True;Property;_HeightMap;Height Map;10;0;Create;True;0;0;0;False;0;False;-1;None;1089e2c5988bab14e8663e2f37d5488b;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;22;-1006.993,534.7032;Inherit;False;Property;_SmoothHeightXY;Smooth Height (XY);11;0;Create;True;0;0;0;False;0;False;0,1;0,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.FunctionNode;64;-345.815,741.2189;Inherit;False;Stripes;-1;;11;8e73a71cdf24db740864b4c3f3357e7f;0;4;5;FLOAT;6;False;4;FLOAT;0;False;3;FLOAT;0.5;False;12;FLOAT;45;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;72;-131.5236,950.0193;Inherit;False;Property;_GlitchSmoothXY;Glitch Smooth (XY);15;0;Create;True;0;0;0;False;0;False;0,1;0,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleTimeNode;25;-971.0588,-190.093;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;21;-644.988,387.7604;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;55;-704.5572,605.6772;Inherit;False;Property;_DepthIntensityXPowerY;Depth IntensityX PowerY;12;0;Create;True;0;0;0;False;0;False;0,0;0.5,0.3;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;70;-417.5236,608.0193;Inherit;False;Property;_GlitchScaleXPowerY;Glitch ScaleX PowerY;14;0;Create;True;0;0;0;False;0;False;0,0;1,0.2;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SmoothstepOpNode;71;-90.12357,802.5192;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;18;-586.4229,38.57996;Inherit;False;Property;_EmissiveColor;Emissive Color;2;0;Create;True;0;0;0;False;0;False;1,1,1,0;0.8687484,0.6921056,0.9716981,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;75;242.602,887.1328;Inherit;False;Property;_GlitchColor;GlitchColor;18;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;4;-545.2476,251.4807;Inherit;False;Property;_EmissionIntensity;Emission Intensity;3;0;Create;True;0;0;0;False;0;False;1;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;76;229.0862,790.0625;Inherit;False;Property;_GlitchIntensity;Glitch Intensity;20;0;Create;True;0;0;0;False;0;False;1;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;58;-1282.137,357.7174;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PowerNode;54;-479.9521,483.8088;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-585.0148,-151.9142;Inherit;True;Property;_MainTex;Albedo;0;0;Create;False;0;0;0;False;0;False;-1;None;c7cce12c240a262468640ced6f8c0b79;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;15;-458.8758,-317.4812;Inherit;False;Stripes;-1;;13;8e73a71cdf24db740864b4c3f3357e7f;0;4;5;FLOAT;6;False;4;FLOAT;0;False;3;FLOAT;0.5;False;12;FLOAT;45;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;38;-338.198,323.0317;Inherit;False;Property;_Opacity;Opacity;1;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;63;-170.3529,551.3173;Inherit;False;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;73;383.9068,528.3415;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;77;468.69,692.9921;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;47;-139.7801,-41.2599;Inherit;False;Property;_Gloss;Smoothness;6;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-331.2476,475.4807;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;32;-190.5958,-328.794;Inherit;False;Property;_ImageNoiseXY;Image Noise (XY);7;0;Create;True;0;0;0;False;0;False;0,1;-0.1,0.1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;68;67.47644,572.0193;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;74;560.8452,532.0276;Inherit;False;Property;_UseGlitchColor;Use Glitch Color;17;0;Create;True;0;0;0;False;0;False;0;0;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;-77.22491,191.7066;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;78;646.7422,674.29;Inherit;False;Property;_UseGlitchIntensity;Use Glitch Intensity;19;0;Create;True;0;0;0;False;0;False;0;0;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;31;-183.2964,-165.6917;Inherit;False;Noise Sine Wave;-1;;15;a6eff29f739ced848846e3b648af87bd;0;2;1;COLOR;0,0,0,0;False;2;FLOAT2;-0.5,0.5;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;49;67.76428,-47.63113;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;52;38.46594,394.1337;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;48;42.76428,83.36887;Inherit;False;Property;_Metallic;Metallic;5;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;3;240.5233,189.7566;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;506.0996,-144.7976;Float;False;True;-1;6;ASEMaterialInspector;0;0;Standard;DLNK Shaders/ASE/SciFi/Hologram;False;False;False;False;False;False;False;False;False;False;False;False;True;False;True;False;False;False;False;False;False;Off;0;False;;0;False;;False;0;False;;0;False;;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;True;0.5;False;2;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
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
WireConnection;66;0;65;2
WireConnection;88;1;86;0
WireConnection;88;0;87;0
WireConnection;5;1;88;0
WireConnection;64;5;65;1
WireConnection;64;4;66;0
WireConnection;64;3;65;3
WireConnection;64;12;65;4
WireConnection;25;0;19;2
WireConnection;21;0;5;1
WireConnection;21;1;22;1
WireConnection;21;2;22;2
WireConnection;71;0;64;0
WireConnection;71;1;72;1
WireConnection;71;2;72;2
WireConnection;58;0;81;0
WireConnection;58;2;82;0
WireConnection;58;1;79;3
WireConnection;54;0;21;0
WireConnection;54;1;55;2
WireConnection;1;1;88;0
WireConnection;15;5;19;1
WireConnection;15;4;25;0
WireConnection;15;3;19;3
WireConnection;15;12;19;4
WireConnection;63;0;58;0
WireConnection;63;1;70;1
WireConnection;73;0;18;0
WireConnection;73;1;75;0
WireConnection;73;2;71;0
WireConnection;77;0;4;0
WireConnection;77;1;76;0
WireConnection;77;2;71;0
WireConnection;6;0;54;0
WireConnection;6;1;55;1
WireConnection;68;0;71;0
WireConnection;68;1;63;0
WireConnection;68;2;70;2
WireConnection;74;1;18;0
WireConnection;74;0;73;0
WireConnection;20;0;1;4
WireConnection;20;1;15;0
WireConnection;20;2;38;0
WireConnection;78;1;4;0
WireConnection;78;0;77;0
WireConnection;31;1;1;0
WireConnection;31;2;32;0
WireConnection;49;0;20;0
WireConnection;49;1;47;0
WireConnection;52;1;6;0
WireConnection;52;2;68;0
WireConnection;3;0;1;0
WireConnection;3;1;78;0
WireConnection;3;2;74;0
WireConnection;0;0;31;0
WireConnection;0;2;3;0
WireConnection;0;3;48;0
WireConnection;0;4;49;0
WireConnection;0;9;20;0
WireConnection;0;11;52;0
ASEEND*/
//CHKSM=6D7AAA76D78D801342EE70F4F9E2436660A434E0