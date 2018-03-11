using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PortalRTController3 : MonoBehaviour {
	public GameObject OtherPortal;
	public GameObject PlayerCamera;
	public int CloneLayer;

	private GameObject portalCamera;
	private RenderTexture rt;
	private List<GameObject> clonedObjects = new List<GameObject>();
	private List<Material> cloneMaterials = new List<Material>();
	
	public List<GameObject> inClonedObjects = new List<GameObject>();
	public List<GameObject> inClones = new List<GameObject>();
	public List<float> inCloneDirections = new List<float>();
	public List<List<MeshRenderer>> inCloneMRs = new List<List<MeshRenderer>>();
	public List<List<MeshRenderer>> inObjectMRs = new List<List<MeshRenderer>>();
	public List<Rigidbody> inCloneRBs = new List<Rigidbody>();
	public List<Rigidbody> inObjectRBs = new List<Rigidbody>();
	public List<GameObject> inObjectChildren = new List<GameObject>();

	void Start () 
	{
		Camera playerCameraComponent = PlayerCamera.GetComponent<Camera>();
		portalCamera = new GameObject();
		portalCamera.name = name + " Camera";
		Camera portalCameraComponent = portalCamera.AddComponent<Camera>();
		portalCameraComponent.CopyFrom(playerCameraComponent);
		rt = new RenderTexture(portalCameraComponent.pixelWidth, portalCameraComponent.pixelHeight, 32);
		rt.name = name + " RenderTexture";
		rt.antiAliasing = QualitySettings.antiAliasing;
		rt.filterMode = FilterMode.Point;
		GetComponent<MeshRenderer>().material.SetTexture("_MainTex", rt);
		portalCameraComponent.targetTexture = rt;
		portalCameraComponent.cullingMask = (1 << CloneLayer);
	}
	
	void LateUpdate () 
	{
		portalCamera.CenterOnObject(PlayerCamera);
		portalCamera.PortalTransform(this.gameObject, OtherPortal);

		List<GameObject> objects = new List<GameObject>(UnityEngine.Object.FindObjectsOfType<GameObject>() as GameObject[]);
		/*
		List<GameObject> allClones = new List<GameObject>(GameObject.FindGameObjectsWithTag("Clone"));
		objects = new List<GameObject>(objects.Except(clonedObjects));
		objects = new List<GameObject>(objects.Except(allClones));
		*/
		objects.Remove(OtherPortal);
		foreach (GameObject obj in objects) {
			if (obj.tag != "Clone" && !clonedObjects.Contains(obj)){
				MeshRenderer objMR = obj.GetComponent<MeshRenderer>();
				MeshFilter objMF = obj.GetComponent<MeshFilter>();
				if (objMR != null && objMF != null){
					clonedObjects.Add(obj);
					GameObject clone = obj.CreateClone(CloneLayer, " - Clone (" + name + ")", UnityEngine.Rendering.ShadowCastingMode.Off);
					clone.tag = "Clone";
					cloneMaterials.Add(clone.GetComponent<MeshRenderer>().material);
					GameObject shadowCaster = obj.CreateShadowCaster(CloneLayer, " - Shadow Caster (" + name + ")");
					shadowCaster.transform.parent = clone.transform;
				}
			}
		}

		Vector3 PlanePosition = OtherPortal.transform.position;
		Vector3 PlaneNormal = OtherPortal.transform.forward;
		if (Vector3.Dot(portalCamera.transform.position - OtherPortal.transform.position, -OtherPortal.transform.forward) < 0)
			PlaneNormal = -PlaneNormal;
		Vector3 position = PlanePosition;
		Vector3 normal = PlaneNormal;
		position -= normal * 0.05f;
		foreach (Material mat in cloneMaterials){	
			mat.SetClipPlane(position, normal);
		}
		PlanePosition = OtherPortal.transform.position;
		PlaneNormal = OtherPortal.transform.forward;
		for (int i=0; i<inClones.Count; ++i){
			GameObject inClone = inClones[i];
			GameObject inObj = inClonedObjects[i];
			inClone.CenterOnObject(inObj);
			inClone.PortalTransform(this.gameObject, OtherPortal);

			normal = (PlaneNormal * inCloneDirections[i]).normalized;
			position = PlanePosition;
			position -= normal * 0.05f * ((Vector3.Dot(PlayerCamera.transform.position - position, normal) < 0) ? -1 : 1);
			foreach (MeshRenderer MR in inCloneMRs[i]){
				MR.material.SetClipPlane(position, normal);
			}

			PlanePosition = transform.position;
			normal = (transform.forward * inCloneDirections[i]).normalized;
			position = PlanePosition;
			position -= normal * 0.05f * ((Vector3.Dot(PlayerCamera.transform.position - position, normal) < 0) ? -1 : 1);
			foreach	(MeshRenderer MR in inObjectMRs[i]){
				MR.material.SetClipPlane(position, normal);
			}
			
			bool is_enabled = Vector3.Dot(PlayerCamera.transform.position - transform.position, -transform.forward) * inCloneDirections[i] < 0;
			List<GameObject> allChildrenClones = inClones[i].GetChildrenClones();
			foreach(GameObject clone in allChildrenClones){
				clone.SetActive(is_enabled);
			}
			is_enabled = Vector3.Dot(PlayerCamera.transform.position - OtherPortal.transform.position, -OtherPortal.transform.forward) * inCloneDirections[i] < 0;
			allChildrenClones = inClonedObjects[i].GetChildrenClones();
			foreach(GameObject clone in allChildrenClones){
				clone.SetActive(is_enabled);
			}
			
		}
	}

	void FixedUpdate()
	{
		for(int i=0; i<inClonedObjects.Count; ++i){
			GameObject obj = inClonedObjects[i];
			GameObject clone = inClones[i];

			float cosTheta = Vector3.Dot(obj.transform.position-transform.position, transform.forward);
			Vector3 closestPoint = obj.transform.position - transform.forward*cosTheta;
			Vector3 localClosestPoint = obj.transform.InverseTransformPoint(closestPoint);
			Debug.DrawLine(obj.transform.position, closestPoint, Color.white);
			if (localClosestPoint.magnitude < 0.5f){

				Debug.Log(0.5f-localClosestPoint.magnitude);
			}

			if (Vector3.Dot(obj.transform.position - transform.position, transform.forward) * inCloneDirections[i] < 0){
				obj.PortalTransform(this.gameObject, OtherPortal);
				Rigidbody objRB = obj.GetComponent<Rigidbody>();
				if (objRB != null){
					Debug.DrawRay(transform.position, objRB.velocity, Color.green, 2);
					objRB.velocity = OtherPortal.transform.TransformVector(Quaternion.AngleAxis(180, Vector3.up) * transform.InverseTransformVector(objRB.velocity));
					objRB.angularVelocity = OtherPortal.transform.TransformVector(Quaternion.AngleAxis(180, Vector3.up) * transform.InverseTransformVector(objRB.angularVelocity));
					Debug.DrawRay(OtherPortal.transform.position, objRB.velocity, Color.green, 2);
				}
			}
		}
	}

	void OnTriggerEnter(Collider coll) 
	{
		GameObject obj = coll.gameObject;
		if (obj.tag != "Teleport Clone"){
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
		}
	}

	void OnTriggerExit(Collider coll)
	{
		GameObject obj = coll.gameObject;
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
	}

	static void SetChildrenTag(GameObject obj, string tag)
	{
		obj.tag = tag;
		foreach	(Transform child in obj.transform){
			SetChildrenTag(child.gameObject, tag);
		}
	}

	static bool areAnyChildrenInList(GameObject obj, List<GameObject> list)
	{
		if (list.Contains(obj)){
			return true;
		}else{
			bool result = false;
			foreach (Transform child in obj.transform){
				result = result || areAnyChildrenInList(child.gameObject, list);
			}
			return result;
		}
	}

	static Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
	{
		Vector3 offsetPos = pos + normal * 0.07f;
		Matrix4x4 m = cam.worldToCameraMatrix;
		Vector3 cpos = m.MultiplyPoint(offsetPos);
		Vector3 point = m.inverse.MultiplyPoint(new Vector3(0.0f, 0.0f, 0.0f));
		cpos -= new Vector3(0.0f, point.y, 0.0f);
		Vector3 cnormal = m.MultiplyVector( normal ).normalized * Mathf.Sign(sideSign);
		return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos,cnormal));
	}    
         
	static void CalculateObliqueMatrix(ref Matrix4x4 projection, Vector4 clipPlane)
	{
		Vector4 q = projection.inverse * new Vector4(
			Mathf.Sign(clipPlane.x),
			Mathf.Sign(clipPlane.y),
			1.0f,
			1.0f
		);
		Vector4 c = clipPlane * (2.0F / (Vector4.Dot (clipPlane, q)));
		// third row = clip plane - fourth row
		projection[2] = c.x - projection[3];
		projection[6] = c.y - projection[7];
		projection[10] = c.z - projection[11];
		projection[14] = c.w - projection[15];
	}
}

public static class ExtensionMethodsOLD
{
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

	public static void CenterOnObject(this GameObject obj, GameObject target)
	{
		obj.transform.parent = target.transform;
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localRotation = Quaternion.identity;
		obj.transform.localScale = Vector3.one;
		obj.transform.parent = null;
	}

	public static void PortalTransform(this GameObject obj, GameObject thisPortal, GameObject otherPortal)
	{
		obj.transform.RotateAround(thisPortal.transform.position, thisPortal.transform.up, 180);
		Vector3 scale = obj.transform.localScale;
		obj.transform.parent = thisPortal.transform;

		Vector3 pos = obj.transform.localPosition;
		Quaternion rot = obj.transform.localRotation;
		

		obj.transform.parent = otherPortal.transform;

		obj.transform.localPosition = pos;
		obj.transform.localRotation = rot;
		//obj.transform.localScale = scale;

		obj.transform.parent = null;
		obj.transform.localScale = scale * otherPortal.transform.localScale.magnitude / thisPortal.transform.localScale.magnitude;
	}

	public static GameObject CreateShadowCaster(this GameObject obj, int cloneLayer, string name)
	{
		GameObject clone = new GameObject();
		clone.name = obj.name + " - " + name;
		clone.layer = cloneLayer;
		clone.tag = "Clone";
		MeshFilter objMF = obj.GetComponent<MeshFilter>();
		if (objMF != null){
			MeshFilter cloneMF = clone.AddComponent<MeshFilter>();
			cloneMF.mesh = objMF.mesh;
		}
		MeshRenderer objMR = obj.GetComponent<MeshRenderer>();
		if (objMR != null){
			MeshRenderer cloneMR = clone.AddComponent<MeshRenderer>();
			cloneMR.material = objMR.material;
			cloneMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
		}
		clone.CenterOnObject(obj);
		clone.transform.parent = obj.transform;
		return clone;
	}

	public static GameObject CreateChildrenClones(this GameObject obj, string name)
	{
		UnityEngine.Rendering.ShadowCastingMode mode = UnityEngine.Rendering.ShadowCastingMode.On;
		MeshRenderer objMR = obj.GetComponent<MeshRenderer>();
		if (objMR != null)
			mode = objMR.shadowCastingMode;
		GameObject clone = obj.CreateClone(obj.layer, name, mode, true);
		clone.transform.parent = null;
		foreach (Transform child in obj.transform){
			if (child.tag != "Clone"){
				GameObject childClone = child.gameObject.CreateChildrenClones(name);
				childClone.transform.parent = clone.transform;
			}
		}
		return clone;
	}

	public static GameObject CreateClone(this GameObject obj, int cloneLayer, string name, UnityEngine.Rendering.ShadowCastingMode mode, bool addCollider = false)
	{
		GameObject clone = new GameObject();
		clone.name = obj.name + name;
		clone.layer = cloneLayer;
		MeshFilter objMF = obj.GetComponent<MeshFilter>();
		if (objMF != null){
			MeshFilter cloneMF = clone.AddComponent<MeshFilter>();
			cloneMF.mesh = objMF.mesh;
		}
		MeshRenderer objMR = obj.GetComponent<MeshRenderer>();
		if (objMR != null){
			MeshRenderer cloneMR = clone.AddComponent<MeshRenderer>();
			cloneMR.material = new Material(Shader.Find("Custom/ClippableStandard"));
			cloneMR.material.CopyPropertiesFromMaterial(objMR.material);
			cloneMR.shadowCastingMode = mode;
		}
		if (addCollider){
			Collider col = obj.GetComponent<Collider>();
			if (col != null){
				CopyComponent(col, clone);
			}
		}
		clone.CenterOnObject(obj);
		clone.transform.parent = obj.transform;
		return clone;
	}

	public static List<MeshRenderer> GetChildrenMRs(this GameObject obj)
	{
		List<MeshRenderer> MRs = new List<MeshRenderer>();
		MeshRenderer objMR = obj.GetComponent<MeshRenderer>();
		if (objMR != null)
			MRs.Add(objMR);
		foreach (Transform child in obj.transform){
			if (child.tag != "Clone"){
				MRs = MRs.Concat(GetChildrenMRs(child.gameObject)).ToList();
			}
		}
		return MRs;
	}

	public static List<GameObject> GetChildrenClones(this GameObject obj)
	{
		List<GameObject> clones = new List<GameObject>();
		if (obj.tag == "Clone")
			clones.Add(obj);
		foreach (Transform child in obj.transform){
			clones = clones.Concat(GetChildrenClones(child.gameObject)).ToList();
		}
		return clones;
	}

	public static void SetClipPlane(this Material mat, Vector3 pos, Vector3 normal)
	{
		mat.SetVector("_PlanePosition", new Vector4(pos.x, pos.y, pos.z, 0));
		mat.SetVector("_PlaneNormal", new Vector4(normal.x, normal.y, normal.z, 0));
	}
}
