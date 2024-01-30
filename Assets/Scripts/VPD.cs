using UnityEngine;
using SMMARTS_SDK;
using System.Collections;
using UnityEngine.UI;

public class VPD : MonoBehaviour
{

    [Range(0, 3.0f)]
    public float MaxPressure;       // initially set to 2
    public float PressureRange;     // initally set to 55
    public float PressureDistance;  // initally set to 35

    private Mesh MyMesh;
    private MeshCollider MyMeshCollider;
    private Vector3[] Vertices;
    private Vector3[] globalVertices;
    //private int[] Triangles;
    private Vector3[] Normals;
    private Vector3[] globalNormals;
    //private Vector3[] OriginalVertices;
    private Vector3[] globalOriginalVertices;
    private VMover[] VertexMovers;
    private int VertexMoverCount;

    private bool isPositionSet;
    int block;
    private bool offlineModeTracker;
    private GameObject bsodObject;


    //public float SpringbackFraction;

    private VMover MyProtoMover;

    private bool HasChanged = false;

    [HideInInspector]
    public float NeedlePressure;

    public float USPressure;


    void Awake()
    {



        // This script is designed to work with gameobjects that have a mesh and a mesh collider.
        // The mesh collider of this mesh should be layered and tagged "Compressible"
        // This mesh collider is never changed, and is used for pressure detection only.
        MyMesh = GetComponent<MeshFilter>().mesh;

        // The ultrasound sees an entirely different mesh collider, layered and tagged "Vein" or something.
        // The mesh collider is a separate object.  include a copy of the mesh as a child gameobject named "Ultrasoundable"
        // remove all components except its MeshCollider - no MeshFilter, MeshRenderer, or scripts.
        // This mesh collider is alligned to this gameObject's mesh vertices periodically at great expense.
        MyMeshCollider = transform.Find("Ultrasoundable").GetComponent<MeshCollider>();
        if (MyMeshCollider == null)
            Debug.Log("A 'PressureDeformer' needs an 'Ultrasoundable' child.");

        GameObject v = GameObject.Find("vmover");
        if (v != null)
            MyProtoMover = v.GetComponent<VMover>();
    }

    // Use this for initialization
    void Start()
    {/*

        MyMesh.RecalculateNormals();

        Vertices = MyMesh.vertices;
        Normals = MyMesh.normals;
        //Triangles = MyMesh.triangles;

        globalVertices = new Vector3[Vertices.Length];
        for (int i = 0; i < Vertices.Length; i++)
        {
            globalVertices[i] = transform.TransformPoint(Vertices[i]);
        }

        globalNormals = new Vector3[Normals.Length];
        for (int i = 0; i < Normals.Length; i++)
        {
            globalNormals[i] = transform.TransformDirection(Normals[i]);
        }


        //OriginalVertices = Vertices.Clone() as Vector3[];
        globalOriginalVertices = globalVertices.Clone() as Vector3[];


        int n = Vertices.Length;
        VertexMovers = new VMover[n];

        VertexMoverCount = 0;

        //Vertices = globalVertices;

        for (int i = 0; i < Vertices.Length; i++)
        {
            VertexMovers[i] = NewVertexMover(i);
        }
        */
    }

    public void positionSet()
    {
        if (isPositionSet == false)
        {

            Debug.Log("position set!!!!");

            MyMesh.RecalculateNormals();

            Vertices = MyMesh.vertices;
            Normals = MyMesh.normals;
            // Triangles = MyMesh.triangles;

            globalVertices = new Vector3[Vertices.Length];
            for (int i = 0; i < Vertices.Length; i++)
            {
                globalVertices[i] = transform.TransformPoint(Vertices[i]);
            }

            globalNormals = new Vector3[Normals.Length];
            for (int i = 0; i < Normals.Length; i++)
            {
                globalNormals[i] = transform.TransformDirection(Normals[i]);
            }


            //OriginalVertices = Vertices.Clone() as Vector3[];
            globalOriginalVertices = globalVertices.Clone() as Vector3[];


            int n = Vertices.Length;
            VertexMovers = new VMover[n];

            VertexMoverCount = 0;

            //Vertices = globalVertices;

            for (int i = 0; i < Vertices.Length; i++)
            {
                VertexMovers[i] = NewVertexMover(i);
            }

            isPositionSet = true;
        }
       
    }

    // the vertex movers (class VMover) move the vertices based on initial vertex positions in 3D space. 
    // if we move the whole mesh, we need to rebuild the VMovers.
    public void ClearAll()
    {
        if (isPositionSet == true)
        {

            BroadcastMessage("RemoveVMover"); //Calls the method named RemoveVertexMover on every MonoBehaviour in this game object or any of its children.
                                              // that message tells the VMovers to delete themselves.

            // at this point, we had a mass of VMovers commit suicide. the last thing they did before going away was to put back their vertices in their initial position,
            // via a mass of MoveVertex() function calls to this script.
            // lets upload the changes immediately.
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i] = transform.InverseTransformPoint(Vertices[i]);
            }
            MyMesh.vertices = Vertices;
            HasChanged = false;

            // UPLOAD A NEW COLLIDER
            // all at once - if the frame is appropriate.  
            // performance-wise, this is the worst thing you can possibly do... good luck.
            MyMeshCollider.sharedMesh = null;
            MyMeshCollider.sharedMesh = MyMesh;
        }

        isPositionSet = false; // flag for resetting later.
    }

    void Update()
    {
       
    }

    // DL - be sure to check that this code is not used, and if not, comment out.
    /*
    public void setPositionSet()
    {
        isPositionSet = false;
    }
    */

    private VMover NewVertexMover(int v)
    {

        Quaternion newRot = Quaternion.FromToRotation(Vector3.forward, globalNormals[v]);

        VMover newMover = Instantiate(MyProtoMover, globalVertices[v], newRot) as VMover;
        newMover.transform.parent = transform;
        //newMover.transform.parent = transform;
        newMover.Assign(this, v, globalVertices[v], globalNormals[v]); // , SpringbackFraction);

        return newMover;
    }

    public void MoveVertex(int i, Vector3 v)
    {
        Vertices[i] = v;
        HasChanged = true;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (isPositionSet)
        {
            if (SMMARTS_SDK.IVMicrocontroller.ME.MicrocontrollerData.Length > 2) {
                

                USPressure = (float.Parse(SMMARTS_SDK.IVMicrocontroller.ME.MicrocontrollerData[1]) + float.Parse(SMMARTS_SDK.IVMicrocontroller.ME.MicrocontrollerData[2]))/ (float)1000.0;
                //USPressure = 20;
                // NeedlePressure = (NeedleManager.ME.needleTactPressure);
            }

            


            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i] = transform.InverseTransformPoint(Vertices[i]);
            }


            if (HasChanged)
            {

                // UPLOAD NEW MESH
                // The entire set of vertices must be uploaded all at once.
                // there is no way to see changes to a single vertex.  The code will compile, but it wont work.
                MyMesh.vertices = Vertices;
                //MyMesh.RecalculateNormals ();
                HasChanged = false;

                // UPLOAD A NEW COLLIDER
                // all at once - if the frame is appropriate.  
                // performance-wise, this is the worst thing you can possibly do... good luck.
                if (FrameConductor.DeformableAnatomyFrame)
                {
                    MyMeshCollider.sharedMesh = null;
                    MyMeshCollider.sharedMesh = MyMesh;
                }

            }

        }
    }
}
