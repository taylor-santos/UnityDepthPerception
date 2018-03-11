Shader "Stencil/StencilMask"
{
	SubShader {
		Tags { "Queue" = "Geometry-1" }

		ZWrite Off
		ColorMask 0
		     
		Pass {
		    Stencil {
		        Ref 1
		        Comp Always
		        Pass Replace
		    }
		}
	}
}