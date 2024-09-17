using UnityEngine;
using System.Collections;

public class VMover : MonoBehaviour
{
    /*
    private VPD Deformer;
    private int Vertex;

    private Vector3 InitialPosition;
    private Vector3 Normal;
    private float Pressure;

    private float needleBackwallDist;
    private float backwallDistance;
    private float USBackwallDist;
    private Vector3 backwallDirection;
    private Vector3 backwallNormal;

    private Vector3 MaxPosition;
    private Vector3 CurrentPosition;

    private float dotProduct;

    private Vector3 backwallVertex;

    // public float SpringbackFraction;

    private bool initialized;
    private bool needleInSkin = false;

    private BackwallDetector MyBackwallDetector;

    public static bool Collapsed;

    private float lerpfraction;

    private bool usingNeedleBackwall = false;

    private bool printCheck = true;

    private GameObject needleMonitor;
    


      */
    public VPD Deformer;
    public int Vertex;

    public Vector3 InitialPosition;
    public Vector3 Normal;
    public float Pressure;

    public float needleBackwallDist;
    public float backwallDistance;
    public float USBackwallDist;
    public Vector3 backwallDirection;
    public Vector3 backwallNormal;

    public Vector3 MaxPosition;
    public Vector3 CurrentPosition;

    public float dotProduct;

    public Vector3 backwallVertex;

    // public float SpringbackFraction;

    public bool initialized;
    public bool needleInSkin = false;

    public BackwallDetector MyBackwallDetector;

    public static bool Collapsed;

    public float lerpfraction;

    public bool usingNeedleBackwall = false;

    public bool printCheck = true;

    public GameObject needleMonitor;

        
    void Start()
    {
        // needleMonitor = GameObject.Find("CVA Needle");
    }

    // Update is called once per frame
    void Update()
    {
        

        if (initialized)
        {

            //needleBackwallDist = Vector3.Distance(InitialPosition, MyBackwallDetector.NeedleAverageBackwallVertex);
            USBackwallDist = Vector3.Distance(InitialPosition, MyBackwallDetector.USAverageBackwallVertex);
            /*
            if (needleInSkin && (needleBackwallDist <= USBackwallDist))
            {
                backwallVertex = MyBackwallDetector.NeedleAverageBackwallVertex;
                backwallNormal = MyBackwallDetector.NeedleAverageBackwallNormal;
                usingNeedleBackwall = true;
            }
            else
            {
                backwallVertex = MyBackwallDetector.USAverageBackwallVertex;
                backwallNormal = MyBackwallDetector.USAverageBackwallNormal;
                usingNeedleBackwall = false;
            }*/

            backwallVertex = MyBackwallDetector.USAverageBackwallVertex;
            backwallNormal = MyBackwallDetector.USAverageBackwallNormal;
            usingNeedleBackwall = false;
            usingNeedleBackwall = false;

            //  backwallVertex = MyBackwallDetector.NeedleAverageBackwallVertex;
            //  backwallNormal = MyBackwallDetector.NeedleAverageBackwallNormal;

            backwallDirection = backwallVertex - InitialPosition;

            backwallDistance = Vector3.Distance(InitialPosition, backwallVertex);  //this is the float of the distance between the initial position and the backwall

            dotProduct = Vector3.Dot(backwallNormal, backwallDirection);

            MaxPosition = InitialPosition + ((backwallNormal * dotProduct) * 1.0F);


            if (Mathf.Abs(needleBackwallDist - USBackwallDist) < 7)
            {
                Pressure = Deformer.NeedlePressure + Deformer.USPressure;
            }

            else if (usingNeedleBackwall)
            {
                Pressure = Deformer.NeedlePressure;
            }
            else
            {
                Pressure = Deformer.USPressure;
            }


            //Pressure = Deformer.NeedlePressure + Deformer.USPressure;

            float crushFraction = Mathf.Clamp01(Pressure / Deformer.MaxPressure);

          
            float distanceFraction1 = Mathf.InverseLerp(Deformer.PressureDistance, 0, backwallDistance);

            lerpfraction = crushFraction * (distanceFraction1) * 4;

            if (lerpfraction > 0.99f) lerpfraction = 0.99f; // DL 12/17/21: If you crush the vein completely flat, you get a dark shadow instead of no vein.
            // this is because the raycasts for the top and bottom surfaces of the vein get swapped. Remove this limit and try it for yourself.

            // if the tourniquet is applied, the veins can get squished down by about half their diameter as per Dr. Acar 1/21/22.
            // so they can never be squished completely flat by the US probe, but you can still use US probe pressure to differentiate between artery and vein with tourniquet.
            if (Tourniquet_Pressure_Monitor.ME.pressureTooHigh || Tourniquet_Pressure_Monitor.ME.pressureCorrect)
            {
                lerpfraction = lerpfraction * 0.5f;
            }



            CurrentPosition = Vector3.Lerp(InitialPosition, MaxPosition, lerpfraction);

            if (backwallDistance < Deformer.PressureRange)
            {
                Deformer.MoveVertex(Vertex, CurrentPosition);
            }
            else
            {
                Deformer.MoveVertex(Vertex, InitialPosition);
            }

            if (usingNeedleBackwall && MyBackwallDetector.NeedleContact == false)
            {
                Deformer.MoveVertex(Vertex, InitialPosition);
            }
            else if (!usingNeedleBackwall && MyBackwallDetector.USContact == false)
            {
                Deformer.MoveVertex(Vertex, InitialPosition);
            }
            /*
            if (Vertex == 333)// && printCheck)
            {
                printThings();
            }
            */
        }
    }

    void printThings()
    {
        print("vertex: " + Vertex + ", NeedleBackwall: " + usingNeedleBackwall + ", nb distance: " + needleBackwallDist + ", us distance: " + USBackwallDist + ", Pressure: " + Pressure);
        printCheck = false;
    }

    void LateUpdate()
    {
        //      needleInSkin = needleMonitor.GetComponent<NeedleVeinCollapser>().needleSpeedMonitor.Inside();


        // After fine tuning, lerpfraction on collapse is slightly larger than 1.4
        // this is about when the vein completely disappears in ultrasound.
        if (lerpfraction > 1.4F)
        {
            Collapsed = true;
        }
    }

    
    public void Assign(VPD myDeformer, int myVertex, Vector3 originalVertex, Vector3 aNormal)
    {

        Deformer = myDeformer;
        Vertex = myVertex;

        Pressure = 0;

        InitialPosition = originalVertex;
        Normal = aNormal;

        MaxPosition = backwallVertex;

        MyBackwallDetector = Deformer.gameObject.GetComponent<BackwallDetector>();

        initialized = true;
    }


    public void RemoveVMover()
    {
        // Debug.Log("removing!!!");
        Deformer.MoveVertex(Vertex, InitialPosition);
        Destroy(this.gameObject);
    }

}
