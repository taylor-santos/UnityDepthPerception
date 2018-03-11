using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CreateClippingClones : MonoBehaviour {
	public GameObject otherPortal;
	public GameObject portalCamera;
	public float offset = 0.01f;
	public int layer;
	public int otherLayer;
	public Material clipPlaneMaterial;
	public List<GameObject> objects = new List<GameObject>();
	public List<GameObject> portalObjects = new List<GameObject>();
	public List<GameObject> clones = new List<GameObject>();
	public List<GameObject> portalClones = new List<GameObject>();
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		foreach (GameObject obj in GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[])
		{
			if (obj.layer != layer && obj.layer != otherLayer && 
				obj.layer != 14 && 
				obj.layer != 22 && 
				obj.layer != 23 &&
				obj.layer != 8 &&
				obj.layer != 9) //Portal clone & Blockers
			{
				if (!objects.Contains(obj))
				{
					if (obj.GetComponent<MeshRenderer>() != null)
					{
						objects.Add(obj);		
						GameObject clone = new GameObject();
						clone.name = obj.name + " Clip Clone (" + gameObject.name + ")";
						clone.layer = layer;
						clones.Add(clone);
						Material oldMaterial = obj.GetComponent<MeshRenderer>().material;
						clone.transform.parent = obj.transform;
						clone.transform.localPosition = Vector3.zero;
						clone.transform.rotation = obj.transform.rotation;
						clone.transform.localScale = Vector3.one;
						clone.AddComponent<MeshFilter>().mesh = obj.GetComponent<MeshFilter>().mesh;
						MeshRenderer MR = clone.AddComponent<MeshRenderer>();
						if (obj.GetComponent<Renderer>().shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.Off)
						{
							clone.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
						}else{
							clone.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
						}
						MR.material = clipPlaneMaterial;
						ClippableObject CO = clone.AddComponent<ClippableObject>();
						CO.clipPlanes = 1;
						CO.plane1Position = otherPortal.transform.position;
						CO.plane1Rotation = otherPortal.transform.eulerAngles;
						MR.material.CopyPropertiesFromMaterial(oldMaterial);
					}
				}
			}else if (obj.layer == 14)
			{
				if (!portalObjects.Contains(obj))
				{
					if (obj.GetComponent<MeshRenderer>() != null)
					{
						portalObjects.Add(obj);	
						GameObject clone = new GameObject();
						clone.name = obj.name + " Clip Clone";
						clone.layer = layer;
						portalClones.Add(clone);
						Material oldMaterial = obj.GetComponent<MeshRenderer>().material;
						clone.transform.parent = obj.transform;
						clone.transform.localPosition = Vector3.zero;
						clone.transform.rotation = obj.transform.rotation;
						clone.transform.localScale = Vector3.one;
						clone.AddComponent<MeshFilter>().mesh = obj.GetComponent<MeshFilter>().mesh;
						MeshRenderer MR = clone.AddComponent<MeshRenderer>();
						clone.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
						MR.material = clipPlaneMaterial;
						ClippableObject CO = clone.AddComponent<ClippableObject>();
						ClippableObject CO2 = obj.GetComponent<ClippableObject>();
						CO.clipPlanes = 2;
						CO.plane1Position = CO2.plane1Position;
						CO.plane1Rotation = CO2.plane1Rotation;
						CO.plane2Position = otherPortal.transform.position;
						CO.plane2Rotation = otherPortal.transform.eulerAngles;
						MR.material.CopyPropertiesFromMaterial(oldMaterial);
					}
				}
			}
		}


		for (int i=0; i<objects.Count; ++i)
		{
			GameObject obj = objects[i];
			if (obj == null || obj.layer == 14)
			{
				//Debug.Log(gameObject.name + ": Old count: " + objects.Count);
				//Debug.Log(gameObject.name + ": Destroying at " + i);
				Destroy(clones[i]);
				clones.RemoveAt(i);
				objects.RemoveAt(i);
				i = 0;
				//Debug.Log(gameObject.name + ": New count: " + objects.Count);
			}
		}
		for (int i=0; i<portalObjects.Count; ++i)
		{
			GameObject obj = portalObjects[i];
			if (obj == null || obj.layer != 14)
			{
				//Debug.Log(gameObject.name + ": Old count: " + portalObjects.Count);
				//Debug.Log(gameObject.name + ": Destroying at " + i);
				Destroy(portalClones[i]);
				portalClones.RemoveAt(i);
				portalObjects.RemoveAt(i);
				i = 0;
				//Debug.Log(gameObject.name + ": New count: " + portalObjects.Count);
			}
		}
		for (int i=0; i<portalClones.Count; ++i)
		{
			GameObject clone = portalClones[i];
			if (clone == null)
			{
				//Debug.Log(gameObject.name + ": Old count: " + portalObjects.Count);
				//Debug.Log(gameObject.name + ": Destroying at " + i);
				portalClones.RemoveAt(i);
				portalObjects.RemoveAt(i);
				i = 0;
				//Debug.Log(gameObject.name + ": New count: " + portalObjects.Count);
			}
		}
		foreach (GameObject clone in clones)
		{
			ClippableObject CO = clone.GetComponent<ClippableObject>();
			CO.clipPlanes = 1;
			CO.plane1Position = otherPortal.transform.position;
			if (Vector3.Dot(portalCamera.transform.position - otherPortal.transform.position, otherPortal.transform.up) < 0)
			{
				CO.plane1Position -= otherPortal.transform.up * offset;
				CO.plane1Rotation = otherPortal.transform.eulerAngles;
			}else{
				CO.plane1Position += otherPortal.transform.up * offset;
				CO.plane1Rotation = (Quaternion.AngleAxis(180,otherPortal.transform.forward)*otherPortal.transform.rotation).eulerAngles;
			}
			
		}
		
		for (int i=0; i<portalObjects.Count; ++i)
		{
			GameObject obj = portalObjects[i];
			GameObject clone = portalClones[i];
			ClippableObject CO = clone.GetComponent<ClippableObject>();
			ClippableObject CO2 = obj.GetComponent<ClippableObject>();
			CO.clipPlanes = 2;
			if (CO2 != null)
			{
				CO.plane1Position = CO2.plane1Position;
				CO.plane1Rotation = CO2.plane1Rotation;
				CO.plane2Position = otherPortal.transform.position;
				if (Vector3.Dot(portalCamera.transform.position - otherPortal.transform.position, otherPortal.transform.up) < 0)
				{
					CO.plane2Position -= otherPortal.transform.up * offset;
					CO.plane2Rotation = otherPortal.transform.eulerAngles;
				}else{
					CO.plane2Position += otherPortal.transform.up * offset;
					CO.plane2Rotation = (Quaternion.AngleAxis(180,otherPortal.transform.forward)*otherPortal.transform.rotation).eulerAngles;
				}
			}
		}
	}
}
