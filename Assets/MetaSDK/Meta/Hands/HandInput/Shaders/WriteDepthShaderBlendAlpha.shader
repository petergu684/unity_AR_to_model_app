// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'



Shader "Unlit/WriteDepthShaderBlendAlpha" {
	Properties
	{
		[PerRendererData] _MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
		_CutOff("Cut Threshold", Range(0,1)) = 0.2
	}
	SubShader{

		Pass{
			Blend SrcAlpha OneMinusSrcAlpha
		Fog{ Mode Off }
		CGPROGRAM
#pragma exclude_renderers gles flash
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
		// vertex input: position, UV
		struct appdata {
		float4 vertex : POSITION;
		float4 texcoord : TEXCOORD0;
	};

	struct v2f {
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
		float4 screen_uv : TEXCOORD1;
	};

	v2f vert(appdata v) {
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord.xy;
		o.screen_uv = ComputeScreenPos(o.pos);
		return o;
	}

	struct C2E2f_Output {
		float4 col:COLOR;
		float dep : DEPTH;
	};

	sampler2D _MainTex;
	float _CutOff;

	C2E2f_Output frag(v2f i) {

		float2 uv = i.screen_uv.xy / i.screen_uv.w; //division is done in pixel shader to avoid warping
													  // i.uv.w is the fragment depth

		C2E2f_Output o;
		half4 c = tex2D(_MainTex, i.uv);
		clip(c.a - _CutOff);
		//c = lerp(tex2D(_BackgroundTexture, uv),c, c.a);
		o.col = c; // this is not iteresting
		o.dep = 9999.9; // this is what I want to output into Z-buffer (0 value is just an example)
		return o;
	}
	ENDCG
	}
	}
}