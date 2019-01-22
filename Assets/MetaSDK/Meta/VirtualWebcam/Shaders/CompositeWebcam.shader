Shader "Meta/CompositeWebcam"
{
	Properties
	{
		_ContentTex("Content Texture", 2D) = "black" {}
		_WebcamTex("Webcam Texture", 2D) = "white" {}
		_Alpha("Alpha", Range(0,1)) = 1.0
	}

	SubShader
	{
		Pass
		{
			CGPROGRAM

			// Unity variables
			uniform sampler2D _ContentTex;
			uniform sampler2D _WebcamTex;
			uniform float _Alpha;
		
			// user variables
			uniform float4 _ContentTex_ST;
			uniform float4 _WebcamTex_ST;

			// settings
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			// Vertex input data
			struct VertexInput
			{
				float4 vertexPosition : POSITION;
				float4 contentTexCoord : TEXCOORD0;
				float4 webcamTexCoord : TEXCOORD1;
			};

			// Vertex output data
			struct VertexOutput
			{
				float4 vertexPosition : SV_POSITION;
				float4 contentTexCoord : TEXCOORD0;
				float4 webcamTexCoord : TEXCOORD1;
			};

			// vertex shader 
			VertexOutput vert(VertexInput vi)
			{
				// output vertex declaration
				VertexOutput vo;

				// convert to unity space
				vo.vertexPosition = UnityObjectToClipPos(vi.vertexPosition);

				// apply the texture coordinates
				vo.contentTexCoord = float4(TRANSFORM_TEX(vi.contentTexCoord, _ContentTex), 0, 0);
				vo.webcamTexCoord = float4(TRANSFORM_TEX(vi.webcamTexCoord, _WebcamTex), 0, 0);

				return vo;
			}

			// fragment shader
			float4 frag(VertexOutput vo) : COLOR
			{
				float4 contentColor = tex2D(_ContentTex, vo.contentTexCoord);
                vo.webcamTexCoord.y = 1 - vo.webcamTexCoord.y;
				float4 webcamColor = tex2D(_WebcamTex, vo.webcamTexCoord);
				webcamColor.a = 1;
                webcamColor = webcamColor.bgra;
				float4 finalColor = lerp(contentColor, webcamColor, _Alpha);

				return finalColor;
			}

			// End CG shader
			ENDCG
		}
	}
	FallBack "Diffuse"
}
