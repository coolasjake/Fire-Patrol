// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "FSP/SRP/Waterfall"
{
	Properties
	{
		_ShallowColor("Shallow Color", Color) = (0,0,0,0)
		_DeepColor("Deep Color", Color) = (0,0,0,0)
		_WaterGradientDepth("Water Gradient Depth", Range( 0 , 5)) = 1.294118
		_WaterTexture("Water Texture", 2D) = "white" {}
		_WaterTexturePlanarScale("Water Texture Planar Scale", Vector) = (1,1,0,0)
		_WaterTextureStrength("Water Texture Strength", Range( 0 , 1)) = 0.5
		_WaterFlowSpeed("Water Flow Speed", Vector) = (0.1,1.5,0,0)
		_Distortion("Distortion", 2D) = "white" {}
		_DistortionPlanarScale("Distortion Planar Scale", Float) = 1
		_DistortionSpeed("Distortion Speed", Vector) = (0.1,1.5,0,0)
		_DistortionAmount("Distortion Amount", Range( -10 , 10)) = 0.2
		[Header(Translucency)]
		_Translucency("Strength", Range( 0 , 50)) = 1
		_TransNormalDistortion("Normal Distortion", Range( 0 , 1)) = 0.1
		_TransScattering("Scaterring Falloff", Range( 1 , 50)) = 2
		_TransDirect("Direct", Range( 0 , 1)) = 1
		_TransAmbient("Ambient", Range( 0 , 1)) = 0.2
		_TransShadow("Shadow", Range( 0 , 1)) = 0.9
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Transparent+0" }
		Cull Off
		CGPROGRAM
		#include "UnityCG.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#pragma target 3.5
		#pragma surface surf StandardCustom keepalpha addshadow fullforwardshadows exclude_path:deferred 
		struct Input
		{
			float4 screenPos;
			float3 worldPos;
		};

		struct SurfaceOutputStandardCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			half3 Translucency;
		};

		uniform float4 _DeepColor;
		uniform float4 _ShallowColor;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _WaterGradientDepth;
		uniform sampler2D _WaterTexture;
		uniform sampler2D _Distortion;
		uniform float _DistortionPlanarScale;
		uniform float2 _DistortionSpeed;
		uniform float _DistortionAmount;
		uniform float2 _WaterFlowSpeed;
		uniform float2 _WaterTexturePlanarScale;
		uniform float _WaterTextureStrength;
		uniform half _Translucency;
		uniform half _TransNormalDistortion;
		uniform half _TransScattering;
		uniform half _TransDirect;
		uniform half _TransAmbient;
		uniform half _TransShadow;

		inline half4 LightingStandardCustom(SurfaceOutputStandardCustom s, half3 viewDir, UnityGI gi )
		{
			#if !DIRECTIONAL
			float3 lightAtten = gi.light.color;
			#else
			float3 lightAtten = lerp( _LightColor0.rgb, gi.light.color, _TransShadow );
			#endif
			half3 lightDir = gi.light.dir + s.Normal * _TransNormalDistortion;
			half transVdotL = pow( saturate( dot( viewDir, -lightDir ) ), _TransScattering );
			half3 translucency = lightAtten * (transVdotL * _TransDirect + gi.indirect.diffuse * _TransAmbient) * s.Translucency;
			half4 c = half4( s.Albedo * translucency * _Translucency, 0 );

			SurfaceOutputStandard r;
			r.Albedo = s.Albedo;
			r.Normal = s.Normal;
			r.Emission = s.Emission;
			r.Metallic = s.Metallic;
			r.Smoothness = s.Smoothness;
			r.Occlusion = s.Occlusion;
			r.Alpha = s.Alpha;
			return LightingStandard (r, viewDir, gi) + c;
		}

		inline void LightingStandardCustom_GI(SurfaceOutputStandardCustom s, UnityGIInput data, inout UnityGI gi )
		{
			#if defined(UNITY_PASS_DEFERRED) && UNITY_ENABLE_REFLECTION_BUFFERS
				gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal);
			#else
				UNITY_GLOSSY_ENV_FROM_SURFACE( g, s, data );
				gi = UnityGlobalIllumination( data, s.Occlusion, s.Normal, g );
			#endif
		}

		void surf( Input i , inout SurfaceOutputStandardCustom o )
		{
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth89 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth89 = abs( ( screenDepth89 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _WaterGradientDepth ) );
			float clampResult93 = clamp( ( ( 1.0 - distanceDepth89 ) * 1.0 ) , 0.0 , 1.0 );
			float4 lerpResult99 = lerp( _DeepColor , _ShallowColor , saturate( ( clampResult93 / 1.0 ) ));
			float3 ase_worldPos = i.worldPos;
			float2 appendResult399 = (float2(ase_worldPos.x , ase_worldPos.z));
			float4 tex2DNode402 = tex2D( _Distortion, (appendResult399*_DistortionPlanarScale + ( _Time.y * _DistortionSpeed )) );
			float2 appendResult403 = (float2(tex2DNode402.r , tex2DNode402.r));
			float2 distortionUV406 = ( appendResult403 * _DistortionAmount );
			float2 appendResult300 = (float2(( ase_worldPos.x * ase_worldPos.z ) , ( distortionUV406 + ase_worldPos.y ).x));
			float2 appendResult299 = (float2(( _Time.y * _WaterFlowSpeed.x ) , ( _Time.y * _WaterFlowSpeed.y )));
			float4 lerpResult59 = lerp( lerpResult99 , tex2D( _WaterTexture, ( ( appendResult300 + appendResult299 ) * _WaterTexturePlanarScale ) ) , _WaterTextureStrength);
			o.Albedo = lerpResult59.rgb;
			float3 temp_cast_2 = (1.0).xxx;
			o.Translucency = temp_cast_2;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18800
0;73.6;1182;439;5610.09;3723.181;2.659553;True;True
Node;AmplifyShaderEditor.CommentaryNode;393;-4729.658,-3382.679;Inherit;False;2080.35;621.9257;;13;406;405;404;403;402;401;400;399;398;397;396;395;394;Distortion;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleTimeNode;394;-4647.785,-3023.416;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;395;-4677.462,-3274.736;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector2Node;396;-4638.71,-2930.352;Float;False;Property;_DistortionSpeed;Distortion Speed;9;0;Create;True;0;0;0;False;0;False;0.1,1.5;0.1,1.5;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;398;-4373.937,-2989.108;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;397;-4392.659,-3125.171;Float;False;Property;_DistortionPlanarScale;Distortion Planar Scale;8;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;399;-4412.099,-3271.465;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;111;-4739.922,-2550.294;Inherit;False;2206.721;1537.672;;4;106;79;87;390;Water;0,0.7294118,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;87;-4654.518,-2355.079;Inherit;False;1501.52;501.4431;;12;99;101;100;102;103;105;93;92;107;90;89;88;Depth Gradient;0,0.7294118,1,1;0;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;400;-3890.793,-3047.52;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT;1;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;401;-3950.498,-3304.271;Inherit;True;Property;_Distortion;Distortion;7;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;88;-4616.822,-2139.204;Inherit;False;Property;_WaterGradientDepth;Water Gradient Depth;2;0;Create;True;0;0;0;False;0;False;1.294118;1.294118;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;402;-3653.315,-3138.272;Inherit;True;Property;_TextureSample2;Texture Sample 2;5;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;404;-3327.19,-2914.662;Float;False;Property;_DistortionAmount;Distortion Amount;10;0;Create;True;0;0;0;False;0;False;0.2;0;-10;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;89;-4343.347,-2157.062;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;403;-3178.901,-3148.329;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;107;-4266.097,-2053.605;Inherit;False;Constant;_WaterGradientTransitionSharpness;Water Gradient Transition Sharpness;14;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;90;-4102.517,-2132.398;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;405;-3016.74,-3026.083;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;79;-4646.457,-1729.544;Inherit;False;1583.647;685.9576;;15;327;326;299;304;1;297;20;292;298;302;300;296;306;305;294;Texture;0,0.7294118,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;92;-3936.647,-2091.846;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;406;-2865.154,-3032.636;Inherit;False;distortionUV;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;305;-4597.99,-1495.214;Inherit;False;406;distortionUV;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;105;-3917.08,-1957.502;Inherit;False;Constant;_WaterDepthColorTransition;Water Depth Color Transition;2;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;20;-4600.602,-1197.656;Inherit;False;Property;_WaterFlowSpeed;Water Flow Speed;6;0;Create;True;0;0;0;False;0;False;0.1,1.5;0.1,1.5;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleTimeNode;292;-4598.299,-1305.798;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;93;-3782.958,-2087.144;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;294;-4600.438,-1668.471;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;306;-4350.272,-1672.402;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;298;-4355.227,-1292.651;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;296;-4348.838,-1519.484;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;297;-4352.699,-1168.602;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;103;-3609.148,-2088.024;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;299;-4166.765,-1292.624;Inherit;True;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;300;-4149.888,-1615.268;Inherit;True;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ColorNode;100;-3859.629,-2282.5;Inherit;False;Property;_ShallowColor;Shallow Color;0;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;102;-3487.761,-2087.519;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;101;-3622.537,-2281.401;Inherit;False;Property;_DeepColor;Deep Color;1;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;302;-3842.586,-1603.977;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;326;-3900.818,-1224.769;Inherit;False;Property;_WaterTexturePlanarScale;Water Texture Planar Scale;4;0;Create;True;0;0;0;False;0;False;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.LerpOp;99;-3328.72,-2222.353;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TexturePropertyNode;1;-3651.139,-1662.308;Inherit;True;Property;_WaterTexture;Water Texture;3;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.WireNode;390;-2831.583,-2075.109;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;327;-3598.31,-1397.494;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;106;-3022.154,-1729.068;Inherit;False;434.5587;318.2333;;3;59;84;391;Blend;0,0.7294118,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;304;-3368.549,-1663.779;Inherit;True;Property;_TextureSample3;Texture Sample 3;13;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;391;-2864.399,-1686.02;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;84;-2995.219,-1487.167;Inherit;False;Property;_WaterTextureStrength;Water Texture Strength;5;0;Create;True;0;0;0;False;0;False;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;59;-2743.996,-1672.99;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;176;-2490.945,-1371.095;Inherit;False;Constant;_TranslucentPower;Translucent Power;18;0;Create;True;0;0;0;False;0;False;1;1;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-2091.289,-1636.561;Float;False;True;-1;3;ASEMaterialInspector;0;0;Standard;FSP/SRP/Waterfall;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Translucent;0.5;True;True;0;False;Opaque;;Transparent;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;11;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;398;0;394;0
WireConnection;398;1;396;0
WireConnection;399;0;395;1
WireConnection;399;1;395;3
WireConnection;400;0;399;0
WireConnection;400;1;397;0
WireConnection;400;2;398;0
WireConnection;402;0;401;0
WireConnection;402;1;400;0
WireConnection;89;0;88;0
WireConnection;403;0;402;1
WireConnection;403;1;402;1
WireConnection;90;0;89;0
WireConnection;405;0;403;0
WireConnection;405;1;404;0
WireConnection;92;0;90;0
WireConnection;92;1;107;0
WireConnection;406;0;405;0
WireConnection;93;0;92;0
WireConnection;306;0;294;1
WireConnection;306;1;294;3
WireConnection;298;0;292;0
WireConnection;298;1;20;1
WireConnection;296;0;305;0
WireConnection;296;1;294;2
WireConnection;297;0;292;0
WireConnection;297;1;20;2
WireConnection;103;0;93;0
WireConnection;103;1;105;0
WireConnection;299;0;298;0
WireConnection;299;1;297;0
WireConnection;300;0;306;0
WireConnection;300;1;296;0
WireConnection;102;0;103;0
WireConnection;302;0;300;0
WireConnection;302;1;299;0
WireConnection;99;0;101;0
WireConnection;99;1;100;0
WireConnection;99;2;102;0
WireConnection;390;0;99;0
WireConnection;327;0;302;0
WireConnection;327;1;326;0
WireConnection;304;0;1;0
WireConnection;304;1;327;0
WireConnection;391;0;390;0
WireConnection;59;0;391;0
WireConnection;59;1;304;0
WireConnection;59;2;84;0
WireConnection;0;0;59;0
WireConnection;0;7;176;0
ASEEND*/
//CHKSM=AB0917A45B3E30B43E57FA07995719AE6386BBD8