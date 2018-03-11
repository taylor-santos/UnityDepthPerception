Shader "Custom/Stencil 2 (Layer 2)" {
	Properties {
	}
	SubShader {
		Tags { "Queue" = "Geometry" }

		 ZWrite Off
		 ColorMask 0
		     
		 Pass {
		     Stencil {
		         Ref 1.0
		         Comp Always
		         Pass replace
		     }
		 }
	} 
	FallBack "Transparent"
}
