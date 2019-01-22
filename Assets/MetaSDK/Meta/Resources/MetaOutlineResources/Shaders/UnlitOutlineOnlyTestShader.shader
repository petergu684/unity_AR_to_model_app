Shader "Unlit/UnlitOutlineOnlyTestShader"
{
	Properties
	{
		_OutlineAmount("OutlineAmount", Range(0,0.1)) = 0.003
		_Threshold("Threshold", Range(0,1)) = 0.03
		_GlowColor("Glow Color", Color) = (1,0.2391481,0.1102941,1)
	}
		SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 100


		Pass
	{
		Cull Front
		ZWrite On
		ZTest LEqual
		CGPROGRAM

#pragma vertex vert
#pragma fragment frag
#pragma geometry geom
		// make fog work
#pragma multi_compile_fog

#include "UnityCG.cginc"
		sampler2D _CameraDepthTexture;


	struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
		float3 normal : NORMAL;
	};

	struct v2g
	{
		float2 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;
	};

	struct v2f
	{
		float2 uv : TEXCOORD0;
		float4 screen_uv : TEXCOORD1;
		float4 vertex : SV_POSITION;
	};

	sampler2D _MainTex;
	float4 _MainTex_ST;
	float _OutlineAmount;
	float _Threshold;
	float4 _GlowColor;


	v2g vert(appdata v)
	{
		v2g o;
		o.vertex = v.vertex;
		o.uv = TRANSFORM_TEX(v.uv, _MainTex);
		return o;
	}


	// Geometry Shader -----------------------------------------------------
	[maxvertexcount(3)]
	void geom(triangle v2g p[3], inout TriangleStream<v2f> triStream)
	{
		v2f pIn;
		p[0].vertex = float4(UnityObjectToViewPos(p[0].vertex),1);
		p[1].vertex = float4(UnityObjectToViewPos(p[1].vertex), 1);
		p[2].vertex = float4(UnityObjectToViewPos(p[2].vertex),1);

		float4 a1 = normalize(p[0].vertex - p[2].vertex);
		float4 a2 = normalize(p[0].vertex - p[1].vertex);
		float4 a3 = normalize(p[1].vertex - p[2].vertex);

		float3 norm = cross(a1, a2);

		float4 more0 = p[0].vertex + (a1 + a2) + float4(norm,0);
		float4 more1 = p[1].vertex + (a3 - a2) + float4(norm,0);
		float4 more2 = p[2].vertex + (-a1 - a3) + float4(norm,0);

		pIn.vertex = mul(UNITY_MATRIX_P, p[0].vertex + more0 * _OutlineAmount);
		pIn.screen_uv = ComputeScreenPos(pIn.vertex);
		pIn.uv = p[0].uv;
		triStream.Append(pIn);

		pIn.vertex = mul(UNITY_MATRIX_P, p[1].vertex + more1 * _OutlineAmount);
		pIn.uv = p[1].uv;
		pIn.screen_uv = ComputeScreenPos(pIn.vertex);
		triStream.Append(pIn);

		pIn.vertex = mul(UNITY_MATRIX_P, p[2].vertex + more2 * _OutlineAmount);
		pIn.screen_uv = ComputeScreenPos(pIn.vertex);
		pIn.uv = p[2].uv;
		triStream.Append(pIn);
	}


	fixed4 frag(v2f i) : SV_Target
	{
		float2 uv = i.screen_uv.xy / i.screen_uv.w; //division is done in pixel shader to avoid warping
													// i.uv.w is the fragment depth

#if defined(UNITY_REVERSED_Z)
		float sceneDepth = tex2D(_CameraDepthTexture, uv);
#else 
		float sceneDepth = 1.0f - tex2D(_CameraDepthTexture, uv);
#endif

		sceneDepth = LinearEyeDepth(sceneDepth);
		float depthValue = i.screen_uv.w;
		if ((sceneDepth - depthValue) < _Threshold)
		{
			discard;
		}


		return _GlowColor;
	}
		ENDCG
	}
	}
}
