// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/AvoidCamera" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		Pass{
			CGPROGRAM
	        #pragma vertex vert
	        #pragma fragment frag
	        // make fog work
	        #pragma multi_compile_fog
	        
	        #include "UnityCG.cginc"

	        struct appdata
	        {
	            float4 vertex : POSITION;
	            float2 uv : TEXCOORD0;
	        };

	        struct v2f
	        {
	            float2 uv : TEXCOORD0;
	            UNITY_FOG_COORDS(1)
	            float4 vertex : SV_POSITION;
	        };

	        sampler2D _MainTex;
	        float4 _MainTex_ST;
	        
	        v2f vert (appdata v)
	        {
	            v2f o;
	            o.vertex = UnityObjectToClipPos(v.vertex) + UnityObjectToClipPos(mul(unity_WorldToObject, mul(unity_ObjectToWorld, v.vertex) - _WorldSpaceCameraPos));
	            return o;
	        }
	        
	        fixed4 frag (v2f i) : SV_Target
	        {
	            // sample the texture
	            fixed4 col = tex2D(_MainTex, i.uv);
	            // apply fog
	            UNITY_APPLY_FOG(i.fogCoord, col);
	            return col;
	        }
	        ENDCG
		}
	}
}
