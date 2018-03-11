Shader "Stencil/StencilMask0"
{
	SubShader {
		Tags { "Queue" = "Geometry-1" }

		ZWrite Off
		ColorMask 0
		     
		Pass {
		    Stencil {
		        Ref 0
		        Comp Always
		        Pass Replace
		    }
		}
	}
}