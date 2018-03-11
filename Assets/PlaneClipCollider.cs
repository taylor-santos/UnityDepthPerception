using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneClipCollider : MonoBehaviour {
	public Vector3 PlanePosition;
	public Vector3 PlaneNormal;
	public float volume;
	public Vector3 CenterOfMass;
	private GameObject colliderChild;
	private MeshCollider[] MCs;
	private MeshCollider[] capMCs;
	private Mesh[] originalMeshes;
	private float[] meshVolumes;
	private Mesh[] newMeshes;
	private Mesh[] capMeshes;
	private List<List<Vector3>> tris;
	private Rigidbody RB;
	private Vector3 CoM;
	private List<Dictionary<Vector3, List<int>>> vertToTris;
	private bool initialized = false;

	void Start () {
		initialized = true;
		MeshCollider[] originalMCs = GetComponents<MeshCollider>();
		MCs = new MeshCollider[originalMCs.Length];
		colliderChild = new GameObject();
		colliderChild.transform.parent = transform;
		colliderChild.transform.localPosition = Vector3.zero;
		colliderChild.transform.localRotation = Quaternion.identity;
		colliderChild.transform.localScale = Vector3.one;
		colliderChild.name = name + " Clip Collider";
		colliderChild.tag = "Clip Collider";

		originalMeshes = new Mesh[MCs.Length];
		meshVolumes = new float[MCs.Length];
		newMeshes = new Mesh[MCs.Length];
		capMeshes = new Mesh[MCs.Length];
		capMCs = new MeshCollider[MCs.Length];
		tris = new List<List<Vector3>>();
		for (int i=0; i<MCs.Length; ++i){
			originalMeshes[i] = originalMCs[i].sharedMesh;
			meshVolumes[i] = VolumeOfMesh(originalMeshes[i]);
			MCs[i] = colliderChild.AddComponent<MeshCollider>();
			MCs[i].convex = true;

			originalMCs[i].isTrigger = true;
			newMeshes[i] = new Mesh();
			newMeshes[i].name = name + " Clipped";
			newMeshes[i].MarkDynamic();
			newMeshes[i].vertices = originalMeshes[i].vertices;
			newMeshes[i].triangles = originalMeshes[i].triangles;
			newMeshes[i].normals = originalMeshes[i].normals;
			tris.Add(new List<Vector3>());
			for (int j=0; j<originalMeshes[i].triangles.Length; ++j){
				tris[i].Add(originalMeshes[i].vertices[originalMeshes[i].triangles[j]]);
			}
			capMeshes[i] = new Mesh();
			capMeshes[i].name = name + " Cap";
			capMeshes[i].MarkDynamic();
			capMCs[i] = colliderChild.AddComponent<MeshCollider>();
			capMCs[i].convex = true;
			capMCs[i].sharedMesh = new Mesh();
			MCs[i].sharedMesh = newMeshes[i];
		}
		RB = GetComponent<Rigidbody>();
		Transform RBHolder = transform;
		while (RB == null && RBHolder.parent != null){
			RBHolder = RBHolder.parent;
			RB = RBHolder.GetComponent<Rigidbody>();
		}
		if (RB != null)
			CoM = RB.centerOfMass;
		vertToTris = new List<Dictionary<Vector3, List<int>>>();
		foreach(Mesh mesh in originalMeshes){
			Dictionary<Vector3, List<int>> v2t = new Dictionary<Vector3, List<int>>();
			vertToTris.Add(v2t);
			int[] tris = mesh.triangles;
			Vector3[] verts = mesh.vertices;
			for (int i=0; i<tris.Length; ++i){
				Vector3 vert = verts[tris[i]];
				if (!v2t.ContainsKey(vert)){
					v2t.Add(vert, new List<int>());
				}
				v2t[vert].Add(i);
			}
		}
		GenerateMeshes();
	}

	void FixedUpdate () {
		GenerateMeshes();
	}

	void GenerateMeshes(){
		CenterOfMass = Vector3.zero;
		float totalVol = 0;
		for (int MCindex=0; MCindex<MCs.Length; ++MCindex){
			List<int> newTris = new List<int>();
			List<Vector3> newVerts = new List<Vector3>();
			Dictionary<Vector3, int> vertIndices = new Dictionary<Vector3, int>();
			HashSet<Vector3> badVertices = new HashSet<Vector3>();
			List<Vector3> capVertices = new List<Vector3>();
			Dictionary<Vector3, int> capVertDict = new Dictionary<Vector3, int>();
			List<int> capTris = new List<int>();
			for (int tri=0; tri<originalMeshes[MCindex].triangles.Length/3; ++tri){
				List<int> outsideVerts = new List<int>();
				List<int> insideVerts = new List<int>();
				Color randCol = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
				for (int j=0; j<3; ++j){
					Vector3 vert = tris[MCindex][3*tri+j];
					if (badVertices.Contains(vert)){
						outsideVerts.Add(3*tri+j);
					}else if (Vector3.Dot(PlaneNormal, transform.TransformPoint(vert) - PlanePosition) < 0){
						//Debug.DrawLine(PlanePosition, transform.TransformPoint(vert), Color.red);
						badVertices.Add(vert);
						outsideVerts.Add(3*tri+j);
					}else{
						insideVerts.Add(3*tri+j);
					}
				}
				if (insideVerts.Count == 3){
					for (int j=0; j<3; ++j){
						Vector3 vert = tris[MCindex][3*tri+j];
						if (!vertIndices.ContainsKey(vert)){
							vertIndices.Add(vert, newVerts.Count);
							newVerts.Add(vert);
						}
						newTris.Add(vertIndices[vert]);
					}
				}else if (insideVerts.Count == 2){
					Vector3 A = transform.TransformPoint(tris[MCindex][insideVerts[0]]);
					Vector3 B = transform.TransformPoint(tris[MCindex][outsideVerts[0]]);
					Vector3 C = transform.TransformPoint(tris[MCindex][insideVerts[1]]);

					Vector3 AB = (B-A).normalized;
					Vector3 D = A + AB * Vector3.Dot(-PlaneNormal, A - PlanePosition)/Vector3.Dot(PlaneNormal, AB);
					Vector3 CB = (B-C).normalized;
					Vector3 E = C + CB * Vector3.Dot(-PlaneNormal, C - PlanePosition)/Vector3.Dot(PlaneNormal, CB);

					/*
					Debug.DrawLine(A, D, randCol);
					Debug.DrawLine(A, E, randCol);
					Debug.DrawLine(C, E, randCol);
					Debug.DrawLine(D, E, randCol);
					*/

					A = transform.InverseTransformPoint(A);
					D = transform.InverseTransformPoint(D);
					E = transform.InverseTransformPoint(E);
					C = transform.InverseTransformPoint(C);

					if (!capVertDict.ContainsKey(E)){
						capVertDict.Add(E, capVertices.Count);
						capVertices.Add(E);
					}
					capTris.Add(capVertDict[E]);

					if (!capVertDict.ContainsKey(A)){
						capVertDict.Add(A, capVertices.Count);
						capVertices.Add(A);
					}
					capTris.Add(capVertDict[A]);

					if (!capVertDict.ContainsKey(D)){
						capVertDict.Add(D, capVertices.Count);
						capVertices.Add(D);
					}
					capTris.Add(capVertDict[D]);

					if (!capVertDict.ContainsKey(C)){
						capVertDict.Add(C, capVertices.Count);
						capVertices.Add(C);
					}
					capTris.Add(capVertDict[C]);

					
					capTris.Add(capVertDict[E]);
					capTris.Add(capVertDict[A]);

				}else if (insideVerts.Count == 1){
					Vector3 A = transform.TransformPoint(tris[MCindex][outsideVerts[0]]);
					Vector3 B = transform.TransformPoint(tris[MCindex][insideVerts[0]]);
					Vector3 C = transform.TransformPoint(tris[MCindex][outsideVerts[1]]);

					Vector3 AB = (B-A).normalized;
					Vector3 D = A + AB * Vector3.Dot(-PlaneNormal, A - PlanePosition)/Vector3.Dot(PlaneNormal, AB);
					Vector3 CB = (B-C).normalized;
					Vector3 E = C + CB * Vector3.Dot(-PlaneNormal, C - PlanePosition)/Vector3.Dot(PlaneNormal, CB);

					/*
					Debug.DrawLine(B, E, randCol);
					Debug.DrawLine(E, D, randCol);
					Debug.DrawLine(D, B, randCol);
					*/

					B = transform.InverseTransformPoint(B);
					E = transform.InverseTransformPoint(E);
					D = transform.InverseTransformPoint(D);

					if (!capVertDict.ContainsKey(B)){
						capVertDict.Add(B, capVertices.Count);
						capVertices.Add(B);
					}
					capTris.Add(capVertDict[B]);

					if (!capVertDict.ContainsKey(D)){
						capVertDict.Add(D, capVertices.Count);
						capVertices.Add(D);
					}
					capTris.Add(capVertDict[D]);

					if (!capVertDict.ContainsKey(E)){
						capVertDict.Add(E, capVertices.Count);
						capVertices.Add(E);
					}
					capTris.Add(capVertDict[E]);
				}
			}
			newMeshes[MCindex].Clear();
			newMeshes[MCindex].vertices = newVerts.ToArray();
			newMeshes[MCindex].triangles = newTris.ToArray();
			newMeshes[MCindex].RecalculateBounds();

			capMeshes[MCindex].Clear();
			capMeshes[MCindex].vertices = capVertices.ToArray();
			capMeshes[MCindex].triangles = capTris.ToArray();
			capMeshes[MCindex].RecalculateBounds();

			float v1 = VolumeOfMesh(newMeshes[MCindex]);
			newMeshes[MCindex].name = name + " Clip Collider (" + v1 + ")";
			if (v1 != 0){
				MCs[MCindex].enabled = true;
				MCs[MCindex].convex = false;
				MCs[MCindex].sharedMesh = newMeshes[MCindex];
				try{
					MCs[MCindex].convex = true;
				}
				catch(Exception e){
				    Debug.Log("Error: " + e.Message);
				}
			}else{
				MCs[MCindex].enabled = false;
			}

			float v2 = VolumeOfMesh(capMeshes[MCindex]);
			capMeshes[MCindex].name = name + " Cap (" + v2 + ")";
			if (v2 != 0){
				capMCs[MCindex].enabled = true;
				capMCs[MCindex].convex = false;
				capMCs[MCindex].sharedMesh = capMeshes[MCindex];
				try{
					capMCs[MCindex].convex = true;
				}
				catch(Exception e){
				    Debug.Log("Error: " + e.Message);
				}
			}else{
				capMCs[MCindex].enabled = false;
			}
			CenterOfMass += newMeshes[MCindex].bounds.center*v1 + capMeshes[MCindex].bounds.center*v2;
			totalVol += v1 + v2;
		}

		CenterOfMass /= totalVol;
		volume = totalVol;
		Debug.DrawRay(transform.TransformPoint(CenterOfMass), Vector3.up, Color.white);

		if (RB != null)
			RB.centerOfMass = CoM;

	}

	float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
	{
		/*
		float v321 = p3.x * p2.y * p1.z;
		float v231 = p2.x * p3.y * p1.z;
		float v312 = p3.x * p1.y * p2.z;
		float v132 = p1.x * p3.y * p2.z;
		float v213 = p2.x * p1.y * p3.z;
		float v123 = p1.x * p2.y * p3.z;
		return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
		*/
		return Mathf.Abs(Vector3.Dot(p1, Vector3.Cross(p2, p3)))/6f;
	}

	float VolumeOfMesh(Mesh mesh)
	{
		float volume = 0;
		Vector3[] vertices = mesh.vertices;
		int[] triangles = mesh.triangles;
		Vector3 c = mesh.bounds.center;
		for (int i = 0; i < mesh.triangles.Length; i += 3){
			Vector3 p1 = vertices[triangles[i + 0]] - c;
			Vector3 p2 = vertices[triangles[i + 1]] - c;
			Vector3 p3 = vertices[triangles[i + 2]] - c;
			Debug.DrawRay(transform.TransformPoint(c), transform.TransformVector(p1), Color.red);
			Debug.DrawRay(transform.TransformPoint(c), transform.TransformVector(p2), Color.green);
			Debug.DrawRay(transform.TransformPoint(c), transform.TransformVector(p3), Color.blue);
			volume += SignedVolumeOfTriangle(p1, p2, p3);
		}
		return Mathf.Abs(volume);
	}

	void OnEnable(){
		if (initialized){
			colliderChild.SetActive(true);
			foreach(MeshCollider MC in GetComponents<MeshCollider>()){
				MC.isTrigger = true;
			}
			GenerateMeshes();
		}
	}

	void OnDisable(){
		colliderChild.SetActive(false);
		foreach(MeshCollider MC in GetComponents<MeshCollider>()){
			MC.isTrigger = false;
		}
	}
}
