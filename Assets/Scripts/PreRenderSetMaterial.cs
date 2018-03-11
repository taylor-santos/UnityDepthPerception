using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreRenderSetMaterial : MonoBehaviour {
	public MeshRenderer MR;
	public PortalCameraController camController;

	void OnPreRender(){
		MR.material.SetTexture("_MainTex", camController.texture);
	}
}
