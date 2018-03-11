using UnityEngine;
using System.Collections;

public class IgnoreCollisions : MonoBehaviour {
	public GameObject obj;
	public Rigidbody RBobj, RBclone;
	public float force;
	// Use this for initialization
	void Start () {
		transform.position = obj.transform.position;
		transform.rotation = obj.transform.rotation;
		transform.localScale = obj.transform.localScale;
		gameObject.layer = 18;
		gameObject.name = obj.gameObject.name + " Physics Clone";
		gameObject.AddComponent(obj.GetComponent<Collider>().GetType()).GetComponent<Collider>().isTrigger = false;
		obj.GetComponent<Collider>().isTrigger = true;
		RBobj = obj.GetComponent<Rigidbody>();
		RBclone = gameObject.AddComponent<Rigidbody>();
		RBclone.mass = RBobj.mass;
		RBclone.velocity = RBobj.velocity;
		RBclone.angularVelocity = RBobj.angularVelocity;
		RBclone.useGravity = RBobj.useGravity;
		RBclone.angularDrag = RBobj.angularDrag;
		RBclone.drag = RBobj.drag;
		RBclone.constraints = RBobj.constraints;
		RBclone.interpolation = RBobj.interpolation;
		RBclone.collisionDetectionMode = RBobj.collisionDetectionMode;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		
		//obj.transform.position = transform.position;
		//obj.transform.rotation = transform.rotation;
		RBobj.velocity = RBclone.velocity;
		RBobj.angularVelocity = RBclone.angularVelocity;
		
		/*
		transform.position = obj.transform.position;
		transform.rotation = obj.transform.rotation;
		RBclone.velocity = RBobj.velocity;
		RBclone.angularVelocity = RBobj.angularVelocity;
		*/
	}
	/*
	void OnCollisionStay(Collision collision) {
        foreach (ContactPoint contact in collision.contacts) {
        	//RBobj.AddForceAtPosition(collision.impulse/force,contact.point,ForceMode.VelocityChange);
        	//Debug.DrawRay(contact.point,collision.impulse,Color.green,Time.deltaTime);
        }
    }
    */
}
