Shader "Custom/DepthMaskFront" {
	 SubShader {
	    Tags {Queue = Background}
	    ZWrite Off
		 ColorMask 0
		     
		 Pass {
		     Stencil {
		         Ref 1
		         Comp always
		         Pass replace
		     }
		 }
	}
}
