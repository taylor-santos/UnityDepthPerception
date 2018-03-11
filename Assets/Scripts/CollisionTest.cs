using UnityEngine;
using System.Collections;

public class CollisionTest : MonoBehaviour {
	private GameObject clone;
	private Rigidbody cloneRB;
	private Collider cloneCol;
	private Color randCol;
	// Use this for initialization
	void Start () {
		randCol = Random.ColorHSV();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnCollisionStay(Collision collision) {
		Debug.Log(collision.transform.gameObject.name, gameObject);
        foreach (ContactPoint contact in collision.contacts) {
            Debug.DrawRay(contact.point, collision.impulse, randCol, 1);
        }
    }
}
