Shader "Custom/DepthStencil" {
	SubShader {
		Tags { "Queue" = "Geometry" }

		ZWrite On
		ColorMask 0
		     
		Pass {
		    Stencil {
		        Ref 100
		        Comp Always
		        Pass Replace
		    }
		}
	}
}
