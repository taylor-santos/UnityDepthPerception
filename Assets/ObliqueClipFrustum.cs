using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObliqueClipFrustum : MonoBehaviour {
	public GameObject target;
	public Matrix4x4 cameraMatrix;
	public Vector3 Normal;
	public Vector3 Point;
	public Vector4 clipPlane;
	public bool getVectorsFromTarget = false;
	public bool calculateClipPlane = false;
	public bool calculateMatrix = false;
	private Camera cam;
	// Use this for initialization
	void Start () {
		cam = GetComponent<Camera>();
		cameraMatrix = cam.projectionMatrix;
	}
	
	// Update is called once per frame
	void Update () {
		Debug.DrawLine(transform.TransformPoint(cameraToWorld(cameraMatrix, new Vector4(-1.0f,-1.0f,-1.0f, 1))), transform.TransformPoint(cameraToWorld(cameraMatrix, new Vector4(1.0f,-1.0f,-1.0f, 1))), Color.red);
		Debug.DrawLine(transform.TransformPoint(cameraToWorld(cameraMatrix, new Vector4(1.0f,-1.0f,-1.0f, 1))), transform.TransformPoint(cameraToWorld(cameraMatrix, new Vector4(1.0f,1.0f,-1.0f, 1))), Color.red);
		Debug.DrawLine(transform.TransformPoint(cameraToWorld(cameraMatrix, new Vector4(1.0f,1.0f,-1.0f, 1))), transform.TransformPoint(cameraToWorld(cameraMatrix, new Vector4(-1.0f,1.0f,-1.0f, 1))), Color.red);
		Debug.DrawLine(transform.TransformPoint(cameraToWorld(cameraMatrix, new Vector4(-1.0f,1.0f,-1.0f, 1))), transform.TransformPoint(cameraToWorld(cameraMatrix, new Vector4(-1.0f,-1.0f,-1.0f, 1))), Color.red);
		
		Debug.DrawLine(transform.TransformPoint(cameraToWorld(cameraMatrix, new Vector4(-1.0f,-1.0f,1.0f,1))), transform.TransformPoint(cameraToWorld(cameraMatrix, new Vector4(1.0f,-1.0f,1.0f,1))), Color.red);
		Debug.DrawLine(transform.TransformPoint(cameraToWorld(cameraMatrix, new Vector4(1.0f,-1.0f,1.0f,1))), transform.TransformPoint(cameraToWorld(cameraMatrix, new Vector4(1.0f,1.0f,1.0f,1))), Color.red);
		Debug.DrawLine(transform.TransformPoint(cameraToWorld(cameraMatrix, new Vector4(1.0f,1.0f,1.0f,1))), transform.TransformPoint(cameraToWorld(cameraMatrix, new Vector4(-1.0f,1.0f,1.0f,1))), Color.red);
		Debug.DrawLine(transform.TransformPoint(cameraToWorld(cameraMatrix, new Vector4(-1.0f,1.0f,1.0f,1))), transform.TransformPoint(cameraToWorld(cameraMatrix, new Vector4(-1.0f,-1.0f,1.0f,1))), Color.red);

		Debug.DrawLine(transform.TransformPoint(cameraToWorld(cameraMatrix, new Vector4(-1.0f,-1.0f,-1.0f,1))), transform.TransformPoint(cameraToWorld(cameraMatrix, new Vector4(-1.0f,-1.0f,1.0f,1))), Color.green);
		Debug.DrawLine(transform.TransformPoint(cameraToWorld(cameraMatrix, new Vector4(1.0f,-1.0f,-1.0f,1))), transform.TransformPoint(cameraToWorld(cameraMatrix, new Vector4(1.0f,-1.0f,1.0f,1))), Color.green);
		Debug.DrawLine(transform.TransformPoint(cameraToWorld(cameraMatrix, new Vector4(1.0f,1.0f,-1.0f,1))), transform.TransformPoint(cameraToWorld(cameraMatrix, new Vector4(1.0f,1.0f,1.0f,1))), Color.green);
		Debug.DrawLine(transform.TransformPoint(cameraToWorld(cameraMatrix, new Vector4(-1.0f,1.0f,-1.0f,1))), transform.TransformPoint(cameraToWorld(cameraMatrix, new Vector4(-1.0f,1.0f,1.0f,1))), Color.green);

		if (getVectorsFromTarget){
			Normal = target.transform.forward;
			Point = target.transform.position;
			Debug.DrawRay(Point, Normal, Color.cyan);
		}
		if (calculateClipPlane){
			//Normal = Normal.normalized;
			//clipPlane = new Vector4(Normal.x, Normal.y, Normal.z, Vector3.Dot(Normal, Point));
			//Debug.DrawRay(transform.TransformPoint(Point), transform.TransformDirection(Normal), Color.white);
			clipPlane = CameraSpacePlane(cam, Point, Normal, Vector3.Dot(transform.InverseTransformPoint(Point), transform.InverseTransformDirection(Normal)));
		}
		if (calculateMatrix){
			cam.ResetProjectionMatrix();
			cameraMatrix = cam.projectionMatrix;
			Vector4 C = CameraSpacePlane( cam, Point, Normal, 1.0f );
			CalculateObliqueMatrix (ref cameraMatrix, C);
			cam.projectionMatrix = cameraMatrix;
			//cameraMatrix = cam.CalculateObliqueMatrix(clipPlane);
			/*
			
			Vector4 Cprime = cameraMatrix.inverse.transpose * C;
			Vector4 Qprime = new Vector4(Mathf.Sign(Cprime.x), Mathf.Sign(Cprime.y), Mathf.Sign(Cprime.z), 1);
			Vector4 Q = cameraMatrix.inverse * Qprime;
			Vector4 m4 = cameraMatrix.GetRow(3);
			cameraMatrix.SetRow(2, (2*Vector4.Dot(m4, Q))/Vector4.Dot(C, Q)*C - m4);
			*/
			cam.projectionMatrix = cameraMatrix;
    	}
	}

	static Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
	{
		Vector3 offsetPos = pos + normal * 0.07f;
		Matrix4x4 m = cam.worldToCameraMatrix;
		Vector3 cpos = m.MultiplyPoint(offsetPos);
		Vector3 point = m.inverse.MultiplyPoint(new Vector3(0.0f, 0.0f, 0.0f));
		cpos -= new Vector3(0, point.y, 0);
		Vector3 cnormal = m.MultiplyVector( normal ).normalized * sideSign;
		return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos,cnormal));
	}    
         
	static void CalculateObliqueMatrix(ref Matrix4x4 projection, Vector4 clipPlane)
	{
		Vector4 q = projection.inverse * new Vector4(
			Mathf.Sign(clipPlane.x),
			Mathf.Sign(clipPlane.y),
			1.0f,
			1.0f
		);
		Vector4 c = clipPlane * (2.0F / (Vector4.Dot (clipPlane, q)));
		// third row = clip plane - fourth row
		projection[2] = c.x - projection[3];
		projection[6] = c.y - projection[7];
		projection[10] = c.z - projection[11];
		projection[14] = c.w - projection[15];
	}

	Vector4 cameraToWorld(Matrix4x4 mat, Vector4 vec)
	{
		Vector4 newVec = vec;
		newVec = mat.inverse * newVec;
		vec = new Vector4(newVec.x, newVec.y, newVec.z, 1)/newVec.w;
		vec.z = -vec.z;
		return vec;
	}
}
