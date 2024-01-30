using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]


public class DynamicVessel : MonoBehaviour
{
    /*
    This script is built upon "TubeRenderer.cs" written by NYC Unity Developer Ray Nothnagel in 2008. 
    It is free for use and available on the Unify Wiki. 
 
    Additions by Dave Lizdas Sept 2018:
    I've removed the vertex colors and removed this strange part where it renders flat planes if maincamera is far away - don't care about that.
    I've added mesh colliders.
    I've added a way for the mesh to flatten in the down direction, which should suffice for most instances.
    Mad thanks to this Ray Nothnagel guy.
    */


    [SerializeField]
    private float enlargedPercentage = 1.2f;

    [SerializeField]
    private float excessivelyEnlargedPercentage = 1.4f;

    [SerializeField]
    private float slapIncrease = 0.1f;

    [SerializeField]
    private float normalSizePercentage = 1.0f;

    private bool slapIncreaseApplied;

    //public Dynamic_Vessel_Needle_Interaction_Manager DVNIM;
    public GameObject clone;
    [Serializable]
    public class VesselSegment
    {
        public VesselSegment(VesselSegment copy)
        {
            Radius = copy.Radius;
            CenterPoint = copy.CenterPoint;
            Roundness = copy.Roundness;
            transform = copy.transform;
            originalPosition = copy.transform.position;

        }
        public VesselSegment()
        {
            Radius = 1;
            CenterPoint = Vector3.zero;
            Roundness = 1;
            transform = null;
            originalPosition = Vector3.zero;
        }
        public VesselSegment(float radius, Vector3 cp, float roundness, Transform Transform)
        {
            Radius = radius;
            CenterPoint = cp;
            Roundness = roundness;
            transform = Transform;
            originalPosition = Transform.position;
        }
        //[Range(0, 3)]
        public float Radius = 1.0f;

        Vector3 centerPoint = Vector3.zero;
        public Vector3 CenterPoint { get { return centerPoint; } internal set { centerPoint = value; } }

        [Range(0, 1)]
        public float Roundness = 1.0F;

        internal Transform transform;

        VesselSegment NextSegment = null;
        VesselSegment LastSegment = null;

        float pressureIndex;
        float pushIndex;
        float spreadIndex;

        private float ReboundRate = 1.0F;

        internal Vector3 originalPosition = Vector3.zero;

        // Chain Construction
        public VesselSegment ChainNext(VesselSegment next)
        {
            NextSegment = next;
            return this;
        }
        // Chain Construction
        public void ChainLast(VesselSegment last)
        {
            LastSegment = last;
        }

        // Applying Pressure
        public void PressOn(float aPressureIndex, float aPushIndex, float aSpreadIndex)
        {
            Roundness = Mathf.Clamp01(Roundness - aPressureIndex);
            float propagatePressureIndex = Mathf.Clamp01(aPressureIndex * aSpreadIndex);
            if (propagatePressureIndex < 0.975F)
            {
                PropagatePressForward(propagatePressureIndex, aPushIndex, aSpreadIndex);
                PropagatePressBack(propagatePressureIndex, aPushIndex, aSpreadIndex);
            }
        }

        public void PropagatePressForward(float aPressureIndex, float aPushIndex, float aSpreadIndex)
        {
            Roundness = Mathf.Clamp01(Roundness - aPressureIndex);
            float propagatePressureIndex = Mathf.Clamp01(aPressureIndex * aSpreadIndex);

            if (NextSegment != null & (propagatePressureIndex < 0.975F))
            {
                NextSegment.PropagatePressForward(propagatePressureIndex, aPushIndex, aSpreadIndex);
            }
        }

        public void PropagatePressBack(float aPressureIndex, float aPushIndex, float aSpreadIndex)
        {

            Roundness = Mathf.Clamp01(Roundness - aPressureIndex);
            float propagatePressureIndex = Mathf.Clamp01(aPressureIndex * aSpreadIndex);

            if (LastSegment != null & (propagatePressureIndex < 0.975F))
            {
                LastSegment.PropagatePressBack(propagatePressureIndex, aPushIndex, aSpreadIndex);
            }
        }


        public void Rebound()
        {
            float dRoundness = Time.deltaTime * (1 - Roundness) * ReboundRate;

            Roundness = Mathf.Clamp01(Roundness + dRoundness);

            if (NextSegment != null)
            {
                NextSegment.Rebound();
            }
        }
    }

    public int PolygonSideCount = 10;
    public VesselSegment[] Segments;
    public float ReboundFactor;

    [SerializeField] Transform WaypointSet;

    Vector3[] circularSetOfPoints;
    int lastCrossSegments;
    Vector3[] MeshVertices;
    int[] Triangles;

    public float radiusMultiplier;
    float mulxxxt;
    bool pulse;
    public static bool noPulse;

    void Start()
    {
        /*
        if (this.transform.name.Contains("Target")){
            radiusMultiplier = .33f;
        }
        else
        {
            radiusMultiplier = 1f;
        }*/

        pulse = true;

        slapIncreaseApplied = false;

        //DVNIM = gameObject.AddComponent<Dynamic_Vessel_Needle_Interaction_Manager>();
        if (WaypointSet != null)
        {
            if (WaypointSet.childCount > 0)
            {
                Segments = new VesselSegment[WaypointSet.childCount];

                int i = 0;
                foreach (Transform child in WaypointSet)
                {
                    // these do indeed appear in order from the hierarchy. 
                    Segments[i] = new VesselSegment();
                    Segments[i].Radius = child.localScale.x / 2F;
                    Segments[i].CenterPoint = child.position;
                    Segments[i].transform = transform;
                    i++;
                }
            }
        }

        BuildSegments();
        if (WaypointSet != null)
            Destroy(WaypointSet.gameObject);
        //DVNIM.InitializeManager(this);
    }


    public void BuildSegments()
    {
        // we must Chain the Segments together.
        VesselSegment lastSegment = null;
        foreach (VesselSegment aSegment in Segments)
        {
            //aSegment.Rebound

            if (lastSegment != null)
            {
                aSegment.ChainLast(lastSegment.ChainNext(aSegment));
            }
            lastSegment = aSegment;
        }
    }
    public void engorgeVeins()
    {
        if(IV_Manager.tournOnOff == true)
        {
            IV_Manager.tournOnOff = false;
        }
        else if (IV_Manager.tournOnOff == false)
        {
            IV_Manager.tournOnOff = true;
        }
    }
    public static bool tournOnOff = false;
    public void engorgeVeinsaLil()
    {
        if (tournOnOff == true)
        {
            tournOnOff = false;
        }
        else if (tournOnOff == false)
        {
            tournOnOff = true;
        }
    }

    void Update()
    {
    
        Segments[0].Rebound();

        //artery
        if (gameObject.layer == 15)
        {
            gameObject.GetComponent<MeshRenderer>().material.color = Color.red;

            if (pulse == true)
            {
                radiusMultiplier += 0.05f;
            }
            if (radiusMultiplier > 1.45f)
            {
                pulse = false;
            }
            if (pulse == false)
            {
                radiusMultiplier -= 0.05f;
            }
            if (radiusMultiplier < 0.95f)
            {
                pulse = true;
            }
        }

        //vein layer
        if (gameObject.layer == 16)
        {
            //determine size of all veins
            if (Tourniquet_Pressure_Monitor.ME.pressureCorrect)
            {

                radiusMultiplier = enlargedPercentage;
                
            }
            else if (Tourniquet_Pressure_Monitor.ME.pressureTooHigh)
            {
                radiusMultiplier = excessivelyEnlargedPercentage;
                //play auditory feedback

            }
            else
            {
                radiusMultiplier = normalSizePercentage;

            }

            //enlarge hand veins only
            if (Hand_Slap_Monitor.ME.SlapDetected)
            {

                //apply slap increase
                if (!slapIncreaseApplied)
                {
                    GameObject[] handVeins = GameObject.FindGameObjectsWithTag("HandVein");

                    foreach (GameObject item in handVeins)
                    {
                        item.GetComponent<VesselSegment>().Radius += slapIncrease;
                    }

                    slapIncreaseApplied = true;
                }



            } else if (slapIncreaseApplied)
            {
                GameObject[] handVeins = GameObject.FindGameObjectsWithTag("HandVein");

                foreach (GameObject item in handVeins)
                {
                    item.GetComponent<VesselSegment>().Radius -= slapIncrease;
                }

                slapIncreaseApplied = false;
            }
        }
    }
            
    public void OutsidePressAgainst(float[] paramaterPack)
    {
        // called from a SendMessage handler, so we can pass in only one variable.  I'm using a float array to pack everything in. 
        // Parameter Pack contains:
        // a hit triangle index, so we can back out what Vessel Segment to message.
        // a pressure index.  this is the actual pressure we use to smash the vessels flat.  1 = completely flat. 
        // a push index.  -1 is to the left one radius; 1 is to the right one radius.
        // a spread index, for how much the change effects neighboring segments.


        if (paramaterPack.Length == 4)
        {

            float pressureIndex = paramaterPack[1];
            float pushIndex = paramaterPack[2];
            float spreadIndex = paramaterPack[3];

            int hitTriangleIndex = (int)paramaterPack[0];
            int touchingSegment = (int)((hitTriangleIndex - 1) / (PolygonSideCount * 2));

            Segments[touchingSegment].PressOn(pressureIndex, pushIndex, spreadIndex);
            Debug.Log("PushIndex: " + pushIndex + " SpreadIndex: " + spreadIndex);
        }

    }

    void LateUpdate()
    {
        /*if (null == Segments || Segments.Length <= 1)
        {
            GetComponent<Renderer>().enabled = false;
            return;
        }
        GetComponent<Renderer>().enabled = true;*/

        //rebuild the mesh?
        bool rebuild = false;

        // for now, we always rebuild it.  later we can add some sort of detection for vertex changes.
        if (Segments.Length > 1)
        {
            rebuild = true;
        }

        if (rebuild)
        {
            //draw tube

            if (PolygonSideCount != lastCrossSegments)
            {
                circularSetOfPoints = new Vector3[PolygonSideCount];
                float theta = 2.0f * Mathf.PI / PolygonSideCount;
                for (int c = 0; c < PolygonSideCount; c++)
                {
                    circularSetOfPoints[c] = new Vector3(Mathf.Cos(theta * c), Mathf.Sin(theta * c), 0);
                }
                lastCrossSegments = PolygonSideCount;
            }
            //Debug.Log(gameObject.name + " " + Segments.Length);
            MeshVertices = new Vector3[Segments.Length * PolygonSideCount];

            Triangles = new int[Segments.Length * PolygonSideCount * 6];
            int[] lastVertices = new int[PolygonSideCount];
            int[] theseVertices = new int[PolygonSideCount];
            Quaternion rotation = Quaternion.identity;

            for (int p = 0; p < Segments.Length; p++)
            {
                if (p < Segments.Length - 1) rotation = Quaternion.FromToRotation(Vector3.forward, Segments[p + 1].CenterPoint - Segments[p].CenterPoint);

                for (int c = 0; c < PolygonSideCount; c++)
                {
                
                    int vertexIndex = p * PolygonSideCount + c;
                    MeshVertices[vertexIndex] = (Segments[p].CenterPoint - transform.position) + rotation * circularSetOfPoints[c] * (Segments[p].Radius * radiusMultiplier);

                    float roundnessFraction = Segments[p].Roundness;
                    if (gameObject.layer == 16)
                    {
                        //roundnessFraction = 1.50f;
                    }

                    // "smashdown" pushes the vein down, as if against a flat surface.
                    // if you don't do this, the vein flattens in the middle of the segment.
                    float smashdown = (1 - roundnessFraction) * (Segments[p].Radius * radiusMultiplier);
                    float pushdown = 1; // make pushdown > 1 and the back of the vein gets pushed in a bit.
                    MeshVertices[vertexIndex].y = (MeshVertices[vertexIndex].y * roundnessFraction);// - (smashdown * pushdown);

                    lastVertices[c] = theseVertices[c];
                    theseVertices[c] = p * PolygonSideCount + c;
                }
                //make triangles
                if (p > 0)
                {
                    for (int c = 0; c < PolygonSideCount; c++)
                    {
                        int start = (p * PolygonSideCount + c) * 6;
                        Triangles[start] = lastVertices[c];
                        Triangles[start + 1] = lastVertices[(c + 1) % PolygonSideCount];
                        Triangles[start + 2] = theseVertices[c];
                        Triangles[start + 3] = Triangles[start + 2];
                        Triangles[start + 4] = Triangles[start + 1];
                        Triangles[start + 5] = theseVertices[(c + 1) % PolygonSideCount];
                    }
                }
            }

            Mesh mesh = GetComponent<MeshFilter>().mesh;
            //if (!mesh)
            //{
            mesh = new Mesh();
            //}
            mesh.vertices = MeshVertices;
            mesh.triangles = Triangles;
            mesh.RecalculateNormals();
            GetComponent<MeshFilter>().mesh = mesh;
            // this updates the mesh collider
            MeshCollider myMC = GetComponent<MeshCollider>();
            Mesh newMesh = new Mesh();
            newMesh = new Mesh();
            newMesh.vertices = MeshVertices;
            newMesh.triangles = Triangles;
            newMesh.RecalculateBounds();
            myMC.sharedMesh = newMesh;
            //Debug.Log("REBUILT");
        }
    }

    //sets all the points to points of a Vector3 array, as well as capping the ends.
    //void SetPoints(Vector3[] points, float radius, float flatness)
    //{
    //  if (points.Length < 2) return;
    //    Segments = new VesselSegment[points.Length + 2];

    //  Vector3 v0offset = (points[0] - points[1]) * 0.01f;
    //  Segments[0] = new VesselSegment(v0offset + points[0], 0.0f, flatness);
    //  Vector3 v1offset = (points[points.Length - 1] - points[points.Length - 2]) * 0.01f;
    //  Segments[Segments.Length - 1] = new VesselSegment(v1offset + points[points.Length - 1], 0.0f, flatness);

    //    for (int p = 0; p < points.Length; p++)
    //    {
    //        Segments[p + 1] = new VesselSegment(points[p], radius, flatness);
    //    }
    // }
}