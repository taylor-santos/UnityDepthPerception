using UnityEngine;
using System.Collections;

public class speedometer : MonoBehaviour {
	public float accel;
	public float vel;
	public Vector3 angularVelocity;
	public float angVel;
	private Rigidbody RB;
	private Vector3 prevVel;
	// Use this for initialization
	void Start () {
		RB = GetComponent<Rigidbody>();
		prevVel = RB.velocity;
	}
	
	// Update is called once per frame
	void Update () {
		if (RB != null)
		{
			vel = RB.velocity.magnitude;
			angularVelocity = RB.angularVelocity;
			angVel = angularVelocity.magnitude;
		}
	}

	void FixedUpdate()
	{
		if (RB != null)
		{
			accel = ((prevVel - RB.velocity)/Time.fixedDeltaTime).magnitude;
			prevVel = RB.velocity;
		}
	}
}
