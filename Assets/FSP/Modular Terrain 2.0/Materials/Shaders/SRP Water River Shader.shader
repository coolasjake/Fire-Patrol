// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "FSP/SRP/Water River"
{
	Properties
	{
		_ShallowColor("Shallow Color", Color) = (0,0,0,0)
		_DeepColor("Deep Color", Color) = (0,0,0,0)
		_WaterGradientDepth("Water Gradient Depth", Range( 0 , 5)) = 1.294118
		_WaterTexture("Water Texture", 2D) = "white" {}
		_WaterTextureStrength("Water Texture Strength", Range( 0 , 1)) = 0.5
		_WaterFlowSpeed("Water Flow Speed", Vector) = (0.1,1.5,0,0)
		_Distortion("Distortion", 2D) = "white" {}
		_DistortionPlanarScale("Distortion Planar Scale", Float) = 1
		_DistortionSpeed("Distortion Speed", Vector) = (0.1,1.5,0,0)
		_DistortionAmount("Distortion Amount", Range( -10 , 10)) = 0.2
		_Foam("Foam", 2D) = "white" {}
		_FoamPlanarScale("Foam Planar Scale", Float) = 1
		_FoamColor("Foam Color", Color) = (0.7924528,0.7924528,0.7924528,0)
		_PlanarFoamSpeed("Planar Foam Speed", Vector) = (0.1,0.1,0,0)
		_FoamSoftness("Foam Softness", Range( 0 , 1)) = 0
		_EdgeDistance("Edge Distance", Range( 0 , 1)) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Transparent+0" }
		Cull Off
		CGPROGRAM
		#include "UnityCG.cginc"
		#include "UnityShaderVariables.cginc"
		#pragma target 3.5
		#pragma surface surf Standard keepalpha noshadow exclude_path:deferred 
		struct Input
		{
			float4 screenPos;
			float2 uv_texcoord;
			float3 worldPos;
		};

		uniform float4 _FoamColor;
		uniform float4 _DeepColor;
		uniform float4 _ShallowColor;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _WaterGradientDepth;
		uniform sampler2D _WaterTexture;
		uniform float2 _WaterFlowSpeed;
		uniform sampler2D _Distortion;
		uniform float _DistortionPlanarScale;
		uniform float2 _DistortionSpeed;
		uniform float _DistortionAmount;
		uniform float _WaterTextureStrength;
		uniform float _EdgeDistance;
		uniform float _FoamSoftness;
		uniform sampler2D _Foam;
		uniform float _FoamPlanarScale;
		uniform float2 _PlanarFoamSpeed;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 foamAlbedo136 = _FoamColor;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth89 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth89 = abs( ( screenDepth89 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _WaterGradientDepth ) );
			float clampResult93 = clamp( ( ( 1.0 - distanceDepth89 ) * 1.0 ) , 0.0 , 1.0 );
			float4 lerpResult99 = lerp( _DeepColor , _ShallowColor , saturate( ( clampResult93 / 1.0 ) ));
			float3 ase_worldPos = i.worldPos;
			float2 appendResult216 = (float2(ase_worldPos.x , ase_worldPos.z));
			float4 tex2DNode220 = tex2D( _Distortion, (appendResult216*_DistortionPlanarScale + ( _Time.y * _DistortionSpeed )) );
			float2 appendResult221 = (float2(tex2DNode220.r , tex2DNode220.r));
			float2 distortionUV224 = ( appendResult221 * _DistortionAmount );
			float2 panner19 = ( 1.0 * _Time.y * _WaterFlowSpeed + ( i.uv_texcoord + distortionUV224 ));
			float4 lerpResult59 = lerp( lerpResult99 , tex2D( _WaterTexture, panner19 ) , _WaterTextureStrength);
			float4 waterAlbedo110 = lerpResult59;
			float4 blendOpSrc135 = foamAlbedo136;
			float4 blendOpDest135 = waterAlbedo110;
			float screenDepth27 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth27 = abs( ( screenDepth27 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _EdgeDistance ) );
			float2 appendResult200 = (float2(ase_worldPos.x , ase_worldPos.y));
			float2 appendResult206 = (float2(( _Time.y * _PlanarFoamSpeed.x ) , ( _Time.y * _PlanarFoamSpeed.y )));
			float smoothstepResult85 = smoothstep( ( distanceDepth27 - _FoamSoftness ) , ( distanceDepth27 + _FoamSoftness ) , tex2D( _Foam, (appendResult200*_FoamPlanarScale + appendResult206) ).r);
			float foamTransparency138 = smoothstepResult85;
			float4 lerpBlendMode135 = lerp(blendOpDest135,	max( blendOpSrc135, blendOpDest135 ),foamTransparency138);
			o.Albedo = ( saturate( lerpBlendMode135 )).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18800
0;73.6;1182;439;3032.99;1084.376;4.234104;True;True
Node;AmplifyShaderEditor.CommentaryNode;209;-725.7512,79.43694;Inherit;False;2080.35;621.9257;;13;224;223;222;221;220;219;218;216;215;214;212;211;210;Distortion;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldPosInputsNode;212;-673.5551,187.3795;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector2Node;210;-634.8029,531.7642;Inherit;False;Property;_DistortionSpeed;Distortion Speed;8;0;Create;True;0;0;0;False;0;False;0.1,1.5;0.1,1.5;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleTimeNode;211;-643.8781,438.6995;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;111;-3102.669,-2065.698;Inherit;False;2659.104;1316.396;;3;106;79;87;Water;0,0.7294118,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;215;-194.6768,195.498;Inherit;False;Property;_DistortionPlanarScale;Distortion Planar Scale;7;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;214;-370.0291,473.0078;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;216;-408.1919,190.6505;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;219;53.4083,157.8442;Inherit;True;Property;_Distortion;Distortion;6;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.ScaleAndOffsetNode;218;113.1131,414.5959;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT;1;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;87;-3017.265,-1870.483;Inherit;False;1501.52;501.4431;;12;99;101;100;102;103;105;93;92;107;90;89;88;Depth Gradient;0,0.7294118,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;88;-2979.569,-1654.608;Inherit;False;Property;_WaterGradientDepth;Water Gradient Depth;2;0;Create;True;0;0;0;False;0;False;1.294118;1.294118;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;220;350.5912,323.8433;Inherit;True;Property;_TextureSample2;Texture Sample 2;5;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;221;825.0051,313.7871;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;108;-3088.349,-548.1245;Inherit;False;2231.973;1305.925;;4;140;112;83;78;Foam;1,0.9986275,0,1;0;0
Node;AmplifyShaderEditor.DepthFade;89;-2706.094,-1672.466;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;222;676.7162,547.4537;Inherit;False;Property;_DistortionAmount;Distortion Amount;9;0;Create;True;0;0;0;False;0;False;0.2;0;-10;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;223;987.1657,436.0327;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;83;-2987.931,106.4198;Inherit;False;1355.892;548.6949;;11;208;207;203;202;200;201;2;41;199;205;206;Planar Texture;1,0.9986275,0,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;107;-2628.845,-1569.009;Inherit;False;Constant;_WaterGradientTransitionSharpness;Water Gradient Transition Sharpness;14;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;90;-2465.266,-1647.802;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;224;1138.752,429.4796;Inherit;False;distortionUV;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;205;-2956.551,506.1563;Inherit;False;Property;_PlanarFoamSpeed;Planar Foam Speed;13;0;Create;True;0;0;0;False;0;False;0.1,0.1;0.1,0.1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleTimeNode;203;-2936.906,414.6167;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;79;-2692.633,-1225.634;Inherit;False;1392.876;394.9987;;7;19;1;21;20;18;165;166;Texture;0,0.7294118,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;92;-2299.396,-1607.25;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;208;-2667.99,532.733;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;165;-2602.666,-1019.827;Inherit;False;224;distortionUV;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WorldPosInputsNode;199;-2966.583,163.2966;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;78;-2993.507,-338.5561;Inherit;False;550.0496;174.0682;;2;27;26;Edge Calc;1,1,0,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;105;-2279.829,-1472.906;Inherit;False;Constant;_WaterDepthColorTransition;Water Depth Color Transition;2;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;93;-2145.707,-1602.548;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;18;-2662.296,-1169.267;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;207;-2674.757,391.725;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;166;-2325.162,-1087.427;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;112;-2376.236,-340.722;Inherit;False;1427.679;388.065;;5;138;52;172;171;85;Blend;1,1,0,1;0;0
Node;AmplifyShaderEditor.Vector2Node;20;-2361.45,-965.3043;Inherit;False;Property;_WaterFlowSpeed;Water Flow Speed;5;0;Create;True;0;0;0;False;0;False;0.1,1.5;0.1,1.5;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;26;-2963.313,-259.1061;Inherit;False;Property;_EdgeDistance;Edge Distance;15;0;Create;True;0;0;0;False;0;False;1;1.294118;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;206;-2453.252,409.0256;Inherit;True;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;200;-2701.22,166.5676;Inherit;True;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;202;-2453.905,255.9154;Inherit;False;Property;_FoamPlanarScale;Foam Planar Scale;11;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;103;-1971.897,-1603.428;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;201;-2179.915,390.5132;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT;1;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ColorNode;101;-1985.286,-1796.805;Inherit;False;Property;_DeepColor;Deep Color;1;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;52;-2336.27,-55.62766;Inherit;False;Property;_FoamSoftness;Foam Softness;14;0;Create;True;0;0;0;False;0;False;0;0.02;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;19;-1971.602,-961.4827;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;2;-2217.271,155.1527;Inherit;True;Property;_Foam;Foam;10;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.CommentaryNode;106;-1188.165,-1220.612;Inherit;False;646.4207;324.5763;;3;59;84;110;Blend;0,0.7294118,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;100;-2222.378,-1797.904;Inherit;False;Property;_ShallowColor;Shallow Color;0;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DepthFade;27;-2689.837,-279.419;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;1;-1969.245,-1166.808;Inherit;True;Property;_WaterTexture;Water Texture;3;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SaturateNode;102;-1850.51,-1602.923;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;172;-1990.205,-165.387;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;84;-1161.231,-978.7136;Inherit;False;Property;_WaterTextureStrength;Water Texture Strength;4;0;Create;True;0;0;0;False;0;False;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;140;-1562.021,293.5101;Inherit;False;562.9144;250.6273;;2;136;114;Color;1,1,0,1;0;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;171;-2014.274,-294.4956;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;99;-1691.469,-1737.757;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;21;-1713.438,-1066.013;Inherit;True;Property;_TextureSample0;Texture Sample 0;5;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;41;-1947.934,266.4174;Inherit;True;Property;_TextureSample1;Texture Sample 1;6;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;114;-1530.455,358.6515;Inherit;False;Property;_FoamColor;Foam Color;12;0;Create;True;0;0;0;False;0;False;0.7924528,0.7924528,0.7924528,0;0.7924528,0.7924528,0.7924528,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;59;-910.0065,-1164.536;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SmoothstepOpNode;85;-1543.964,-103.7973;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;138;-1216.409,-267.3874;Inherit;False;foamTransparency;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;136;-1205.766,368.6271;Inherit;False;foamAlbedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;110;-741.017,-1030.034;Inherit;False;waterAlbedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;139;-28.1859,-439.6622;Inherit;False;138;foamTransparency;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;109;-19.24583,-535.655;Inherit;False;110;waterAlbedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;137;22.30683,-661.477;Inherit;False;136;foamAlbedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.BlendOpsNode;135;272.7241,-553.8171;Inherit;False;Lighten;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;522.4627,-533.7786;Float;False;True;-1;3;ASEMaterialInspector;0;0;Standard;FSP/SRP/Water River;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Translucent;0.5;True;False;0;False;Opaque;;Transparent;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;214;0;211;0
WireConnection;214;1;210;0
WireConnection;216;0;212;1
WireConnection;216;1;212;3
WireConnection;218;0;216;0
WireConnection;218;1;215;0
WireConnection;218;2;214;0
WireConnection;220;0;219;0
WireConnection;220;1;218;0
WireConnection;221;0;220;1
WireConnection;221;1;220;1
WireConnection;89;0;88;0
WireConnection;223;0;221;0
WireConnection;223;1;222;0
WireConnection;90;0;89;0
WireConnection;224;0;223;0
WireConnection;92;0;90;0
WireConnection;92;1;107;0
WireConnection;208;0;203;0
WireConnection;208;1;205;2
WireConnection;93;0;92;0
WireConnection;207;0;203;0
WireConnection;207;1;205;1
WireConnection;166;0;18;0
WireConnection;166;1;165;0
WireConnection;206;0;207;0
WireConnection;206;1;208;0
WireConnection;200;0;199;1
WireConnection;200;1;199;2
WireConnection;103;0;93;0
WireConnection;103;1;105;0
WireConnection;201;0;200;0
WireConnection;201;1;202;0
WireConnection;201;2;206;0
WireConnection;19;0;166;0
WireConnection;19;2;20;0
WireConnection;27;0;26;0
WireConnection;102;0;103;0
WireConnection;172;0;27;0
WireConnection;172;1;52;0
WireConnection;171;0;27;0
WireConnection;171;1;52;0
WireConnection;99;0;101;0
WireConnection;99;1;100;0
WireConnection;99;2;102;0
WireConnection;21;0;1;0
WireConnection;21;1;19;0
WireConnection;41;0;2;0
WireConnection;41;1;201;0
WireConnection;59;0;99;0
WireConnection;59;1;21;0
WireConnection;59;2;84;0
WireConnection;85;0;41;1
WireConnection;85;1;171;0
WireConnection;85;2;172;0
WireConnection;138;0;85;0
WireConnection;136;0;114;0
WireConnection;110;0;59;0
WireConnection;135;0;137;0
WireConnection;135;1;109;0
WireConnection;135;2;139;0
WireConnection;0;0;135;0
ASEEND*/
//CHKSM=A28A0846AC8061367C22BA72B53F7EAC1BB9BBC9