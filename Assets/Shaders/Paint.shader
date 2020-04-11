Shader "Custom/Paint" {
	Properties{

		_MainTex("Albedo (RGB)", 2D) = "white" {}

		_Smoothness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0

		_powerNormal("Paint power norm", Range(0,1)) = 0.0
		_noiseAmount("Paint noise amount", Range(0,1)) = 0.01
		_ScaleNoiseTex("Paint noise scale", Range(0,10)) = 1.0

			//Save Paint
			_PaintTex("Paint Texture", 2D) = "black" {}
			_PaintNormalTex("Paint Normal Texture", 2D) = "black" {}

		_NoiseTex("Paint Noise", 2D) = "black" {}
		_NormalTex("Paint Normal Map", 2D) = "" {}
		_PaintSmoothness("Paint Smoothness", Range(0,1)) = 0.0
		_DetailTex("Paint Detail Texture", 2D) = "white" {}

			_Cube("Cubemap", CUBE) = "" {}
	}
		SubShader{
			Tags { "RenderType" = "Opaque" }

			LOD 200

			CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf StandardSpecular fullforwardshadows 
		#pragma multi_compile_fog
		#pragma multi_compile _ LIGHTMAP_ON
		#pragma multi_compile _ UNITY_HDR_ON
		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _PaintTex;
		sampler2D _PaintNormalTex;
		sampler2D _NormalTex;
		sampler2D _DetailTex;
		sampler2D _NoiseTex;

		samplerCUBE _Cube;

		half _Smoothness;
		half _Metallic;

		float _PaintSmoothness;
		float _powerNormal;
		float _noiseAmount;
		float _ScaleNoiseTex;

		struct Input {
			float2 uv_MainTex;
			float2 uv2_PaintTex;
			float3 worldRefl;
			INTERNAL_DATA
		};

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf(Input IN, inout SurfaceOutputStandardSpecular o) {

			//Light map uv
			float2 UV_paintColor = IN.uv2_PaintTex * unity_LightmapST.xy + unity_LightmapST.zw;
			fixed4 noise = tex2D(_NoiseTex, UV_paintColor * _ScaleNoiseTex) * _noiseAmount;
			float2 UV_paintColorNoise = float2(UV_paintColor.x + noise.x - noise.y, UV_paintColor.y + noise.y - noise.x);
			float4 paintAlbedo;
			float2 UV_detail;

			

			fixed4 normalMap = tex2D(_NormalTex, UV_paintColor) * _powerNormal;

			UV_detail = UV_paintColor + normalMap.xy * 0.3;

			fixed4 detail = tex2D(_DetailTex, UV_detail) * 1.8;
			fixed4 albedo = tex2D(_MainTex, IN.uv_MainTex);

			fixed4 paintColor = tex2D(_PaintTex, UV_paintColorNoise);
			fixed4 paintNormal = tex2D(_PaintNormalTex, UV_paintColorNoise);

			paintAlbedo = (paintColor + detail);

			float mask = step(0.2, paintColor.r + paintColor.g + paintColor.b);

			o.Albedo = lerp(albedo, paintAlbedo, mask);
			o.Normal = lerp(o.Normal, UnpackNormal(normalMap), mask);
			o.Smoothness = lerp(_Smoothness, _PaintSmoothness, mask);
			o.Specular = lerp(0.0, texCUBE(_Cube, WorldReflectionVector(IN, o.Normal)).rgb * paintColor, mask);

		}
		ENDCG
	}
		FallBack "Diffuse"
}
