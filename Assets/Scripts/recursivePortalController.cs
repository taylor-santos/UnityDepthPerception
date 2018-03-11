using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class recursivePortalController : MonoBehaviour {
	public GameObject OtherPortal;
	public GameObject Player;
	public uint depth;
	public int OtherPortalLayer;
	private List<Camera> cameras;
	private MeshRenderer MR;
	private PortalCameraController prevCamController;
	// Use this for initialization
	void Start () {
		cameras = new List<Camera>();
		cameras.Add(Player.GetComponent<Camera>());
		MR = GetComponent<MeshRenderer>();
		MR.material = new Material(Shader.Find("Custom/ScreenSpace"));
	}
	
	// Update is called once per frame
	void LateUpdate () {
		for (int i = 1; i <= depth + 1; ++i){
			if (cameras.Count < i+1){
				
				GameObject newCameraObject = new GameObject(name + " Camera " + i);
				Camera newCamera = newCameraObject.AddComponent<Camera>();
				newCamera.CopyFrom(cameras[i-1]);
				newCamera.cullingMask &= ~(1 << OtherPortalLayer);
				newCamera.depth = cameras[i-1].depth - 1;
				cameras.Add(newCamera);
				PortalCameraController camController = newCameraObject.AddComponent<PortalCameraController>();
				camController.Portal = OtherPortal;
				camController.renderer = MR;
				if (prevCamController){
					prevCamController.prevCamera = camController;
				}else{
					PreRenderSetMaterial setMat = Player.AddComponent<PreRenderSetMaterial>();
					setMat.MR = MR;
					setMat.camController = camController;
				}
				prevCamController = camController;
			}
			CenterOn(cameras[i].transform, cameras[i-1].transform);
			PortalTransform(cameras[i].transform, transform, OtherPortal.transform);
			cameras[i].transform.parent = OtherPortal.transform;
		}
	}

	void OnGUI() {
		//Graphics.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texture);
	}

	void CenterOn(Transform obj, Transform target){
		obj.parent = target;
		obj.localPosition = Vector3.zero;
		obj.localScale = Vector3.one;
		obj.localRotation = Quaternion.identity; 
		obj.parent = null;
	}

	void PortalTransform(Transform obj, Transform entry, Transform exit){
		Vector3 scale = obj.localScale;
		obj.parent = entry;
		Vector3 pos = obj.localPosition;
		Quaternion rot = obj.localRotation;
		obj.parent = exit;
		obj.localPosition = pos;
		obj.localRotation = rot;
		obj.parent = null;
		obj.localScale = scale;
	}

	Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign,float clipPlaneOffset)
    {        
        Vector3 offsetPos = pos + normal * clipPlaneOffset;
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 cpos = m.MultiplyPoint(offsetPos);
        Vector3 cnormal = m.MultiplyVector(normal).normalized * Mathf.Sign(sideSign);
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
