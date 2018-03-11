using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ForcedPerspective : MonoBehaviour {
	public Texture2D crosshairTexture; 
	public bool lockCrosshair = true;
	public float scale;
	public float maxEdgeLength = 0.1f;
	private Rect position; 
	private GameObject heldObject;
	//private Mesh objMesh;
	private int objLayer;
	private RigidbodyConstraints rbConst;
	//private Vector3 heldSeparation;
	//private Vector3 oldPlayerPos;
	//private float yRot;
	private Vector3 objScale;
	private Vector3 objPos;
	private Vector3 objUp;
	private Vector3 objHitPoint;
	public List<Vector3> vertices;
	//public List<int> triangles;
	private List<Vector3> normals;
	private Quaternion objRot;
	private bool canPlace = false;

	// Use this for initialization
	void Start () {
		if (lockCrosshair)
			Cursor.lockState = CursorLockMode.Locked; 
	   	Cursor.visible = true; 
	}

	void LateUpdate()
	{
		if (Input.GetMouseButtonDown(0))
		{
			if (heldObject == null)
			{
				RaycastHit hit;
				Vector3 start = transform.position;
				Vector3 dir = transform.forward.normalized;
				Quaternion rot = Quaternion.identity;
				float dist = 0;
				float portalScale = 1;
				while (Physics.Raycast(start, dir, out hit, Mathf.Infinity)){
					dist += hit.distance / dir.magnitude;
					if (hit.transform.tag == "Portal"){
						GameObject portal = hit.transform.gameObject;
						PortalRTController3 portalComponent = portal.GetComponent<PortalRTController3>();
						GameObject other = portalComponent.OtherPortal;
						rot = Quaternion.Inverse(portal.transform.rotation) * rot;
						rot = Quaternion.AngleAxis(180, Vector3.up) * rot;
						rot = other.transform.rotation * rot;
						start = other.transform.TransformPoint(Quaternion.AngleAxis(180, Vector3.up) * portal.transform.InverseTransformPoint(hit.point));
						dir = other.transform.TransformVector(Quaternion.AngleAxis(180, Vector3.up) * portal.transform.InverseTransformVector(dir));
						portalScale *= other.transform.localScale.magnitude / portal.transform.localScale.magnitude;
						start += dir * 0.01f;
					}else{
						GameObject obj = hit.collider.gameObject;
						while (obj.transform.parent != null){
							obj = obj.transform.parent.gameObject;
						}
						Rigidbody rb = obj.GetComponent<Rigidbody>();
						if (rb != null){
							heldObject = obj;
							rb.useGravity = false;
							rbConst = rb.constraints;
							rb.constraints = RigidbodyConstraints.FreezeAll;
							foreach(Collider coll in obj.GetComponents<Collider>())
							{
								//coll.enabled = false;
								coll.isTrigger = true;
							}
							/*
							MeshFilter mf = heldObject.GetComponent<MeshFilter>();
							objMesh = mf.mesh;
							vertices = new List<Vector3>(objMesh.vertices);
							normals = new List<Vector3>(objMesh.normals);
							triangles = new List<int>(objMesh.triangles);
							*/
							objLayer = heldObject.layer;
							heldObject.layer = LayerMask.NameToLayer("Ignore Raycast");
							objUp = heldObject.transform.InverseTransformDirection(Vector3.up);
							objHitPoint = heldObject.transform.InverseTransformPoint(hit.point);
							Vector3 oldPos = hit.point;
							float scale = dist * portalScale;
							heldObject.transform.localScale /= scale;
							Vector3 newPos = transform.position + transform.forward - rot * heldObject.transform.TransformVector(objHitPoint);
							heldObject.transform.position = newPos;
							heldObject.transform.rotation = rot * heldObject.transform.rotation;
							objPos = transform.InverseTransformPoint(heldObject.transform.position);
							objScale = heldObject.transform.localScale;
							objRot = Quaternion.Inverse(transform.rotation) * heldObject.transform.rotation;

							subDivideMesh(heldObject.GetComponent<MeshFilter>().mesh, maxEdgeLength);
							GetChildrenMeshes(heldObject.transform, heldObject, ref vertices, ref normals);
						}
						break;
					}
				}
			}else if (canPlace){
				foreach(Collider coll in heldObject.GetComponents<Collider>())
				{
					coll.enabled = true;
					coll.isTrigger = false;
				}
				Rigidbody rb = heldObject.GetComponent<Rigidbody>();
				rb.constraints = rbConst;
				rb.useGravity = true;
				heldObject.layer = objLayer;
				heldObject = null;
				canPlace = false;
			}
		}
		if (heldObject != null)
		{
			heldObject.transform.position = transform.TransformPoint(objPos);
			heldObject.transform.localScale = objScale;
			canPlace = false;

			float bestDist = Mathf.Infinity;
			float bestScale = 0;
			Quaternion startingRot = transform.rotation * objRot;
			heldObject.transform.rotation = startingRot;
			startingRot = Quaternion.FromToRotation(heldObject.transform.TransformDirection(objUp), Vector3.up) * startingRot;
			heldObject.transform.rotation = startingRot;
			heldObject.transform.position -= (heldObject.transform.TransformPoint(objHitPoint) - transform.position - transform.forward);
			Quaternion bestRotation = startingRot;
			bool foundBest = false;
			List<Vector3[]> bestLines = new List<Vector3[]>();
			Vector3 bestSep = Vector3.zero;
			Vector3 bestPos = heldObject.transform.position;
			for(int i=0; i < vertices.Count; ++i){
				Vector3 vert = vertices[i];
				Vector3 norm = heldObject.transform.TransformDirection(normals[i]);
				Vector3 start = heldObject.transform.TransformPoint(vert);
				Vector3 dir = (start - transform.position).normalized;
				Vector3 sep = heldObject.transform.TransformVector(vert);
				float portalScale = 1;
				List<Vector3[]> lines = new List<Vector3[]>();
				if (Vector3.Dot(norm, dir) > 0){
					float currDist = 0;
					Quaternion currRot = startingRot;
					currDist = (start - transform.position).magnitude;
					float currCenterDist = (heldObject.transform.position - transform.position).magnitude;
					float startDist = currDist;
					Vector3 currNorm = norm;
					RaycastHit hit;
					Vector3 prevPos = heldObject.transform.position;
					while (Physics.Raycast(start, dir, out hit, Mathf.Infinity)){
						Debug.DrawLine(start, hit.point, new Color(1,1,1,0.1f));
						Transform hitTransform = hit.transform;
						Vector3 hitPoint = hit.point;
						currDist += hit.distance / dir.magnitude;
						float currScale = currDist/startDist;
						Vector3 currPos = hitPoint - sep * currScale;
						currCenterDist += (currPos - prevPos).magnitude / dir.magnitude;
						prevPos = currPos;
						lines.Add(new Vector3[2]{start, hitPoint});
						if (hitTransform.tag == "Portal"){
							//Debug.DrawLine(start, hit.point, new Color(1,1,1,0.1f));
							GameObject portal = hitTransform.gameObject;
							PortalRTController portalComponent = portal.GetComponent<PortalRTController>();
							GameObject other = portalComponent.OtherPortal;
							start = other.transform.TransformPoint(Quaternion.AngleAxis(180, Vector3.up) * portal.transform.InverseTransformPoint(hit.point));
							dir = other.transform.TransformVector(Quaternion.AngleAxis(180, Vector3.up) * portal.transform.InverseTransformVector(dir));
							prevPos = other.transform.TransformPoint(Quaternion.AngleAxis(180, Vector3.up) * portal.transform.InverseTransformPoint(prevPos));
							currNorm = other.transform.TransformDirection(Quaternion.AngleAxis(180, Vector3.up) * portal.transform.InverseTransformDirection(currNorm));
							sep = other.transform.TransformVector(Quaternion.AngleAxis(180, Vector3.up) * portal.transform.InverseTransformVector(sep));
							currRot = Quaternion.Inverse(hitTransform.rotation) * currRot;
							currRot = Quaternion.AngleAxis(180, Vector3.up) * currRot;
							currRot = other.transform.rotation * currRot;
							portalScale *= other.transform.localScale.magnitude / portal.transform.localScale.magnitude;
							start += dir * 0.01f;
							currDist += dir.magnitude * 0.01f;
						}else if (Vector3.Dot(hit.normal, currNorm) < 0){
							if (currCenterDist < bestDist){
								bestDist = currCenterDist;
								bestScale = currDist/startDist * portalScale;
								bestLines = lines;
								bestSep = sep;
								bestPos = currPos;
								bestRotation = currRot;
								foundBest = true;
							}
							
							//foreach(Vector3[] line in lines){
							//	Debug.DrawLine(line[0], line[1], new Color(1,1,1,0.1f));
							//}
							
							//Debug.DrawRay(hit.point, hit.normal/10, new Color(1,0,0,0.2f));
							//Debug.DrawRay(hit.point, currNorm/10, new Color(0,1f,1,0.5f));
							break;
						}else{
							/*
							foreach(Vector3[] line in lines){
								Debug.DrawLine(line[0], line[1], new Color(1,0,0,0.2f));
							}
							Debug.DrawRay(hit.point, hit.normal/10, new Color(1,0,0,0.2f));
							Debug.DrawRay(hit.point, currNorm/10, new Color(0,1f,1,0.5f));
							*/
							break;
						}
					}
					//Debug.DrawRay(transform.position, (heldObject.transform.TransformPoint(vert) - transform.position).normalized * currDist, new Color(0,0,1,0.05f));
				}
			}
			heldObject.transform.rotation = bestRotation;
			if (foundBest){
				
				foreach(Vector3[] line in bestLines){
					Debug.DrawLine(line[0], line[1], Color.green);
				}
				
				heldObject.transform.position = bestPos;
				heldObject.transform.localScale = objScale * bestScale;
				canPlace = true;
			}
		}
	}

	List<Vector3> subDivideMesh(Mesh mesh, float maxLength)
	{
		DCEL newDCEL = new DCEL(mesh);
		List<Face> facesToSubdivide = new List<Face>(newDCEL.faces);
		Color[] edgeColors = new Color[6]{Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.magenta};
		while(facesToSubdivide.Count > 0){
			Face f = facesToSubdivide[facesToSubdivide.Count-1];
			facesToSubdivide.RemoveAt(facesToSubdivide.Count-1);
			HalfEdge currEdge = f.edge;
			HalfEdge startEdge = currEdge;
			int edgeCount = 0;
			do{
				edgeCount++;
				currEdge = currEdge.next;
			}while(currEdge != f.edge);
			/*
			if (edgeCount > 3)
			{
				do{
					//Debug.DrawLine(currEdge.vertex.vert, currEdge.next.vertex.vert, Color.black, 10);
					currEdge = currEdge.next;
				}while(currEdge != f.edge);
				while(currEdge.vertex.newVertex == 0){
					currEdge = currEdge.next;
					if (currEdge == f.edge){
						Debug.Log("ERROR");
						break;
					}
				}
				Vector3 offset = new Vector3(0, 0f, 0);
				Vector3[] verts = new Vector3[3]{currEdge.vertex.vert, currEdge.next.vertex.vert, currEdge.next.next.vertex.vert};
				Vector3 avg = verts[0]+verts[1]+verts[2];
				avg /= 3;
				verts[0] += (avg - verts[0]) * 0.1f;
				verts[1] += (avg - verts[1]) * 0.1f;
				verts[2] += (avg - verts[2]) * 0.1f;
				//Debug.DrawLine(verts[0], verts[1], Color.red, 10);
				//Debug.DrawLine(verts[1], verts[2], Color.green, 10);
				//Debug.DrawLine(verts[2], verts[0], Color.blue, 10);
			}else{
				*/
				int edgeIndex = 0;
				bool noSubDivision = true;
				do{
					float len = (currEdge.prev().vertex.vert - currEdge.vertex.vert).magnitude;
					/*
					Vector3[] verts = new Vector3[3]{currEdge.vertex.vert, currEdge.next.vertex.vert, currEdge.next.next.vertex.vert};
					Vector3 avg = verts[0]+verts[1]+verts[2];
					avg /= 3;
					verts[0] += (avg - verts[0]) * 0.1f;
					verts[1] += (avg - verts[1]) * 0.1f;
					verts[2] += (avg - verts[2]) * 0.1f;
					Debug.DrawLine(verts[0], verts[1], Color.red, 10);
					Debug.DrawLine(verts[1], verts[2], Color.green, 10);
					Debug.DrawLine(verts[2], verts[0], Color.blue, 10);
					*/
					
					if (len > maxLength){
						noSubDivision = false;
						Vertex newVert = new Vertex(currEdge.prev().vertex.vert/2 + currEdge.vertex.vert/2);
						newVert.newVertex = 2;
						HalfEdge newEdge = new HalfEdge(currEdge.vertex, currEdge.face);
						HalfEdge oppositeEdge = currEdge.opposite;

						currEdge.vertex = newVert;
						newEdge.next = currEdge.next;
						currEdge.next = newEdge;
						
						newVert.edge = newEdge;
						
						if (oppositeEdge != null)
						{
							HalfEdge newOppositeEdge = new HalfEdge(currEdge.prev().vertex, oppositeEdge.face);
							oppositeEdge.vertex = newVert;
							newOppositeEdge.next = oppositeEdge.next;
							oppositeEdge.next = newOppositeEdge;
							newOppositeEdge.opposite = currEdge;
							currEdge.opposite = newOppositeEdge;
							newEdge.opposite = oppositeEdge;
							oppositeEdge.opposite = newEdge;
						}
						currEdge = currEdge.next;
					}else{
						//Debug.DrawLine(currEdge.prev().vertex.vert, currEdge.vertex.vert, Color.white, 10);
					}
					
					currEdge = currEdge.next;		
					edgeIndex++;		
				}while (currEdge != startEdge);

				Vector3 avg = Vector3.zero;
				HalfEdge currForAvg = currEdge;
				int avgCount = 0;
				do{
					avg += currForAvg.vertex.vert;
					currForAvg = currForAvg.next;
					avgCount++;
				}while (currForAvg != currEdge);
				avg /= avgCount;

				edgeIndex = 0;
				do{
					Vector3 v1 = currEdge.prev().vertex.vert;
					Vector3 v2 = currEdge.vertex.vert;
					v1 += (avg - v1)*0.1f;
					v2 += (avg - v2)*0.1f;
					Debug.DrawLine(v1, v2, edgeColors[edgeIndex++%6], 10);
					currEdge = currEdge.next;
				}while (currEdge != startEdge);

				if (noSubDivision == false){
					//facesToSubdivide.Add(currEdge.face);
				}

				if (noSubDivision){
					/*
					currEdge = f.edge;
					Vector3 avg = Vector3.zero;
					HalfEdge currForAvg = currEdge;
					int avgCount = 0;
					do{
						avg += currForAvg.vertex.vert;
						currForAvg = currForAvg.next;
						avgCount++;
					}while (currForAvg != currEdge);
					avg /= avgCount;
					edgeIndex = 0;
					do{
						Vector3 edgeStart = currEdge.prev().vertex.vert;
						Vector3 edgeEnd = currEdge.vertex.vert;
						Vector3 newEdgeStart = edgeStart + (avg - edgeStart) * 0.1f;
						Vector3 newEdgeEnd = edgeEnd + (avg - edgeEnd) * 0.1f;
						Debug.DrawRay(newEdgeStart, (newEdgeEnd - newEdgeStart) * 0.95f, edgeColors[edgeIndex%6], 10);
						if (currEdge.prev().vertex.edge == currEdge)
							Debug.DrawLine(currEdge.prev().vertex.vert, newEdgeStart, edgeColors[edgeIndex%6]*0.8f, 10);
						currEdge = currEdge.next;		
						edgeIndex++;
					}while (currEdge != f.edge);
					*/
//				}
			}
		}
		return new List<Vector3>();
	}

	void GetChildrenMeshes(Transform parent, GameObject obj, ref List<Vector3> vertices, ref List<Vector3> normals)
	{
		MeshFilter MF = obj.GetComponent<MeshFilter>();
		vertices = new List<Vector3>();
		normals = new List<Vector3>();
		if (MF != null){
			Mesh mesh = MF.mesh;
			foreach (Vector3 vert in mesh.vertices){
				vertices.Add( parent.InverseTransformPoint( obj.transform.TransformPoint( vert ) ) );
			}
			foreach (Vector3 norm in mesh.vertices){
				normals.Add( parent.InverseTransformDirection( obj.transform.TransformDirection( norm ) ) );
			}
		}
		foreach(Transform child in obj.transform){
			List<Vector3> newVerts = new List<Vector3>();
			List<Vector3> newNorms = new List<Vector3>();
			GetChildrenMeshes(parent, child.gameObject, ref newVerts, ref newNorms);
			vertices.AddRange(newVerts);
			normals.AddRange(newNorms);
		}
	}

	void OnGUI() 
	{
		position = new Rect((Screen.width - crosshairTexture.width*scale) / 2, (Screen.height - crosshairTexture.height*scale) /2, crosshairTexture.width*scale, crosshairTexture.height*scale);
	    GUI.DrawTexture(position, crosshairTexture); 
	}

	public class HalfEdge
	{
		public HalfEdge opposite;
		public HalfEdge next;
		public Vertex vertex;
		public Face face;
		public HalfEdge(Vertex v, Face f){
			vertex = v;
			face = f;
		}
		public HalfEdge prev(){
			HalfEdge curr = this;
			while (curr.next != this){
				curr = curr.next;
			}
			return curr;
		}
	}

	public class Vertex
	{
		public Vector3 vert;
		public HalfEdge edge;
		public int newVertex = 0;
		public Vertex(Vector3 v){
			vert = v;
		}
	}

	public class Face
	{
		public HalfEdge edge;
		public Face(){
		}
	}

	public class DCEL
	{
		public List<Face> faces;
		public DCEL(Mesh mesh){
			faces = new List<Face>();
			Vector3[] mesh_vertices = mesh.vertices;
			int[] mesh_triangles = mesh.triangles;
			Dictionary<int, Dictionary<int, HalfEdge>> Edges = new Dictionary<int, Dictionary<int, HalfEdge>>();
			Vertex[] verts = new Vertex[mesh_vertices.Length];
			
			List<Vector3> vert_list = new List<Vector3>(mesh_vertices);
			Dictionary<Vector3, int> vert_dict = new Dictionary<Vector3, int>(mesh_vertices.Length);
			HashSet<Vector3> vert_hash = new HashSet<Vector3>();
			
			int[] duplicate_indices = new int[mesh_vertices.Length];
			for (int i=0; i<duplicate_indices.Length; ++i){
				duplicate_indices[i] = -1;
			}
			for (int i=0; i<mesh_vertices.Length; ++i){
				if (vert_hash.Add(mesh_vertices[i])){	//New Vert
					vert_dict.Add(mesh_vertices[i], i);
				}
			}
			for (int i=0; i<mesh_triangles.Length; ++i){
				mesh_triangles[i] = vert_dict[mesh_vertices[mesh_triangles[i]]];
			}
			
			/*
			for (int i=0; i<mesh_triangles.Length; ++i){
				mesh_triangles[i] = duplicate_indices[mesh_triangles[i]];
			}
			*/
			
			for (int i=0; i<mesh_triangles.Length/3; ++i){
				Face F = new Face();
				faces.Add(F);
				HalfEdge[] faceEdges = new HalfEdge[3];
				for (int j=0; j<3; ++j){
					int u = 3*i + j;
					int v = 3*i + (j + 1) % 3;
					if (verts[mesh_triangles[v]] == null){
						verts[mesh_triangles[v]] = new Vertex(mesh_vertices[mesh_triangles[v]]);
					}
					if (!Edges.ContainsKey(mesh_triangles[u])){
						Edges[mesh_triangles[u]] = new Dictionary<int, HalfEdge>();
					}	
					Edges[mesh_triangles[u]][mesh_triangles[v]] = new HalfEdge(verts[mesh_triangles[v]], F);
					faceEdges[j] = Edges[mesh_triangles[u]][mesh_triangles[v]];
				}
				F.edge = faceEdges[0];
				for (int j=0; j<3; ++j){
					faceEdges[j].next = faceEdges[(j + 1) % 3];
					faceEdges[j].vertex.edge = faceEdges[(j + 1) % 3];
					int u = 3*i + j;
					int v = 3*i + (j + 1) % 3;
					if (Edges.ContainsKey(mesh_triangles[v]) && Edges[mesh_triangles[v]].ContainsKey(mesh_triangles[u])){
						Edges[mesh_triangles[u]][mesh_triangles[v]].opposite = Edges[mesh_triangles[v]][mesh_triangles[u]];
						Edges[mesh_triangles[v]][mesh_triangles[u]].opposite = Edges[mesh_triangles[u]][mesh_triangles[v]];
					} 
				}
			}
		}
	}

	/*
	public class Vertex
	{
		private Vector3 vert;
		private List<Edge> ownedEdges;

		public Vector3 getVertex(){
			return vert;
		}
		public bool isEqual(Vector3 v){
			return v == vert;
		}
		public void AddEdge(Edge e){
			ownedEdges.Add(e);
		}
		public Vertex(Vector3 vert_in){
			vert = vert_in;
			ownedEdges = new List<Edge>();
		}
	}

	public class Edge
	{
		private Vertex A, B;
		private List<Edge> EdgesWithSharedVertexA;
		private List<Edge> EdgesWithSharedVertexB;

		public Vertex[] getVertices(){
			return new Vertex[2]{A,B};
		}
		public bool	isEqual(Vertex a, Vertex b){
			return ((a==A && b==B) || (a==B && b==A));
		}
		public Edge(Vertex A_in, Vertex B_in){
			A = A_in;
			A.AddEdge(this);
			B = B_in;
			B.AddEdge(this);
			EdgesWithSharedVertexA = new List<Edge>();
			EdgesWithSharedVertexB = new List<Edge>();
		}
	}

	public class Triangle
	{
		Edge A, B, C;
		List<Triangle> TrianglesWithSharedEdgeA;
		List<Triangle> TrianglesWithSharedEdgeB;
		List<Triangle> TrianglesWithSharedEdgeC;
	}
	*/
}