Shader "BasicAura"
{
	Properties
	{
		_FinalPower("Final Power", Range( 0 , 10)) = 2
		[Toggle]_MaskConstantThickness("Mask Constant Thickness", Int) = 0
		_MaskThickness("Mask Thickness", Float) = 1
		_MaskDistance("Mask Distance", Float) = 1
		_MaskMultiply("Mask Multiply", Range( 0 , 4)) = 1
		_MaskExp("Mask Exp", Range( 0.2 , 10)) = 1
		[Toggle]_MaskTextureEnabled("Mask Texture Enabled", Int) = 1
		_MaskTexture("Mask Texture", 2D) = "white" {}
		_Ramp("Ramp", 2D) = "white" {}
		_RampColorTint("Ramp Color Tint", Color) = (1,1,1,1)
		_RampMultiplyTiling("Ramp Multiply Tiling", Float) = 1
		[Toggle]_RampFlip("Ramp Flip", Int) = 0
		_NoiseDistortionPower("Noise Distortion Power", Range( 0 , 10)) = 1
		_Noise01("Noise 01", 2D) = "white" {}
		_Noise01Tiling("Noise 01 Tiling", Float) = 1
		_Noise01ScrollSpeed("Noise 01 Scroll Speed", Float) = 0.25
		[Toggle]_Noise02Enabled("Noise 02 Enabled", Int) = 1
		_Noise02("Noise 02", 2D) = "white" {}
		_Noise02Tiling("Noise 02 Tiling", Float) = 1
		_Noise02ScrollSpeed("Noise 02 Scroll Speed", Float) = 0.25
		[Toggle]_NoiseMaskDistortionEnabled("Noise Mask Distortion Enabled", Int) = 1
		_NoiseMaskDistortion("Noise Mask Distortion", 2D) = "white" {}
		_NoiseMaskDistortionPower("Noise Mask Distortion Power", Range( 0 , 2)) = 0.5
		_NoiseMaskDistortionTiling("Noise Mask Distortion Tiling", Float) = 0.5
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
		#pragma shader_feature _NOISEMASKDISTORTIONENABLED_ON
		#pragma shader_feature _NOISE02ENABLED_ON
		#pragma shader_feature _MASKCONSTANTTHICKNESS_ON
		#pragma shader_feature _MASKTEXTUREENABLED_ON
		#pragma shader_feature _RAMPFLIP_ON
		#pragma surface surf Unlit keepalpha noshadow noambient novertexlights nolightmap  nodynlightmap nodirlightmap nometa noforwardadd 
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
		};

		uniform float4 _RampColorTint;
		uniform float _FinalPower;
		uniform sampler2D _Ramp;
		uniform float _RampMultiplyTiling;
		uniform sampler2D _MaskTexture;
		uniform float4 _AuraSourcePosition;
		uniform float _MaskDistance;
		uniform sampler2D _Noise01;
		uniform float _Noise01Tiling;
		uniform float _Noise01ScrollSpeed;
		uniform sampler2D _NoiseMaskDistortion;
		uniform float _NoiseMaskDistortionTiling;
		uniform float _NoiseMaskDistortionPower;
		uniform sampler2D _Noise02;
		uniform float _Noise02Tiling;
		uniform float _Noise02ScrollSpeed;
		uniform float _NoiseDistortionPower;
		uniform float _MaskThickness;
		uniform float _MaskExp;
		uniform float _MaskMultiply;


		inline float4 TriplanarSamplingSF( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float tilling, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= projNormal.x + projNormal.y + projNormal.z;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = ( tex2D( topTexMap, tilling * worldPos.zy * float2( nsign.x, 1.0 ) ) );
			yNorm = ( tex2D( topTexMap, tilling * worldPos.xz * float2( nsign.y, 1.0 ) ) );
			zNorm = ( tex2D( topTexMap, tilling * worldPos.xy * float2( -nsign.z, 1.0 ) ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + zNorm * projNormal.z;
		}


		inline fixed4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return fixed4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			o.Normal = float3(0,0,1);
			float3 ase_worldPos = i.worldPos;
			float3 appendResult17 = (float3(_AuraSourcePosition.x , _AuraSourcePosition.y , _AuraSourcePosition.z));
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 temp_output_96_0 = abs( ase_worldNormal );
			float3 temp_output_103_0 = ( temp_output_96_0 * temp_output_96_0 );
			float4 triplanar118 = TriplanarSamplingSF( _NoiseMaskDistortion, ase_worldPos, ase_worldNormal, 1.0, _NoiseMaskDistortionTiling, 0 );
			float4 temp_cast_1 = (0.0).xxxx;
			#ifdef _NOISEMASKDISTORTIONENABLED_ON
				float4 staticSwitch126 = ( triplanar118 * _NoiseMaskDistortionPower );
			#else
				float4 staticSwitch126 = temp_cast_1;
			#endif
			float2 appendResult61 = (float2(( float4( ( ase_worldPos * _Noise01Tiling ) , 0.0 ) + ( _Time.y * _Noise01ScrollSpeed ) + staticSwitch126 ).y , ( float4( ( ase_worldPos * _Noise01Tiling ) , 0.0 ) + ( _Time.y * _Noise01ScrollSpeed ) + staticSwitch126 ).z));
			float2 appendResult63 = (float2(( float4( ( ase_worldPos * _Noise01Tiling ) , 0.0 ) + ( _Time.y * _Noise01ScrollSpeed ) + staticSwitch126 ).z , ( float4( ( ase_worldPos * _Noise01Tiling ) , 0.0 ) + ( _Time.y * _Noise01ScrollSpeed ) + staticSwitch126 ).x));
			float2 appendResult62 = (float2(( float4( ( ase_worldPos * _Noise01Tiling ) , 0.0 ) + ( _Time.y * _Noise01ScrollSpeed ) + staticSwitch126 ).x , ( float4( ( ase_worldPos * _Noise01Tiling ) , 0.0 ) + ( _Time.y * _Noise01ScrollSpeed ) + staticSwitch126 ).y));
			float3 weightedBlendVar72 = temp_output_103_0;
			float weightedBlend72 = ( weightedBlendVar72.x*tex2D( _Noise01, appendResult61 ).r + weightedBlendVar72.y*tex2D( _Noise01, appendResult63 ).r + weightedBlendVar72.z*tex2D( _Noise01, appendResult62 ).r );
			float2 appendResult99 = (float2(( float4( ( ase_worldPos * _Noise02Tiling ) , 0.0 ) + ( _Time.y * _Noise02ScrollSpeed ) + staticSwitch126 ).y , ( float4( ( ase_worldPos * _Noise02Tiling ) , 0.0 ) + ( _Time.y * _Noise02ScrollSpeed ) + staticSwitch126 ).z));
			float2 appendResult97 = (float2(( float4( ( ase_worldPos * _Noise02Tiling ) , 0.0 ) + ( _Time.y * _Noise02ScrollSpeed ) + staticSwitch126 ).z , ( float4( ( ase_worldPos * _Noise02Tiling ) , 0.0 ) + ( _Time.y * _Noise02ScrollSpeed ) + staticSwitch126 ).x));
			float2 appendResult95 = (float2(( float4( ( ase_worldPos * _Noise02Tiling ) , 0.0 ) + ( _Time.y * _Noise02ScrollSpeed ) + staticSwitch126 ).x , ( float4( ( ase_worldPos * _Noise02Tiling ) , 0.0 ) + ( _Time.y * _Noise02ScrollSpeed ) + staticSwitch126 ).y));
			float3 weightedBlendVar104 = temp_output_103_0;
			float weightedBlend104 = ( weightedBlendVar104.x*tex2D( _Noise02, appendResult99 ).r + weightedBlendVar104.y*tex2D( _Noise02, appendResult97 ).r + weightedBlendVar104.z*tex2D( _Noise02, appendResult95 ).r );
			#ifdef _NOISE02ENABLED_ON
				float staticSwitch128 = weightedBlend104;
			#else
				float staticSwitch128 = 1.0;
			#endif
			float NoiseResult116 = ( weightedBlend72 * staticSwitch128 * _NoiseDistortionPower );
			float temp_output_18_0 = ( -distance( ase_worldPos , appendResult17 ) + _MaskDistance + NoiseResult116 );
			float clampResult125 = clamp( (0.0 + (temp_output_18_0 - 0.0) * (1.0 - 0.0) / (( _MaskThickness + NoiseResult116 ) - 0.0)) , 0.0 , 1.0 );
			float clampResult6 = clamp( (0.0 + (temp_output_18_0 - 0.0) * (1.0 - 0.0) / (( _MaskDistance + NoiseResult116 ) - 0.0)) , 0.0 , 1.0 );
			#ifdef _MASKCONSTANTTHICKNESS_ON
				float staticSwitch51 = clampResult125;
			#else
				float staticSwitch51 = clampResult6;
			#endif
			float clampResult28 = clamp( ( ( 1.0 - pow( ( 1.0 - staticSwitch51 ) , _MaskExp ) ) * _MaskMultiply ) , 0.0 , 1.0 );
			float2 appendResult37 = (float2(clampResult28 , 0.0));
			#ifdef _MASKTEXTUREENABLED_ON
				float staticSwitch50 = tex2D( _MaskTexture, appendResult37 ).r;
			#else
				float staticSwitch50 = clampResult28;
			#endif
			float FinalMask53 = staticSwitch50;
			float clampResult45 = clamp( ( _RampMultiplyTiling * FinalMask53 ) , 0.0 , 1.0 );
			#ifdef _RAMPFLIP_ON
				float staticSwitch49 = ( 1.0 - clampResult45 );
			#else
				float staticSwitch49 = clampResult45;
			#endif
			float2 appendResult46 = (float2(staticSwitch49 , 0.0));
			o.Emission = ( _RampColorTint * _FinalPower * tex2D( _Ramp, appendResult46 ) ).rgb;
			o.Alpha = staticSwitch50;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}