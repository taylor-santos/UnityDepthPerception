using UnityEngine;
using System.Collections;

public class Clipper : MonoBehaviour 
 {
   	public Vector3 pos;
   	public Vector3 normal;
   	public Matrix4x4 obliqueProjection;
   	public Matrix4x4 newProj;
   	public Matrix4x4 testProj;
    
    void Start()
    {
    	obliqueProjection = GetComponent<Camera>().projectionMatrix;
    }

    void OnPreRender() 
    {
        
        Vector3 normalnorm = transform.rotation * normal.normalized;  
        Vector4 C = new Vector4(normalnorm.x,normalnorm.y,normalnorm.z,Vector3.Dot(-normalnorm,transform.InverseTransformPoint(pos)));
        C = CameraSpacePlane(pos, normal, 1);
        newProj = obliqueProjection;
        Vector4 Q = new Vector4(Mathf.Sign(C.x), Mathf.Sign(C.y), 1, 1);
        //float a = Vector4.Dot(2 * newProj.GetRow(3), Q)/Vector4.Dot(C,Q);
        //newProj.SetRow(2, a*C - newProj.GetRow(3));
        newProj.SetRow(2, (-2 * Q.z)/Vector4.Dot(C,Q)*C + new Vector4(0,0,1,0));

        GetComponent<Camera>().projectionMatrix = newProj;
        testProj = GetComponent<Camera>().CalculateObliqueMatrix(C);
    }

    Vector4 CameraSpacePlane(Vector3 pos, Vector3 normal, float sideSign)
    {
    	Camera cam = GetComponent<Camera>();
        Vector3 offsetPos = pos + normal * 0.07f;
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 cpos = m.MultiplyPoint(offsetPos);
        Vector3 point = m.inverse.MultiplyPoint(new Vector3(0.0f, 0.0f, 0.0f));
        cpos -= new Vector3(0.0f, point.y, 0.0f);
        Vector3 cnormal = m.MultiplyVector( normal ).normalized * sideSign;
        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos,cnormal));
    }  
}