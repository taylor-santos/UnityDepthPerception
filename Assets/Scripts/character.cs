using UnityEngine;
using System;
using System.Collections;

public class character : MonoBehaviour {
	public GameObject head;
	public GameObject body;
	public float sensitivity = 150;
	public float speed = 10;
	public Vector2 cameraXbounds = new Vector2(-50,85);
	public Vector2 cameraYbounds = new Vector2(-50, 50);
	public float cameraRotationX;
	public float cameraRotationY;
	private Rigidbody bodyRB;
	// Use this for initialization
	void Start () {
		bodyRB = body.transform.GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
		body.transform.rotation = Quaternion.Slerp(body.transform.rotation,Quaternion.Euler(0,body.transform.eulerAngles.y,0),Mathf.Pow(Time.deltaTime,1f/2));
		cameraRotationX -= Input.GetAxis("Mouse Y")*Time.deltaTime*sensitivity;
		//cameraRotationY += Input.GetAxis("Mouse X")*Time.fixedDeltaTime*sensitivity;
		cameraRotationX = Mathf.Clamp(cameraRotationX,cameraXbounds.x,cameraXbounds.y);
		//cameraRotationY = Mathf.Clamp(cameraRotationY,cameraYbounds.x,cameraYbounds.y);
		head.transform.localEulerAngles = new Vector3(cameraRotationX,0,0);
		//body.transform.localEulerAngles = new Vector3(0,cameraRotationY,0);
		body.transform.Rotate(new Vector3(0,Input.GetAxis("Mouse X")*Time.deltaTime*sensitivity,0));

		float x = Input.GetAxis("Horizontal") * speed * body.transform.lossyScale.magnitude;
		float z = Input.GetAxis("Vertical") * speed * body.transform.lossyScale.magnitude;

		bodyRB.velocity = body.transform.rotation * new Vector3(x,bodyRB.velocity.y,z);
		if (Input.GetKeyDown (KeyCode.Space))
		{
			Collider[] cols = Physics.OverlapSphere(body.transform.TransformPoint (new Vector3(0f, -0.66f, 0f)), 0.35f * Mathf.Max(body.transform.localScale.x, body.transform.localScale.y, body.transform.localScale.z));
			string names = "";
			bool jump = false;
			Collider[] bodyCols = body.GetComponentsInChildren<Collider>();
			foreach(Collider col in cols)
			{
				if (Array.IndexOf(bodyCols, col) == -1 && !col.isTrigger)
				{
					names += col.gameObject.name + ", ";
					jump = true;
				}
			}
			if (jump)
			{
				Debug.Log(names);
				bodyRB.velocity += body.transform.up * 8;
			}
		}
	}
}
