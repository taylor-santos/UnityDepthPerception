using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObliqueFrustum : MonoBehaviour {
	public Vector3 point;
	public Vector3 normal;
	public float ReflectClipPlaneOffset = 0;
	private Camera cam;
	private Matrix4x4 start_projection;
	// Use this for initialization
	void Start () {
		cam = GetComponent<Camera>();
		start_projection = cam.projectionMatrix;
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 pos = transform.position;
		Vector4 clipPlane = CameraSpacePlane(cam, point, normal.normalized, 1.0f, ReflectClipPlaneOffset);
		Matrix4x4 projection = start_projection;
		projection = CalculateObliqueMatrix(projection, clipPlane, -1);
		cam.projectionMatrix = projection;
	}

	Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign,float clipPlaneOffset)
    {        
        Vector3 offsetPos = pos + normal * clipPlaneOffset;
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 cpos = m.MultiplyPoint(offsetPos);
        Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
    }

	Matrix4x4 CalculateObliqueMatrix(Matrix4x4 projection, Vector4 clipPlane, float sideSign)
    {
        Vector4 q = projection.inverse * new Vector4(
            Mathf.Sign(clipPlane.x),
            Mathf.Sign(clipPlane.y),
            1.0f,
            1.0f
        );
        Vector4 c = clipPlane * (2.0F / (Vector4.Dot(clipPlane, q)));
        // third row = clip plane - fourth row
        projection[2] = c.x + Mathf.Sign(sideSign)*projection[3];
        projection[6] = c.y + Mathf.Sign(sideSign) * projection[7];
        projection[10] = c.z + Mathf.Sign(sideSign) * projection[11];
        projection[14] = c.w + Mathf.Sign(sideSign) * projection[15];
        return projection;
    }
}
