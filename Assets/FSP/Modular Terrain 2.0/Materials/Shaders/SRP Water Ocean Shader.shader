// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "FSP/SRP/Water Ocean"
{
	Properties
	{
		_Smoothness("Smoothness", Range( -1 , 1)) = 0.2
		_ShallowColor("Shallow Color", Color) = (0,0,0,0)
		_DeepColor("Deep Color", Color) = (0,0,0,0)
		_WaterGradientDepth("Water Gradient Depth", Range( 0 , 5)) = 1.294118
		_WaterTexture("Water Texture", 2D) = "white" {}
		_WaterTextureStrength("Water Texture Strength", Range( 0 , 1)) = 0.5
		_WaterFlowSpeed("Water Flow Speed", Vector) = (0.1,1.5,0,0)
		_Foam("Foam", 2D) = "white" {}
		_FoamColor("Foam Color", Color) = (0.7924528,0.7924528,0.7924528,0)
		_FoamPlanarScale("Foam Planar Scale", Float) = 1
		_PlanarFoamMovement("Planar Foam Movement", Vector) = (0.1,0.1,0,0)
		_FoamSoftness("Foam Softness", Range( 0 , 1)) = 0
		_EdgeDistance("Edge Distance", Range( 0 , 1)) = 1
		_Distortion("Distortion", 2D) = "white" {}
		_DistortionPlanarScale("Distortion Planar Scale", Float) = 1
		_DistortionSpeed("Distortion Speed", Vector) = (0.1,1.5,0,0)
		_DistortionAmount("Distortion Amount", Range( -10 , 10)) = 0.2
		_WavesHeightmap("Waves Heightmap", 2D) = "white" {}
		_WaveHeight("Wave Height", Float) = 0.2
		_WaveSpeed("Wave Speed", Vector) = (0,0.05,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Transparent+0" }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#pragma target 3.5
		#pragma surface surf Standard keepalpha noshadow exclude_path:deferred vertex:vertexDataFunc 
		struct Input
		{
			float4 screenPos;
			float2 uv_texcoord;
			float3 worldPos;
		};

		uniform sampler2D _WavesHeightmap;
		uniform float2 _WaveSpeed;
		uniform float4 _WavesHeightmap_ST;
		uniform float _WaveHeight;
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
		uniform float2 _PlanarFoamMovement;
		uniform float _Smoothness;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float2 uv_WavesHeightmap = v.texcoord.xy * _WavesHeightmap_ST.xy + _WavesHeightmap_ST.zw;
			float2 panner218 = ( 1.0 * _Time.y * _WaveSpeed + uv_WavesHeightmap);
			float3 ase_vertexNormal = v.normal.xyz;
			float4 appendResult229 = (float4(0.0 , 0.0 , ( ( tex2Dlod( _WavesHeightmap, float4( panner218, 0, 0.0) ).r * _WaveHeight ) + ase_vertexNormal.y ) , 0.0));
			float4 waveOffset239 = appendResult229;
			v.vertex.xyz += waveOffset239.xyz;
			v.vertex.w = 1;
		}

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
			float2 appendResult262 = (float2(ase_worldPos.x , ase_worldPos.z));
			float2 appendResult261 = (float2(( _Time.y * _DistortionSpeed.x ) , ( _Time.y * _DistortionSpeed.y )));
			float4 tex2DNode53 = tex2D( _Distortion, (appendResult262*_DistortionPlanarScale + appendResult261) );
			float2 appendResult162 = (float2(tex2DNode53.r , tex2DNode53.r));
			float2 distortionUV58 = ( appendResult162 * _DistortionAmount );
			float2 panner19 = ( 1.0 * _Time.y * _WaterFlowSpeed + ( i.uv_texcoord + distortionUV58 ));
			float4 lerpResult59 = lerp( lerpResult99 , tex2D( _WaterTexture, panner19 ) , _WaterTextureStrength);
			float4 waterAlbedo110 = lerpResult59;
			float4 blendOpSrc135 = foamAlbedo136;
			float4 blendOpDest135 = waterAlbedo110;
			float screenDepth27 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth27 = abs( ( screenDepth27 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _EdgeDistance ) );
			float2 appendResult200 = (float2(ase_worldPos.x , ase_worldPos.z));
			float2 appendResult206 = (float2(( _Time.y * _PlanarFoamMovement.x ) , ( _Time.y * _PlanarFoamMovement.y )));
			float smoothstepResult85 = smoothstep( ( distanceDepth27 - _FoamSoftness ) , ( distanceDepth27 + _FoamSoftness ) , tex2D( _Foam, (appendResult200*_FoamPlanarScale + appendResult206) ).r);
			float foamTransparency138 = smoothstepResult85;
			float4 lerpBlendMode135 = lerp(blendOpDest135,	max( blendOpSrc135, blendOpDest135 ),foamTransparency138);
			o.Albedo = ( saturate( lerpBlendMode135 )).rgb;
			o.Smoothness = _Smoothness;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18800
0;73.6;1182;439;2692.015;799.8316;4.031147;True;True
Node;AmplifyShaderEditor.CommentaryNode;95;-716.4376,121.39;Inherit;False;2080.35;621.9257;;15;53;3;57;257;258;259;260;261;262;263;255;58;164;162;163;Distortion;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector2Node;57;-625.4894,573.7174;Inherit;False;Property;_DistortionSpeed;Distortion Speed;15;0;Create;True;0;0;0;False;0;False;0.1,1.5;0.1,1.5;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.CommentaryNode;210;-732.7111,-606.8791;Inherit;False;1513.793;570.5061;;17;245;246;239;229;213;230;243;244;242;241;217;222;212;215;216;218;221;Waves;0.3093628,0.8301887,0.3435135,1;0;0
Node;AmplifyShaderEditor.SimpleTimeNode;255;-634.5645,480.6526;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;259;-664.2415,229.3325;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;258;-365.6485,598.7687;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;257;-372.4156,457.761;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;217;-690.7291,-530.8853;Inherit;True;Property;_WavesHeightmap;Waves Heightmap;17;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.DynamicAppendNode;261;-150.9104,475.0616;Inherit;True;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;111;-3102.669,-2065.698;Inherit;False;2659.104;1316.396;;3;106;79;87;Water;0,0.7294118,1,1;0;0
Node;AmplifyShaderEditor.DynamicAppendNode;262;-398.8785,232.6035;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;222;-444.3842,-404.2419;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;260;-185.3635,237.4511;Inherit;False;Property;_DistortionPlanarScale;Distortion Planar Scale;14;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;263;122.4265,456.549;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT;1;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WireNode;241;-221.7652,-291.2413;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;87;-3017.265,-1870.483;Inherit;False;1501.52;501.4431;;12;99;101;100;102;103;105;93;92;107;90;89;88;Depth Gradient;0,0.7294118,1,1;0;0
Node;AmplifyShaderEditor.TexturePropertyNode;3;62.72168,199.7973;Inherit;True;Property;_Distortion;Distortion;13;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.WireNode;242;-291.4412,-273.8224;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;88;-2979.569,-1654.608;Inherit;False;Property;_WaterGradientDepth;Water Gradient Depth;3;0;Create;True;0;0;0;False;0;False;1.294118;1.294118;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;221;-497.4097,-181.8815;Inherit;False;Property;_WaveSpeed;Wave Speed;19;0;Create;True;0;0;0;False;0;False;0,0.05;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SamplerNode;53;359.9046,365.7964;Inherit;True;Property;_TextureSample2;Texture Sample 2;5;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;108;-3088.349,-548.1245;Inherit;False;2231.973;1305.925;;4;140;112;83;78;Foam;1,0.9986275,0,1;0;0
Node;AmplifyShaderEditor.PannerNode;218;-241.0612,-239.9818;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DepthFade;89;-2706.094,-1672.466;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;162;834.3186,355.7402;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;163;686.0298,589.4069;Inherit;False;Property;_DistortionAmount;Distortion Amount;16;0;Create;True;0;0;0;False;0;False;0.2;0;-10;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;107;-2628.845,-1569.009;Inherit;False;Constant;_WaterGradientTransitionSharpness;Water Gradient Transition Sharpness;14;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;83;-2987.931,106.4198;Inherit;False;1355.892;548.6949;;11;208;207;203;202;200;201;2;41;199;205;206;Planar Texture;1,0.9986275,0,1;0;0
Node;AmplifyShaderEditor.OneMinusNode;90;-2465.266,-1647.802;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;164;996.4791,477.9858;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WireNode;244;-59.23975,-282.249;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;58;1148.065,471.4327;Inherit;False;distortionUV;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WireNode;243;-145.4366,-406.3725;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;92;-2299.396,-1607.25;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;205;-2956.551,506.1563;Inherit;False;Property;_PlanarFoamMovement;Planar Foam Movement;10;0;Create;True;0;0;0;False;0;False;0.1,0.1;0.1,0.1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleTimeNode;203;-2936.906,414.6167;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;105;-2279.829,-1472.906;Inherit;False;Constant;_WaterDepthColorTransition;Water Depth Color Transition;2;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;208;-2667.99,532.733;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;18;-3056.841,-1265.002;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;78;-2993.507,-338.5561;Inherit;False;550.0496;174.0682;;2;27;26;Edge Calc;1,1,0,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;207;-2674.757,391.725;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;215;-62.03303,-540.6847;Inherit;True;Property;_WaveHeightmap;Wave Heightmap;17;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;216;66.23912,-314.1469;Inherit;False;Property;_WaveHeight;Wave Height;18;0;Create;True;0;0;0;False;0;False;0.2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;199;-2966.583,163.2966;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;79;-2692.633,-1225.634;Inherit;False;1392.876;394.9987;;5;19;1;21;20;166;Texture;0,0.7294118,1,1;0;0
Node;AmplifyShaderEditor.ClampOpNode;93;-2145.707,-1602.548;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;165;-3032.723,-1088.573;Inherit;False;58;distortionUV;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;26;-2963.313,-259.1061;Inherit;False;Property;_EdgeDistance;Edge Distance;12;0;Create;True;0;0;0;False;0;False;1;1.294118;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;202;-2453.905,255.9154;Inherit;False;Property;_FoamPlanarScale;Foam Planar Scale;9;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;200;-2701.22,166.5676;Inherit;True;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;166;-2325.162,-1087.427;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;213;265.8562,-426.1712;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;20;-2361.45,-965.3043;Inherit;False;Property;_WaterFlowSpeed;Water Flow Speed;6;0;Create;True;0;0;0;False;0;False;0.1,1.5;0.1,1.5;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleDivideOpNode;103;-1971.897,-1603.428;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;112;-2376.236,-340.722;Inherit;False;1427.679;388.065;;5;138;52;172;171;85;Blend;1,1,0,1;0;0
Node;AmplifyShaderEditor.DynamicAppendNode;206;-2453.252,409.0256;Inherit;True;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.NormalVertexDataNode;212;147.6382,-197.2662;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScaleAndOffsetNode;201;-2179.915,390.5132;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT;1;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SaturateNode;102;-1850.51,-1602.923;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;101;-1985.286,-1796.805;Inherit;False;Property;_DeepColor;Deep Color;2;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DepthFade;27;-2689.837,-279.419;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;1;-1969.245,-1166.808;Inherit;True;Property;_WaterTexture;Water Texture;4;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.CommentaryNode;106;-1188.165,-1220.612;Inherit;False;646.4207;324.5763;;3;59;84;110;Blend;0,0.7294118,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;230;420.5878,-414.0711;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;19;-1971.602,-961.4827;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;2;-2217.271,155.1527;Inherit;True;Property;_Foam;Foam;7;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.ColorNode;100;-2222.378,-1797.904;Inherit;False;Property;_ShallowColor;Shallow Color;1;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;52;-2336.27,-55.62766;Inherit;False;Property;_FoamSoftness;Foam Softness;11;0;Create;True;0;0;0;False;0;False;0;0.02;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;171;-2014.274,-294.4956;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;21;-1713.438,-1066.013;Inherit;True;Property;_TextureSample0;Texture Sample 0;5;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;172;-1990.205,-165.387;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;84;-1161.231,-978.7136;Inherit;False;Property;_WaterTextureStrength;Water Texture Strength;5;0;Create;True;0;0;0;False;0;False;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;140;-1562.021,293.5101;Inherit;False;562.9144;250.6273;;2;136;114;Color;1,1,0,1;0;0
Node;AmplifyShaderEditor.DynamicAppendNode;229;589.6451,-481.1357;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.LerpOp;99;-1691.469,-1737.757;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;41;-1947.934,266.4174;Inherit;True;Property;_TextureSample1;Texture Sample 1;6;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;245;694.1662,-330.5014;Inherit;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.LerpOp;59;-910.0065,-1164.536;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SmoothstepOpNode;85;-1543.964,-103.7973;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;114;-1530.455,358.6515;Inherit;False;Property;_FoamColor;Foam Color;8;0;Create;True;0;0;0;False;0;False;0.7924528,0.7924528,0.7924528,0;0.7924528,0.7924528,0.7924528,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;136;-1205.766,368.6271;Inherit;False;foamAlbedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;138;-1216.409,-267.3874;Inherit;False;foamTransparency;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;246;468.5788,-240.6336;Inherit;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;110;-741.017,-1030.034;Inherit;False;waterAlbedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;109;-18.59737,-1536.019;Inherit;False;110;waterAlbedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;139;-27.53743,-1440.026;Inherit;False;138;foamTransparency;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;137;22.95498,-1661.841;Inherit;False;136;foamAlbedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;251;-4189.56,-1366.709;Inherit;False;738.5503;567.7406;;5;249;247;248;253;266;Waves Timing;0,0.7294118,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;239;518.2048,-145.9445;Inherit;False;waveOffset;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleTimeNode;248;-4117.844,-973.3617;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;135;273.3713,-1554.181;Inherit;False;Lighten;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;266;-3932.591,-1072.671;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;240;282.226,-1267.356;Inherit;False;239;waveOffset;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;209;201.7246,-1379.633;Inherit;False;Property;_Smoothness;Smoothness;0;0;Create;True;0;0;0;False;0;False;0.2;1;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;253;-4127.557,-1103.303;Inherit;False;Constant;_Float0;Float 0;19;0;Create;True;0;0;0;False;0;False;1.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SinTimeNode;247;-4123.278,-1264.094;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;249;-3748.688,-1126.76;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;548.2004,-1544.179;Float;False;True;-1;3;ASEMaterialInspector;0;0;Standard;FSP/SRP/Water Ocean;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Translucent;0.5;True;False;0;False;Opaque;;Transparent;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;258;0;255;0
WireConnection;258;1;57;2
WireConnection;257;0;255;0
WireConnection;257;1;57;1
WireConnection;261;0;257;0
WireConnection;261;1;258;0
WireConnection;262;0;259;1
WireConnection;262;1;259;3
WireConnection;222;2;217;0
WireConnection;263;0;262;0
WireConnection;263;1;260;0
WireConnection;263;2;261;0
WireConnection;241;0;222;0
WireConnection;242;0;241;0
WireConnection;53;0;3;0
WireConnection;53;1;263;0
WireConnection;218;0;242;0
WireConnection;218;2;221;0
WireConnection;89;0;88;0
WireConnection;162;0;53;1
WireConnection;162;1;53;1
WireConnection;90;0;89;0
WireConnection;164;0;162;0
WireConnection;164;1;163;0
WireConnection;244;0;218;0
WireConnection;58;0;164;0
WireConnection;243;0;244;0
WireConnection;92;0;90;0
WireConnection;92;1;107;0
WireConnection;208;0;203;0
WireConnection;208;1;205;2
WireConnection;207;0;203;0
WireConnection;207;1;205;1
WireConnection;215;0;217;0
WireConnection;215;1;243;0
WireConnection;93;0;92;0
WireConnection;200;0;199;1
WireConnection;200;1;199;3
WireConnection;166;0;18;0
WireConnection;166;1;165;0
WireConnection;213;0;215;1
WireConnection;213;1;216;0
WireConnection;103;0;93;0
WireConnection;103;1;105;0
WireConnection;206;0;207;0
WireConnection;206;1;208;0
WireConnection;201;0;200;0
WireConnection;201;1;202;0
WireConnection;201;2;206;0
WireConnection;102;0;103;0
WireConnection;27;0;26;0
WireConnection;230;0;213;0
WireConnection;230;1;212;2
WireConnection;19;0;166;0
WireConnection;19;2;20;0
WireConnection;171;0;27;0
WireConnection;171;1;52;0
WireConnection;21;0;1;0
WireConnection;21;1;19;0
WireConnection;172;0;27;0
WireConnection;172;1;52;0
WireConnection;229;2;230;0
WireConnection;99;0;101;0
WireConnection;99;1;100;0
WireConnection;99;2;102;0
WireConnection;41;0;2;0
WireConnection;41;1;201;0
WireConnection;245;0;229;0
WireConnection;59;0;99;0
WireConnection;59;1;21;0
WireConnection;59;2;84;0
WireConnection;85;0;41;1
WireConnection;85;1;171;0
WireConnection;85;2;172;0
WireConnection;136;0;114;0
WireConnection;138;0;85;0
WireConnection;246;0;245;0
WireConnection;110;0;59;0
WireConnection;239;0;246;0
WireConnection;135;0;137;0
WireConnection;135;1;109;0
WireConnection;135;2;139;0
WireConnection;266;0;248;0
WireConnection;266;1;253;0
WireConnection;249;0;247;4
WireConnection;249;1;248;0
WireConnection;0;0;135;0
WireConnection;0;4;209;0
WireConnection;0;11;240;0
ASEEND*/
//CHKSM=953993719A20D2FF159BA50913E17B44EEC59591