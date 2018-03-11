Shader "Custom/ClipPlane" {
	Properties {
		_PlanePosition ("Plane Position", Vector) = (0,0,0,0)
		_PlaneNormal ("Plane Normal", Vector) = (0,0,0,0)
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo", 2D) = "white" { }
		_Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		[Gamma]  _Metallic ("Metallic", Range(0,1)) = 0
		 _MetallicGlossMap ("Metallic", 2D) = "white" { }
		 _BumpScale ("Scale", Float) = 1
		 _BumpMap ("Normal Map", 2D) = "bump" { }
		 _Parallax ("Height Scale", Range(0.005,0.08)) = 0.02
		 _ParallaxMap ("Height Map", 2D) = "black" { }
		 _OcclusionStrength ("Strength", Range(0,1)) = 1
		 _OcclusionMap ("Occlusion", 2D) = "white" { }
		 _EmissionColor ("Color", Color) = (0,0,0,1)
		 _EmissionMap ("Emission", 2D) = "white" { }
		 _DetailMask ("Detail Mask", 2D) = "white" { }
		 _DetailAlbedoMap ("Detail Albedo x2", 2D) = "grey" { }
		 _DetailNormalMapScale ("Scale", Float) = 1
		 _DetailNormalMap ("Normal Map", 2D) = "bump" { }
	}
	SubShader {
		Stencil {
			Ref 1
			Comp NotEqual
			Pass keep
			Fail keep
		}
		Tags { "RenderType"="Cutout" "Queue" = "Geometry+1"}
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		fixed4 _PlanePosition;
		fixed4 _PlaneNormal;

		void surf (Input IN, inout SurfaceOutputStandard o) {

			float3 normal = normalize(_PlaneNormal.xyz);
			float dist = dot(normal.xyz, IN.worldPos - _PlanePosition.xyz);
			if (dist < 0)
			{
				discard;
			}else{
				// Albedo comes from a texture tinted by color
				fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
				o.Albedo = c.rgb;
				// Metallic and smoothness come from slider variables
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Alpha = c.a;
			}
		}
		ENDCG
	}
	FallBack "Transparent"
}
