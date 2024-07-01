// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "DLNK Shaders/ASE/SciFi/ScreenGlitchy"
{
	Properties
	{
		_MainTex("Albedo", 2D) = "white" {}
		_Smoothness("Smoothness", Float) = 0
		_StripesIntensity("Stripes Intensity", Float) = 1
		_EmissiveColor("Emissive Color", Color) = (0.5943396,0.5943396,0.5943396,0)
		_EmissionIntensity("Emission Intensity", Float) = 0.5
		_PannerSpeed("Panner Speed", Float) = 0.1
		_StripesXSpeedYThickZRotW("StripesX SpeedY ThickZ RotW", Vector) = (100,-10,0.5,90)
		_GlitchSmoothXY("Glitch Smooth (XY)", Vector) = (0,1,0,0)
		_GlitchXSpeedYThickZRotW("GlitchX SpeedY ThickZ RotW", Vector) = (2,1,0.1,90)
		[Toggle(_USEGLITCHCOLOR_ON)] _UseGlitchColor("Use Glitch Color", Float) = 1
		_GlitchColor("GlitchColor", Color) = (1,1,1,0)
		[Toggle(_USEGLITCHINTENSITY_ON)] _UseGlitchIntensity("Use Glitch Intensity", Float) = 1
		_GlitchIntensity("Glitch Intensity", Float) = 2
		[Toggle(_DISPLACECONTINUOUS_ON)] _DisplaceContinuous("Displace Continuous", Float) = 0
		[Toggle(_DISPLACEHORIZONTAL_ON)] _DisplaceHorizontal("Displace Horizontal", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _DISPLACEHORIZONTAL_ON
		#pragma shader_feature_local _DISPLACECONTINUOUS_ON
		#pragma shader_feature_local _USEGLITCHINTENSITY_ON
		#pragma shader_feature_local _USEGLITCHCOLOR_ON
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows noshadow dithercrossfade 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _MainTex;
		uniform float _PannerSpeed;
		uniform float4 _StripesXSpeedYThickZRotW;
		uniform float _StripesIntensity;
		uniform float _EmissionIntensity;
		uniform float _GlitchIntensity;
		uniform float2 _GlitchSmoothXY;
		uniform float4 _GlitchXSpeedYThickZRotW;
		uniform float4 _EmissiveColor;
		uniform float4 _GlitchColor;
		uniform float _Smoothness;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			#ifdef _DISPLACECONTINUOUS_ON
				float staticSwitch80 = _Time.y;
			#else
				float staticSwitch80 = _CosTime.z;
			#endif
			float2 temp_cast_0 = (_PannerSpeed).xx;
			float2 temp_cast_1 = (i.uv_texcoord.y).xx;
			float2 panner9 = ( staticSwitch80 * temp_cast_0 + temp_cast_1);
			float2 appendResult14 = (float2(i.uv_texcoord.x , panner9.x));
			float2 temp_cast_3 = (_PannerSpeed).xx;
			float2 temp_cast_4 = (i.uv_texcoord.x).xx;
			float2 panner83 = ( staticSwitch80 * temp_cast_3 + temp_cast_4);
			float2 appendResult88 = (float2(panner83.x , i.uv_texcoord.y));
			#ifdef _DISPLACEHORIZONTAL_ON
				float2 staticSwitch82 = appendResult88;
			#else
				float2 staticSwitch82 = appendResult14;
			#endif
			float4 tex2DNode1 = tex2D( _MainTex, staticSwitch82 );
			float cos10_g13 = cos( radians( _StripesXSpeedYThickZRotW.w ) );
			float sin10_g13 = sin( radians( _StripesXSpeedYThickZRotW.w ) );
			float2 rotator10_g13 = mul( i.uv_texcoord - float2( 0.5,0.5 ) , float2x2( cos10_g13 , -sin10_g13 , sin10_g13 , cos10_g13 )) + float2( 0.5,0.5 );
			float2 appendResult8_g13 = (float2(_StripesXSpeedYThickZRotW.x , 1.0));
			float mulTime25 = _Time.y * _StripesXSpeedYThickZRotW.y;
			float2 appendResult9_g13 = (float2(mulTime25 , 0.0));
			float2 appendResult10_g14 = (float2(_StripesXSpeedYThickZRotW.z , 1.0));
			float2 temp_output_11_0_g14 = ( abs( (frac( (rotator10_g13*appendResult8_g13 + appendResult9_g13) )*2.0 + -1.0) ) - appendResult10_g14 );
			float2 break16_g14 = ( 1.0 - ( temp_output_11_0_g14 / fwidth( temp_output_11_0_g14 ) ) );
			o.Albedo = ( tex2DNode1 * saturate( min( break16_g14.x , break16_g14.y ) ) * _StripesIntensity ).rgb;
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
			o.Metallic = 0.0;
			o.Smoothness = _Smoothness;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.CosTime;12;-1545.248,130.4807;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;81;-1724.324,353.9684;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;65;-1179.181,602.3859;Inherit;False;Property;_GlitchXSpeedYThickZRotW;GlitchX SpeedY ThickZ RotW;8;0;Create;True;0;0;0;False;0;False;2,1,0.1,90;2,1,0.5,90;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;8;-1502.41,-20.81871;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;13;-1604.995,288.4049;Inherit;False;Property;_PannerSpeed;Panner Speed;5;0;Create;True;0;0;0;False;0;False;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;80;-1523.397,393.4456;Inherit;False;Property;_DisplaceContinuous;Displace Continuous;13;0;Create;True;0;0;0;False;0;False;0;0;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;66;-862.9035,705.5998;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;9;-1302.248,163.4807;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;83;-1216.577,370.7172;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;64;-820.3151,535.8188;Inherit;False;Stripes;-1;;11;8e73a71cdf24db740864b4c3f3357e7f;0;4;5;FLOAT;6;False;4;FLOAT;0;False;3;FLOAT;0.5;False;12;FLOAT;45;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;72;-574.8235,708.2194;Inherit;False;Property;_GlitchSmoothXY;Glitch Smooth (XY);7;0;Create;True;0;0;0;False;0;False;0,1;-2,2;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector4Node;19;-779.4229,-372.42;Inherit;False;Property;_StripesXSpeedYThickZRotW;StripesX SpeedY ThickZ RotW;6;0;Create;True;0;0;0;False;0;False;100,-10,0.5,90;100,-10,0.5,90;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;14;-1080.366,-50.58148;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;88;-1011.577,311.7172;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ColorNode;75;-226.698,672.6328;Inherit;False;Property;_GlitchColor;GlitchColor;10;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,0.5056796,0.3537736,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;76;-245.4138,601.5625;Inherit;False;Property;_GlitchIntensity;Glitch Intensity;12;0;Create;True;0;0;0;False;0;False;2;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;71;-530.8237,550.3191;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-523.1477,410.0807;Inherit;False;Property;_EmissionIntensity;Emission Intensity;4;0;Create;True;0;0;0;False;0;False;0.5;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;18;-468.1229,214.08;Inherit;False;Property;_EmissiveColor;Emissive Color;3;0;Create;True;0;0;0;False;0;False;0.5943396,0.5943396,0.5943396,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;25;-971.0588,-190.093;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;82;-974.8234,136.2206;Inherit;False;Property;_DisplaceHorizontal;Displace Horizontal;14;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;73;-126.9932,376.2416;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;77;25.98999,479.3925;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;15;-458.8758,-317.4812;Inherit;False;Stripes;-1;;13;8e73a71cdf24db740864b4c3f3357e7f;0;4;5;FLOAT;6;False;4;FLOAT;0;False;3;FLOAT;0.5;False;12;FLOAT;45;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-627.9148,-67.41421;Inherit;True;Property;_MainTex;Albedo;0;0;Create;False;0;0;0;False;0;False;-1;None;16a58ab2782cfc84cb3c305e4245cd8d;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;38;-188.3486,-262.8178;Inherit;False;Property;_StripesIntensity;Stripes Intensity;2;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;78;218.4824,397.2338;Inherit;False;Property;_UseGlitchIntensity;Use Glitch Intensity;11;0;Create;True;0;0;0;False;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;74;-13.45483,195.9276;Inherit;False;Property;_UseGlitchColor;Use Glitch Color;9;0;Create;True;0;0;0;False;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;3;313.2232,-34.54338;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;-16.65443,-135.4545;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;91;195.1075,-186.75;Inherit;False;Constant;_Float0;Float 0;14;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;92;173.5598,-112.5292;Inherit;False;Property;_Smoothness;Smoothness;1;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;90;604.8995,-169.4976;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;DLNK Shaders/ASE/SciFi/ScreenGlitchy;False;False;False;False;False;False;False;False;False;False;False;False;True;False;True;False;False;False;False;False;False;Off;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;False;0;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;80;1;12;3
WireConnection;80;0;81;0
WireConnection;66;0;65;2
WireConnection;9;0;8;2
WireConnection;9;2;13;0
WireConnection;9;1;80;0
WireConnection;83;0;8;1
WireConnection;83;2;13;0
WireConnection;83;1;80;0
WireConnection;64;5;65;1
WireConnection;64;4;66;0
WireConnection;64;3;65;3
WireConnection;64;12;65;4
WireConnection;14;0;8;1
WireConnection;14;1;9;0
WireConnection;88;0;83;0
WireConnection;88;1;8;2
WireConnection;71;0;64;0
WireConnection;71;1;72;1
WireConnection;71;2;72;2
WireConnection;25;0;19;2
WireConnection;82;1;14;0
WireConnection;82;0;88;0
WireConnection;73;0;18;0
WireConnection;73;1;75;0
WireConnection;73;2;71;0
WireConnection;77;0;4;0
WireConnection;77;1;76;0
WireConnection;77;2;71;0
WireConnection;15;5;19;1
WireConnection;15;4;25;0
WireConnection;15;3;19;3
WireConnection;15;12;19;4
WireConnection;1;1;82;0
WireConnection;78;1;4;0
WireConnection;78;0;77;0
WireConnection;74;1;18;0
WireConnection;74;0;73;0
WireConnection;3;0;1;0
WireConnection;3;1;78;0
WireConnection;3;2;74;0
WireConnection;20;0;1;0
WireConnection;20;1;15;0
WireConnection;20;2;38;0
WireConnection;90;0;20;0
WireConnection;90;2;3;0
WireConnection;90;3;91;0
WireConnection;90;4;92;0
ASEEND*/
//CHKSM=A7D80AE49B988A02565A028C489F19DD1473092D