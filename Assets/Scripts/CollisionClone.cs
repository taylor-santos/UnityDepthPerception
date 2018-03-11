using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionClone : MonoBehaviour {
	private GameObject clone;
	private Rigidbody cloneRB;
	private Rigidbody RB;
	// Use this for initialization
	void Start () {
		clone = gameObject.Clone("force-clone", 0, new System.Type[]{typeof(MeshFilter), typeof(MeshRenderer), typeof(Collider)}, true);
		Collider[] cols = clone.GetComponents<Collider>();
		foreach(Collider col in cols){
			col.isTrigger = true;
		}
		clone.transform.parent = null;
		clone.transform.position += Vector3.up*5;
		RB = GetComponent<Rigidbody>();
		cloneRB = clone.AddComponent<Rigidbody>();
		cloneRB.mass = RB.mass;
		cloneRB.velocity = RB.velocity;
		cloneRB.angularVelocity = RB.angularVelocity;
		cloneRB.useGravity = RB.useGravity;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void FixedUpdate(){
		Debug.DrawRay(transform.TransformPoint(RB.centerOfMass), RB.velocity, Color.green);
		Debug.DrawRay(transform.TransformPoint(RB.centerOfMass), RB.angularVelocity, Color.red);
		Debug.DrawRay(clone.transform.TransformPoint(cloneRB.centerOfMass), cloneRB.velocity, Color.green);
		Debug.DrawRay(clone.transform.TransformPoint(cloneRB.centerOfMass), cloneRB.angularVelocity, Color.red);
	}

	void OnCollisionEnter(Collision collision) {
		Debug.Log(collision.gameObject.name);
		foreach (ContactPoint contact in collision.contacts) {
			Debug.DrawRay(contact.point, collision.relativeVelocity, Color.red);
            Debug.DrawRay(contact.point, collision.impulse, Color.cyan);
            cloneRB.AddForceAtPosition(collision.impulse, clone.transform.TransformPoint(transform.InverseTransformPoint(contact.point)), ForceMode.Impulse);
        }
        Debug.Break();
    }
    void OnCollisionStay(Collision collision) {
    	Debug.Log(collision.gameObject.name);
    	foreach (ContactPoint contact in collision.contacts) {
    		Debug.DrawRay(contact.point, collision.relativeVelocity, Color.red);
            Debug.DrawRay(contact.point, collision.impulse, Color.yellow);
            cloneRB.AddForceAtPosition(collision.impulse, clone.transform.TransformPoint(transform.InverseTransformPoint(contact.point)), ForceMode.Impulse);
        }
        Debug.Break();
    }
}
