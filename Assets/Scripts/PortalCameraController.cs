using UnityEngine;
using System.Collections;

public class PortalCameraController : MonoBehaviour {
    public GameObject Portal;
    public MeshRenderer renderer;
    public PortalCameraController prevCamera;
    public RenderTexture texture;
    public Texture2D altTexture;
    public bool Draw;
    private Camera camera;
    private Matrix4x4 defaultProjection;

    void Start () {
        camera = GetComponent<Camera>();
        defaultProjection = camera.projectionMatrix;
        texture = new RenderTexture(Screen.width, Screen.height, 32);
        texture.name = name + " RenderTexture";
        texture.antiAliasing = 1;
        texture.filterMode = FilterMode.Point;
        camera.targetTexture = texture;
    }

    void OnPreRender() {
        if (prevCamera)
            renderer.material.SetTexture("_MainTex", prevCamera.texture);
        else
            renderer.material.SetTexture("_MainTex", altTexture);
    }

    void LateUpdate () {
        float radAngle = camera.fieldOfView * Mathf.Deg2Rad;
        float radHFOV = 2 * Mathf.Atan(Mathf.Tan(radAngle / 2) * camera.aspect);
        float cosTheta = Vector3.Dot(transform.forward, (transform.position-Portal.transform.position).normalized);
        if (Draw)
            Debug.Log((Mathf.Rad2Deg*Mathf.Acos(cosTheta) + Mathf.Rad2Deg*radHFOV/2));
        if (Mathf.Rad2Deg*Mathf.Acos(cosTheta) + Mathf.Rad2Deg*radHFOV/2 > 90f){
            camera.enabled = true;
            float sign = -Vector3.Dot(transform.localPosition, Vector3.forward);
            Vector3 pos_offset = Portal.transform.position - Portal.transform.forward * Mathf.Sign(sign) * 0.01f;
            Vector4 clipPlane = CameraSpacePlane(camera, pos_offset, Portal.transform.forward, sign, 0);
            Matrix4x4 projection = defaultProjection;
            projection = CalculateObliqueMatrix(projection, clipPlane, -1);
            camera.projectionMatrix = projection;
            if (Draw)
                DrawFrustum(camera);
        }else{
            camera.enabled = false;
        }
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

    void DrawFrustum ( Camera cam ) {
        Vector3[] nearCorners = new Vector3[4]; //Approx'd nearplane corners
        Vector3[] farCorners = new Vector3[4]; //Approx'd farplane corners
        Plane[] camPlanes = GeometryUtility.CalculateFrustumPlanes( cam ); //get planes from matrix
        Plane temp = camPlanes[1]; camPlanes[1] = camPlanes[2]; camPlanes[2] = temp; //swap [1] and [2] so the order is better for the loop
 
        for ( int i = 0; i < 4; i++ ) {
            nearCorners[i] = Plane3Intersect( camPlanes[4], camPlanes[i], camPlanes[( i + 1 ) % 4] ); //near corners on the created projection matrix
            farCorners[i] = Plane3Intersect( camPlanes[5], camPlanes[i], camPlanes[( i + 1 ) % 4] ); //far corners on the created projection matrix
        }
 
        for ( int i = 0; i < 4; i++ ) {
            Debug.DrawLine( nearCorners[i], nearCorners[( i + 1 ) % 4], Color.red, Time.deltaTime, true ); //near corners on the created projection matrix
            Debug.DrawLine( farCorners[i], farCorners[( i + 1 ) % 4], Color.blue, Time.deltaTime, true ); //far corners on the created projection matrix
            Debug.DrawLine( nearCorners[i], farCorners[i], Color.green, Time.deltaTime, true ); //sides of the created projection matrix
        }
    }
 
    Vector3 Plane3Intersect ( Plane p1, Plane p2, Plane p3 ) { //get the intersection point of 3 planes
        return ( ( -p1.distance * Vector3.Cross( p2.normal, p3.normal ) ) +
                ( -p2.distance * Vector3.Cross( p3.normal, p1.normal ) ) +
                ( -p3.distance * Vector3.Cross( p1.normal, p2.normal ) ) ) /
            ( Vector3.Dot( p1.normal, Vector3.Cross( p2.normal, p3.normal ) ) );
    }
}
