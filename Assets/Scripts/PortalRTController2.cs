using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PortalRTController2 : MonoBehaviour {
	public GameObject OtherPortal;
	public GameObject playerCamera;
	public int cloneLayer;
	public int iterations;
	public bool first = true;

	private bool firstPortal;

	private GameObject portalCamera;

	private RenderTexture rt;

	public List<GameObject> inObjects = new List<GameObject>();
	public List<GameObject> inClones = new List<GameObject>();
	public List<float> inDirections = new List<float>();
	private List<string> inTags = new List<string>();
	
	void Start () 
	{
		firstPortal = first;

		portalCamera = new GameObject();
		Camera playerCam = playerCamera.GetComponent<Camera>();

		//playerCam.cullingMask &= ~(1<<cloneLayer);

		Camera portalCam = portalCamera.AddComponent<Camera>();
		portalCam.fieldOfView = playerCam.fieldOfView;
		portalCam.nearClipPlane = 0.01f;

		/*
		portalCam.cullingMask = playerCam.cullingMask;

		portalCam.cullingMask &= ~(1<<gameObject.layer);
		portalCam.cullingMask &= ~(1<<OtherPortal.gameObject.layer);
		*/
		portalCam.cullingMask = (1<<cloneLayer);
		portalCam.cullingMask |= (1<<gameObject.layer+2);
		if (firstPortal)
		{
			portalCam.cullingMask |= (1<<gameObject.layer+4);
		}

		portalCamera.name = name + " Camera";

		rt = new RenderTexture(portalCam.pixelWidth, portalCam.pixelHeight, 32);
		rt.antiAliasing = 8;
		portalCam.targetTexture = rt;

		//GetComponent<MeshRenderer>().material.SetTexture("_Detail", rt);
		GetComponent<MeshRenderer>().material.SetTexture("_MainTex", rt);
		
		if (iterations > 0 && gameObject.layer + 2 < 32)
		{
			GameObject newPortal = new GameObject();
			newPortal.transform.parent = transform;
			newPortal.transform.localPosition = Vector3.zero;
			newPortal.transform.localScale = Vector3.one;
			newPortal.transform.localRotation = Quaternion.identity;
			newPortal.name = name + "-";
			newPortal.tag = "Portal";
			if (firstPortal)
			{
				newPortal.layer = gameObject.layer + 4;
			}else{
				newPortal.layer = gameObject.layer + 2;
			}


			MeshFilter mf = newPortal.AddComponent<MeshFilter>();
			mf.mesh = GetComponent<MeshFilter>().mesh;
			MeshRenderer mr = newPortal.AddComponent<MeshRenderer>();
			mr.material = GetComponent<MeshRenderer>().material;

			PortalRTController2 controller = newPortal.AddComponent<PortalRTController2>();
			controller.iterations = iterations-1;
			controller.OtherPortal = OtherPortal;
			controller.playerCamera = portalCamera;
			controller.cloneLayer = cloneLayer;
			
			controller.first = false;

			//portalCam.cullingMask |= 1 << newPortal.layer;
		}
	}

	void LateUpdate () 
	{
		portalCamera.transform.parent = playerCamera.transform;
		portalCamera.transform.localPosition = Vector3.zero;
		portalCamera.transform.localRotation = Quaternion.identity;
		portalCamera.transform.localScale = Vector3.one;
		portalCamera.transform.parent = null;
		
		portalCamera.transform.parent = transform;
		portalCamera.transform.RotateAround(transform.position, transform.up, 180);

		Vector3 pos = portalCamera.transform.localPosition;
		Quaternion rot = portalCamera.transform.localRotation;

		portalCamera.transform.parent = OtherPortal.transform;
		portalCamera.transform.localPosition = pos;
		portalCamera.transform.localRotation = rot;

		portalCamera.transform.parent = null;

		portalCamera.transform.localScale = Vector3.one;		

		if (firstPortal)
		{
			GameObject[] objects = UnityEngine.Object.FindObjectsOfType<GameObject>() as GameObject[];
			foreach (GameObject obj in objects)
			{
				if (obj.tag != "Clone" && obj.tag != "Portal")
				{
					GameObject clone = null;
					MeshRenderer cloneMR = null;
					foreach(Transform child in obj.transform)
					{
						if (child.gameObject.tag == "Clone" && child.gameObject.layer == cloneLayer)
						{
							clone = child.gameObject;
							cloneMR = clone.GetComponent<MeshRenderer>();
							break;
						}
					}

					if (clone == null)
					{
						clone = new GameObject();
						clone.name = obj.name + " " + name + " Clone";
						clone.layer = cloneLayer;
						clone.tag = "Clone";

						MeshFilter objMF = obj.GetComponent<MeshFilter>();
						if (objMF != null)
						{
							MeshFilter cloneMF = clone.AddComponent<MeshFilter>();
							cloneMF.mesh = objMF.mesh;
						}

						MeshRenderer objMR = obj.GetComponent<MeshRenderer>();
						if (objMR != null)
						{
							cloneMR = clone.AddComponent<MeshRenderer>();
							cloneMR.material = new Material(Shader.Find("Custom/ClippableStandard"));
							cloneMR.material.CopyPropertiesFromMaterial(objMR.material);
						}

						clone.transform.parent = obj.transform;
						clone.transform.localPosition = Vector3.zero;
						clone.transform.localRotation = Quaternion.identity;
						clone.transform.localScale = Vector3.one;
					}

					clone.SetActive(obj.activeInHierarchy);
					clone.transform.localPosition = Vector3.zero;
					clone.transform.localRotation = Quaternion.identity;
					clone.transform.localScale = Vector3.one;

					if (cloneMR != null)
					{
						Material mat = cloneMR.material;
						Vector3 PlanePosition = OtherPortal.transform.position;
						Vector3 PlaneNormal = OtherPortal.transform.forward;
						if (Vector3.Dot(portalCamera.transform.position - OtherPortal.transform.position, -OtherPortal.transform.forward) < 0)
						{
							PlaneNormal = -PlaneNormal;
						}
						PlanePosition -= PlaneNormal * 0.05f;
						mat.SetVector("_PlanePosition", new Vector4(PlanePosition.x, PlanePosition.y, PlanePosition.z, 0));
						mat.SetVector("_PlaneNormal", new Vector4(PlaneNormal.x, PlaneNormal.y, PlaneNormal.z, 0));
						cloneMR.enabled = obj.GetComponent<MeshRenderer>().enabled;
					}
				}
			}
		}
	}
	
	void Update()
	{
		for(int i=0; i<inObjects.Count; ++i)
		{
			
			GameObject obj = inObjects[i];
			GameObject clone = inClones[i];

			obj.transform.parent = null;

			clone.transform.parent = null;
			
			Vector3 scale = obj.transform.localScale;
			clone.transform.parent = OtherPortal.transform;
			Vector3 pos = clone.transform.localPosition;
			Quaternion rot = clone.transform.localRotation;
			clone.transform.parent = transform;
			clone.transform.localPosition = pos;
			clone.transform.localRotation = rot;
			clone.transform.RotateAround(transform.position, transform.up, 180);
			clone.transform.parent = null;
			Debug.DrawLine(clone.transform.position, obj.transform.position, Color.black, 0);
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
			clone.transform.parent = transform;

			clone.transform.RotateAround(transform.position, transform.up, 180);
			pos = clone.transform.localPosition;
			rot = clone.transform.localRotation;
			clone.transform.parent = OtherPortal.transform;
			clone.transform.localPosition = pos;
			clone.transform.localRotation = rot;
			clone.transform.parent = null;

			clone.transform.localScale = scale * OtherPortal.transform.localScale.magnitude / transform.localScale.magnitude;
			


			Vector3 PlaneNormal = (OtherPortal.transform.forward*inDirections[i]).normalized;
			Vector3 PlanePosition = OtherPortal.transform.position;

			Material mat = clone.GetComponent<MeshRenderer>().material;
			mat.SetVector("_PlanePosition", new Vector4(PlanePosition.x, PlanePosition.y, PlanePosition.z, 0));
			mat.SetVector("_PlaneNormal", new Vector4(PlaneNormal.x, PlaneNormal.y, PlaneNormal.z, 0));
			foreach(Transform child in clone.GetComponentsInChildren<Transform>())
			{
				MeshRenderer childMR = child.GetComponent<MeshRenderer>();
				if (childMR != null)
				{
					mat = child.gameObject.GetComponent<MeshRenderer>().material;
					mat.SetVector("_PlanePosition", new Vector4(PlanePosition.x, PlanePosition.y, PlanePosition.z, 0));
					mat.SetVector("_PlaneNormal", new Vector4(PlaneNormal.x, PlaneNormal.y, PlaneNormal.z, 0));
				}
			}
			
			MeshRenderer objMR = obj.GetComponent<MeshRenderer>();
			Material oldMat = objMR.material;
			objMR.material = new Material(Shader.Find("Custom/ClippableStandard"));
			objMR.material.CopyPropertiesFromMaterial(oldMat);
			foreach(Transform child in obj.GetComponentsInChildren<Transform>())
			{
				MeshRenderer childMR = child.gameObject.GetComponent<MeshRenderer>();
				if (childMR != null)
				{
					oldMat = childMR.material;
					childMR.material = new Material(Shader.Find("Custom/ClippableStandard"));
					childMR.material.CopyPropertiesFromMaterial(oldMat);
				}
			}

			PlaneNormal	= (transform.forward*inDirections[i]).normalized;
			PlanePosition = transform.position;

			mat = obj.GetComponent<MeshRenderer>().material;
			mat.SetVector("_PlanePosition", new Vector4(PlanePosition.x, PlanePosition.y, PlanePosition.z, 0));
			mat.SetVector("_PlaneNormal", new Vector4(PlaneNormal.x, PlaneNormal.y, PlaneNormal.z, 0));
			foreach(Transform child in obj.GetComponentsInChildren<Transform>())
			{
				MeshRenderer childMR = child.gameObject.GetComponent<MeshRenderer>();
				if (childMR != null)
				{
					mat = child.gameObject.GetComponent<MeshRenderer>().material;
					mat.SetVector("_PlanePosition", new Vector4(PlanePosition.x, PlanePosition.y, PlanePosition.z, 0));
					mat.SetVector("_PlaneNormal", new Vector4(PlaneNormal.x, PlaneNormal.y, PlaneNormal.z, 0));
				}
			}


			Rigidbody ObjRB = obj.GetComponent<Rigidbody>();
			if (ObjRB != null && obj.tag != "Player")
			{
				
				Rigidbody CloneRB = clone.GetComponent<Rigidbody>();
				Rigidbody portalRB = GetComponent<Rigidbody>();
				Rigidbody otherPortalRB = OtherPortal.GetComponent<Rigidbody>();

				ObjRB.ResetCenterOfMass();
				ObjRB.ResetInertiaTensor();
				CloneRB.ResetCenterOfMass();
				CloneRB.ResetInertiaTensor();

				Vector3 cloneVel = CloneRB.velocity - otherPortalRB.velocity;
				cloneVel = transform.TransformVector(Quaternion.AngleAxis(180, Vector3.up)*OtherPortal.transform.InverseTransformVector(cloneVel));
				Vector3 vel = ObjRB.velocity - portalRB.velocity;

				Debug.DrawRay(obj.transform.position, cloneVel, new Color(1,1,1,0.8f), 0);
				Debug.DrawRay(obj.transform.position, vel, new Color(0,0,0,0.8f), 0);

				vel = Vector3.Lerp(vel,cloneVel,0.5f);
				cloneVel = vel;

				cloneVel = OtherPortal.transform.TransformVector(Quaternion.AngleAxis(180, Vector3.up)*transform.InverseTransformVector(cloneVel));

				ObjRB.velocity = vel + portalRB.velocity;
				CloneRB.velocity = cloneVel + otherPortalRB.velocity;

				Debug.DrawRay(obj.transform.position, ObjRB.velocity, Color.green, 0);
				Debug.DrawRay(clone.transform.position, CloneRB.velocity, Color.green, 0);

				Vector3 cloneAng = CloneRB.angularVelocity;
				cloneAng = transform.TransformDirection(Quaternion.AngleAxis(180, Vector3.up)*OtherPortal.transform.InverseTransformDirection(cloneAng));
				Vector3 ang = ObjRB.angularVelocity;
				ang = Vector3.Lerp(ang,cloneAng,0.5f);
				cloneAng = OtherPortal.transform.TransformDirection(Quaternion.AngleAxis(180, Vector3.up)*transform.InverseTransformDirection(ang));
				CloneRB.angularVelocity = cloneAng;
				ObjRB.angularVelocity = ang;
				
			}
		}
	}

	void FixedUpdate()
	{
		for(int i=0; i<inObjects.Count; ++i)
		{
			
			GameObject obj = inObjects[i];
			GameObject clone = inClones[i];
			if (Vector3.Dot(obj.transform.position-transform.position,transform.forward) * inDirections[i] < 0)
			{
				Vector3 pos = obj.transform.position;
				Quaternion rot = obj.transform.rotation;
				Vector3 scale = obj.transform.localScale;

				obj.transform.position = clone.transform.position;
				obj.transform.rotation = clone.transform.rotation;
				obj.transform.localScale = scale * OtherPortal.transform.localScale.magnitude/transform.localScale.magnitude;

				clone.transform.position = pos;
				clone.transform.rotation = rot;
				clone.transform.localScale = scale;

				Rigidbody ObjRB = obj.GetComponent<Rigidbody>();
				if (ObjRB != null)
				{
					Rigidbody CloneRB = clone.GetComponent<Rigidbody>();
					Vector3 vel = ObjRB.velocity;
					Vector3 ang = ObjRB.angularVelocity;

					ObjRB.velocity = CloneRB.velocity;
					ObjRB.angularVelocity = CloneRB.angularVelocity;

					CloneRB.velocity = vel;
					CloneRB.angularVelocity = ang;
				}
			}
		}
	}
	

	GameObject createClone(GameObject obj, string shader, string tag, int layerMask)
	{
		GameObject clone = new GameObject();
		clone.name = obj.name + " " + name + " in-Clone";
		clone.layer = obj.layer;
		clone.tag = tag;
		MeshFilter objMF = obj.GetComponent<MeshFilter>();
		if (objMF != null)
		{
			MeshFilter cloneMF = clone.AddComponent<MeshFilter>();
			cloneMF.mesh = objMF.mesh;
		}

		MeshRenderer objMR = obj.GetComponent<MeshRenderer>();
		if (objMR != null)
		{
			MeshRenderer cloneMR = clone.AddComponent<MeshRenderer>();
			cloneMR.material = new Material(Shader.Find(shader));
			cloneMR.material.CopyPropertiesFromMaterial(objMR.material);
		}

		foreach (Collider coll in obj.GetComponents<Collider>())
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
			//if ((layerMask & (1 << child.gameObject.layer)) > 0 && child.tag != tag)
			//{
				GameObject newChild = createClone(child.gameObject, shader, tag, layerMask);
				newChild.transform.parent = clone.transform;
				newChild.transform.localPosition = child.localPosition;
				newChild.transform.localRotation = child.localRotation;
				newChild.transform.localScale = child.localScale;
			//}
		}
		return clone;
	}
	
	void OnTriggerEnter(Collider coll) 
	{
		GameObject obj = coll.gameObject;
		while (obj.transform.parent != null)
		{
			obj = obj.transform.parent.gameObject;
		}
		if (inObjects.IndexOf(obj) == -1 && obj.tag != "Clone" && obj.tag != "Teleport Clone")
		{
			float direction = Vector3.Dot(obj.transform.position - transform.position, transform.forward);
			inDirections.Add(direction);
			GameObject clone = createClone(obj, "Custom/ClippableStandard", "Teleport Clone", (1<<gameObject.layer | 1<<OtherPortal.layer));

			clone.transform.parent = obj.transform;
			clone.transform.localPosition = Vector3.zero;
			clone.transform.localRotation = Quaternion.identity;
			clone.transform.localScale = Vector3.one;

			clone.transform.parent = null;
			Vector3 scale = clone.transform.localScale;

			clone.transform.parent = transform;
			clone.transform.RotateAround(transform.position, transform.up, 180);
			Vector3 pos = clone.transform.localPosition;
			Quaternion rot = clone.transform.localRotation;
			
			clone.transform.parent = OtherPortal.transform;
			clone.transform.localPosition = pos;
			clone.transform.localRotation = rot;
			
			clone.transform.parent = null;
			clone.transform.localScale = scale * OtherPortal.transform.localScale.magnitude / transform.localScale.magnitude;

			Vector3 PlaneNormal = (OtherPortal.transform.forward*direction).normalized;
			Vector3 PlanePosition = OtherPortal.transform.position;

			Material mat = clone.GetComponent<MeshRenderer>().material;
			mat.SetVector("_PlanePosition", new Vector4(PlanePosition.x, PlanePosition.y, PlanePosition.z, 0));
			mat.SetVector("_PlaneNormal", new Vector4(PlaneNormal.x, PlaneNormal.y, PlaneNormal.z, 0));
			foreach(Transform child in clone.GetComponentsInChildren<Transform>())
			{
				MeshRenderer childMR = child.GetComponent<MeshRenderer>();
				if (childMR != null)
				{
					mat = child.gameObject.GetComponent<MeshRenderer>().material;
					mat.SetVector("_PlanePosition", new Vector4(PlanePosition.x, PlanePosition.y, PlanePosition.z, 0));
					mat.SetVector("_PlaneNormal", new Vector4(PlaneNormal.x, PlaneNormal.y, PlaneNormal.z, 0));
				}
			}

			MeshRenderer objMR = obj.GetComponent<MeshRenderer>();
			Material oldMat = objMR.material;
			objMR.material = new Material(Shader.Find("Custom/ClippableStandard"));
			objMR.material.CopyPropertiesFromMaterial(oldMat);
			foreach(Transform child in obj.GetComponentsInChildren<Transform>())
			{
				MeshRenderer childMR = child.gameObject.GetComponent<MeshRenderer>();
				if (childMR != null)
				{
					oldMat = childMR.material;
					childMR.material = new Material(Shader.Find("Custom/ClippableStandard"));
					childMR.material.CopyPropertiesFromMaterial(oldMat);
				}
			}

			PlaneNormal	= (transform.forward*direction).normalized;
			PlanePosition = transform.position;

			mat = obj.GetComponent<MeshRenderer>().material;
			mat.SetVector("_PlanePosition", new Vector4(PlanePosition.x, PlanePosition.y, PlanePosition.z, 0));
			mat.SetVector("_PlaneNormal", new Vector4(PlaneNormal.x, PlaneNormal.y, PlaneNormal.z, 0));
			foreach(Transform child in obj.GetComponentsInChildren<Transform>())
			{
				MeshRenderer childMR = child.gameObject.GetComponent<MeshRenderer>();
				if (childMR != null)
				{
					mat = child.gameObject.GetComponent<MeshRenderer>().material;
					mat.SetVector("_PlanePosition", new Vector4(PlanePosition.x, PlanePosition.y, PlanePosition.z, 0));
					mat.SetVector("_PlaneNormal", new Vector4(PlaneNormal.x, PlaneNormal.y, PlaneNormal.z, 0));
				}
			}
			//SetChildrenClipPlanes(obj, PlanePosition, PlaneNormal);
			//mat = objMR.material;
			//mat.SetVector("_PlanePosition", new Vector4(PlanePosition.x, PlanePosition.y, PlanePosition.z, 0));
			//mat.SetVector("_PlaneNormal", new Vector4(PlaneNormal.x, PlaneNormal.y, PlaneNormal.z, 0));

			//DestroyAllTaggedChildren(obj, "Clone");
			/*
			foreach(Transform child in obj.GetComponentsInChildren<Transform>())
			{
				if (child.gameObject.tag == "Clone")
				{
					Destroy(child.gameObject);
				}
			}
			*/
			inTags.Add(obj.tag);
			obj.tag = "Teleport Clone";

			foreach(Transform child in obj.GetComponentsInChildren<Transform>())
			{
				if (child.gameObject.tag != "Clone")
					child.gameObject.tag = "Teleport Clone";
			}

			
			Rigidbody ObjRB = obj.GetComponent<Rigidbody>();
			if (ObjRB != null)
			{
				Rigidbody CloneRB = clone.AddComponent<Rigidbody>();
				ObjRB.mass /= 2;
				CloneRB.mass = ObjRB.mass;
				CloneRB.velocity = OtherPortal.transform.TransformVector(Quaternion.AngleAxis(180, Vector3.up)*transform.InverseTransformVector(ObjRB.velocity - GetComponent<Rigidbody>().velocity)) + OtherPortal.GetComponent<Rigidbody>().velocity;
				CloneRB.angularVelocity = OtherPortal.transform.TransformDirection(Quaternion.AngleAxis(180, Vector3.up)*transform.InverseTransformDirection(ObjRB.angularVelocity));
				

				CloneRB.constraints = ObjRB.constraints;
			}


			inObjects.Add(obj);
			inClones.Add(clone);
		}
	}

	void OnTriggerExit(Collider coll)
	{
		GameObject obj = coll.gameObject;
		int i = inObjects.IndexOf(obj);
		if (i >= 0)
		{
			Destroy(inClones[i]);
			inClones.RemoveAt(i);

			obj.tag = inTags[i];
			Material mat = obj.GetComponent<MeshRenderer>().material;
			obj.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
			obj.GetComponent<MeshRenderer>().material.CopyPropertiesFromMaterial(mat);
			foreach(Transform child in obj.GetComponentsInChildren<Transform>())
			{
				if (child.gameObject.tag == "Teleport Clone")
					child.gameObject.tag = inTags[i];
				MeshRenderer childMR = child.gameObject.GetComponent<MeshRenderer>();
				if (childMR != null)
				{
					mat = childMR.material;
					childMR.material = new Material(Shader.Find("Standard"));
					childMR.material.CopyPropertiesFromMaterial(mat);
				}
			}

			Rigidbody objRB = obj.GetComponent<Rigidbody>();
			if (objRB != null)
			{
				objRB.mass *= 2;
			}

			inObjects.RemoveAt(i);
			inTags.RemoveAt(i);
			inDirections.RemoveAt(i);

		}
	}
}
