// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "FSP/SRP/Accumulation Triplanar"
{
	Properties
	{
		_TopAlbedo("Top Albedo", 2D) = "white" {}
		_TopNormal("Top Normal", 2D) = "bump" {}
		_TriplanarAlbedo("Triplanar Albedo", 2D) = "white" {}
		_TriplanarNormal("Triplanar Normal", 2D) = "bump" {}
		_CoverageFalloff("Coverage Falloff", Range( 0.01 , 5)) = 0.5
		_AccumTopAlbedo("Accum Top Albedo", 2D) = "white" {}
		_AccumTopNormal("Accum Top Normal", 2D) = "bump" {}
		_AccumTriplanarAlbedo("Accum Triplanar Albedo", 2D) = "white" {}
		_AccumTriplanarNormal("Accum Triplanar Normal", 2D) = "bump" {}
		_AccumulationAmount("Accumulation Amount", Range( 0 , 10)) = 0.13
		_Tiling("Tiling", Vector) = (0.1,0.1,0,0)
		_Specular("Specular", Range( 0 , 1)) = 1
		_Smoothness("Smoothness", Range( 0 , 1)) = 0.5
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		ZTest LEqual
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf StandardSpecular keepalpha addshadow fullforwardshadows exclude_path:deferred 
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
		};

		uniform sampler2D _TopNormal;
		uniform sampler2D _TriplanarNormal;
		uniform float2 _Tiling;
		uniform float _CoverageFalloff;
		uniform sampler2D _AccumTopNormal;
		uniform sampler2D _AccumTriplanarNormal;
		uniform float _AccumulationAmount;
		uniform sampler2D _TopAlbedo;
		uniform sampler2D _TriplanarAlbedo;
		uniform sampler2D _AccumTopAlbedo;
		uniform sampler2D _AccumTriplanarAlbedo;
		uniform float _Specular;
		uniform float _Smoothness;


		inline float3 TriplanarSampling345( sampler2D topTexMap, sampler2D midTexMap, sampler2D botTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			float negProjNormalY = max( 0, projNormal.y * -nsign.y );
			projNormal.y = max( 0, projNormal.y * nsign.y );
			half4 xNorm; half4 yNorm; half4 yNormN; half4 zNorm;
			xNorm  = tex2D( midTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm  = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			yNormN = tex2D( botTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm  = tex2D( midTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			xNorm.xyz  = half3( UnpackNormal( xNorm ).xy * float2(  nsign.x, 1.0 ) + worldNormal.zy, worldNormal.x ).zyx;
			yNorm.xyz  = half3( UnpackNormal( yNorm ).xy * float2(  nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
			zNorm.xyz  = half3( UnpackNormal( zNorm ).xy * float2( -nsign.z, 1.0 ) + worldNormal.xy, worldNormal.z ).xyz;
			yNormN.xyz = half3( UnpackNormal( yNormN ).xy * float2( nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
			return normalize( xNorm.xyz * projNormal.x + yNorm.xyz * projNormal.y + yNormN.xyz * negProjNormalY + zNorm.xyz * projNormal.z );
		}


		inline float3 TriplanarSampling378( sampler2D topTexMap, sampler2D midTexMap, sampler2D botTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			float negProjNormalY = max( 0, projNormal.y * -nsign.y );
			projNormal.y = max( 0, projNormal.y * nsign.y );
			half4 xNorm; half4 yNorm; half4 yNormN; half4 zNorm;
			xNorm  = tex2D( midTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm  = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			yNormN = tex2D( botTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm  = tex2D( midTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			xNorm.xyz  = half3( UnpackNormal( xNorm ).xy * float2(  nsign.x, 1.0 ) + worldNormal.zy, worldNormal.x ).zyx;
			yNorm.xyz  = half3( UnpackNormal( yNorm ).xy * float2(  nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
			zNorm.xyz  = half3( UnpackNormal( zNorm ).xy * float2( -nsign.z, 1.0 ) + worldNormal.xy, worldNormal.z ).xyz;
			yNormN.xyz = half3( UnpackNormal( yNormN ).xy * float2( nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
			return normalize( xNorm.xyz * projNormal.x + yNorm.xyz * projNormal.y + yNormN.xyz * negProjNormalY + zNorm.xyz * projNormal.z );
		}


		inline float4 TriplanarSampling343( sampler2D topTexMap, sampler2D midTexMap, sampler2D botTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			float negProjNormalY = max( 0, projNormal.y * -nsign.y );
			projNormal.y = max( 0, projNormal.y * nsign.y );
			half4 xNorm; half4 yNorm; half4 yNormN; half4 zNorm;
			xNorm  = tex2D( midTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm  = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			yNormN = tex2D( botTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm  = tex2D( midTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + yNormN * negProjNormalY + zNorm * projNormal.z;
		}


		inline float4 TriplanarSampling375( sampler2D topTexMap, sampler2D midTexMap, sampler2D botTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			float negProjNormalY = max( 0, projNormal.y * -nsign.y );
			projNormal.y = max( 0, projNormal.y * nsign.y );
			half4 xNorm; half4 yNorm; half4 yNormN; half4 zNorm;
			xNorm  = tex2D( midTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm  = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			yNormN = tex2D( botTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm  = tex2D( midTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + yNormN * negProjNormalY + zNorm * projNormal.z;
		}


		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_worldTangent = WorldNormalVector( i, float3( 1, 0, 0 ) );
			float3 ase_worldBitangent = WorldNormalVector( i, float3( 0, 1, 0 ) );
			float3x3 ase_worldToTangent = float3x3( ase_worldTangent, ase_worldBitangent, ase_worldNormal );
			float3 triplanar345 = TriplanarSampling345( _TopNormal, _TriplanarNormal, _TriplanarNormal, ase_worldPos, ase_worldNormal, _CoverageFalloff, _Tiling, float3( 1,1,1 ), float3(0,0,0) );
			float3 tanTriplanarNormal345 = mul( ase_worldToTangent, triplanar345 );
			float3 triplanar378 = TriplanarSampling378( _AccumTopNormal, _AccumTriplanarNormal, _AccumTriplanarNormal, ase_worldPos, ase_worldNormal, _CoverageFalloff, _Tiling, float3( 1,1,1 ), float3(0,0,0) );
			float3 tanTriplanarNormal378 = mul( ase_worldToTangent, triplanar378 );
			float3 lerpResult361 = lerp( tanTriplanarNormal345 , tanTriplanarNormal378 , saturate( ( ase_worldNormal.y * _AccumulationAmount ) ));
			o.Normal = lerpResult361;
			float4 triplanar343 = TriplanarSampling343( _TopAlbedo, _TriplanarAlbedo, _TriplanarAlbedo, ase_worldPos, ase_worldNormal, _CoverageFalloff, _Tiling, float3( 1,1,1 ), float3(0,0,0) );
			float4 triplanar375 = TriplanarSampling375( _AccumTopAlbedo, _AccumTriplanarAlbedo, _AccumTriplanarAlbedo, ase_worldPos, ase_worldNormal, _CoverageFalloff, _Tiling, float3( 1,1,1 ), float3(0,0,0) );
			float4 lerpResult367 = lerp( triplanar343 , triplanar375 , saturate( ( (WorldNormalVector( i , lerpResult361 )).y * _AccumulationAmount ) ));
			o.Albedo = lerpResult367.xyz;
			float3 temp_cast_1 = (_Specular).xxx;
			o.Specular = temp_cast_1;
			o.Smoothness = _Smoothness;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18800
0;73.6;1182;439;-1207.801;-255.1444;2.13764;True;True
Node;AmplifyShaderEditor.WorldNormalVector;355;1565.994,868.931;Inherit;False;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;356;1489.427,1086.44;Float;False;Property;_AccumulationAmount;Accumulation Amount;9;0;Create;True;0;0;0;False;0;False;0.13;0.95;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;357;2028.177,930.5482;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;344;693.3672,-122.8975;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TexturePropertyNode;346;1248.221,110.9151;Float;True;Property;_TopNormal;Top Normal;1;0;Create;True;0;0;0;False;0;False;None;None;True;bump;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexturePropertyNode;377;1492.301,560.5243;Float;True;Property;_AccumTriplanarNormal;Accum Triplanar Normal;8;0;Create;True;0;0;0;False;0;False;None;None;True;bump;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;352;629.4737,352.0598;Float;False;Property;_CoverageFalloff;Coverage Falloff;4;0;Create;True;0;0;0;False;0;False;0.5;5;0.01;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;354;634.5654,108.6367;Inherit;False;Property;_Tiling;Tiling;10;0;Create;True;0;0;0;False;0;False;0.1,0.1;0.1,0.1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TexturePropertyNode;376;1259.568,456.8812;Float;True;Property;_AccumTopNormal;Accum Top Normal;6;0;Create;True;0;0;0;False;0;False;None;None;True;bump;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexturePropertyNode;347;1485.921,189.7233;Float;True;Property;_TriplanarNormal;Triplanar Normal;3;0;Create;True;0;0;0;False;0;False;None;None;True;bump;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TriplanarNode;345;1771.427,141.8192;Inherit;True;Cylindrical;World;True;Top Texture 1;_TopTexture1;white;-1;None;Mid Texture 1;_MidTexture1;white;-1;None;Bot Texture 1;_BotTexture1;white;-1;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT3;1,1,1;False;3;FLOAT2;1,1;False;4;FLOAT;2;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TriplanarNode;378;1780.29,512.6202;Inherit;True;Cylindrical;World;True;Top Texture 3;_TopTexture3;white;-1;None;Mid Texture 3;_MidTexture3;white;-1;None;Bot Texture 3;_BotTexture3;white;-1;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT3;1,1,1;False;3;FLOAT2;1,1;False;4;FLOAT;2;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;360;2247.053,929.4755;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;361;2491.66,457.5483;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldNormalVector;362;2622.992,176.6156;Inherit;False;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TexturePropertyNode;338;1237.671,-562.722;Float;True;Property;_TopAlbedo;Top Albedo;0;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;363;2809.191,219.2664;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;373;1236.317,-218.3332;Float;True;Property;_AccumTopAlbedo;Accum Top Albedo;5;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexturePropertyNode;374;1468.963,-147.725;Float;True;Property;_AccumTriplanarAlbedo;Accum Triplanar Albedo;7;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexturePropertyNode;339;1469.012,-499.694;Float;True;Property;_TriplanarAlbedo;Triplanar Albedo;2;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TriplanarNode;375;1763.652,-210.7884;Inherit;True;Cylindrical;World;False;Top Texture 2;_TopTexture2;white;-1;None;Mid Texture 2;_MidTexture2;white;-1;None;Bot Texture 2;_BotTexture2;white;-1;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT3;1,1,1;False;3;FLOAT2;1,1;False;4;FLOAT;2;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TriplanarNode;343;1761.665,-559.4638;Inherit;True;Cylindrical;World;False;Top Texture 0;_TopTexture0;white;-1;None;Mid Texture 0;_MidTexture0;white;-1;None;Bot Texture 0;_BotTexture0;white;-1;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT3;1,1,1;False;3;FLOAT2;1,1;False;4;FLOAT;2;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;364;3027.899,77.65752;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;367;3180.858,-84.39523;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;213;3071.797,606.0383;Float;False;Property;_Smoothness;Smoothness;12;0;Create;True;0;0;0;False;0;False;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;212;3015.651,495.8344;Float;False;Property;_Specular;Specular;11;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;3400.503,-12.19367;Float;False;True;-1;2;ASEMaterialInspector;0;0;StandardSpecular;FSP/SRP/Accumulation Triplanar;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;Back;0;False;-1;3;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;0;4;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;1;False;-1;1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;357;0;355;2
WireConnection;357;1;356;0
WireConnection;345;0;346;0
WireConnection;345;1;347;0
WireConnection;345;2;347;0
WireConnection;345;9;344;0
WireConnection;345;3;354;0
WireConnection;345;4;352;0
WireConnection;378;0;376;0
WireConnection;378;1;377;0
WireConnection;378;2;377;0
WireConnection;378;9;344;0
WireConnection;378;3;354;0
WireConnection;378;4;352;0
WireConnection;360;0;357;0
WireConnection;361;0;345;0
WireConnection;361;1;378;0
WireConnection;361;2;360;0
WireConnection;362;0;361;0
WireConnection;363;0;362;2
WireConnection;363;1;356;0
WireConnection;375;0;373;0
WireConnection;375;1;374;0
WireConnection;375;2;374;0
WireConnection;375;9;344;0
WireConnection;375;3;354;0
WireConnection;375;4;352;0
WireConnection;343;0;338;0
WireConnection;343;1;339;0
WireConnection;343;2;339;0
WireConnection;343;9;344;0
WireConnection;343;3;354;0
WireConnection;343;4;352;0
WireConnection;364;0;363;0
WireConnection;367;0;343;0
WireConnection;367;1;375;0
WireConnection;367;2;364;0
WireConnection;0;0;367;0
WireConnection;0;1;361;0
WireConnection;0;3;212;0
WireConnection;0;4;213;0
ASEEND*/
//CHKSM=45230D9175C87ED9C80F2BC0A77E17D63AB67F0A