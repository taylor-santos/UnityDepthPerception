Shader "Custom/Stencil 1" {
	SubShader {
		Tags { "Queue" = "Geometry" }

		ColorMask 0
		ZWrite On
		ZTest Always


		 Stencil {
			 Ref 1
			 Comp Equal
		 }

		 Pass {}
	} 
}
