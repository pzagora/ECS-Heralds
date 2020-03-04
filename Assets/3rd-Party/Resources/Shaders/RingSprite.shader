Shader "RingSprite"
{
	Properties
	{
		_FinalPower("Final Power", Range( 0 , 10)) = 2
		_FinalOpacityPower("Final Opacity Power", Range( 0 , 4)) = 1
		_MaskTex("Mask Tex", 2D) = "white" {}
		_MaskDistortionPower("Mask Distortion Power", Range( 0 , 1)) = 0.25
		_MaskMofidyAdd("Mask Mofidy Add", Range( 0 , 1)) = 1
		_Ramp("Ramp", 2D) = "white" {}
		_RampMask("Ramp Mask", 2D) = "white" {}
		_RampColorTint("Ramp Color Tint", Color) = (1,1,1,1)
		_RampTilingMultiply("Ramp Tiling Multiply", Float) = 1
		[Toggle]_RampFlip("Ramp Flip", Int) = 0
		_Noise01("Noise 01", 2D) = "white" {}
		_Noise01ScrollSpeed("Noise 01 Scroll Speed", Float) = -0.25
		_Noise01TilingU("Noise 01 Tiling U", Float) = 1
		_Noise01TilingV("Noise 01 Tiling V", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma shader_feature _RAMPFLIP_ON
		#pragma surface surf Unlit alpha:fade keepalpha noshadow noambient novertexlights nolightmap  nodynlightmap nodirlightmap nometa noforwardadd 
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform float4 _RampColorTint;
		uniform float _FinalPower;
		uniform sampler2D _Ramp;
		uniform float _RampTilingMultiply;
		uniform sampler2D _Noise01;
		uniform float _Noise01ScrollSpeed;
		uniform float _Noise01TilingU;
		uniform float _Noise01TilingV;
		uniform sampler2D _MaskTex;
		uniform float _MaskDistortionPower;
		uniform sampler2D _RampMask;
		uniform float _FinalOpacityPower;
		uniform float _MaskMofidyAdd;

		inline fixed4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return fixed4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_TexCoord2 = i.uv_texcoord * float2( 1,1 ) + float2( 0,0 );
			float2 temp_output_3_0 = (float2( -1,-1 ) + (uv_TexCoord2 - float2( 0,0 )) * (float2( 1,1 ) - float2( -1,-1 )) / (float2( 1,1 ) - float2( 0,0 )));
			float2 appendResult10 = (float2(( ( _Time.y * _Noise01ScrollSpeed ) + ( _Noise01TilingU * length( temp_output_3_0 ) ) ) , (0.0 + (atan2( temp_output_3_0.x , temp_output_3_0.y ) - ( -1.0 * UNITY_PI )) * (_Noise01TilingV - 0.0) / (UNITY_PI - ( -1.0 * UNITY_PI )))));
			float4 tex2DNode11 = tex2D( _Noise01, appendResult10 );
			float2 uv_TexCoord41 = i.uv_texcoord * float2( 1,1 ) + float2( 0,0 );
			float2 normalizeResult43 = normalize( (float2( -1,-1 ) + (uv_TexCoord41 - float2( 0,0 )) * (float2( 1,1 ) - float2( -1,-1 )) / (float2( 1,1 ) - float2( 0,0 ))) );
			float4 tex2DNode1 = tex2D( _MaskTex, ( uv_TexCoord41 + ( tex2DNode11.r * normalizeResult43 * _MaskDistortionPower ) ) );
			float ResultMask38 = ( tex2DNode11.r * tex2DNode1.r );
			float clampResult28 = clamp( ( _RampTilingMultiply * ResultMask38 ) , 0.0 , 1.0 );
			#ifdef _RAMPFLIP_ON
				float staticSwitch30 = ( 1.0 - clampResult28 );
			#else
				float staticSwitch30 = clampResult28;
			#endif
			float2 appendResult32 = (float2(staticSwitch30 , 0.0));
			o.Emission = ( _RampColorTint * _FinalPower * tex2D( _Ramp, appendResult32 ) * tex2D( _RampMask, appendResult32 ).r * i.vertexColor ).rgb;
			float clampResult50 = clamp( ( tex2DNode1.b + _MaskMofidyAdd ) , 0.0 , 1.0 );
			float clampResult19 = clamp( ( tex2DNode1.g * _FinalOpacityPower * i.vertexColor.a * clampResult50 ) , 0.0 , 1.0 );
			o.Alpha = clampResult19;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}