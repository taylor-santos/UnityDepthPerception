using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class portalCloneController : MonoBehaviour {
	public GameObject otherPortal;
	public Material clipPlaneMaterial;
	public float offset = 0.01f;
	public float gravity = 20;
	public bool applyGravity = true;
	public Vector3 localAccel;
	public Vector3 otherPortalAccel;

	private Vector3 prevVel;
	private Vector3 otherPrevVel;
	private List<GameObject> objects = new List<GameObject>();
	private List<GameObject> clones = new List<GameObject>();
	private List<bool> directions = new List<bool>();
	private List<Material> materials = new List<Material>();
	private List<int> layers = new List<int>();

	void Start () {
		prevVel = GetComponent<Rigidbody>().velocity;
		otherPrevVel = otherPortal.GetComponent<Rigidbody>().velocity;
	}

	void FixedUpdate()
	{
		Vector3 currVel = GetComponent<Rigidbody>().velocity;
		Vector3 velDifference = currVel - prevVel;
		Vector3 gravAccel = -Vector3.up * gravity * Time.fixedDeltaTime;
		localAccel = (velDifference - gravAccel)/Time.fixedDeltaTime;
		prevVel = currVel;

		currVel = otherPortal.GetComponent<Rigidbody>().velocity;
		velDifference = currVel - otherPrevVel;
		gravAccel = -Vector3.up * gravity * Time.fixedDeltaTime;
		otherPortalAccel = (velDifference - gravAccel)/Time.fixedDeltaTime;
		otherPrevVel = currVel;

		for (int index = 0; index < objects.Count; ++index)
		{
			GameObject obj = objects[index];
			GameObject clone = clones[index];

			Rigidbody RBobj = obj.GetComponent<Rigidbody>();
			Rigidbody RBclone = clone.GetComponent<Rigidbody>();
			Rigidbody RBthis = GetComponent<Rigidbody>();
			Rigidbody RBother = otherPortal.GetComponent<Rigidbody>();
			if (RBobj != null)
			{
				Vector3 vel_obj = RBobj.velocity;
				Vector3 angVel_obj = RBobj.angularVelocity;

				vel_obj -= RBthis.velocity;
				//angVel_obj -= RBthis.angularVelocity;

				vel_obj /= transform.localScale.magnitude;
				//angVel_obj /= transform.localScale.magnitude;

				Vector3 vel_clone = Quaternion.AngleAxis(180,otherPortal.transform.forward)*otherPortal.transform.TransformDirection(transform.InverseTransformDirection(vel_obj));
				Vector3 angVel_clone = Quaternion.AngleAxis(180,otherPortal.transform.forward)*otherPortal.transform.TransformDirection(transform.InverseTransformDirection(angVel_obj));

				vel_clone *= otherPortal.transform.localScale.magnitude;
				//angVel_clone *= otherPortal.transform.localScale.magnitude;

				vel_clone += RBother.velocity;
				//angVel_clone += RBother.angularVelocity;

				vel_clone = Vector3.Lerp(RBclone.velocity, vel_clone, 0.5f);
				angVel_clone = Vector3.Lerp(RBclone.angularVelocity, angVel_clone, 0.5f);

				RBclone.velocity = vel_clone;
				RBclone.angularVelocity = angVel_clone;

				vel_clone -= RBother.velocity;
				//angVel_clone -= RBother.angularVelocity;

				vel_clone /= otherPortal.transform.localScale.magnitude;
				//angVel_clone /= otherPortal.transform.localScale.magnitude;

				vel_obj = Quaternion.AngleAxis(180,transform.forward)*transform.TransformDirection(otherPortal.transform.InverseTransformDirection(vel_clone));
				angVel_obj = Quaternion.AngleAxis(180,transform.forward)*transform.TransformDirection(otherPortal.transform.InverseTransformDirection(angVel_clone));

				vel_obj *= transform.localScale.magnitude;
				//angVel_obj *= transform.localScale.magnitude;

				vel_obj += RBthis.velocity;
				//angVel_obj += RBother.angularVelocity;

				RBobj.velocity = vel_obj;
				RBobj.angularVelocity = angVel_obj;
				

				
				if (applyGravity)
				{
					Vector3 normal = transform.up;
					if (directions[index] == false)
					{
						normal = -transform.up;
					}

					float d = Vector3.Dot(normal,obj.transform.position - transform.position);
					float x = Mathf.Clamp(d / obj.GetComponent<Collider>().bounds.extents.magnitude,0f,1f)*0.5f+0.5f;

					Vector3 COM1 = obj.transform.InverseTransformDirection(normal) * (1f - x)/2;
					Vector3 COM2 = obj.transform.InverseTransformDirection(normal) * (x)/2;

					RBobj.AddForceAtPosition(
						-localAccel * RBobj.mass * 2 * x, 
						obj.transform.TransformPoint(COM1));
					RBobj.AddForceAtPosition(
						Quaternion.AngleAxis(180,otherPortal.transform.forward)*otherPortal.transform.TransformDirection(transform.InverseTransformDirection(-otherPortalAccel)) * RBobj.mass * 2 * (1f-x), 
						obj.transform.TransformPoint(-COM2));
				}	
			}

			Vector3 scale = obj.transform.localScale;
			scale.x *= otherPortal.transform.lossyScale.x/transform.lossyScale.x;
			scale.y *= otherPortal.transform.lossyScale.y/transform.lossyScale.y;
			scale.z *= otherPortal.transform.lossyScale.z/transform.lossyScale.z;
			clone.transform.parent = otherPortal.transform;
			Vector3 pos = clone.transform.localPosition;
			Quaternion rot = clone.transform.localRotation;
			clone.transform.parent = transform;
			clone.transform.localPosition = pos;
			clone.transform.localRotation = rot;
			clone.transform.parent = null;
			clone.transform.localScale = scale;
			clone.transform.RotateAround(transform.position, transform.forward, 180);

			obj.transform.position = Vector3.Lerp(clone.transform.position,obj.transform.position,0.5f);;
			clone.transform.position = obj.transform.position;
			obj.transform.rotation = Quaternion.Lerp(clone.transform.rotation,obj.transform.rotation,0.5f);
			clone.transform.rotation = obj.transform.rotation;

			clone.transform.parent = transform;
			pos = clone.transform.localPosition;
			rot = clone.transform.localRotation;
			clone.transform.parent = otherPortal.transform;
			clone.transform.localPosition = pos;
			clone.transform.localRotation = rot;
			clone.transform.parent = null;
			clone.transform.localScale = scale;
			clone.transform.RotateAround(otherPortal.transform.position, otherPortal.transform.forward, 180);
			
			ClippableObject COobj = obj.GetComponent<ClippableObject>();
			ClippableObject COclone = clone.GetComponent<ClippableObject>();

			COobj.plane1Position = transform.position;
			COclone.plane1Position = otherPortal.transform.position;

			if (directions[index] == true)
			{
				COobj.plane1Position += transform.up * offset;
				COclone.plane1Position += otherPortal.transform.up * offset;
				COobj.plane1Rotation = transform.eulerAngles;
				COclone.plane1Rotation = otherPortal.transform.eulerAngles;
			}else{
				COobj.plane1Position -= transform.up * offset;
				COclone.plane1Position -= otherPortal.transform.up * offset;
				COobj.plane1Rotation = (Quaternion.AngleAxis(180,transform.forward)*transform.rotation).eulerAngles;
				COclone.plane1Rotation = (Quaternion.AngleAxis(180,otherPortal.transform.forward)*otherPortal.transform.rotation).eulerAngles;
			}

			bool direction = true;
			if (Vector3.Dot(obj.transform.position - transform.position,transform.up) < 0)
				direction = false;
			
			if(!(direction == directions[index]) && obj.transform.parent == null)
			{
				Vector3 oldPos = obj.transform.position;
				Quaternion oldRot = obj.transform.rotation;
				Vector3 oldScale = obj.transform.localScale;
				Vector3 oldVel = RBobj.velocity;
				Vector3 oldAngVel = RBobj.angularVelocity;

				obj.transform.position = clone.transform.position;
				obj.transform.rotation = clone.transform.rotation;
				obj.transform.localScale = clone.transform.localScale;
				RBobj.velocity = RBclone.velocity;
				RBobj.angularVelocity = RBclone.angularVelocity;

				clone.transform.position = oldPos;
				clone.transform.rotation = oldRot;
				clone.transform.localScale = oldScale;
				RBclone.velocity = oldVel;
				RBclone.angularVelocity = oldAngVel;

				Vector3 oldClipPos = COobj.plane1Position;
				Vector3 oldClipRot = COobj.plane1Rotation;
				COobj.plane1Position = COclone.plane1Position;
				COobj.plane1Rotation = COclone.plane1Rotation;
				COclone.plane1Position = oldClipPos;
				COclone.plane1Rotation = oldClipRot;
			}

		}
	}

	void OnTriggerEnter(Collider coll) {
		GameObject obj = coll.gameObject;
		if (obj.layer != 14 && obj.layer != 18 && obj.layer != 21)
		{
			Debug.Log("Adding " + obj.name);
			objects.Add(obj);

			GameObject clone = new GameObject();
			clone.name = obj.name + " Clone (" + gameObject.name + " -> " + otherPortal.name + ")";
			clone.layer = 14;
			layers.Add(obj.layer);
			obj.layer = 14;
			clone.AddComponent<MeshFilter>().mesh = obj.GetComponent<MeshFilter>().mesh;
			MeshRenderer MRclone = clone.AddComponent<MeshRenderer>();
			clone.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
			MeshRenderer MRobj = obj.GetComponent<MeshRenderer>();
			Material oldMaterial = MRobj.material;
			materials.Add(oldMaterial);
			MRobj.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
			clones.Add(clone);

			//Vector3 normal;
			ClippableObject COclone = clone.AddComponent<ClippableObject>();
			ClippableObject COobj = obj.AddComponent<ClippableObject>();
			COobj.clipPlanes = 1;
			COclone.clipPlanes = 1;
			COobj.plane1Position = transform.position;
			COclone.plane1Position = otherPortal.transform.position;
			if (Vector3.Dot(obj.transform.position - transform.position,transform.up) > 0)
			{
				directions.Add(true);
				//normal = transform.up;
				COobj.plane1Rotation = transform.eulerAngles;
				COclone.plane1Rotation = otherPortal.transform.eulerAngles;
			}else{
				directions.Add(false);
				//normal = -transform.up;
				COobj.plane1Rotation = (Quaternion.AngleAxis(180,transform.forward)*transform.rotation).eulerAngles;
				COclone.plane1Rotation = (Quaternion.AngleAxis(180,otherPortal.transform.forward)*otherPortal.transform.rotation).eulerAngles;
			}

			clone.transform.position = obj.transform.position;
			clone.transform.rotation = obj.transform.rotation;
			
			Vector3 scale = obj.transform.localScale;
			scale.x *= otherPortal.transform.lossyScale.x/transform.lossyScale.x;
			scale.y *= otherPortal.transform.lossyScale.y/transform.lossyScale.y;
			scale.z *= otherPortal.transform.lossyScale.z/transform.lossyScale.z;
			clone.transform.parent = transform;
			Vector3 pos = clone.transform.localPosition;
			Quaternion rot = clone.transform.localRotation;
			clone.transform.parent = otherPortal.transform;
			clone.transform.localPosition = pos;
			clone.transform.localRotation = rot;
			clone.transform.parent = null;
			clone.transform.localScale = scale;
			clone.transform.RotateAround(otherPortal.transform.position, otherPortal.transform.forward, 180);
			
			MRclone.material = clipPlaneMaterial;
			MRclone.material.CopyPropertiesFromMaterial(oldMaterial);
			MRobj.material.CopyPropertiesFromMaterial(oldMaterial);
			

			Rigidbody RBobj = obj.GetComponent<Rigidbody>();
			if (RBobj != null)
			{
				Rigidbody RBclone = clone.AddComponent<Rigidbody>();
				clone.AddComponent(coll.GetType()).GetComponent<Collider>().isTrigger = false;

				RBobj.useGravity = false;
				RBclone.useGravity = false;

				RBobj.mass /= 2;
				RBclone.mass = RBobj.mass;

				RBclone.angularDrag = RBobj.angularDrag;
				RBclone.drag = RBobj.drag;
				RBclone.constraints = RBobj.constraints;
				RBclone.interpolation = RBobj.interpolation;
				RBclone.collisionDetectionMode = RBobj.collisionDetectionMode;

				Vector3 vel_obj = RBobj.velocity;
				Vector3 angVel_obj = RBobj.angularVelocity;
				vel_obj -= GetComponent<Rigidbody>().velocity;
				//angVel_obj -= GetComponent<Rigidbody>().angularVelocity;
				vel_obj /= transform.localScale.magnitude;
				Vector3 vel_clone = Quaternion.AngleAxis(180,otherPortal.transform.forward)*otherPortal.transform.TransformDirection(transform.InverseTransformDirection(vel_obj));
				Vector3 angVel_clone = Quaternion.AngleAxis(180,otherPortal.transform.forward)*otherPortal.transform.TransformDirection(transform.InverseTransformDirection(angVel_obj));
				vel_clone *= otherPortal.transform.localScale.magnitude;
				vel_clone += otherPortal.GetComponent<Rigidbody>().velocity;
				//angVel_clone += otherPortal.GetComponent<Rigidbody>().angularVelocity;
				RBclone.velocity = vel_clone;
				RBclone.angularVelocity = angVel_clone;
			}

			//GameObject physClone = new GameObject();
			//IgnoreCollisions ICclone = physClone.AddComponent<IgnoreCollisions>();
			//ICclone.obj = obj;
		}
	}

	void OnTriggerExit(Collider coll) {
		GameObject obj = coll.gameObject;
		Debug.Log("Removing " + obj.name);
		int index = objects.IndexOf(obj);
		if (index != -1)
		{
			Rigidbody RBobj = obj.GetComponent<Rigidbody>();
			if (RBobj != null)
			{
				RBobj.mass *= 2;
				if (applyGravity)
					RBobj.useGravity = true;
			}
			obj.GetComponent<MeshRenderer>().material = materials[index];
			obj.layer = layers[index];
			Destroy(obj.GetComponent<ClippableObject>());
			Destroy(clones[index]);
			objects.RemoveAt(index);
			clones.RemoveAt(index);
			directions.RemoveAt(index);
			materials.RemoveAt(index);
			layers.RemoveAt(index);
		}
	}
}
