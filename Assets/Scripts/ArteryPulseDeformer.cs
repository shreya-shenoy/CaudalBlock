using UnityEngine;
using System.Collections;

public class ArteryPulseDeformer : MonoBehaviour {

    public float pulseRate;  //in beats per minute - default should be 60 for now
    private float timeTracker;
    public GameObject Artery_1; //GameObject containing the smallest artery meshes
    public GameObject Artery_2; //contains the second smallest artery meshes
    public GameObject Artery_3; //contains the second largest artery meshes
    public GameObject Artery_4; //contains the largest artery meshes
    

	// Use this for initialization
	void Start () {
        timeTracker = 0;
        pulseRate = 60;
	}
	
	// Update is called once per frame
	void Update () {
        timeTracker = timeTracker + Time.smoothDeltaTime;
	    if(timeTracker >= ((60/pulseRate)/4))
        {
            ArteryToggler();
            timeTracker = 0;
        }
	}

    private void ArteryToggler()
    {
        if (Artery_4.activeInHierarchy)
        {
            Artery_3.SetActive(true);
            Artery_4.SetActive(false);
        }
        else if(Artery_3.activeInHierarchy){
            Artery_2.SetActive(true);
            Artery_3.SetActive(false);
        }
        else if (Artery_2.activeInHierarchy)
        {
            Artery_1.SetActive(true);
            Artery_2.SetActive(false);
        }
        else if (Artery_1.activeInHierarchy)
        {
            Artery_4.SetActive(true);
            Artery_1.SetActive(false);
        }
        else
        {
            print("ERROR: No Artery Meshes Active");
        }
    }
}
