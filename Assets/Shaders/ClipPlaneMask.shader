Shader "Custom/ClipPlaneMask" {
	SubShader 
	{
		Tags { "RenderType"="Opaque" "Queue"="Geometry-1"}
		ColorMask 0
		ZWrite off
		Stencil 
		{
			Ref 1
			Comp always
			Pass replace
		}
		
		Pass
		{
		}
	}
}
