﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/CopyTexture"
{
Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Overlay ("Overlay", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _Overlay;

            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
            	float2 uv = i.screenPos.xy / i.screenPos.w;
            	if (tex2D(_Overlay, uv).a == 1.0){
            		fixed3 col = tex2D(_Overlay, uv).rgb;
                	return fixed4(col, 1);
            	}else if (tex2D(_MainTex, uv).a == 1.0){
                    fixed3 col = tex2D(_MainTex, uv).rgb;
                    return fixed4(col, 1);
                }else{
            		clip(-1);
            		return fixed4(0,0,0,0);
            	}
            	
                
            }
            ENDCG
        }
    }
}