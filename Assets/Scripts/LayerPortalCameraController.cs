using UnityEngine;
using System.Collections;

public class LayerPortalCameraController : MonoBehaviour {
	public GameObject playerCamera;
	public GameObject portalCamera;
	public GameObject otherPortal;
	public GameObject portalBlocker;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void LateUpdate () {
		portalCamera.transform.position = playerCamera.transform.position - transform.forward * Mathf.Abs(transform.localScale.y);
		portalCamera.transform.rotation = playerCamera.transform.rotation;
		portalCamera.transform.localScale = playerCamera.transform.localScale;

		portalCamera.transform.SetParent(transform,true);

		Vector3 pos = portalCamera.transform.localPosition;
		Vector3 scale = portalCamera.transform.localScale;
		Quaternion rotation = portalCamera.transform.localRotation;

		portalCamera.transform.parent = otherPortal.transform;

		portalCamera.transform.localPosition = pos;
		portalCamera.transform.localScale = scale;
		portalCamera.transform.localRotation = rotation;

		portalCamera.transform.parent = null;

		portalCamera.transform.RotateAround(otherPortal.transform.position, otherPortal.transform.forward, 180);
		float near = Vector3.Dot((portalCamera.transform.position - otherPortal.GetComponent<Collider>().ClosestPointOnBounds(portalCamera.transform.position)),-portalCamera.transform.forward) - Mathf.Pow(otherPortal.GetComponent<Collider>().bounds.extents.magnitude,1f/3);
		near = Mathf.Clamp(near,0.01f,10000);
		portalCamera.GetComponent<Camera>().nearClipPlane = 0.01f;

		portalCamera.transform.localScale = Vector3.one;

		Transform parent = playerCamera.transform.parent;
		playerCamera.transform.parent = null;
		playerCamera.transform.localScale = Vector3.one;

		

		if (portalBlocker != null)
		{
			portalBlocker.transform.parent = otherPortal.transform.parent;
			portalBlocker.transform.position = otherPortal.transform.position;
			portalBlocker.transform.rotation = otherPortal.transform.rotation;
			portalBlocker.transform.localScale = otherPortal.transform.localScale;


			scale = otherPortal.transform.localScale;

			portalBlocker.transform.parent = playerCamera.transform;

			pos = portalBlocker.transform.localPosition;
			rotation = portalBlocker.transform.localRotation;

			portalBlocker.transform.parent = portalCamera.transform;

			portalBlocker.transform.localPosition = pos;
			portalBlocker.transform.localRotation = rotation;

			portalBlocker.transform.parent = null;

			portalBlocker.transform.localScale = scale;
		}
		portalCamera.transform.parent = otherPortal.transform;

		playerCamera.transform.parent = parent;

		//float d1 = Vector3.Dot(portalCamera.transform.forward,otherPortal.GetComponent<Collider>().ClosestPointOnBounds(portalCamera.transform.position) - portalCamera.transform.position);
		//float d2 = Vector3.Dot(portalCamera.transform.forward,portalBlocker.GetComponent<Collider>().ClosestPointOnBounds(portalCamera.transform.position) - portalCamera.transform.position);
		//float d = Mathf.Min(d1,d2);
		float d = 0.01f;
		portalCamera.GetComponent<Camera>().nearClipPlane = Mathf.Clamp(d-otherPortal.GetComponent<Collider>().bounds.extents.magnitude/2,0.1f,Mathf.Infinity);
	}
}
