using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightCulling : MonoBehaviour {
	public List<Light> Lights;
	public bool Option;
	void OnPreRender(){
		if (Lights != null){
			foreach (Light light in Lights){
				light.enabled = Option;
			}
		}
	}
}
