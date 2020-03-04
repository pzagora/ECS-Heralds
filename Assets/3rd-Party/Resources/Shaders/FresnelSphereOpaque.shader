Shader "FresnelSphereOpaque"
{
	Properties
	{
		_FinalPower("Final Power", Range( 0 , 10)) = 2
		_FinalColor("Final Color", Color) = (1,1,1,1)
		_FinalFresnelExp("Final Fresnel Exp", Range( 0.2 , 4)) = 1
		_Ramp("Ramp", 2D) = "white" {}
		_OffsetTex("Offset Tex", 2D) = "white" {}
		_OffsetTiling("Offset Tiling", Float) = 1
		_OffsetScrollSpeed("Offset Scroll Speed", Float) = 0.25
		_OffsetPower("Offset Power", Range( 0 , 1)) = 0.15
		_OffsetRemapMinNew("Offset Remap Min New", Float) = -1
		_OffsetRemapMaxNew("Offset Remap Max New", Float) = 1
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		ZWrite On
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha noshadow noambient novertexlights nolightmap  nodynlightmap nodirlightmap nometa noforwardadd vertex:vertexDataFunc 
		struct Input
		{
			float3 worldNormal;
			INTERNAL_DATA
			float3 worldPos;
			float4 vertexColor : COLOR;
		};

		uniform sampler2D _Ramp;
		uniform float _FinalFresnelExp;
		uniform float _FinalPower;
		uniform float4 _FinalColor;
		uniform sampler2D _OffsetTex;
		uniform float _OffsetTiling;
		uniform float _OffsetScrollSpeed;
		uniform float _OffsetRemapMinNew;
		uniform float _OffsetRemapMaxNew;
		uniform float _OffsetPower;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_worldNormal = UnityObjectToWorldNormal( v.normal );
			float3 temp_output_41_0 = abs( ase_worldNormal );
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float2 appendResult31 = (float2(( ( ase_worldPos * _OffsetTiling ) + ( _Time.y * _OffsetScrollSpeed ) ).y , ( ( ase_worldPos * _OffsetTiling ) + ( _Time.y * _OffsetScrollSpeed ) ).z));
			float2 appendResult32 = (float2(( ( ase_worldPos * _OffsetTiling ) + ( _Time.y * _OffsetScrollSpeed ) ).z , ( ( ase_worldPos * _OffsetTiling ) + ( _Time.y * _OffsetScrollSpeed ) ).x));
			float2 appendResult34 = (float2(( ( ase_worldPos * _OffsetTiling ) + ( _Time.y * _OffsetScrollSpeed ) ).x , ( ( ase_worldPos * _OffsetTiling ) + ( _Time.y * _OffsetScrollSpeed ) ).y));
			float3 weightedBlendVar38 = ( temp_output_41_0 * temp_output_41_0 );
			float weightedBlend38 = ( weightedBlendVar38.x*tex2Dlod( _OffsetTex, float4( appendResult31, 0, 0.0) ).r + weightedBlendVar38.y*tex2Dlod( _OffsetTex, float4( appendResult32, 0, 0.0) ).g + weightedBlendVar38.z*tex2Dlod( _OffsetTex, float4( appendResult34, 0, 0.0) ).b );
			float3 ase_vertexNormal = v.normal.xyz;
			v.vertex.xyz += ( (_OffsetRemapMinNew + (weightedBlend38 - 0) * (_OffsetRemapMaxNew - _OffsetRemapMinNew) / (1 - 0)) * _OffsetPower * ase_vertexNormal );
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_vertexNormal = mul( unity_WorldToObject, float4( ase_worldNormal, 0 ) );
			o.Normal = ase_vertexNormal;
			float temp_output_49_0 = 0.0;
			float3 temp_cast_0 = (temp_output_49_0).xxx;
			o.Albedo = temp_cast_0;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float dotResult16 = dot( ase_worldViewDir , ase_vertexNormal );
			float temp_output_21_0 = pow( ( 1.0 - dotResult16 ) , _FinalFresnelExp );
			float2 appendResult43 = (float2(temp_output_21_0 , 0.0));
			o.Emission = ( tex2D( _Ramp, appendResult43 ) * temp_output_21_0 * _FinalPower * _FinalColor * i.vertexColor ).rgb;
			o.Metallic = temp_output_49_0;
			o.Smoothness = temp_output_49_0;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}