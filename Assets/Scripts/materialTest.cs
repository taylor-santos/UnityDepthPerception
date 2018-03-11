using UnityEngine;
using System.Collections;

public class materialTest : MonoBehaviour {
	public Vector3 PlanePosition;
	public Vector3 PlaneNormal;

	private Material mat;

	// Use this for initialization
	void Start () {
		mat = GetComponent<Renderer>().material;
	}
	
	// Update is called once per frame
	void Update () {
		mat.SetVector("_PlanePosition", new Vector4(PlanePosition.x, PlanePosition.y, PlanePosition.z, 0));
		mat.SetVector("_PlaneNormal", new Vector4(PlaneNormal.x, PlaneNormal.y, PlaneNormal.z, 0));
	}
}
