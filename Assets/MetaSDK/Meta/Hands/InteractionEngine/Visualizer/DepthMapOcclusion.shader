// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Meta/DepthOcclusionShader" {
	Properties
	{
		_Color ("Color", Color) = (0,0,0,1)
		_ParticleTex ("Particle Texture", 2D) = "black" {}
		_DepthTex ("Depth Texture", 2D) = "white" {}
		_DepthScale ("Depth Scale", float) = 0.001
		_DepthWidth("_DepthWidth", float) = 320
		_DepthHeight("_DepthHeight", float) = 240
		_DepthFocalLengthX("_DepthFocalLengthX", float) = 368.096588
		_DepthFocalLengthY("_DepthFocalLengthy", float) = 368.096588
		_DepthPrincipalPointX("_DepthPrincipalPointX", float) = 261.696594
		_DepthPrincipalPointY("_DepthPrincipalPointy", float) = 202.522202
		_XGAP("X Gap", float) = 0.0
		_YGAP("Y Gap", float) = 0.0
		_QUAD_WIDTH("Quad Width", float) = 1
		_QUAD_HEIGHT("Quad Height", float) = 1
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque"}

		LOD 100
		ColorMask 0

		Pass
		{
			// Begin CG shader
			CGPROGRAM

			// Unity variables
			uniform float4 _Color;
			uniform sampler2D _ParticleTex;
			uniform sampler2D _DepthTex;
			uniform float _DepthScale;
			uniform float _DepthWidth;
			uniform float _DepthHeight;
			uniform float _DepthFocalLengthX;
			uniform float _DepthFocalLengthY;
			uniform float _DepthPrincipalPointX;
			uniform float _DepthPrincipalPointY;
			uniform float _XGAP;
			uniform float _YGAP;
			uniform float _QUAD_WIDTH;
			uniform float _QUAD_HEIGHT;

			// user variables
			uniform float4 _ParticleTex_ST;

			// settings
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			
			// Vertex input data
			struct VertexInput
			{
				float4 vertexPosition : POSITION;
				float4 particleTexCoord : TEXCOORD0;
				float4 depthTexCoord : TEXCOORD1;
			};

			// Vertex output data
			struct VertexOutput
			{
				float4 vertexPosition : SV_POSITION;
				float4 particleTexCoord : TEXCOORD0;
			};

			// vertex shader 
			VertexOutput vert(VertexInput vi)
			{
				// output vertex declaration
				VertexOutput vo;

				// get the depth which is returned as a color
                // RGBA -> XYZ(valid bit)
				float4 depthColor = tex2Dlod(_DepthTex, vi.depthTexCoord);

                float indexX = depthColor.r;
				float indexY = depthColor.g;

				float z = depthColor.b;

                // center quad and scale size of quad by depth
                float x = depthColor.r + (-_QUAD_WIDTH/2 + _QUAD_WIDTH * vi.particleTexCoord.x) * _DepthScale * z;
                float y = - depthColor.g + (-_QUAD_HEIGHT/2 + _QUAD_HEIGHT * vi.particleTexCoord.y) * _DepthScale * z;
				
                // mask values from depth image that bilinear interpolation 
                // screws up at edges 
                // mask is held in the A of the RGBA texture 
                // 0 for invalid depth, 1 for valid depth
                int masked = (depthColor.a == 1);
				float4 finalPos = masked * float4(x, y, z, 1);

				// convert to unity space
				vo.vertexPosition = UnityObjectToClipPos(finalPos);

				// apply the texture coordinates
				vo.particleTexCoord = float4(TRANSFORM_TEX(vi.particleTexCoord, _ParticleTex), 0, 0);

				return vo;
			}

			// fragment shader
			float4 frag(VertexOutput vo) : COLOR
			{
				float4 finalColor = tex2D(_ParticleTex, vo.particleTexCoord) * _Color;

				return finalColor;
			}

			// End CG shader
			ENDCG
		}
	} 
}
