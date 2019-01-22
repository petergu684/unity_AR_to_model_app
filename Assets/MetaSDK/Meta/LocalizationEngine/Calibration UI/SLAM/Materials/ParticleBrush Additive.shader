// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Meta/ParticleBrush Additive" {
Properties {
	[HDR]
	_MultiplyColor("Multiply Color", Color) = (1,1,1,1)

	[HDR]
	_TintColor ("Tint Color 1", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Particle Texture 1", 2D) = "white" {}

	[HDR]
	_TintColor2("Tint Color 2", Color) = (0.5,0.5,0.5,0.5)
	_MainTex2("Particle Texture 2", 2D) = "white" {}

	[HDR]
	_TintColor3("Tint Color 3", Color) = (0.5,0.5,0.5,0.5)
	_MainTex3("Particle Texture 3", 2D) = "white" {}

	_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
	Blend SrcAlpha One
	ColorMask RGB
	Cull Off Lighting Off ZWrite Off
	
	SubShader {
		Pass {
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc"

			float4 _MultiplyColor;

			sampler2D _MainTex;
			float4 _TintColor;

			sampler2D _MainTex2;
			float4 _TintColor2;

			sampler2D _MainTex3;
			float4 _TintColor3;

			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 texcoord2 : TEXCOORD1;
				float2 texcoord3 : TEXCOORD2;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 texcoord2 : TEXCOORD1;
				float2 texcoord3 : TEXCOORD2;
			};
			
			float4 _MainTex_ST;
			float4 _MainTex2_ST;
			float4 _MainTex3_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				o.texcoord2 = TRANSFORM_TEX(v.texcoord2, _MainTex2);
				o.texcoord3 = TRANSFORM_TEX(v.texcoord3, _MainTex3);

				return o;
			}

			sampler2D_float _CameraDepthTexture;
			float _InvFade;
			
			float4 frag (v2f i) : SV_Target
			{
				
				
				float4 col  = 2.0f * i.color * _TintColor * tex2D(_MainTex, i.texcoord);
				float4 col2 = 2.0f * i.color * _TintColor2 * tex2D(_MainTex2, i.texcoord2);
				float4 col3 = 2.0f * i.color * _TintColor3 * tex2D(_MainTex3, i.texcoord3);

				return (col + col2 + col3) * _MultiplyColor;
			}
			ENDCG  
		}
	}	
}
}
