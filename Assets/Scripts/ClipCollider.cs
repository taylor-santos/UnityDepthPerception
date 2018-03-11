using UnityEngine;
using System.Collections;

public class ClipCollider : MonoBehaviour {
	private MeshCollider MC;
	// Use this for initialization
	void Start () {
		MC = gameObject.GetComponent<MeshCollider>();
		if (MC == null)
		{
			gameObject.GetComponent<Collider>().enabled = false;
			MC = gameObject.AddComponent<MeshCollider>();
			MC.convex = true;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
