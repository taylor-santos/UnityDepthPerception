using UnityEngine;
using System.Collections;

public class VoxelCollider : MonoBehaviour {
	public float boxWidth;
	// Use this for initialization
	void Start () {
		if (boxWidth > 0)
		{
			MeshFilter MF = gameObject.GetComponent<MeshFilter>();
			Mesh mesh = MF.mesh;
			Vector3[] verts = mesh.vertices;
			int[] tris = mesh.triangles;

			Vector3 minVert = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
			Vector3 maxVert = new Vector3(-Mathf.Infinity, -Mathf.Infinity, -Mathf.Infinity);

			for (int i=0; i<tris.Length; i++)
			{
				minVert = new Vector3(Mathf.Min(minVert.x, verts[tris[i]].x), Mathf.Min(minVert.y, verts[tris[i]].y), Mathf.Min(minVert.z, verts[tris[i]].z));
				maxVert = new Vector3(Mathf.Max(maxVert.x, verts[tris[i]].x), Mathf.Max(maxVert.y, verts[tris[i]].y), Mathf.Max(maxVert.z, verts[tris[i]].z));
			}

			int gridX = (int)Mathf.Ceil(maxVert.x/boxWidth);
			int gridY = (int)Mathf.Ceil(maxVert.y/boxWidth);
			int gridZ = (int)Mathf.Ceil(maxVert.z/boxWidth);

			int gridMinX = (int)Mathf.Floor(minVert.x/boxWidth);
			int gridMinY = (int)Mathf.Floor(minVert.y/boxWidth);
			int gridMinZ = (int)Mathf.Floor(minVert.z/boxWidth);

			Debug.DrawLine(new Vector3(gridMinX,gridMinY,gridMinZ)*boxWidth,new Vector3(gridX,gridY,gridZ)*boxWidth, Color.black, Time.deltaTime);

			bool[,,] boxes = new bool[gridX-gridMinX, gridY-gridMinY, gridZ-gridMinZ];

			for (int i=0; i<tris.Length; i+=3)
			{
				/*
				Vector3 A = transform.TransformPoint(verts[tris[i]]);
				Vector3 B = transform.TransformPoint(verts[tris[i+1]]);
				Vector3 C = transform.TransformPoint(verts[tris[i+2]]);
				*/
				Vector3 A = verts[tris[i]];
				Vector3 B = verts[tris[i+1]];
				Vector3 C = verts[tris[i+2]];

				Vector3 AABBMin = new Vector3(Mathf.Min(A.x,B.x,C.x), Mathf.Min(A.y,B.y,C.y), Mathf.Min(A.z,B.z,C.z));
				Vector3 AABBMax = new Vector3(Mathf.Max(A.x,B.x,C.x), Mathf.Max(A.y,B.y,C.y), Mathf.Max(A.z,B.z,C.z));

				AABBMin	= new Vector3(Mathf.Floor(AABBMin.x/boxWidth), Mathf.Floor(AABBMin.y/boxWidth), Mathf.Floor(AABBMin.z/boxWidth));
				AABBMax	= new Vector3(Mathf.Ceil(AABBMax.x/boxWidth), Mathf.Ceil(AABBMax.y/boxWidth), Mathf.Ceil(AABBMax.z/boxWidth));
				//Color randColor = new Color(Random.value, Random.value, Random.value);
				//DrawBox(AABBMin,AABBMax,randColor);
				//Debug.DrawLine(A,B,randColor,Time.deltaTime);
				//Debug.DrawLine(C,B,randColor,Time.deltaTime);
				//Debug.DrawLine(A,C,randColor,Time.deltaTime);

				for (int x = (int)AABBMin.x; x<AABBMax.x; x++)
				{
					for (int y = (int)AABBMin.y; y<AABBMax.y; y++)
					{
						for (int z = (int)AABBMin.z; z<AABBMax.z; z++)
						{
							if (boxes[x-gridMinX, y-gridMinY, z-gridMinZ] == false)
							{
								if (triAABBIntersection(A,B,C, new Vector3(x,y,z)*boxWidth + Vector3.one*boxWidth/2, Vector3.one*boxWidth))
								{
									//DrawBox(new Vector3(x,y,z)*boxWidth, new Vector3(x,y,z)*boxWidth + Vector3.one * boxWidth, randColor);
									GameObject newCube = new GameObject();
									newCube.transform.parent = transform;
									newCube.transform.localPosition =  new Vector3(x,y,z)*boxWidth + Vector3.one*boxWidth/2;
									newCube.transform.localScale = Vector3.one * boxWidth;
									newCube.transform.localRotation = Quaternion.identity;
									newCube.name = "(" + x + ", " + y + ", " + z + ")";

									boxes[x-gridMinX, y-gridMinY, z-gridMinZ] = true;
								}
							}
						}	
					}
				}
				
			}
		}
	}

	bool triAABBIntersection(Vector3 A, Vector3 B, Vector3 C, Vector3 boxCenter, Vector3 boxExtents)
	{
		/*
		Vector3[] tri = new Vector3[3]{A,B,C};
		Vector3 boxMin = boxCenter - boxExtents/2;
		for (int i=0; i<3; ++i)
		{
			if (tri[i].x - boxMin.x <= boxExtents.x && tri[i].x - boxMin.x > 0 &&
				tri[i].y - boxMin.y <= boxExtents.y && tri[i].y - boxMin.y > 0 &&
				tri[i].z - boxMin.z <= boxExtents.z && tri[i].z - boxMin.z > 0)
			{
				return true;
			}
		}
			
		return false;
		*/
		Vector3 boxStart = boxCenter - boxExtents/2;
		Vector3 boxEnd = boxCenter + boxExtents/2;
		Vector3[] boxNormals = new Vector3[3]{Vector3.right, Vector3.up, Vector3.forward};
		Vector3[] triangle = new Vector3[3]{A, B, C};
		Vector3[] boxVertices = new Vector3[8]{
			boxCenter - boxExtents/2, 
			boxCenter - new Vector3(boxExtents.x/2,boxExtents.y/2,-boxExtents.z/2),
			boxCenter - new Vector3(boxExtents.x/2,-boxExtents.y/2,boxExtents.z/2),
			boxCenter - new Vector3(boxExtents.x/2,-boxExtents.y/2,-boxExtents.z/2),
			boxCenter - new Vector3(-boxExtents.x/2,boxExtents.y/2,boxExtents.z/2),
			boxCenter - new Vector3(-boxExtents.x/2,boxExtents.y/2,-boxExtents.z/2),
			boxCenter - new Vector3(-boxExtents.x/2,-boxExtents.y/2,boxExtents.z/2),
			boxCenter - new Vector3(-boxExtents.x/2,-boxExtents.y/2,-boxExtents.z/2)};
		float triangleMin, triangleMax, boxMin, boxMax;

		for (int i=0; i<3; ++i)
		{
			Vector3 n = boxNormals[i];
			
			Project(triangle, n, out triangleMin, out triangleMax);
			if (triangleMax < boxStart[i] || triangleMin > boxEnd[i])
			{
				return false;
			}
		}

		Vector3 triangleNormal = Vector3.Cross(B-A,C-B).normalized;
		float triangleOffset = Vector3.Dot(triangleNormal, A);
		Project(boxVertices, triangleNormal, out boxMin, out boxMax);
		if (boxMax < triangleOffset || boxMin > triangleOffset)
			return false;
		Vector3[] triangleEdges = new Vector3[3]{A-B, B-C, C-A};

		for (int i=0; i<3; ++i)
		{
			for (int j = 0; j<3; ++j)
			{
				Vector3 axis = Vector3.Cross(triangleEdges[i],boxNormals[j]);
				Project(boxVertices, axis, out boxMin, out boxMax);
				Project(triangle, axis, out triangleMin, out triangleMax);
				if (boxMax <= triangleMin || boxMin >= triangleMax)
				{
					return false;
				}
			}
		}
		return true;
	}

	void Project(Vector3[] points, Vector3 axis, out float minVal, out float maxVal)
	{
		minVal = Mathf.Infinity;
		maxVal = -Mathf.Infinity;
		foreach(Vector3 p in points)
		{
			float val = Vector3.Dot(axis,p);
			minVal = Mathf.Min(val,minVal);
			maxVal = Mathf.Max(val,maxVal);
		}
	}

	void DrawBox(Vector3 min, Vector3 max, Color color)
	{
		Debug.DrawRay(min,Vector3.right * (max.x-min.x), color, Time.deltaTime);
		Debug.DrawRay(min,Vector3.up * (max.y-min.y), color, Time.deltaTime);
		Debug.DrawRay(min,Vector3.forward * (max.z-min.z), color, Time.deltaTime);

		Debug.DrawRay(min+Vector3.right*(max.x-min.x), Vector3.up*(max.y-min.y), color, Time.deltaTime);
		Debug.DrawRay(min+Vector3.right*(max.x-min.x), Vector3.forward*(max.z-min.z), color, Time.deltaTime);

		Debug.DrawRay(min+Vector3.forward*(max.z-min.z), Vector3.up*(max.y-min.y), color, Time.deltaTime);
		Debug.DrawRay(min+Vector3.forward*(max.z-min.z), Vector3.right*(max.x-min.x), color, Time.deltaTime);

		Debug.DrawRay(min+Vector3.up*(max.y-min.y), Vector3.forward*(max.z-min.z), color, Time.deltaTime);
		Debug.DrawRay(min+Vector3.up*(max.y-min.y), Vector3.right*(max.x-min.x), color, Time.deltaTime);

		Debug.DrawRay(max,Vector3.right * (min.x-max.x), color, Time.deltaTime);
		Debug.DrawRay(max,Vector3.up * (min.y-max.y), color, Time.deltaTime);
		Debug.DrawRay(max,Vector3.forward * (min.z-max.z), color, Time.deltaTime);
	}
}
