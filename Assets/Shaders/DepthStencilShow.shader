Shader "Custom/DepthStencilShow" {
	SubShader {
		Tags {"Queue" = "Geometry-1"}
		Stencil {
				Ref 1
				Comp NotEqual
				Pass keep
		}
		
	    Pass {
	    	ColorMask 0
	    }
	}
}
