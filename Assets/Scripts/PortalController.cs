using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PortalController : MonoBehaviour {
	public List<GameObject> inObjects = new List<GameObject>();
	public float offset = 0.01f;
	public float gravity = 20;
	public bool applyGravity = true;
	private List<GameObject> clones = new List<GameObject>();
	private List<bool> directions = new List<bool>();
	private List<bool> moved = new List<bool>();
	private List<int> layers = new List<int>();
	private List<float> masses = new List<float>();
	private List<Material> originalMaterials = new List<Material>();
	public GameObject otherPortal;
	public Material clipPlaneMaterial;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		for (int i=0; i<clones.Count; ++i)
		{
			if (moved[i] == false)
			{
				GameObject clone = clones[i];
				GameObject obj = inObjects[i];

				Vector3 oldPos = clone.transform.position;
				Quaternion oldRot = clone.transform.rotation;

				clone.transform.position = obj.transform.position;
				//clone.transform.position = obj.transform.position;
				Vector3 scale = obj.transform.lossyScale;
				scale.x *= otherPortal.transform.lossyScale.x/transform.lossyScale.x;
				scale.y *= otherPortal.transform.lossyScale.y/transform.lossyScale.y;
				scale.z *= otherPortal.transform.lossyScale.z/transform.lossyScale.z;
				clone.transform.rotation = obj.transform.rotation;

				clone.transform.parent = transform;

				Vector3 pos = clone.transform.localPosition;
				Quaternion rotation = clone.transform.localRotation;

				clone.transform.parent = otherPortal.transform;

				clone.transform.localPosition = pos;
				clone.transform.localRotation = rotation;

				clone.transform.parent = null;
				clone.transform.localScale = scale;

				clone.transform.RotateAround(otherPortal.transform.position, otherPortal.transform.forward, 180);
				
				if (layers[i] != 17) //not player
				{
					Vector3 normal = transform.up;
					if (directions[i] == false)
						normal = -normal;
					float d = Vector3.Dot(normal,obj.transform.position - transform.position);
					float x = Mathf.Clamp(d / obj.GetComponent<Collider>().bounds.extents.magnitude,0f,1f)*0.5f+0.5f;
					//Debug.Log("Fraction: "+x);
					Rigidbody rb1 = obj.GetComponent<Rigidbody>();
					Rigidbody rb2 = clone.GetComponent<Rigidbody>();
					if (rb1 != null)
					{
						rb1.mass = masses[i]/2;
						rb2.mass = masses[i]/2;
						//rb1.AddForce(-Vector3.up * masses[i] * gravity);
						//rb2.AddForce(-Vector3.up * masses[i] * (1f-x) * gravity);
						Vector3 COM1 = obj.transform.InverseTransformDirection(normal) * (1f - x)/2;
						Vector3 COM2 = obj.transform.InverseTransformDirection(normal) * (x)/2;



						Vector3 v1 = rb1.velocity;
						v1 -= transform.GetComponent<Rigidbody>().velocity;
						v1 /= transform.lossyScale.magnitude;
						Vector3 localVel = transform.InverseTransformDirection(v1);
						Vector3 v2 = Quaternion.AngleAxis(180,otherPortal.transform.forward)*otherPortal.transform.TransformDirection(localVel);
						v2 *= otherPortal.transform.lossyScale.magnitude;
						v2 += otherPortal.GetComponent<Rigidbody>().velocity;
						rb2.velocity = v2;
						//Debug.DrawRay(obj.transform.position, rb1.velocity,Color.white, Time.deltaTime);
						//Debug.DrawRay(clone.transform.position, rb2.velocity, Color.white, Time.deltaTime);


						rb1.AddForceAtPosition(
							-Vector3.up * masses[i] * gravity * x, 
							obj.transform.TransformPoint(COM1));
						rb1.AddForceAtPosition(
							Quaternion.AngleAxis(180,otherPortal.transform.forward)*otherPortal.transform.TransformDirection(transform.InverseTransformDirection(-Vector3.up)) * masses[i] * gravity * (1f-x), 
							obj.transform.TransformPoint(-COM2));


						Debug.DrawRay(obj.transform.TransformPoint(COM1), Vector3.up * masses[i] * gravity * x,Color.green,Time.deltaTime);
						Debug.DrawRay(obj.transform.TransformPoint(-COM2), Quaternion.AngleAxis(180,otherPortal.transform.forward)*otherPortal.transform.TransformDirection(transform.InverseTransformDirection(-Vector3.up)) * masses[i] * gravity * (1-x), Color.red, Time.deltaTime);
						//Vector3 COM2 = otherPortal.transform.up * clone.GetComponent<Collider>().bounds.extents.magnitude * (x);

						//rb1.AddForceAtPosition(-Vector3.up * rb1.mass * gravity, obj.transform.position + COM1);
						//rb2.AddForceAtPosition(-Vector3.up * rb2.mass * gravity, clone.transform.position + COM2);
						//Debug.Log("Dist: " + (0.5f - x/2));
						//Debug.DrawRay(obj.transform.position,transform.up * obj.GetComponent<Collider>().bounds.extents.magnitude,new Color(1,1,1,0.5f),Time.deltaTime);
						//Debug.DrawRay(obj.transform.position,COM1,Color.black,Time.deltaTime);
					}
					//Debug.DrawRay(transform.position,transform.up*d,Color.red,Time.deltaTime);
					//Vector3 posDiff = clone.transform.position - oldPos;
					//Quaternion rotDiff = Quaternion.Inverse(oldRot)*clone.transform.rotation;
					//Debug.DrawRay(oldPos,posDiff,Color.red,1);

					//clone.transform.position = Vector3.Lerp(oldPos,clone.transform.position,0.5f);
					//clone.transform.rotation = Quaternion.Lerp(oldRot,clone.transform.rotation,0.5f);
					//Vector3 posDiff2 = otherPortal.transform.TransformDirection(transform.InverseTransformDirection(posDiff));
					//Debug.DrawRay(obj.transform.position,posDiff2,Color.red,1);
					//obj.transform.position += posDiff2/2;

					//Vector3 objPos = obj.transform.position;
					//Quaternion objRot = obj.transform.rotation;

					scale = obj.transform.localScale;
					obj.transform.position = Vector3.Lerp(oldPos,clone.transform.position,0.5f);
					obj.transform.rotation = Quaternion.Lerp(oldRot,clone.transform.rotation,0.5f);
					clone.transform.position = Vector3.Lerp(oldPos,clone.transform.position,0.5f);
					clone.transform.rotation = Quaternion.Lerp(oldRot,clone.transform.rotation,0.5f);
					//obj.transform.position = clone.transform.position;
					//obj.transform.rotation = clone.transform.rotation;
					obj.transform.localScale = clone.transform.localScale;

					obj.transform.parent = otherPortal.transform;

					pos = obj.transform.localPosition;
					rotation = obj.transform.localRotation;

					obj.transform.parent = transform;

					obj.transform.localPosition = pos;
					obj.transform.localRotation = rotation;

					obj.transform.parent = null;
					obj.transform.localScale = scale;

					obj.transform.RotateAround(transform.position, transform.forward, 180);
				}
				
				bool direction = true;
				if (Vector3.Dot(obj.transform.position - transform.position,transform.up) < 0)
					direction = false;

				ClippableObject CO = clone.GetComponent<ClippableObject>();
				CO.clipPlanes = 1;
				if (direction)
				{
					CO.plane1Position = otherPortal.transform.position - otherPortal.transform.up * offset;
					CO.plane1Rotation = otherPortal.transform.eulerAngles;
				}else{
					CO.plane1Position = otherPortal.transform.position + otherPortal.transform.up * offset;
					CO.plane1Rotation = (Quaternion.AngleAxis(180,otherPortal.transform.forward)*otherPortal.transform.rotation).eulerAngles;
				}

				ClippableObject CO2 = obj.GetComponent<ClippableObject>();
				CO2.clipPlanes = 1;
				if (direction)
				{
					CO2.plane1Position = transform.position - transform.up * offset;
					CO2.plane1Rotation = transform.eulerAngles;
				}else{
					CO2.plane1Position = transform.position + transform.up * offset;
					CO2.plane1Rotation = (Quaternion.AngleAxis(180,transform.forward)*transform.rotation).eulerAngles;
				}
			
				if(!(direction == directions[i]) && obj.transform.parent == null && moved[i] == false)
				{
					moved[i] = true;

					//Debug.Log(obj.name + " " +Vector3.Dot(obj.transform.position - transform.position,transform.up) + " " + gameObject.name);
					/*
					scale = obj.transform.lossyScale;
					scale.x *= otherPortal.transform.lossyScale.x/transform.lossyScale.x;
					scale.y *= otherPortal.transform.lossyScale.y/transform.lossyScale.y;
					scale.z *= otherPortal.transform.lossyScale.z/transform.lossyScale.z;

					obj.transform.parent = transform;

					pos = obj.transform.localPosition;
					rotation = obj.transform.localRotation;

					obj.transform.parent = otherPortal.transform;

					obj.transform.localPosition = pos;
					obj.transform.localRotation = rotation;

					obj.transform.parent = null;
					obj.transform.localScale = scale;

					obj.transform.RotateAround(otherPortal.transform.position, otherPortal.transform.forward, 180);
					*/
					pos = obj.transform.position;
					rotation = obj.transform.rotation;
					scale = obj.transform.localScale;

					obj.transform.position = clone.transform.position;
					obj.transform.rotation = clone.transform.rotation;
					obj.transform.localScale = clone.transform.localScale;

					clone.transform.position = pos;
					clone.transform.rotation = rotation;
					clone.transform.localScale = scale;

					Rigidbody rb = obj.GetComponent<Rigidbody>();
					if (rb != null)
					{
						rb.mass = masses[i];
						rb.useGravity = true;
						Vector3 v1 = rb.velocity;
						v1 -= transform.GetComponent<Rigidbody>().velocity;
						v1 /= transform.lossyScale.magnitude;
						Vector3 localVel = transform.InverseTransformDirection(v1);
						Vector3 v2 = Quaternion.AngleAxis(180,otherPortal.transform.forward)*otherPortal.transform.TransformDirection(localVel);
						v2 *= otherPortal.transform.lossyScale.magnitude;
						v2 += otherPortal.GetComponent<Rigidbody>().velocity;
						rb.velocity = v2;
					}
					
					CO = obj.GetComponent<ClippableObject>();
					CO2 = clone.GetComponent<ClippableObject>();

					CO.plane1Position = otherPortal.transform.position;
					CO.plane1Rotation = otherPortal.transform.eulerAngles;

					CO2.plane1Position = transform.position;
					CO2.plane2Rotation = transform.eulerAngles;
					/*
					Vector3 clipPos = CO2.plane1Position;
					Vector3 clipRot = CO2.plane2Rotation;

					CO2.plane1Position = CO.plane1Position;
					CO2.plane1Rotation = CO.plane1Rotation;
					CO.plane1Position = clipPos;
					CO.plane1Rotation = clipRot;
					*/
					/*
					obj.GetComponent<MeshRenderer>().material = originalMaterials[i];
					
					originalMaterials.RemoveAt(i);
					Destroy(obj.GetComponent<ClippableObject>());
					obj.layer = layers[i];

					Destroy(clones[i]);
					clones.RemoveAt(i);
					masses.RemoveAt(i);
					inObjects.RemoveAt(i);
					directions.RemoveAt(i);
					moved.RemoveAt(i);
					layers.RemoveAt(i);
					i=0;
					*/
				}
			}
		}
	}

	void OnTriggerEnter(Collider coll) {
		GameObject obj = coll.gameObject;
		//Debug.Log(obj.name);
		if (obj.layer != 14 && obj.layer != 21)
		{
			if (!inObjects.Contains(obj))
			{
				Debug.Log("Adding " + obj.name);
				inObjects.Add(obj);
				GameObject clone = new GameObject();
				if (obj.layer != 17)
				{
					Rigidbody rb1 = obj.GetComponent<Rigidbody>();
					if (rb1 != null)
					{
						masses.Add(rb1.mass);
						Rigidbody rb2 = clone.AddComponent<Rigidbody>();
						rb1.useGravity = false;
						rb2.useGravity = false;

						Vector3 v1 = rb1.velocity;
						v1 -= transform.GetComponent<Rigidbody>().velocity;
						v1 /= transform.lossyScale.magnitude;
						Vector3 localVel = transform.InverseTransformDirection(v1);
						Vector3 v2 = Quaternion.AngleAxis(180,otherPortal.transform.forward)*otherPortal.transform.TransformDirection(localVel);
						v2 *= otherPortal.transform.lossyScale.magnitude;
						v2 += otherPortal.GetComponent<Rigidbody>().velocity;
						rb2.velocity = v2;
					}else{
						masses.Add(0);
					}
					
				}else{
					masses.Add(0);
				}
				layers.Add(obj.layer);
				obj.layer = 14;
				clone.layer = 14;

				clone.transform.position = obj.transform.position;
				Vector3 scale = obj.transform.lossyScale;
				scale.x *= otherPortal.transform.lossyScale.x/transform.lossyScale.x;
				scale.y *= otherPortal.transform.lossyScale.y/transform.lossyScale.y;
				scale.z *= otherPortal.transform.lossyScale.z/transform.lossyScale.z;
				clone.transform.rotation = obj.transform.rotation;

				clone.transform.parent = transform;

				Vector3 pos = clone.transform.localPosition;
				Quaternion rotation = clone.transform.localRotation;

				clone.transform.parent = otherPortal.transform;

				clone.transform.localPosition = pos;
				clone.transform.localRotation = rotation;

				clone.transform.parent = null;
				clone.transform.localScale = scale;

				clone.transform.RotateAround(otherPortal.transform.position, otherPortal.transform.forward, 180);

				bool direction;
				if (Vector3.Dot(obj.transform.position - transform.position,transform.up) > 0)
				{
					direction = true;
					directions.Add(true);
					clone.name = obj.name + " Clone (Front)" + gameObject.name;
				}else{
					direction = false;
					directions.Add(false);
					clone.name = obj.name + " Clone (Back)" + gameObject.name;
				}
				moved.Add(false);
				clones.Add(clone);
				Material oldMaterial = obj.GetComponent<MeshRenderer>().material;
				originalMaterials.Add(oldMaterial);
				
				clone.AddComponent<MeshFilter>().mesh = obj.GetComponent<MeshFilter>().mesh;
				MeshRenderer MR = clone.AddComponent<MeshRenderer>();
				clone.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
				MR.material = clipPlaneMaterial;
				
				ClippableObject CO = clone.AddComponent<ClippableObject>();
				CO.clipPlanes = 1;
				if (direction)
				{
					CO.plane1Position = otherPortal.transform.position - otherPortal.transform.up * offset;
					CO.plane1Rotation = otherPortal.transform.eulerAngles;
				}else{
					CO.plane1Position = otherPortal.transform.position + otherPortal.transform.up * offset;
					CO.plane1Rotation = (Quaternion.AngleAxis(180,otherPortal.transform.forward)*otherPortal.transform.rotation).eulerAngles;
				}
				MR.material.CopyPropertiesFromMaterial(oldMaterial);

				CO = obj.AddComponent<ClippableObject>();
				CO.clipPlanes = 1;
				if (direction)
				{
					CO.plane1Position = transform.position - transform.up * offset;
					CO.plane1Rotation = transform.eulerAngles;
				}else{
					CO.plane1Position = transform.position + transform.up * offset;
					CO.plane1Rotation = (Quaternion.AngleAxis(180,transform.forward)*transform.rotation).eulerAngles;
				}
				
				obj.GetComponent<MeshRenderer>().material.CopyPropertiesFromMaterial(oldMaterial);

				clone.AddComponent(coll.GetType()).GetComponent<Collider>().isTrigger = false;
			}
		}
    }

    void OnTriggerExit(Collider coll) {
    	GameObject obj = coll.gameObject;
		//Debug.Log(coll.gameObject.name);
		if (inObjects.Contains(obj))
		{
			Debug.Log("Removing " + obj.name);
			int index = inObjects.IndexOf(obj);
			obj.GetComponent<MeshRenderer>().material = originalMaterials[index];
			originalMaterials.RemoveAt(index);
			Destroy(obj.GetComponent<ClippableObject>());
			obj.layer = layers[index];
			Rigidbody rb = obj.GetComponent<Rigidbody>();
			if (rb != null)
			{
				rb.useGravity = true;
				rb.mass = masses[index];
			}
			GameObject clone = clones[index];
			clones.RemoveAt(index);
			Destroy(clone);
			inObjects.RemoveAt(index);
			directions.RemoveAt(index);
			moved.RemoveAt(index);
			masses.RemoveAt(index);
			layers.RemoveAt(index);
		}
    }
}
