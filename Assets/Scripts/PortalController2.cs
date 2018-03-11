using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PortalController2 : MonoBehaviour {
	public GameObject OtherPortal;
	public GameObject playerCamera;
	public int inCount;
	public int BorderLayer;
	public int BlockerLayer;
	public int CloneLayer;
	public int StencilLayer;
	public int PortalCloneLayer;
	public int PortalCloneOriginalLayer;

	private GameObject portalCamera;
	private GameObject portalStencil;
	private GameObject portalBlocker;

	private PortalController2 otherPortalController;
	
	public List<GameObject> inObjects = new List<GameObject>();
	public List<GameObject> inClones = new List<GameObject>();
	public List<GameObject> inClones2 = new List<GameObject>();
	public List<int> inLayers = new List<int>();
	public List<float> inDirections = new List<float>();
	public List<bool> inCrossed = new List<bool>();
	
	// Use this for initialization
	void Start () {
		otherPortalController = OtherPortal.GetComponent<PortalController2>();

		playerCamera.GetComponent<Camera>().cullingMask |= 1 << PortalCloneOriginalLayer;
		playerCamera.GetComponent<Camera>().cullingMask |= 1 << PortalCloneLayer;
		portalCamera = GameObject.Instantiate(playerCamera);
		Component[] comps = portalCamera.GetComponents(typeof(Component));
		foreach(Component comp in comps)
		{
			if (!(comp is Camera) && !(comp is Transform))
				Destroy(comp);
		}
		portalCamera.GetComponent<Camera>().depth--;
		portalCamera.GetComponent<Camera>().cullingMask = 1 << CloneLayer;
		portalCamera.GetComponent<Camera>().cullingMask |= 1 << otherPortalController.BlockerLayer;
		portalCamera.GetComponent<Camera>().cullingMask |= 1 << otherPortalController.StencilLayer;
		portalCamera.name = name + " Camera";

		portalStencil = new GameObject();
		portalStencil.AddComponent<MeshFilter>();
		portalStencil.GetComponent<MeshFilter>().mesh = GetComponent<MeshFilter>().mesh;
		portalStencil.AddComponent<MeshRenderer>();
		portalStencil.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Custom/StencilMask"));
		portalStencil.name = name + " Stencil";
		portalStencil.transform.parent = transform;
		portalStencil.transform.localPosition = Vector3.zero;
		portalStencil.transform.localRotation = Quaternion.identity;
		portalStencil.transform.localScale = Vector3.one;
		portalStencil.layer = StencilLayer;
		
		portalBlocker = new GameObject();
		MeshFilter MF = portalBlocker.AddComponent<MeshFilter>();
		MF.mesh = OtherPortal.GetComponent<MeshFilter>().mesh;
		MeshRenderer MR = portalBlocker.AddComponent<MeshRenderer>();
		MR.material = new Material(Shader.Find("Custom/DepthStencilShow"));
		portalBlocker.transform.position = OtherPortal.transform.position;
		portalBlocker.transform.rotation = OtherPortal.transform.rotation;
		portalBlocker.transform.localScale = OtherPortal.transform.localScale;
		portalBlocker.name = name + " Blocker";
		portalBlocker.layer = otherPortalController.BlockerLayer;

		
	}

	void Update() {
		List<GameObject> objects = UnityEngine.Object.FindObjectsOfType<GameObject>().ToList();

		foreach (GameObject obj in objects)
		{
			if (obj.activeInHierarchy && 
				obj.layer != LayerMask.NameToLayer("TransparentFX") &&
				obj.layer != BlockerLayer && 
				obj.layer != otherPortalController.BlockerLayer && 
				obj.layer != CloneLayer && 
				obj.layer != otherPortalController.CloneLayer && 
				obj.layer != StencilLayer && 
				obj.layer != otherPortalController.StencilLayer && 
				//obj.layer != gameObject.layer + 15 && 
				obj.layer != otherPortalController.PortalCloneLayer && 
				//obj.layer != gameObject.layer + 18 && 
				obj.layer != otherPortalController.PortalCloneOriginalLayer && 
				obj.layer != gameObject.layer && 
				obj.layer != OtherPortal.layer
				)
			{
				bool hasClone = false;
				GameObject clone = null;
				MeshRenderer objMR = obj.GetComponent<MeshRenderer>();
				//Transform parent = obj.transform;

				foreach(Transform child in obj.transform)
				{
					if (child.gameObject.layer == CloneLayer)
					{
						hasClone = true;
						clone = child.gameObject;
					}
				}
				if (hasClone == false)
				{
					clone = new GameObject();
					clone.name = obj.name + " (" + name + " Camera Clone)";
					//Debug.Log("Creating " + clone.name, gameObject);
					clone.layer = CloneLayer;
					clone.transform.parent = obj.transform;
					clone.transform.localPosition = Vector3.zero;
					clone.transform.localRotation = Quaternion.identity;
					clone.transform.localScale = Vector3.one;
					if (objMR != null)
					{
						MeshFilter MF = clone.AddComponent<MeshFilter>();
						MF.mesh = obj.GetComponent<MeshFilter>().mesh;
						MeshRenderer MR = clone.AddComponent<MeshRenderer>();

						if (objMR.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.Off)
						{
							MR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
							objMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
						}else{
							MR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
						}
						Material oldMaterial = obj.GetComponent<MeshRenderer>().material;
						MR.material = new Material(Shader.Find("Custom/ClippableStandardStencilShow"));
						MR.material.CopyPropertiesFromMaterial(oldMaterial);
					}
					Light objLight = obj.GetComponent<Light>();
					if (objLight != null && objLight.enabled)
					{
						System.Type type = objLight.GetType();
						Component comp = clone.AddComponent(type);
						Component other = obj.GetComponent(type);

						System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.DeclaredOnly;
						System.Reflection.PropertyInfo[] pinfos = type.GetProperties(flags);
						foreach (System.Reflection.PropertyInfo pinfo in pinfos)
						{
							if (pinfo.CanWrite)
							{
								pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
							}
						}
						System.Reflection.FieldInfo[] finfos = type.GetFields(flags);
						foreach (System.Reflection.FieldInfo finfo in finfos)
						{
							finfo.SetValue(comp, finfo.GetValue(other));
						}
						Light cloneLight = clone.GetComponent<Light>();
						cloneLight.cullingMask = 1 << CloneLayer;
						cloneLight.cullingMask |= 1 << LayerMask.NameToLayer("TransparentFX");
					}
					clone.transform.parent = obj.transform;
				}
				if (objMR != null)
				{

					Material mat = clone.GetComponent<Renderer>().material;
					Vector3 PlanePosition = OtherPortal.transform.position;
					Vector3 PlaneNormal = OtherPortal.transform.forward;
					if (Vector3.Dot(playerCamera.transform.position-transform.position,transform.forward)<0)
					{
						PlaneNormal = -PlaneNormal;
					}
					PlanePosition -= PlaneNormal * 0.05f;
					mat.SetVector("_PlanePosition", new Vector4(PlanePosition.x, PlanePosition.y, PlanePosition.z, 0));
					mat.SetVector("_PlaneNormal", new Vector4(PlaneNormal.x, PlaneNormal.y, PlaneNormal.z, 0));
				}
				
				/*
				if (obj.layer == PortalCloneLayer)
				{
					int index = inClones.IndexOf(obj);
					if (index != -1)
					{
						if (Vector3.Dot(playerCamera.transform.position-transform.position,transform.forward) * inDirections[index] < 0)
						{
							clone.SetActive(false);
						}else{
							clone.SetActive(true);
						}
					}
				}
				*/
			}
		}
	}

	void LateUpdate () {
		inCount = inObjects.Count;
		portalCamera.transform.parent = null;
		portalCamera.transform.position = playerCamera.transform.position;
		portalCamera.transform.rotation = playerCamera.transform.rotation;

		portalCamera.transform.parent = transform;
		portalCamera.transform.RotateAround(transform.position, transform.up, 180);

		Vector3 pos = portalCamera.transform.localPosition;
		Quaternion rot = portalCamera.transform.localRotation;

		portalCamera.transform.parent = OtherPortal.transform;
		portalCamera.transform.localPosition = pos;
		portalCamera.transform.localRotation = rot;
		portalCamera.transform.localScale = Vector3.one;

		playerCamera.transform.localScale = Vector3.one;

		
		portalBlocker.transform.position = OtherPortal.transform.position;
		portalBlocker.transform.rotation = OtherPortal.transform.rotation;
		portalBlocker.transform.localScale = OtherPortal.transform.localScale;

		portalBlocker.transform.parent = playerCamera.transform;

		pos = portalBlocker.transform.localPosition;
		rot = portalBlocker.transform.localRotation;
		Vector3 scale = portalBlocker.transform.localScale;

		portalCamera.transform.parent = null;
		portalCamera.transform.localScale = Vector3.one;

		portalBlocker.transform.parent = portalCamera.transform;
		portalBlocker.transform.localPosition = pos;
		portalBlocker.transform.localRotation = rot;
		portalBlocker.transform.localScale = scale;

		portalBlocker.transform.parent = null;

		if (Vector3.Dot(OtherPortal.transform.position - transform.position, transform.position - playerCamera.transform.position) < 0)
		{
			portalBlocker.SetActive(true);
			//Debug.Log("1 on " + portalBlocker);
		}else{
			portalBlocker.SetActive(false);
			//Debug.Log("1 off " + portalBlocker);
		}
	}

	void FixedUpdate()
	{
		for (int i=0; i<inObjects.Count; ++i)
		{
			//Debug.Log(name);
			GameObject obj = inObjects[i];
			if (inCrossed[i] == false)
			{
				GameObject clone = inClones[i];

				List<GameObject> objDescendents = getAllDescendents(obj);
				List<GameObject> cameraClones = new List<GameObject>();
				List<GameObject> cloneDescendents = getAllDescendents(clone);

				for (int index = objDescendents.Count-1; index >= 0; index--)
				{
					GameObject descendent = objDescendents[index];
					if (descendent.layer == CloneLayer || descendent.layer == otherPortalController.CloneLayer)
					{
						cameraClones.Add(descendent);
					}
					if (descendent.layer != obj.layer)
					{
						objDescendents.RemoveAt(index);
					}
				}
				for (int index = cloneDescendents.Count-1; index >= 0; index--)
				{
					GameObject descendent = cloneDescendents[index];
					if (descendent.layer == CloneLayer || descendent.layer == otherPortalController.CloneLayer)
					{
						cameraClones.Add(descendent);
					}
					if (descendent.layer != clone.layer)
					{
						cloneDescendents.RemoveAt(index);
					}
				}

				clone.transform.parent = null;
				Vector3 scale = clone.transform.localScale;
				//scale = transform.TransformVector(OtherPortal.transform.InverseTransformVector(scale));
				clone.transform.parent = OtherPortal.transform;
				Vector3 pos = clone.transform.localPosition;
				Quaternion rot = clone.transform.localRotation;
				clone.transform.parent = transform;
				clone.transform.localPosition = pos;
				clone.transform.localRotation = rot;
				clone.transform.RotateAround(transform.position, transform.up, 180);
				clone.transform.parent = null;
				//clone.transform.localScale = scale;
				if (obj.tag != "Player")
				{
					obj.transform.position = Vector3.Lerp(obj.transform.position, clone.transform.position, 0.5f);
					obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation,clone.transform.rotation, 0.5f);
				}
				clone.transform.parent = obj.transform;
				clone.transform.localPosition = Vector3.zero;
				clone.transform.localRotation = Quaternion.identity;
				clone.transform.localScale = Vector3.one;
				
				clone.transform.parent = null;
				scale = clone.transform.localScale;
				clone.transform.parent = transform;
				clone.transform.RotateAround(transform.position, transform.up, 180);
				pos = clone.transform.localPosition;
				rot = clone.transform.localRotation;
				clone.transform.parent = OtherPortal.transform;
				clone.transform.localPosition = pos;
				clone.transform.localRotation = rot;
				clone.transform.parent = null;
				clone.transform.localScale = OtherPortal.transform.TransformVector(transform.InverseTransformVector(scale));
				//clone.transform.localScale = scale;
				//Debug.Log(clone.name + " scale: " + clone.transform.localScale, clone);
				//clone.transform.localScale = scale;
			
				//clone.transform.parent = parent;

				Rigidbody ObjRB = obj.GetComponent<Rigidbody>();
				if (ObjRB != null && obj.tag != "Player")
				{
					Rigidbody CloneRB = clone.GetComponent<Rigidbody>();

					Vector3 cloneVel = CloneRB.velocity;
					Vector3 cloneAng = CloneRB.angularVelocity;
					Vector3 vel = ObjRB.velocity;
					Vector3 ang = ObjRB.angularVelocity;

					Rigidbody portalRB = GetComponent<Rigidbody>();
					Rigidbody otherPortalRB = OtherPortal.GetComponent<Rigidbody>();

					cloneVel -= otherPortalRB.velocity;
					cloneVel = OtherPortal.transform.InverseTransformVector(cloneVel);
					cloneVel = Quaternion.AngleAxis(180, Vector3.up) * cloneVel;
					cloneVel = transform.TransformVector(cloneVel);
					cloneVel += portalRB.velocity;
					cloneAng = Quaternion.AngleAxis(180,transform.up)*transform.TransformDirection(OtherPortal.transform.InverseTransformDirection(cloneAng));

					vel += portalRB.velocity;
					vel = Vector3.Lerp(vel,cloneVel,0.5f);
					ang = Vector3.Lerp(ang,cloneAng,0.5f);
					vel -= portalRB.velocity;
					cloneVel = vel;
					cloneAng = ang;

					ObjRB.velocity = vel;
					ObjRB.angularVelocity = ang;

					cloneVel = transform.InverseTransformVector(cloneVel);
					cloneVel = Quaternion.AngleAxis(180, Vector3.up) * cloneVel;
					cloneVel = OtherPortal.transform.TransformVector(cloneVel);
					cloneVel += otherPortalRB.velocity;

					cloneAng = Quaternion.AngleAxis(180,OtherPortal.transform.up)*OtherPortal.transform.TransformDirection(transform.InverseTransformDirection(cloneAng));

					CloneRB.velocity = cloneVel;
					CloneRB.angularVelocity = cloneAng;


				}

				
				Vector3 PlanePosition = OtherPortal.transform.position;
				Vector3 PlaneNormal = OtherPortal.transform.forward;
				if (inDirections[i]<0)
				{
					PlaneNormal = -PlaneNormal;
				}
				PlanePosition -= PlaneNormal * 0.05f * ((Vector3.Dot(PlaneNormal,playerCamera.transform.position - PlanePosition)) < 0f ?-1:1);
				foreach(GameObject child in cloneDescendents)
				{
					MeshRenderer MR = child.GetComponent<MeshRenderer>();
					if (MR != null)
					{
						Material mat = MR.material;
						mat.SetVector("_PlanePosition", new Vector4(PlanePosition.x, PlanePosition.y, PlanePosition.z, 0));
						mat.SetVector("_PlaneNormal", new Vector4(PlaneNormal.x, PlaneNormal.y, PlaneNormal.z, 0));
					}
				}

				
				PlanePosition = transform.position;
				PlaneNormal = transform.forward;
				if (inDirections[i]<0)
				{
					PlaneNormal = -PlaneNormal;
				}
				PlanePosition -= PlaneNormal * 0.05f * ((Vector3.Dot(PlaneNormal,playerCamera.transform.position - PlanePosition)) < 0f ?-1:1);
				foreach(GameObject child in objDescendents)
				{
					MeshRenderer MR = child.GetComponent<MeshRenderer>();
					if (MR != null)
					{
						Material mat = MR.material;
						mat.SetVector("_PlanePosition", new Vector4(PlanePosition.x, PlanePosition.y, PlanePosition.z, 0));
						mat.SetVector("_PlaneNormal", new Vector4(PlaneNormal.x, PlaneNormal.y, PlaneNormal.z, 0));
					}
				}
				
				Vector3 OtherCameraPosition = playerCamera.transform.position;
				OtherCameraPosition = transform.InverseTransformPoint(OtherCameraPosition);
				OtherCameraPosition = Quaternion.AngleAxis(180, Vector3.up) * OtherCameraPosition;
				OtherCameraPosition = OtherPortal.transform.TransformPoint(OtherCameraPosition);

				foreach (GameObject child in cameraClones)
				{
					if (child.layer == CloneLayer)
					{
						if (Vector3.Dot(playerCamera.transform.position - transform.position, transform.forward) * inDirections[i] < 0)
						{
							child.SetActive(false);
							//Debug.Log("2 off " + child);
						}else{
							child.SetActive(true);
							//Debug.Log("2 on " + child);
						}
					}else{
						if (Vector3.Dot(playerCamera.transform.position - OtherPortal.transform.position, OtherPortal.transform.forward) * inDirections[i] < 0)
						{
							child.SetActive(false);
							//Debug.Log("3 off " + child);
						}else{
							child.SetActive(true);
							//Debug.Log("3 on " + child);
						}
					}
				}

				/*
				Vector3 normal = transform.forward;
				if (inDirections[i] < 0)
					normal = -normal;

				float d = Vector3.Dot(normal, obj.transform.position - transform.position);
				float x = Mathf.Clamp(d / obj.GetComponent<Collider>().bounds.extents.magnitude,0f,1f)*0.5f+0.5f;

				Vector3 COM1 = obj.transform.InverseTransformDirection(normal) * (1f - x)/2;
				Vector3 COM2 = obj.transform.InverseTransformDirection(normal) * (x)/2;

				Rigidbody ORB = obj.GetComponent<Rigidbody>();
				if (ORB != null && obj.tag != "Player")
				{
					Rigidbody CRB = clone.GetComponent<Rigidbody>();

					ORB.AddForceAtPosition(
						Physics.gravity * ORB.mass * 2 * x, 
						obj.transform.TransformPoint(COM1));

					Debug.DrawRay(obj.transform.TransformPoint(COM1), -Physics.gravity * x, Color.white, Time.fixedDeltaTime);

					CRB.AddForceAtPosition(
						Physics.gravity * CRB.mass * 2 * (1f-x), 
						clone.transform.TransformPoint(-COM2));

					Debug.DrawRay(clone.transform.TransformPoint(-COM2), -Physics.gravity * (1f-x), Color.white, Time.fixedDeltaTime);
				}
				*/

				if (Vector3.Dot(obj.transform.position-transform.position,transform.forward) * inDirections[i] < 0 && obj.transform.parent == null)
				{
					clone.transform.parent = obj.transform;
					clone.transform.localPosition = Vector3.zero;
					clone.transform.localRotation = Quaternion.identity;
					clone.transform.localScale = Vector3.one;
					clone.transform.parent = null;

					scale = obj.transform.localScale;
					obj.transform.parent = transform;
					obj.transform.RotateAround(transform.position, transform.up, 180);
					pos = obj.transform.localPosition;
					rot = obj.transform.localRotation;

					obj.transform.parent = OtherPortal.transform;
					obj.transform.localPosition = pos;
					obj.transform.localRotation = rot;

					obj.transform.parent = null;
					obj.transform.localScale = scale * OtherPortal.transform.localScale.magnitude / transform.localScale.magnitude;	

					Rigidbody RB = obj.GetComponent<Rigidbody>();
					Rigidbody portalRB = GetComponent<Rigidbody>();
					Rigidbody otherPortalRB = OtherPortal.GetComponent<Rigidbody>();

					Vector3 vel = RB.velocity;
					Vector3 ang = RB.angularVelocity;

					//Rigidbody cloneRB = clone.GetComponent<Rigidbody>();
					//cloneRB.velocity = vel;
					//cloneRB.angularVelocity = ang;

					vel -= portalRB.velocity;
					vel = transform.InverseTransformVector(vel);
					vel = Quaternion.AngleAxis(180, Vector3.up) * vel;
					vel = OtherPortal.transform.TransformVector(vel);
					vel += otherPortalRB.velocity;

					ang = Quaternion.AngleAxis(180,OtherPortal.transform.up)*OtherPortal.transform.TransformDirection(transform.InverseTransformDirection(ang));

					RB.velocity = vel;
					RB.angularVelocity = ang;


					PlanePosition = OtherPortal.transform.position;
					PlaneNormal = OtherPortal.transform.forward;
					if (inDirections[i]<0)
					{
						PlaneNormal = -PlaneNormal;
					}
					PlanePosition -= PlaneNormal * 0.05f * ((Vector3.Dot(PlaneNormal,playerCamera.transform.position - PlanePosition)) < 0f ?-1:1);
					foreach(GameObject child in objDescendents)
					{
						MeshRenderer MR = child.GetComponent<MeshRenderer>();
						if (MR != null)
						{
							Material mat = MR.material;
							mat.SetVector("_PlanePosition", new Vector4(PlanePosition.x, PlanePosition.y, PlanePosition.z, 0));
							mat.SetVector("_PlaneNormal", new Vector4(PlaneNormal.x, PlaneNormal.y, PlaneNormal.z, 0));
						}
					}

					PlanePosition = transform.position;
					PlaneNormal = transform.forward;
					if (inDirections[i]<0)
					{
						PlaneNormal = -PlaneNormal;
					}
					PlanePosition -= PlaneNormal * 0.05f * ((Vector3.Dot(PlaneNormal,playerCamera.transform.position - PlanePosition)) < 0f ?-1:1);
					foreach(GameObject child in cloneDescendents)
					{
						MeshRenderer MR = child.GetComponent<MeshRenderer>();
						if (MR != null)
						{
							Material mat = MR.material;
							mat.SetVector("_PlanePosition", new Vector4(PlanePosition.x, PlanePosition.y, PlanePosition.z, 0));
							mat.SetVector("_PlaneNormal", new Vector4(PlaneNormal.x, PlaneNormal.y, PlaneNormal.z, 0));
						}
					}
					
					foreach (GameObject child in cameraClones)
					{
						//Debug.Log(child.name);
						//child.layer = otherPortalController.CloneLayer;
						
						if (child.layer == CloneLayer)
						{
							child.layer = otherPortalController.CloneLayer;
						}else{
							child.layer = CloneLayer;
						}
						
					}
					

					inCrossed[i] = true;
					//UnityEditor.EditorApplication.isPaused = true;
				}
			}
		}
	}

	List<GameObject> getAllDescendents(GameObject obj)
	{
		List<GameObject> descendents = new List<GameObject>();
		descendents.Add(obj);
		foreach(Transform child in obj.transform)
		{
			List<GameObject> childDescendents = getAllDescendents(child.gameObject);
			foreach(GameObject descendent in childDescendents)
			{
				descendents.Add(descendent);
			}
		}
		return descendents;
	}
	
	GameObject makeChildrenClones(GameObject obj, string shader, int layer, string name)
	{
		GameObject clone = new GameObject();
		clone.name = obj.name + " " + name;
		//Debug.Log("Creating " + clone.name, gameObject);
		clone.layer = layer;
		clone.transform.parent = obj.transform;
		clone.transform.localPosition = Vector3.zero;
		clone.transform.localRotation = Quaternion.identity;
		clone.transform.localScale = Vector3.one;
		MeshRenderer objMR = obj.GetComponent<MeshRenderer>();
		if (objMR != null)
		{
			MeshFilter MF = clone.AddComponent<MeshFilter>();
			MF.mesh = obj.GetComponent<MeshFilter>().mesh;
			MeshRenderer MR = clone.AddComponent<MeshRenderer>();

			if (objMR.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.Off)
			{
				MR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
				objMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
			}else{
				MR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			}
			Material oldMaterial = obj.GetComponent<MeshRenderer>().material;
			MR.material = new Material(Shader.Find(shader));
			MR.material.CopyPropertiesFromMaterial(oldMaterial);
		}
		Light objLight = obj.GetComponent<Light>();
		if (objLight != null && objLight.enabled)
		{
			System.Type type = objLight.GetType();
			Component comp = clone.AddComponent(type);
			Component other = obj.GetComponent(type);

			System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.DeclaredOnly;
			System.Reflection.PropertyInfo[] pinfos = type.GetProperties(flags);
			foreach (System.Reflection.PropertyInfo pinfo in pinfos)
			{
				if (pinfo.CanWrite)
				{
					pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
				}
			}
			System.Reflection.FieldInfo[] finfos = type.GetFields(flags);
			foreach (System.Reflection.FieldInfo finfo in finfos)
			{
				finfo.SetValue(comp, finfo.GetValue(other));
			}
			Light cloneLight = clone.GetComponent<Light>();
			cloneLight.cullingMask = 1 << CloneLayer;
			cloneLight.cullingMask |= 1 << LayerMask.NameToLayer("TransparentFX");
		}

		foreach(Collider coll in obj.GetComponents<Collider>())
		{
			System.Type type = coll.GetType();
			Component comp = clone.AddComponent(type);
			Component other = obj.GetComponent(type);

			System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.DeclaredOnly;
			System.Reflection.PropertyInfo[] pinfos = type.GetProperties(flags);
			foreach (System.Reflection.PropertyInfo pinfo in pinfos)
			{
				if (pinfo.CanWrite)
				{
					pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
				}
			}
			System.Reflection.FieldInfo[] finfos = type.GetFields(flags);
			foreach (System.Reflection.FieldInfo finfo in finfos)
			{
				finfo.SetValue(comp, finfo.GetValue(other));
			}
		}

		foreach (Transform child in obj.transform)
		{
			if (child.gameObject.layer != PortalCloneLayer && 
				child.gameObject.layer != StencilLayer &&
				child.gameObject.layer != BlockerLayer && 
				child.gameObject.layer != CloneLayer &&
				child.gameObject.layer != gameObject.layer &&
				child.gameObject.layer != OtherPortal.layer &&
				child.gameObject.layer != otherPortalController.PortalCloneLayer && 
				child.gameObject.layer != otherPortalController.StencilLayer &&
				child.gameObject.layer != otherPortalController.BlockerLayer && 
				child.gameObject.layer != otherPortalController.CloneLayer &&
				child.gameObject.tag != "Scene Geometry")
			{	
				GameObject childClone = makeChildrenClones(child.gameObject, shader, layer, name);
				childClone.transform.parent = clone.transform;
			}
		}

		return clone;
	}
	
	void OnTriggerEnter(Collider coll) {
		GameObject obj = coll.gameObject;
		while (obj.transform.parent != null)
		{
			obj = obj.transform.parent.gameObject;
		}
		if (inObjects.IndexOf(obj) == -1 &&
			obj.layer != PortalCloneLayer && 
			obj.layer != StencilLayer &&
			obj.layer != BlockerLayer && 
			obj.layer != CloneLayer &&
			obj.layer != gameObject.layer &&
			obj.layer != OtherPortal.layer &&
			obj.layer != otherPortalController.PortalCloneLayer && 
			obj.layer != otherPortalController.StencilLayer &&
			obj.layer != otherPortalController.BlockerLayer && 
			obj.layer != otherPortalController.CloneLayer &&
			obj.tag != "Scene Geometry")
		{
			Debug.Log(name + " Adding " + obj.name, obj);

			inObjects.Add(obj);
			inLayers.Add(obj.layer);
			inDirections.Add(Vector3.Dot(obj.transform.position-transform.position,transform.forward));
			inCrossed.Add(false);
			//GameObject clone = GameObject.Instantiate(obj);
			/*
			GameObject clone = new GameObject();
			MeshFilter MF = clone.AddComponent<MeshFilter>();
			MF.mesh = obj.GetComponent<MeshFilter>().mesh;
			MeshRenderer MR = clone.AddComponent<MeshRenderer>();
			MR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
			MeshRenderer ObjMR = obj.GetComponent<MeshRenderer>();
			ObjMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
			clone.transform.position = obj.transform.position;
			clone.transform.rotation = obj.transform.rotation;
			clone.transform.localScale = obj.transform.localScale;

			clone.name = obj.name + " (" + name + " inClone)";
			clone.layer = PortalCloneLayer;
			obj.layer = PortalCloneOriginalLayer;
			inClones.Add(clone);
			inMaterials.Add(obj.GetComponent<Renderer>().material);
			Material oldMaterial = obj.GetComponent<Renderer>().material;
			
			MR.material = new Material(Shader.Find("Custom/ClippableStandard"));
			MR.material.CopyPropertiesFromMaterial(oldMaterial);
			ObjMR.material = new Material(Shader.Find("Custom/ClippableStandard"));
			ObjMR.material.CopyPropertiesFromMaterial(oldMaterial);
			*/

			GameObject clone = makeChildrenClones(obj, "Custom/ClippableStandard", PortalCloneLayer, "(" + name + " inClone)");
			clone.transform.parent = obj.transform;
			clone.transform.localScale = Vector3.one;
			clone.transform.localPosition = Vector3.zero;
			clone.transform.localRotation = Quaternion.identity;
			clone.transform.parent = null;

			//objects.Add(clone);

			Vector3 scale = clone.transform.localScale;
			clone.transform.parent = transform;
			clone.transform.RotateAround(transform.position, transform.up, 180);
			Vector3 pos = clone.transform.localPosition;
			Quaternion rot = clone.transform.localRotation;
			clone.transform.parent = OtherPortal.transform;
			clone.transform.localPosition = pos;
			clone.transform.localRotation = rot;
			clone.transform.parent = null;

			clone.transform.localScale = OtherPortal.transform.TransformVector(transform.InverseTransformVector(scale));
			inClones.Add(clone);
			//Material oldMaterial = obj.GetComponent<Renderer>().material;

			Rigidbody ObjRB = obj.GetComponent<Rigidbody>();

			if (ObjRB != null && obj.tag != "Player")
			{
				ObjRB.mass /= 2;
				//ObjRB.useGravity = false;
				System.Type type = ObjRB.GetType();
				Component comp = clone.AddComponent(type);
				Component other = obj.GetComponent(type);

				System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.DeclaredOnly;
				System.Reflection.PropertyInfo[] pinfos = type.GetProperties(flags);
				foreach (System.Reflection.PropertyInfo pinfo in pinfos)
				{
					if (pinfo.CanWrite)
					{
						pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
					}
				}
				System.Reflection.FieldInfo[] finfos = type.GetFields(flags);
				foreach (System.Reflection.FieldInfo finfo in finfos)
				{
					finfo.SetValue(comp, finfo.GetValue(other));
				}

				Rigidbody RB = clone.GetComponent<Rigidbody>();

				Vector3 vel = RB.velocity;
				Vector3 ang = RB.angularVelocity;

				vel -= gameObject.GetComponent<Rigidbody>().velocity;
				vel = transform.InverseTransformVector(vel);
				vel = Quaternion.AngleAxis(180, Vector3.up) * vel;
				vel = OtherPortal.transform.TransformVector(vel);
				vel += OtherPortal.GetComponent<Rigidbody>().velocity;

				ang = Quaternion.AngleAxis(180,OtherPortal.transform.up)*OtherPortal.transform.TransformDirection(transform.InverseTransformDirection(ang));

				RB.velocity = vel;
				RB.angularVelocity = ang;

				RB.constraints = ObjRB.constraints;
			}


			List<GameObject> objDescendents = getAllDescendents(obj);
			for (int index = objDescendents.Count-1; index >= 0; index--)
			{
				GameObject child = objDescendents[index];
				if (child.layer == CloneLayer || child.layer == otherPortalController.CloneLayer)
				{
					Destroy(child.gameObject);
					objDescendents.RemoveAt(index);
				}	
			}
			
			foreach(GameObject child in objDescendents)
			{
				if (child.layer != CloneLayer && child.layer != otherPortalController.CloneLayer)
				{
					MeshRenderer MR = child.GetComponent<MeshRenderer>();
					if (MR != null)
					{
						Material oldMat = MR.material;
						MR.material = new Material(Shader.Find("Custom/ClippableStandard"));
						MR.material.CopyPropertiesFromMaterial(oldMat);
					}
				}
			}

			/*
			MeshRenderer ObjMR = obj.GetComponent<MeshRenderer>();
			ObjMR.material = new Material(Shader.Find("Custom/ClippableStandard"));
			ObjMR.material.CopyPropertiesFromMaterial(oldMaterial);
			*/
			/*
			GameObject clone2 = new GameObject();
			clone2.name = obj.name + " (Other Portal : " + OtherPortal.name + " Camera inClone)";
			//clone2.layer = otherPortalController.CloneLayer;
			inClones2.Add(clone2);
			MeshFilter MF2 = clone2.AddComponent<MeshFilter>();
			MF2.mesh = obj.GetComponent<MeshFilter>().mesh;
			MeshRenderer MR2 = clone2.AddComponent<MeshRenderer>();
			MR2.material = new Material(Shader.Find("Custom/ClippableStandardStencilShow"));
			MR2.material.CopyPropertiesFromMaterial(oldMaterial);

			clone2.transform.parent = obj.transform;
			clone2.transform.localPosition = Vector3.zero;
			clone2.transform.localRotation = Quaternion.identity;
			clone2.transform.localScale = Vector3.one;
			*/
			
		}
	}
	void OnTriggerExit(Collider coll) {
		GameObject obj = coll.gameObject;
		int index = inObjects.IndexOf(obj);
		
		if (index != -1)
		{
			Debug.Log(name + " Removing " + obj.name, obj);
			List<GameObject> objDescendents = getAllDescendents(obj);

			foreach(GameObject child in objDescendents)
			{
				if (child.layer != CloneLayer && child.layer != otherPortalController.CloneLayer)
				{
					MeshRenderer MR = child.GetComponent<MeshRenderer>();
					if (MR != null)
					{
						Material oldMaterial = MR.material;
						MR.material = new Material(Shader.Find("Standard"));
						MR.material.CopyPropertiesFromMaterial(oldMaterial);
					}
				}
			}

			Rigidbody ObjRB = obj.GetComponent<Rigidbody>();
			if(ObjRB != null)
			{
				//ObjRB.useGravity = true;
				ObjRB.mass *= 2;
			}
			
			obj.layer = inLayers[index];
			//objects.RemoveAt(objects.IndexOf(inClones[index]));
			Destroy(inClones[index]);
			
			inObjects.RemoveAt(index);
			inClones.RemoveAt(index);

			//Destroy(inClones2[index]);
			//inClones2.RemoveAt(index);

			inLayers.RemoveAt(index);
			inDirections.RemoveAt(index);
			inCrossed.RemoveAt(index);
		}
	}
	
}
