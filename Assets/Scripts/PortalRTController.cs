using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class PortalRTController : MonoBehaviour 
{
	public GameObject OtherPortal;
	public GameObject MainCamera;
	public int CloneLayer;

	private GameObject portalCamera;
	private Dictionary<GameObject, GameObject> clones = new Dictionary<GameObject, GameObject>();
	private Dictionary<GameObject, ClonedObject> inClonedObjects = new Dictionary<GameObject, ClonedObject>();
	private List<ClonedObject> inClones = new List<ClonedObject>();
	private List<MeshRenderer> cloneRenderers = new List<MeshRenderer>();

	void CreatePortalCamera() {
		portalCamera = MainCamera.Clone();
		portalCamera.name = name + " Camera";
		Camera cam = portalCamera.AddComponent<Camera>();
		cam.CopyFrom(MainCamera.GetComponent<Camera>());
		RenderTexture rt = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 16);
		//rt.antiAliasing = QualitySettings.antiAliasing;
		rt.filterMode = FilterMode.Point;
		GetComponent<MeshRenderer>().material.SetTexture("_MainTex", rt);
		cam.targetTexture = rt;
		cam.cullingMask = 1 << (int)CloneLayer;
	}

	void UpdatePortalCamera() {
		portalCamera.CenterOn(MainCamera);
		portalCamera.TransformThroughPortal(gameObject, OtherPortal);
	}

	void CloneNewObjects() {
		List<GameObject> rootObjects = new List<GameObject>();
  		Scene scene = SceneManager.GetActiveScene();
  		scene.GetRootGameObjects( rootObjects );
  		foreach (GameObject obj in rootObjects){
			Queue<GameObject> Q = new Queue<GameObject>();
  			Q.Enqueue(obj);
  			while(Q.Count > 0){
  				GameObject objToClone = Q.Dequeue();
  				if (!clones.ContainsKey(objToClone) && objToClone.tag != "Portal" && objToClone.tag != "Clone" && objToClone.tag != "Inside Portal" && objToClone.tag != "Clip Collider"){
	  				GameObject clone = objToClone.Clone("Clone ("+name+")", CloneLayer, new System.Type[]{typeof(MeshFilter), typeof(MeshRenderer)}, true);
	  				MeshRenderer MR = clone.GetComponent<MeshRenderer>();
	  				if (MR)
	  					cloneRenderers.Add(MR);
  					clone.transform.parent = objToClone.transform;
  					clones.Add(objToClone, clone);
  				}
  				foreach (Transform child in objToClone.transform){
					if (child.tag != "Clone"){
						Q.Enqueue(child.gameObject);
					}
				}
  			}
  		}
	}

	void Start () {
		if (!MainCamera)
			MainCamera = Camera.main.gameObject;
		CreatePortalCamera();
	}

	void LateUpdate () {
		UpdatePortalCamera();
	}

	void FixedUpdate() {
		CloneNewObjects();
		cloneRenderers.SetClipPlanes(gameObject, OtherPortal, Vector3.Dot(portalCamera.transform.position - OtherPortal.transform.position, -OtherPortal.transform.forward) < 0);
		foreach(ClonedObject clonePair in inClones){
			clonePair.Update(gameObject, OtherPortal, MainCamera, portalCamera);
		}
	}

	void OnTriggerEnter(Collider coll) {
		if (!coll.isTrigger){
			GameObject obj = coll.gameObject;
			if (obj.tag != "Clip Collider"){
				while(obj.transform.parent != null)
					obj = obj.transform.parent.gameObject;
				if (obj.tag != "Inside Portal" && !obj.isStatic){
					if (!inClonedObjects.ContainsKey(obj)){
						Debug.Log(obj.name + " added to " + name);
						int direction = (int)Mathf.Sign(Vector3.Dot(obj.transform.position - transform.position, transform.forward));
						List<GameObject> otherPortalClones = new List<GameObject>();
						Queue<Transform> children = new Queue<Transform>();
						children.Enqueue(obj.transform);
						while (children.Count > 0){
							Transform child = children.Dequeue();
							if (child.tag == "Clone" && !clones.ContainsValue(child.gameObject)){
								otherPortalClones.Add(child.gameObject);
							}
							foreach (Transform nextChild in child){
								children.Enqueue(nextChild);
							}
						}
						ClonedObject clonedPair = new ClonedObject(obj, direction, gameObject, OtherPortal, CloneLayer, otherPortalClones);
						inClonedObjects.Add(obj, clonedPair);
						inClones.Add(clonedPair);
					}else{
						inClonedObjects[obj].AddInPortal();
					}
				}
			}
		}
	}

	void OnTriggerExit(Collider coll) {
		GameObject obj = coll.gameObject;
		if (obj.tag != "Clip Collider"){
			while(obj.transform.parent != null)
				obj = obj.transform.parent.gameObject;
			if (inClonedObjects.ContainsKey(obj)){
				if (inClonedObjects[obj].RemoveInPortal()){
					Debug.Log(obj.name + " removed from " + name);
					inClones.Remove(inClonedObjects[obj]);
					inClonedObjects.Remove(obj);

					Queue<Transform> children = new Queue<Transform>();
					children.Enqueue(obj.transform);
					while (children.Count > 0){
						Transform child = children.Dequeue();
						if (child.tag == "Clone" && !clones.ContainsValue(child.gameObject)){
							child.gameObject.SetActive(true);
						}
						foreach (Transform nextChild in child){
							children.Enqueue(nextChild);
						}
					}
				}
			}
		}
	}
}

public class ClonedObject 
{
	private GameObject _original;
	private GameObject _clone;
	private int _objectsInPortalCount;
	private int _direction;
	private List<GameObject> _otherPortalClones;
	private Dictionary<GameObject, GameObject> clonedChildren;
	private Dictionary<GameObject, GameObject> cloneCameraClones;
	private Dictionary<MeshRenderer, Material[]> oldMaterials;
	private List<MeshRenderer> objMRs;
	private List<MeshRenderer> cloneMRs;
	private List<MeshRenderer> cloneCamCloneMRs;
	private List<PlaneClipCollider> objClipCols;
	private List<PlaneClipCollider> cloneClipCols;
	private List<Collider[]> objCols;
	private List<Collider[]> cloneCols;
	private Rigidbody objRB;
	private Rigidbody cloneRB;
	private bool interpolate = true;
	public ClonedObject(GameObject original, int direction, GameObject inPortal, GameObject outPortal, int CloneLayer, List<GameObject> otherPortalClones){
		_original = original;
		_direction = direction;
		_objectsInPortalCount = 1;
		_clone = _original.Clone("in-clone", _original.layer, new System.Type[]{typeof(MeshFilter), typeof(MeshRenderer), typeof(Collider)}, true);
		_clone.tag = "Inside Portal";
		_clone.transform.parent = null;
		_otherPortalClones = otherPortalClones;
		clonedChildren = new Dictionary<GameObject, GameObject>();
		cloneCameraClones = new Dictionary<GameObject, GameObject>();
		cloneCamCloneMRs = new List<MeshRenderer>();

		GameObject cloneCameraClone = _clone.Clone("in-camera-clone", CloneLayer, new System.Type[]{typeof(MeshFilter), typeof(MeshRenderer)}, true);
		cloneCameraClones.Add(_clone, cloneCameraClone);
		cloneCamCloneMRs.Add(cloneCameraClone.GetComponent<MeshRenderer>());

		oldMaterials = new Dictionary<MeshRenderer, Material[]>();
		objMRs = new List<MeshRenderer>();
		cloneMRs = new List<MeshRenderer>();
		cloneCols = new List<Collider[]>();
		objClipCols = new List<PlaneClipCollider>();
		cloneClipCols = new List<PlaneClipCollider>();
		MeshRenderer objMR = _original.GetComponent<MeshRenderer>();
		if (objMR != null){
			oldMaterials.Add(objMR, objMR.materials);
			objMR.SetToClipMaterial();
			objMRs.Add(objMR);
		}
		MeshRenderer cloneMR = _clone.GetComponent<MeshRenderer>();
		if (cloneMR != null){
			cloneMRs.Add(cloneMR);
		}
		Collider[] cols = _clone.GetComponents<Collider>();
		cloneCols.Add(cols);
		objRB = _original.GetComponent<Rigidbody>();
		if (_original.tag != "Player"){
			if (objRB != null){
				objRB.mass /= 2;
				cloneRB = _clone.AddComponent<Rigidbody>();
				cloneRB.mass = objRB.mass;
				cloneRB.velocity = objRB.velocity;
				cloneRB.angularVelocity = objRB.angularVelocity;
				cloneRB.useGravity = false;
				objRB.useGravity = false;
				cloneRB.isKinematic = objRB.isKinematic;
				cloneRB.constraints = objRB.constraints;
				cloneRB.TransformRigidBodyThroughPortal(inPortal, outPortal);
				cloneRB.ResetCenterOfMass();
				cloneRB.ResetInertiaTensor();
			}
		}else{
			interpolate = false;
		}

		PlaneClipCollider objClipCol = _original.GetComponent<PlaneClipCollider>();
		if (objClipCol == null)
			objClipCol = _original.AddComponent<PlaneClipCollider>();
		objClipCol.enabled = true;
		objClipCols.Add(objClipCol);
		PlaneClipCollider cloneClipCol = _clone.AddComponent<PlaneClipCollider>();
		cloneClipCol.enabled = true;
		cloneClipCols.Add(cloneClipCol);

		CreateCloneHirearchy(_original, _clone, CloneLayer);
		_clone.TransformThroughPortal(inPortal, outPortal);

		objMRs.SetClipPlanes(outPortal, inPortal, _direction == -1);
		cloneMRs.SetClipPlanes(inPortal, outPortal, _direction == -1);
		cloneCamCloneMRs.SetClipPlanes(inPortal, outPortal, false);

		foreach(PlaneClipCollider clipCol in objClipCols){
			clipCol.PlanePosition = inPortal.transform.position;
			clipCol.PlaneNormal = inPortal.transform.forward * Mathf.Sign(_direction);
		}
		foreach(PlaneClipCollider clipCol in cloneClipCols){
			clipCol.PlanePosition = outPortal.transform.position;
			clipCol.PlaneNormal = outPortal.transform.forward * Mathf.Sign(_direction);
		}
	}

	public void Update(GameObject inPortal, GameObject outPortal, GameObject mainCamera, GameObject portalCamera){
		foreach(PlaneClipCollider clipCol in cloneClipCols){
			clipCol.enabled = false;
		}
		foreach(PlaneClipCollider clipCol in objClipCols){
			clipCol.enabled = false;
		}
		foreach(Collider[] cols in cloneCols){
			foreach(Collider col in cols){
				col.enabled = false;
			}
		}
		
		if (cloneRB != null){
			cloneRB.AddForce(Physics.gravity, ForceMode.Acceleration);
			objRB.AddForce(Physics.gravity, ForceMode.Acceleration);
			cloneRB.TransformRigidBodyThroughPortal(outPortal, inPortal);
			if (interpolate){
				objRB.velocity = Vector3.Lerp(objRB.velocity, cloneRB.velocity, 0.5f);
				objRB.angularVelocity = Vector3.Lerp(objRB.angularVelocity, cloneRB.angularVelocity, 0.5f);
			}
			cloneRB.velocity = objRB.velocity;
			cloneRB.angularVelocity = objRB.angularVelocity;
			cloneRB.TransformRigidBodyThroughPortal(inPortal, outPortal);
		}

		_clone.TransformThroughPortal(outPortal, inPortal);
		if (interpolate){
			_original.transform.position = Vector3.Lerp(_original.transform.position, _clone.transform.position, 0.5f);
			_original.transform.rotation = Quaternion.Slerp(_original.transform.rotation, _clone.transform.rotation, 0.5f);
		}
		_clone.CenterOn(_original);
		_clone.TransformThroughPortal(inPortal, outPortal);
		
		objMRs.SetClipPlanes(outPortal, inPortal, _direction == -1);
		cloneMRs.SetClipPlanes(inPortal, outPortal, _direction == -1);
		cloneCamCloneMRs.SetClipPlanes(inPortal, outPortal, _direction == -1);

		if (Vector3.Dot(mainCamera.transform.position - inPortal.transform.position, inPortal.transform.forward) * _direction < 0){
			foreach (MeshRenderer MR in cloneCamCloneMRs){
				MR.enabled = false;
			}
		}else{
			foreach (MeshRenderer MR in cloneCamCloneMRs){
				MR.enabled = true;
			}
		}
		if (Vector3.Dot(mainCamera.transform.position - outPortal.transform.position, outPortal.transform.forward) * _direction < 0){
			foreach(GameObject otherPortalClone in _otherPortalClones){
				otherPortalClone.SetActive(false);
			}
		}else{
			foreach(GameObject otherPortalClone in _otherPortalClones){
				otherPortalClone.SetActive(true);
			}
		}

		int direction = (int)Mathf.Sign(Vector3.Dot(_original.transform.position - inPortal.transform.position, inPortal.transform.forward));
		if (direction != _direction){
			_original.TransformThroughPortal(inPortal, outPortal);
			_clone.TransformThroughPortal(outPortal, inPortal);
			objMRs.SetClipPlanes(inPortal, outPortal, false);
			cloneMRs.SetClipPlanes(outPortal, inPortal, false);
			if (objRB != null){
				objRB.TransformRigidBodyThroughPortal(inPortal, outPortal);
				cloneRB.TransformRigidBodyThroughPortal(outPortal, inPortal);
			}
			foreach(PlaneClipCollider clipCol in objClipCols){
				clipCol.PlanePosition = outPortal.transform.position + (outPortal.transform.forward * Mathf.Sign(_direction)).normalized * 0.1f;
				clipCol.PlaneNormal = outPortal.transform.forward * Mathf.Sign(_direction);
			}
			foreach(PlaneClipCollider clipCol in cloneClipCols){
				clipCol.PlanePosition = inPortal.transform.position + (inPortal.transform.forward * Mathf.Sign(_direction)).normalized * 0.1f;
				clipCol.PlaneNormal = inPortal.transform.forward * Mathf.Sign(_direction);
			}
			foreach (PlaneClipCollider clipCol in objClipCols){
				clipCol.enabled = false;
			}
		}else{
			foreach(PlaneClipCollider clipCol in objClipCols){
				clipCol.PlanePosition = inPortal.transform.position + (inPortal.transform.forward * Mathf.Sign(_direction)).normalized * 0.1f;
				clipCol.PlaneNormal = inPortal.transform.forward * Mathf.Sign(_direction);
			}
			foreach(PlaneClipCollider clipCol in cloneClipCols){
				clipCol.PlanePosition = outPortal.transform.position + (outPortal.transform.forward * Mathf.Sign(_direction)).normalized * 0.1f;
				clipCol.PlaneNormal = outPortal.transform.forward * Mathf.Sign(_direction);
			}
		}
		foreach(Collider[] cols in cloneCols){
			foreach(Collider col in cols){
				col.enabled = true;
			}
		}
		foreach(PlaneClipCollider clipCol in cloneClipCols){
			clipCol.enabled = true;
		}
		foreach(PlaneClipCollider clipCol in objClipCols){
			clipCol.enabled = true;
		}
	}

	public void AddInPortal(){
		_objectsInPortalCount++;
	}

	public bool RemoveInPortal(){
		_objectsInPortalCount--;
		if (_objectsInPortalCount == 0){
			if (objRB != null)
				objRB.mass *= 2;
			UnityEngine.Object.Destroy(_clone);
			foreach (MeshRenderer MR in objMRs){
				MR.materials = oldMaterials[MR];
			}
			foreach (PlaneClipCollider clipCol in objClipCols){
				clipCol.enabled = false;
			}
			return true;
		}
		return false;
	}

	private void CreateCloneHirearchy(GameObject original, GameObject clone, int CloneLayer){
		foreach (Transform child in original.transform){
			if (child.tag != "Clone" && child.tag != "Clip Collider"){
				GameObject childClone = child.gameObject.Clone("in-clone", child.gameObject.layer, new System.Type[]{typeof(MeshFilter), typeof(MeshRenderer), typeof(Collider), typeof(CollisionTest)}, true);
				childClone.tag = "Inside Portal";
				childClone.transform.parent = clone.transform;
				childClone.transform.localPosition = child.localPosition;
				childClone.transform.localRotation = child.localRotation;
				childClone.transform.localScale = child.localScale;
				MeshRenderer objMR = child.gameObject.GetComponent<MeshRenderer>();
				if (objMR != null){
					oldMaterials.Add(objMR, objMR.materials);
					objMR.SetToClipMaterial();
					objMRs.Add(objMR);
				}
				MeshRenderer cloneMR = childClone.GetComponent<MeshRenderer>();
				if (cloneMR != null){
					cloneMRs.Add(cloneMR);
				}
				Collider[] cols = childClone.GetComponents<Collider>();
				cloneCols.Add(cols);

				GameObject cloneCameraClone = childClone.Clone("in-camera-clone", CloneLayer, new System.Type[]{typeof(MeshFilter), typeof(MeshRenderer)}, true);
				cloneCameraClones.Add(childClone, cloneCameraClone);

				PlaneClipCollider objClipCol = child.gameObject.GetComponent<PlaneClipCollider>();
				if (objClipCol == null)
					objClipCol = child.gameObject.AddComponent<PlaneClipCollider>();
				objClipCol.enabled = true;
				objClipCols.Add(objClipCol);
				PlaneClipCollider cloneClipCol = childClone.AddComponent<PlaneClipCollider>();
				cloneClipCol.enabled = true;
				cloneClipCols.Add(cloneClipCol);

				CreateCloneHirearchy(child.gameObject, childClone, CloneLayer);
				clonedChildren.Add(child.gameObject, childClone);
			}
		}
	}
}

public static class ExtensionMethods
{
	public static GameObject Clone(this GameObject obj, string name = "", int layer = 0, System.Type[] typesToCopy = null, bool setToClipMaterial = false){
		GameObject clone = new GameObject();
		clone.name = obj.name + " " + name;
		clone.tag = "Clone";
		clone.layer = layer;
		clone.CenterOn(obj);
		clone.transform.parent = obj.transform;
		
		if (typesToCopy != null){
			foreach (System.Type type in typesToCopy){
				Component[] comps = obj.GetComponents(type);
				foreach (Component comp in comps){
					comp.CopyComponent(clone, true);
				}
			}
		}
		if (setToClipMaterial){
			MeshRenderer MR = clone.GetComponent<MeshRenderer>();
			if (MR != null){
				MR.SetToClipMaterial();
			}
		}
		return clone;
	}

	public static void CenterOn(this GameObject obj, GameObject target){
		obj.transform.parent = target.transform;
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localRotation = Quaternion.identity;
		obj.transform.localScale = Vector3.one;
		obj.transform.parent = null;
	}

	public static void TransformThroughPortal(this GameObject obj, GameObject inPortal, GameObject outPortal){
		obj.transform.RotateAround(inPortal.transform.position, inPortal.transform.up, 180);
		Vector3 scale = obj.transform.localScale;
		obj.transform.parent = inPortal.transform;

		Vector3 pos = obj.transform.localPosition;
		Quaternion rot = obj.transform.localRotation;

		obj.transform.parent = outPortal.transform;

		obj.transform.localPosition = pos;
		obj.transform.localRotation = rot;

		obj.transform.parent = null;
		obj.transform.localScale = scale * outPortal.transform.localScale.magnitude / inPortal.transform.localScale.magnitude;
	}

	public static void TransformRigidBodyThroughPortal(this Rigidbody RB, GameObject inPortal, GameObject outPortal){
		if (RB != null){
			RB.velocity = outPortal.transform.TransformVector(Quaternion.AngleAxis(180, Vector3.up) * inPortal.transform.InverseTransformVector(RB.velocity));
			RB.angularVelocity = outPortal.transform.TransformDirection(Quaternion.AngleAxis(180, Vector3.up) * inPortal.transform.InverseTransformDirection(RB.angularVelocity));
		}
	}

	public static T CopyComponent<T>(this T original, GameObject destination, bool createNewInstance=false) where T : Component{
		System.Type type = original.GetType();
		var dst = destination.GetComponent(type) as T;
		if (!dst || createNewInstance) dst = destination.AddComponent(type) as T;
		var fields = type.GetFields();
		foreach (var field in fields){
			if (field.IsStatic) 
				continue;
			field.SetValue(dst, field.GetValue(original));
		}
		var props = type.GetProperties();
		foreach (var prop in props){
			if (!prop.CanWrite || !prop.CanWrite || prop.Name == "name" || prop.Name == "tag") 
				continue;
			prop.SetValue(dst, prop.GetValue(original, null), null);
		}
		return dst as T;
	}

	public static void SetToClipMaterial(this MeshRenderer MR){
		Material[] mats = MR.materials;
		for (int i=0; i<mats.Length; ++i){
			Material newMat = new Material(Shader.Find("Custom/ClippableStandard"));
			newMat.CopyPropertiesFromMaterial(mats[i]);
			mats[i] = newMat;
		}
		MR.materials = mats;
	}

	public static void SetClipPlanes(this List<MeshRenderer> renderers, GameObject inPortal, GameObject outPortal, bool flipNormal) {
  		Vector3 planePosition = outPortal.transform.position;
  		Vector3 planeNormal = outPortal.transform.forward;
  		if (flipNormal)
  			planeNormal = -planeNormal;
  		planePosition -= planeNormal * 0.01f;
  		foreach(MeshRenderer MR in renderers){
  			foreach(Material mat in MR.materials){
  				mat.SetVector("_PlanePosition", (Vector4)planePosition);
  				mat.SetVector("_PlaneNormal", (Vector4)planeNormal);
  			}
  		}
	}
}