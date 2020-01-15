Shader "BetterEmbers"
{
	Properties
	{
		_FinalPower("Final Power", Range( 0 , 10)) = 2
		_OpacityPower("Opacity Power", Range( 0 , 4)) = 1
		_EmbersTexture("Embers Texture", 2D) = "white" {}
		_Ramp("Ramp", 2D) = "white" {}
		_RampMask("Ramp Mask", 2D) = "white" {}
		_RampColorTint("Ramp Color Tint", Color) = (1,1,1,1)
		_RampMultiplyTiling("Ramp Multiply Tiling", Float) = 1
		[Toggle]_RampFlip("Ramp Flip", Int) = 0
		_Noise01("Noise 01", 2D) = "white" {}
		_Noise01Tiling("Noise 01 Tiling", Float) = 1.25
		_Noise01ScrollSpeedU("Noise 01 Scroll Speed U", Float) = 0.25
		_Noise01ScrollSpeedV("Noise 01 Scroll Speed V", Float) = 0.25
		_Noise01Power("Noise 01 Power", Range( 0 , 1)) = 0.25
		_DistortionNoise("Distortion Noise", 2D) = "white" {}
		_DistortionNoiseTiling("Distortion Noise Tiling", Float) = 1
		_DistortionNoiseScrollSpeedU("Distortion Noise Scroll Speed U", Float) = 0.25
		_DistortionNoiseScrollSpeedV("Distortion Noise Scroll Speed V", Float) = 0.25
		_DistortionNoisePower("Distortion Noise Power", Range( 0 , 1)) = 0.25
		_SoftParticleValue("Soft Particle Value", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#pragma target 3.0
		#pragma shader_feature _RAMPFLIP_ON
		#pragma surface surf Unlit alpha:fade keepalpha noshadow noambient novertexlights nolightmap  nodynlightmap nodirlightmap nometa noforwardadd 
		#undef TRANSFORM_TEX
		#define TRANSFORM_TEX(tex,name) float4(tex.xy * name##_ST.xy + name##_ST.zw, tex.z, tex.w)
		struct Input
		{
			float2 uv_texcoord;
			float3 uv2_texcoord2;
			float4 vertexColor : COLOR;
			float4 screenPos;
		};

		uniform float4 _RampColorTint;
		uniform float _FinalPower;
		uniform sampler2D _Ramp;
		uniform float _RampMultiplyTiling;
		uniform sampler2D _EmbersTexture;
		uniform sampler2D _DistortionNoise;
		uniform float _DistortionNoiseTiling;
		uniform float _DistortionNoiseScrollSpeedU;
		uniform float _DistortionNoiseScrollSpeedV;
		uniform float _DistortionNoisePower;
		uniform sampler2D _Noise01;
		uniform float _Noise01Tiling;
		uniform float _Noise01ScrollSpeedU;
		uniform float _Noise01ScrollSpeedV;
		uniform float _Noise01Power;
		uniform sampler2D _RampMask;
		uniform float _OpacityPower;
		uniform sampler2D _CameraDepthTexture;
		uniform float _SoftParticleValue;

		inline fixed4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return fixed4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_TexCoord3 = i.uv_texcoord * float2( 1,1 ) + float2( 0,0 );
			float3 uv2_TexCoord36 = i.uv2_texcoord2;
			uv2_TexCoord36.xy = i.uv2_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
			float2 appendResult5 = (float2(uv_TexCoord3.x , ( ( uv_TexCoord3.y * 0.25 ) + ( 0.25 * floor( uv2_TexCoord36.x ) ) )));
			float2 uv_TexCoord17 = i.uv_texcoord * float2( 1,1 ) + float2( 0,0 );
			float2 appendResult24 = (float2(( ( _DistortionNoiseTiling * uv_TexCoord17 * uv2_TexCoord36.y ).x + ( _Time.y * _DistortionNoiseScrollSpeedU ) + uv2_TexCoord36.x ) , ( ( _DistortionNoiseTiling * uv_TexCoord17 * uv2_TexCoord36.y ).y + ( _Time.y * _DistortionNoiseScrollSpeedV ) + uv2_TexCoord36.x )));
			float2 appendResult78 = (float2(( ( _Noise01Tiling * uv_TexCoord17 * uv2_TexCoord36.y ).x + ( _Time.y * _Noise01ScrollSpeedU ) + uv2_TexCoord36.x ) , ( ( _Noise01Tiling * uv_TexCoord17 * uv2_TexCoord36.y ).y + ( _Time.y * _Noise01ScrollSpeedV ) + uv2_TexCoord36.x )));
			float clampResult52 = clamp( ( tex2D( _Noise01, appendResult78 ).r + ( _Noise01Power * uv2_TexCoord36.z ) ) , 0.0 , 1.0 );
			float temp_output_41_0 = ( tex2D( _EmbersTexture, ( appendResult5 + ( (-1.0 + (tex2D( _DistortionNoise, appendResult24 ).r - 0.0) * (1.0 - -1.0) / (1.0 - 0.0)) * _DistortionNoisePower ) ) ).r * i.vertexColor.a * clampResult52 );
			float ResultMask68 = temp_output_41_0;
			float clampResult67 = clamp( ( _RampMultiplyTiling * ResultMask68 ) , 0.0 , 1.0 );
			#ifdef _RAMPFLIP_ON
				float staticSwitch61 = ( 1.0 - clampResult67 );
			#else
				float staticSwitch61 = clampResult67;
			#endif
			float2 appendResult62 = (float2(staticSwitch61 , 0.0));
			o.Emission = ( _RampColorTint * _FinalPower * tex2D( _Ramp, appendResult62 ) * i.vertexColor * tex2D( _RampMask, appendResult62 ).r ).rgb;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth81 = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture,UNITY_PROJ_COORD(ase_screenPos))));
			float distanceDepth81 = abs( ( screenDepth81 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _SoftParticleValue ) );
			float clampResult83 = clamp( distanceDepth81 , 0.0 , 1.0 );
			float clampResult43 = clamp( ( temp_output_41_0 * _OpacityPower * clampResult83 ) , 0.0 , 1.0 );
			o.Alpha = clampResult43;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}