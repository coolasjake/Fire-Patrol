// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "FSP/SRP/Planar Shader"
{
	Properties
	{
		_Albedo("Albedo", 2D) = "white" {}
		_NormalMap("NormalMap", 2D) = "white" {}
		_NormalStrength("Normal Strength", Float) = 0.5
		_Scale("Scale", Float) = 1
		_Specular("Specular", Float) = 0
		_Smoothness("Smoothness", Float) = 0
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf StandardSpecular keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float3 worldPos;
		};

		uniform sampler2D _NormalMap;
		uniform float _Scale;
		uniform float _NormalStrength;
		uniform sampler2D _Albedo;
		uniform float _Specular;
		uniform float _Smoothness;

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			float3 ase_worldPos = i.worldPos;
			float2 appendResult12 = (float2(ase_worldPos.x , ase_worldPos.z));
			float2 temp_output_24_0 = ( appendResult12 * _Scale );
			float2 temp_output_2_0_g2 = temp_output_24_0;
			float2 break6_g2 = temp_output_2_0_g2;
			float temp_output_25_0_g2 = ( pow( 0.5 , 3.0 ) * 0.1 );
			float2 appendResult8_g2 = (float2(( break6_g2.x + temp_output_25_0_g2 ) , break6_g2.y));
			float4 tex2DNode14_g2 = tex2D( _NormalMap, temp_output_2_0_g2 );
			float temp_output_4_0_g2 = _NormalStrength;
			float3 appendResult13_g2 = (float3(1.0 , 0.0 , ( ( tex2D( _NormalMap, appendResult8_g2 ).g - tex2DNode14_g2.g ) * temp_output_4_0_g2 )));
			float2 appendResult9_g2 = (float2(break6_g2.x , ( break6_g2.y + temp_output_25_0_g2 )));
			float3 appendResult16_g2 = (float3(0.0 , 1.0 , ( ( tex2D( _NormalMap, appendResult9_g2 ).g - tex2DNode14_g2.g ) * temp_output_4_0_g2 )));
			float3 normalizeResult22_g2 = normalize( cross( appendResult13_g2 , appendResult16_g2 ) );
			o.Normal = normalizeResult22_g2;
			o.Albedo = tex2D( _Albedo, temp_output_24_0 ).rgb;
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
0;0;1536;803;2840.672;1081.426;2.067316;True;True
Node;AmplifyShaderEditor.CommentaryNode;40;-2120.931,-393.7394;Inherit;False;645.2571;413.5777;;4;14;9;12;24;Planar Calculation;0,1,0.01695085,1;0;0
Node;AmplifyShaderEditor.WorldPosInputsNode;9;-2088.042,-324.9776;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;12;-1846.499,-321.8093;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-1896.45,-125.3781;Inherit;False;Property;_Scale;Scale;5;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;3;-1370.665,-750.9476;Inherit;False;599.4371;477.3273;;2;21;18;Texture;0,0.5551176,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;25;-1359.947,-185.1553;Inherit;False;592.2188;462.891;;3;37;33;31;Normalmap;0,1,0.9728527,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-1652.093,-259.1805;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;31;-1326.843,-129.0444;Inherit;True;Property;_NormalMap;NormalMap;1;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexturePropertyNode;18;-1337.559,-682.2048;Inherit;True;Property;_Albedo;Albedo;0;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;37;-1325.73,176.3954;Inherit;False;Property;_NormalStrength;Normal Strength;4;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;38;-1374.588,29.44316;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;23;-667.7029,258.4576;Inherit;False;Property;_Smoothness;Smoothness;7;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;22;-731.7106,152.7648;Inherit;False;Property;_Specular;Specular;6;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;33;-1003.225,115.8855;Inherit;False;NormalCreate;2;;2;e12f7ae19d416b942820e3932b56220f;0;4;1;SAMPLER2D;;False;2;FLOAT2;0,0;False;3;FLOAT;0.5;False;4;FLOAT;2;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;21;-1075.895,-482.9889;Inherit;True;Property;_TextureSample1;Texture Sample 1;6;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-469.8962,-270.4768;Float;False;True;-1;2;ASEMaterialInspector;0;0;StandardSpecular;FSP/SRP/Planar Shader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;12;0;9;1
WireConnection;12;1;9;3
WireConnection;24;0;12;0
WireConnection;24;1;14;0
WireConnection;38;0;24;0
WireConnection;33;1;31;0
WireConnection;33;2;38;0
WireConnection;33;4;37;0
WireConnection;21;0;18;0
WireConnection;21;1;24;0
WireConnection;0;0;21;0
WireConnection;0;1;33;0
WireConnection;0;3;22;0
WireConnection;0;4;23;0
ASEEND*/
//CHKSM=8C86DC39A5EFA820AE2E5C4C82526969615D69A0