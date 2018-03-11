Shader "Custom/DepthMask" {
	SubShader {
	    Tags {"Queue" = "Geometry-1"}
	    Pass {
	    	ColorMask 0
	    	ZWrite On
	    }
	}
}
