using UnityEngine;
using System.Collections;

// DL - THIS SCRIPT IS NOT USED AS OF RIGHT NOW and its easy to confuse it with VMover, which is being used. 
// So, I am commenting it out. 

/*
public class VertexMover : MonoBehaviour {
	
	private PressureDeformer Deformer;
	private int Vertex;
	
	private Vector3 InitialPosition;
	private Vector3 Normal;
	private float Pressure;
	
	private Vector3 MaxPosition;
	private Vector3 CurrentPosition;
	
	public bool Pressing;
	
	public float SpringbackFraction, FinishPressureThreshold;

	void Start () {
		
	}
	
	public void Assign (PressureDeformer myDeformer, int myVertex, Vector3 origionalVertex, 
		Vector3 aNormal, float aSpringbackFraction, float aFinishPressureThreshold) {

		Deformer = myDeformer;
		Vertex = myVertex;
		
		Pressure = 0;
		Pressing = true;
		
		InitialPosition =  origionalVertex;
		Normal = aNormal;
		
		SpringbackFraction = aSpringbackFraction; 				// origionally this was 0.2 but its useful to tune it
		FinishPressureThreshold = aFinishPressureThreshold;		// origionally this was 0.1 but its useful to tune it
		
		// calculate the position of maximum deformation right now based on the origional vertex
		Ray maxRay = new Ray(InitialPosition, -Normal);
		MaxPosition = maxRay.GetPoint(Deformer.DeformationLimit);
		
		// current position should be the origional vertex
		CurrentPosition = InitialPosition;

        // this turns the script on in the gameobject; the prototype does not have this script enabled.
        enabled = true;
	}
	
	
	public void NotePressure (float aPressure) {

		// Mark this vertex mover as active so it will not spring back and vanish.
		Pressing = true;

		// DEV - adding the pressures up, seems wierd.
		// Pressure = Pressure + aPressure;
		
		// We consider the largest pressure thrown at this vertex.
		if (aPressure > Pressure)
			Pressure = aPressure;

		// put a limit on the presure
		// consider never throwing pressure to this vertexMover in the first place?
		// if (Pressure > Deformer.MaxPressure) Pressure = Deformer.MaxPressure;

	}

	
	void Update () {

  
            // DEV - RENDER THE NORMAL AS WE SEE IT
            // Vector3 longnormal = Normal * 3;
            // Debug.DrawRay(transform.position, longnormal, Color.red);

            // DEV - move the vertex to its max and see what happens.
            // CurrentPosition = MaxPosition;
            // Deformer.Vertices[Vertex] = CurrentPosition;

            // move the vertex according to our pressure
            float lerpFraction = Pressure / Deformer.MaxPressure;
            // float lerpFraction = Pressure;

            if (lerpFraction > 1) lerpFraction = 1;
            if (lerpFraction < 0) lerpFraction = 0;

            CurrentPosition = Vector3.Lerp(InitialPosition, MaxPosition, lerpFraction);

            // the Pressure Deformer actually does the moving based on what this Vertex Mover says.
            Deformer.MoveVertex(Vertex, CurrentPosition);

            // for DEV - take a look at the maximum position
            // Deformer.MoveVertex(Vertex, MaxPosition);

            if (Pressing == false)
            {
 
                // Slowly drop the pressure so the mesh springs back; use Time.deltaTime for framerate independance
                Pressure = Pressure - (Pressure * SpringbackFraction * Time.deltaTime);
                //Debug.Log(Time.deltaTime);
                // Cleanup and finish if the pressure has dropped below a threshold value
                if (Pressure <= FinishPressureThreshold) RemoveVertexMover();
            }



            // This will be reset to 'true' as soon as it gets pressed again
            // otherwise it will remain 'false' and facilitate clean-up.
            Pressing = false;


	}



    // called when the vertex mover is not visible on the heirarchy i.e. the lung difficulty was changed.
    void OnDisable()
    {
        RemoveVertexMover();
    }
	

	public void RemoveVertexMover() {
        Debug.Log("removing!!!");
        Deformer.MoveVertex(Vertex, InitialPosition);
		Deformer.ForgetVertexMover(Vertex);
		Destroy(this.gameObject);
	}
	
	
}

*/
