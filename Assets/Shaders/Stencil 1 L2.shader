Shader "Custom/Stencil 1 (Layer 2)" {
	Properties {
	}
	SubShader {
		Tags { "Queue" = "Geometry" }

		ColorMask 0
		ZWrite On
		ZTest Always


		 Stencil {
			 Ref 1.0
			 Comp Greater
		 }

		 Pass {}
	} 
	FallBack "Transparent"
}
