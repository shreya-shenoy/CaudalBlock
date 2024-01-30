using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// DL - THIS IS NOT BEING USED as of right now and its easy to get it confused with what is being used, so I'm commenting it out.
/*
	
	// - HOW THIS WORKS
	// - use the hierarchy to your advantage.
	// - monobehaviour the VertexMovers.
	// - dont call them with new(); instantiate them as children of this transform.
	// - place them at the vertex position (why not?) 
	// - consider aligning them to their normals (why not?)
	// - throw them alot of NotePressures(), perhaps many times per frame.
	// - allow them to Update() their positions.
	// - on a LateUpdate, walk down the heirarchy and match the verticies to the VertexMovers positions.
	// - when a VertexMover is done, they delete themselves from the heirarchy.
	// - periodically, update the mesh collider that the ultrasound sees.
	// - it has to be a different mesh collider than this mesh for simple stability.
	

public class PressureDeformer : MonoBehaviour
{
	
	public float DeformationLimit;
	public float MaxPressure;
	
	public float SpringbackFraction;  // controls how fast the vertex movers return to origional position
	public float ZeroPressureThreshold; // pressures lower than this cause the vertex movers to delete themselves
	
	private Mesh MyMesh;
	private MeshCollider MyMeshCollider;
	private Vector3[] Vertices;
	private int[] Triangles;
	private Vector3[] Normals;
	private Vector3[] OriginalVertices;
	private VertexMover[] VertexMovers;
	private int VertexMoverCount;
	
	public VertexMover MyProtoMover; // public reference facilitates instantiation
	
	private bool HasChanged = false;

	void Awake ()
	{
		// This script is designed to work with gameobjects that have a mesh and a mesh collider.
		// The mesh collider of this mesh should be layered and tagged "Compressible"
		// This mesh collider is never changed, and is used for pressure detection only.
		MyMesh = GetComponent<MeshFilter> ().mesh;
		
		// The ultrasound sees an entirely different mesh collider, layered and tagged "Vein" or something.
		// The mesh collider is a separate object.  include a copy of the mesh as a child gameobject named "Ultrasoundable"
		// remove all components except its MeshCollider - no MeshFilter, MeshRenderer, or scrips.
		// This mesh collider is alligned to this gameObject's mesh vertices periodically at great expense.
		MyMeshCollider = transform.Find ("Ultrasoundable").GetComponent<MeshCollider> (); 
		if (MyMeshCollider == null) 
			Debug.Log("A 'PressureDeformer' needs an 'Ultrasoundable' child.");
		
	}
	
	void Start ()
	{
		
		MyMesh.RecalculateNormals ();
		
		Vertices = MyMesh.vertices;
		Normals = MyMesh.normals;
		Triangles = MyMesh.triangles;
		
		// LOCAL VERTEX ALIGNMENT - COMMENT OUT AFTER DEV
		// vertex and normals are for the prefab, and have no alignment to any particular GameObject.
		// since its useful to render vertex movers and their parts during development,
		// allign vertex and normals to this transform.
		// NOTE - move this transform and this is all worthless.
		// for (int i = 0; i < Vertices.Length; i++) { //each (Vector3 vs in Vertices) {
		//	Vertices[i] = transform.TransformPoint(Vertices[i]);
		//	Normals[i] = transform.TransformDirection(Normals[i]);
		//	Normals[i] = Vector3.Normalize(Normals[i]);
		// }

		OriginalVertices = Vertices.Clone () as Vector3[];
		
		int n = Vertices.Length;
		VertexMovers = new VertexMover[n];

		VertexMoverCount = 0;

	}
	
	// called from another script tied to UPDATE()
	public void PressOn (RaycastHit hit, Ray pressureRay, float aPressure)
	{

		int hitTriangle = hit.triangleIndex;
		int v1 = Triangles [hitTriangle * 3 + 0];
		int v2 = Triangles [hitTriangle * 3 + 1]; 
		int v3 = Triangles [hitTriangle * 3 + 2];
		
		// int[] HitVertices = new int [3] {v1, v2, v3};
		
		Vector3 n1 = Normals[v1];
		Vector3 n2 = Normals[v2];
		Vector3 n3 = Normals[v3];

		Vector3 baryCenter = hit.barycentricCoordinate;

		Vector3 interpolatedNormal = n1 * baryCenter.x + n2 * baryCenter.y + n3 * baryCenter.z;
        interpolatedNormal = interpolatedNormal.normalized;

		// Transform hitTransform = hit.collider.transform;
		interpolatedNormal = transform.TransformDirection(interpolatedNormal);
		
		float p1 = aPressure;// * baryCenter.x;
		float p2 = aPressure;// * baryCenter.y;
		float p3 = aPressure;// * baryCenter.z;

		float nf = Mathf.Abs(Vector3.Dot (interpolatedNormal, pressureRay.direction));
		nf = (nf * 0.5f) + (nf * nf * 0.1f);
		

		if (VertexMovers [v1] == null)
			VertexMovers [v1] = NewVertexMover (v1);
		if (VertexMovers [v2] == null)
			VertexMovers [v2] = NewVertexMover (v2);
		if (VertexMovers [v3] == null)
			VertexMovers [v3] = NewVertexMover (v3);

		VertexMovers [v1].NotePressure (p1 * nf);
		VertexMovers [v2].NotePressure (p2 * nf);
		VertexMovers [v3].NotePressure (p3 * nf);
		
	}
	
	
	private VertexMover NewVertexMover (int v)
	{
		
		Quaternion newRot = Quaternion.FromToRotation (Vector3.forward, Normals [v]);

		VertexMover newMover = Instantiate (MyProtoMover, OriginalVertices [v], newRot) as VertexMover;
		newMover.transform.parent = transform;
		newMover.Assign (this, v, OriginalVertices [v], Normals [v], SpringbackFraction, ZeroPressureThreshold);
		
		return newMover;
	
	}
	
	
	// Update is called once per frame
	//void Update ()
	//{
		// for DEV:  RENDER THE NORMALS in two different ways	
		// for (int i = 0; i < Vertices.Length; i++) {
		// RENDER THE NORMALS - Note that all this important stuff we use doen't move with transform changes.
		// Debug.DrawRay(Vertices[i], Normals[i] * 3, Color.blue);
		// RENDER THE NORMALS - Properly, tolerates transform changes.
		// This works only if TransformPoint() and TransformDirection() have not been used in Start()
		// Performance hit is unclear.
		// Debug.DrawRay(transform.TransformPoint(Vertices[i]), transform.TransformDirection(Normals[i]) * 3, Color.blue);
		// }
	//}
	
	public void MoveVertex (int i, Vector3 v)
	{
		Vertices [i] = v;
		HasChanged = true;
	}
	
	public void ForgetVertexMover (int i)
	{
		
		// make sure that the vertex is back in its unmoved position
		Vertices [i] = OriginalVertices [i];
		HasChanged = true;
		
	}
	
	void LateUpdate ()
	{

		if (HasChanged) {
		
			// UPLOAD NEW MESH
			// The entire set of vertices must be uploaded all at once.
			// there is no way to see changes to a single vertex.  The code will compile, but it wont work.
			MyMesh.vertices = Vertices;
			//MyMesh.RecalculateNormals ();
			
			// UPLOAD A NEW COLLIDER
			// all at once - if the frame is appropriate.  
			// performance-wise, this is the worst thing you can possibly do... good luck.
			if (FrameConductor.DeformableAnatomyFrame) {
				MyMeshCollider.sharedMesh = null;
				MyMeshCollider.sharedMesh = MyMesh;
				HasChanged = false;
			}
			
		}
			
		
	}
	
	public void Reset () {
	
		// any vertex movers will reset their vertices and delete themselves.
		BroadcastMessage("RemoveVertexMover", SendMessageOptions.DontRequireReceiver);
		
	}
	
	
}
*/
