Shader "Custom/StencilShow" {
	Properties{

	}
	SubShader {
		Tags { "Queue" = "Geometry" }

		ColorMask 0
		ZWrite On
		ZTest Always


		Stencil {
			Ref 100
			Comp Greater
		}

		Pass {}
	} 
}
