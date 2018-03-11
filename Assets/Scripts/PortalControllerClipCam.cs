using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

public class PortalControllerClipCam : MonoBehaviour {
	public GameObject OtherPortal;
	public GameObject Player;
	public uint depth;
	public int layer;
	public int maskLayer;
	public int renderLayer;
	private Camera playerCamera;
	private Camera portalCamera;
	private Camera stencilCamera;
	private GameObject MaskPortal;
	private Matrix4x4 start_projection;
	public RenderTexture cameraRT;
	public RenderTexture stencilRT;
	//public Texture2D tex;
	public Material overlay;

	void Start () {
		playerCamera = Player.GetComponent<Camera>();
		PortalControllerClipCam otherPortalComponent = OtherPortal.GetComponent<PortalControllerClipCam>();

		GameObject portalCameraObject = new GameObject();
		portalCameraObject.name = name + " Camera";
		portalCamera = portalCameraObject.AddComponent<Camera>();
		portalCamera.CopyFrom(playerCamera);
		portalCamera.cullingMask &= ~((1 << renderLayer) | (1 << otherPortalComponent.renderLayer) | 
		                              (1 << maskLayer)   | (1 << otherPortalComponent.maskLayer) |
		                              (1 << layer)       | (1 << otherPortalComponent.layer));

		start_projection = portalCamera.projectionMatrix;

		GameObject stencilCameraObject = new GameObject();
		stencilCameraObject.name = name + " Stencil Camera";
		stencilCamera = stencilCameraObject.AddComponent<Camera>();
		stencilCamera.CopyFrom(playerCamera);
		stencilCamera.cullingMask = playerCamera.cullingMask;
		stencilCamera.cullingMask &= ~((1 << renderLayer) | (1 << otherPortalComponent.renderLayer) | 
		                               (1 << maskLayer)   |
		                               (1 << layer));
		stencilCamera.cullingMask |= (1 << otherPortalComponent.maskLayer) | (1 << otherPortalComponent.layer);
		stencilCamera.clearFlags = CameraClearFlags.Skybox;
		stencilCamera.backgroundColor = new Color(0,0,0,0);
		stencilCameraObject.transform.parent = Player.transform;
		stencilCameraObject.transform.localPosition = Vector3.zero;
		stencilCameraObject.transform.localScale = Vector3.one;
		stencilCameraObject.transform.localRotation = Quaternion.identity;

		cameraRT = new RenderTexture(playerCamera.pixelWidth, playerCamera.pixelHeight, 32);
		cameraRT.name = name + " RenderTexture";
		cameraRT.antiAliasing = 1;
		cameraRT.filterMode = FilterMode.Point;
		portalCamera.targetTexture = cameraRT;

		stencilRT = new RenderTexture(playerCamera.pixelWidth, playerCamera.pixelHeight, 32);
		stencilRT.name = name + " Stencil RenderTexture";
		stencilRT.antiAliasing = 1;
		stencilRT.filterMode = FilterMode.Point;
		stencilCamera.targetTexture = stencilRT;

		overlay = new Material(Shader.Find("Custom/MaskedTexture"));
		overlay.SetTexture("_MainTex", cameraRT);
		overlay.SetTexture("_Mask", stencilRT);

		//tex = new Texture2D(Screen.width, Screen.height);

		MeshFilter MF = GetComponent<MeshFilter>();

		MaskPortal = new GameObject();
		MaskPortal.name = name + " Mask";
		MaskPortal.layer = otherPortalComponent.maskLayer;
		MaskPortal.transform.parent = transform;
		MaskPortal.transform.localPosition = Vector3.zero;
		MaskPortal.transform.localScale = Vector3.one;
		MaskPortal.transform.localRotation = Quaternion.identity;
		MeshFilter maskMF = MaskPortal.AddComponent<MeshFilter>();
		maskMF.mesh = MF.mesh;
		MeshRenderer maskMR = MaskPortal.AddComponent<MeshRenderer>();
		maskMR.material = new Material(Shader.Find("Stencil/StencilHide"));
	}

	void LateUpdate () {
		portalCamera.transform.position = playerCamera.transform.position;
		portalCamera.transform.rotation = playerCamera.transform.rotation;
		portalCamera.transform.localScale = playerCamera.transform.localScale;

		portalCamera.transform.parent = OtherPortal.transform;
		Vector3 pos = portalCamera.transform.localPosition;
		Quaternion rot = portalCamera.transform.localRotation;
		portalCamera.transform.parent = transform;
		portalCamera.transform.localPosition = pos;
		portalCamera.transform.localRotation = rot;

		float sign = -Vector3.Dot(portalCamera.transform.localPosition, Vector3.forward);

		Vector3 pos_offset = transform.position - transform.forward * Mathf.Sign(sign) * 0.1f;

		Vector4 clipPlane = CameraSpacePlane(portalCamera, pos_offset, transform.forward, sign, 0);
		Matrix4x4 projection = start_projection;
		projection = CalculateObliqueMatrix(projection, clipPlane, -1);
		portalCamera.projectionMatrix = projection;
	}

	void OnTriggerEnter(Collider coll) 
	{
		GameObject obj = coll.gameObject;
		if (obj.tag != "Teleport Clone"){
			/*
			if (!inObjectChildren.Contains(obj)){
				inObjectChildren.Add(obj);
			}
			while (obj.transform.parent != null){
				obj = obj.transform.parent.gameObject;
			}
			if (!inClonedObjects.Contains(obj)){
				Debug.Log(obj.name + " in " + name);
				GameObject inClone = obj.CreateChildrenClones(" - In-Clone (" + name + ")");
				inCloneMRs.Add(inClone.GetChildrenMRs());
				inCloneRBs.Add(inClone.GetComponent<Rigidbody>());
				inObjectRBs.Add(obj.GetComponent<Rigidbody>());
				SetChildrenTag(inClone, "Teleport Clone");
				inClonedObjects.Add(obj);
				List<MeshRenderer> objMRs = obj.GetChildrenMRs();
				foreach (MeshRenderer MR in objMRs)
				{
					Material newMat = new Material(Shader.Find("Custom/ClippableStandard"));
					newMat.CopyPropertiesFromMaterial(MR.material);
					MR.material = newMat;
				}
				inObjectMRs.Add(objMRs);
				inClones.Add(inClone);
				float inDirection = Vector3.Dot(obj.transform.position - transform.position, transform.forward);
				inCloneDirections.Add(inDirection);
				inClone.CenterOnObject(obj);
				inClone.PortalTransform(this.gameObject, OtherPortal);
			}
			*/
		}
	}

	void OnTriggerExit(Collider coll)
	{
		GameObject obj = coll.gameObject;
		/*
		inObjectChildren.Remove(obj);
		while (obj.transform.parent != null)
			obj = obj.transform.parent.gameObject;
		if (!areAnyChildrenInList(obj, inObjectChildren))
		{
			int index = inClonedObjects.IndexOf(obj);
			if (index != -1)
			{
				Debug.Log(obj.name + " out " + name + "[" + index + "]");
				Destroy(inClones[index]);

				List<GameObject> allChildrenClones = inClonedObjects[index].GetChildrenClones();
				foreach(GameObject childClone in allChildrenClones){
					childClone.SetActive(true);
				}

				foreach (MeshRenderer MR in inObjectMRs[index])
				{
					Material newMat = new Material(Shader.Find("Standard"));
					newMat.CopyPropertiesFromMaterial(MR.material);
					MR.material = newMat;
				}

				inClones.RemoveAt(index);
				inCloneDirections.RemoveAt(index);
				inCloneMRs.RemoveAt(index);
				inObjectMRs.RemoveAt(index);
				inCloneRBs.RemoveAt(index);
				inObjectRBs.RemoveAt(index);
				inClonedObjects.RemoveAt(index);
			}
		}
		*/
	}

	void OnGUI() {
		GUI.color = Color.white;
		Graphics.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), cameraRT, overlay, 0);
	}

	Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign,float clipPlaneOffset)
    {        
        Vector3 offsetPos = pos + normal * clipPlaneOffset;
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 cpos = m.MultiplyPoint(offsetPos);
        Vector3 cnormal = m.MultiplyVector(normal).normalized * Mathf.Sign(sideSign);
        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
    }

	Matrix4x4 CalculateObliqueMatrix(Matrix4x4 projection, Vector4 clipPlane, float sideSign)
    {
        Vector4 q = projection.inverse * new Vector4(
            Mathf.Sign(clipPlane.x),
            Mathf.Sign(clipPlane.y),
            1.0f,
            1.0f
        );
        Vector4 c = clipPlane * (2.0F / (Vector4.Dot(clipPlane, q)));
        // third row = clip plane - fourth row
        projection[2] = c.x + Mathf.Sign(sideSign)*projection[3];
        projection[6] = c.y + Mathf.Sign(sideSign) * projection[7];
        projection[10] = c.z + Mathf.Sign(sideSign) * projection[11];
        projection[14] = c.w + Mathf.Sign(sideSign) * projection[15];
        return projection;
    }

    static T CopyComponent<T>(T original, GameObject destination) where T : Component
	{
		System.Type type = original.GetType();
		var dst = destination.GetComponent(type) as T;
		if (!dst) 
			dst = destination.AddComponent(type) as T;
		var fields = type.GetFields();
		foreach (var field in fields){
			if (field.IsStatic) continue;
			field.SetValue(dst, field.GetValue(original));
		}
		var props = type.GetProperties();
		foreach (var prop in props){
			if (!prop.CanWrite || !prop.CanWrite || prop.Name == "name") continue;
			prop.SetValue(dst, prop.GetValue(original, null), null);
		}
		return dst as T;
	}
}
