Shader "Meta/UnwarpShader"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_DistMap("Distortion map", 2D) = "black" {}
		_Mask("Mask", 2D) = "black" {}
		_IsLeft("Is Left", Range(0,1)) = 0
		x_max("x_max", float) = 0
		y_max("y_max", float) = 0
		_Tilt("Tilt", float) = 0 //tangent of the tilt angle
		_TiltSecant("_TiltSecant", float) = 0 //secant of the tilt angle
	}
		SubShader
	{
		Pass
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM

#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

	sampler2D _MainTex;
	uniform float4 _MainTex_TexelSize;
	sampler2D _DistMap;
	sampler2D _Mask;
	uniform float _IsLeft;
	uniform float x_max;
	uniform float y_max;
	uniform float _Tilt;
	uniform float _TiltSecant;
	uniform int _FlipVertical;

	struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	appdata vert(appdata i)
	{
		appdata o;
		//Subtraction of 1 occurs on y axis irregardless of use on left/right eye
		// o.vertex = float4(i.uv.x - _IsLeft, 2.0 * i.uv.y - 1.0, 0.0, 1.0);
		o.vertex = float4(i.uv.x - _IsLeft, 2.0 * i.uv.y - 1.0, 0.0, 1.0);
		o.uv = i.uv;

		// flip in RenderTexture
		if (_ProjectionParams.x < 0)
			o.uv.y = 1 - o.uv.y;

		// TODO need to further investigate why the flipping happens in direct mode
		if(_FlipVertical > 0)
			o.uv.y = 1 - o.uv.y;

		return o;
	}

	float4 frag(appdata i) : COLOR
	{
		float4 mxySampler = tex2D(_DistMap, i.uv);
		float4 maskSampler = tex2D(_Mask, i.uv);
		float tx = (mxySampler.r * 255 + mxySampler.g) / 256;
		float ty = (mxySampler.b * 255 + mxySampler.a) / 256;
	
		float tan_x = 2 * (tx - .5);
		float tan_y = 2 * (ty - .5);
		
		float y_denom = (1 - tan_y*_Tilt);
		tan_y = (tan_y + _Tilt) / y_denom;			// y Tilt correction
		tan_x = tan_x * _TiltSecant/ y_denom;	// x Tilt correction

		tx = .5 + tan_x/(2*x_max);
		ty = .5 + tan_y/(2*y_max);

		return tex2D(_MainTex, float2(tx, ty)) * maskSampler;
	}
		ENDCG
	}
	}
		FallBack "Diffuse"
}
