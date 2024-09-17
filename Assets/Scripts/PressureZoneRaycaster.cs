using UnityEngine;
using System.Collections;

// DL - COMMENTING IT OUT FOR NOW BECAUSE ITS NOT BEING USED and its confusing - were using another implementation currently.
/*

public class PressureZoneRaycaster : MonoBehaviour {

	public GameObject Hub;
	public GameObject Tip;
	//private float ScaleFactor;
	public float MaxDistance;
	public LayerMask TargetMask;
	
	
	private Mesh MeshSpread;
	private Vector3[] Vertices;
	
	private int[] SelectedIndices;
	
	//public float testPressure;
	        /*
	void Start () {

		//MeshSpread = Tip.GetComponent<MeshFilter> ().mesh;
		//BuildRaycastPatternFromOptimizedMesh();

	}
	        */
		/*
	private void BuildRaycastPatternFromOptimizedMesh () {

		// this builds up a list of vertices in the mesh assuming every vertex in the mesh is useful
		
		Vertices = MeshSpread.vertices;
		SelectedIndices = new int[Vertices.Length];

		for (int i = 0; i < Vertices.Length; i ++) {
			SelectedIndices [i] = i;
		}


	}
	        */

		/*	
	private void BuildRaycastPatternFromaMeshSphere () {

		// this builds up a list of vertices in the mesh that have unique locations
		
		Vertices = MeshSpread.vertices;
		SelectedIndices = new int[Vertices.Length];
		
		ArrayList notedVertices = new ArrayList ();
		int j = 0;
		for (int i = 0; i < Vertices.Length; i ++) {
		
			Vector3 test = Vertices [i];
			if (notedVertices.Contains (test)) {
			} else {
				
				// only consider locations IN FRONT of the needle, which is aligned along the Z axis 
				if (Tip.transform.TransformPoint (test).z < Tip.transform.position.z + 0.01F) {
					notedVertices.Add (test);
					SelectedIndices [j] = i;
					j++;
				}
			}
		}
		
		System.Array.Resize (ref SelectedIndices, j);  

	}
        */	
	
	//public float GlancingBlowOffsetFraction;
			/*
	// called from an UPDATE script
	public void PressAgainst (PressureDeformer aDeformableThing, float aPressure)
	{

		//Debug.Log("RIGHt");
		
		// This passes the pressure through, multiplied by a fraction based on direction
		
		RaycastHit hit;
		
		//aPressure = testPressure; // this is the arduino pressure
		
		
		Vector3 Centerline = (Tip.transform.position - Hub.transform.position).normalized;
		
		
		for (int i = 0; i < SelectedIndices.Length; i++) {
			
			
			Vector3 direction = Tip.transform.TransformPoint (Vertices [SelectedIndices [i]]) - Hub.transform.position;
			//direction = direction.normalized * 45;
			
			Ray pressureRay = new Ray (Hub.transform.position, direction);
			//Debug.DrawRay(Hub.transform.position, direction, Color.grey);
			
			float PressureOffsetFraction = Mathf.Abs(Vector3.Dot(Centerline, direction.normalized));
			//float PressureOffsetFraction = (Vector3.Dot(Centerline, direction.normalized));
			
			// PressureOffsetFraction = Mathf.InverseLerp(0.93F, 1, PressureOffsetFraction); // from CVA
			
			PressureOffsetFraction = Mathf.InverseLerp(GlancingBlowOffsetFraction, 1, PressureOffsetFraction);
			//Debug.Log(PressureOffsetFraction);
			
			
			if (Physics.Raycast (pressureRay, out hit, MaxDistance, TargetMask)) {
		
				if (hit.collider.gameObject == aDeformableThing.gameObject) {
					//aPressure = Mathf.Lerp(aPressure, 0, hit.distance / MaxDistance);
					
					aDeformableThing.PressOn(hit, pressureRay, (aPressure * PressureOffsetFraction));
				}
				
			}


		}
		

	}

	
	
}

		*/	