using UnityEngine;
using System.Collections;

public class BackwallDetector : MonoBehaviour
{

    // Public because VMovers need to reference these - but they are not to be tuned with the inspector.
    [HideInInspector]
    public bool USContact = false;
    public bool NeedleContact = false;

    public Vector3 USAverageBackwallVertex;
    public Vector3 NeedleAverageBackwallVertex;

    public Vector3 USAverageBackwallNormal;
    public Vector3 NeedleAverageBackwallNormal;


    // Internally used variables and sets
    public Transform USBackwallRaycasterSet;
    public Transform NeedleBackwallRaycasterSet;
    public Vector3[] vertices;
    public int[] triangles;
    public Vector3[] globalVertices;
    public Vector3 BackwallVertex;
    public Vector3 BackwallNormal;
    public int Mask;
    public RaycastHit[] USbackwallHits;
    public RaycastHit[] NeedlebackwallHits;
	[SerializeField]
    public Mesh VeinMesh;
    public int backwallIndex;
    public int USBackwallDetectorCount;
    public int NeedleBackwallDetectorCount;

    public float Tune;

    void Awake()
    {
        GameObject usv = GameObject.Find("US Probe Backwall Detector Set");
        //GameObject nv = GameObject.Find("Needle Backwall Detector Set");
        if (usv != null)
            USBackwallRaycasterSet = usv.GetComponent<Transform>();
        //if (nv != null)
        //    NeedleBackwallRaycasterSet = nv.GetComponent<Transform>();
    }

    // Use this for initialization
    void Start()
    {

        Mask = LayerMask.GetMask("Epidural");

        VeinMesh = GetComponent<MeshFilter>().mesh;
        vertices = VeinMesh.vertices;
        triangles = VeinMesh.triangles;
        globalVertices = new Vector3[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            globalVertices[i] = transform.TransformPoint(vertices[i]);
        }

        USBackwallDetectorCount = USBackwallRaycasterSet.childCount;
        //NeedleBackwallDetectorCount = NeedleBackwallRaycasterSet.childCount;

        USbackwallHits = new RaycastHit[USBackwallDetectorCount];
        //NeedlebackwallHits = new RaycastHit[NeedleBackwallDetectorCount];

    }

    // Update is called once per frame
    void Update()
    {

        USContact = false;
        NeedleContact = false;

        //Since we now have two sets of backwall detectors we will run through them separately.
        //First is for the US Probe set and second is for the Needle set.
        USProbeDetection();
        //NeedleDetection();
    }

    private void USProbeDetection()
    {
        for (int i = 0; i < USBackwallRaycasterSet.childCount; i++)
        {
            Transform detector = USBackwallRaycasterSet.GetChild(i);
            Transform target = detector.GetChild(0);
            Ray ray = new Ray(detector.position, target.position - detector.position);
            RaycastHit hit;

			if (Physics.Raycast(ray, out hit, Mathf.Infinity, Mask))
			{
				if (hit.transform.GetInstanceID() == transform.GetInstanceID())
				{
					USbackwallHits[i] = hit;
					USContact = true;
				}
				else
					USbackwallHits[i] = new RaycastHit();
			}
			else
			{
				USbackwallHits[i] = new RaycastHit();
			}


        }

        float smallestDistance = 10000;
        int lowestIndex = -1;

        int backwallHitsDetected = 0;
        Vector3 aggregateBackwall = Vector3.zero;

        for (int i = 0; i < USbackwallHits.Length; i++)
        {
            // this finds the farthest backwall
            if ((USbackwallHits[i].distance < smallestDistance) && (USbackwallHits[i].distance != 0))
            {
                smallestDistance = USbackwallHits[i].distance;
                lowestIndex = i;
            }

            // this finds the average backwall
            if (USbackwallHits[i].distance != 0)
            {
                aggregateBackwall = aggregateBackwall + USbackwallHits[i].point;
                backwallHitsDetected++;
            }
        }

        // works better if we add a slight bias towards the farthest backwall 
        if (lowestIndex > -1)
            aggregateBackwall = aggregateBackwall - USbackwallHits[lowestIndex].point;

        if (USContact)
        {
			/*Debug.Log("TL "+triangles.Length);
			Debug.Log("USBL " + USbackwallHits.Length);
			Debug.Log("LI " + lowestIndex);
			Debug.Log("USBHLI " + USbackwallHits[lowestIndex].ToString());
			Debug.Log("USBHLITI " + USbackwallHits[lowestIndex].triangleIndex);
			Debug.Log(USbackwallHits[lowestIndex].transform.name);
			Debug.Log(name);
			Debug.Log(VeinMesh.name);
			Debug.Log(VeinMesh.triangles.Length);*/
			//USED TO FIND ERROR IN BACKWALL BETWEEN LEFT AND RIGHT VEIN TRIANGLE INDEX OUT OF BOUNDS EXCEPTION
			backwallIndex = triangles[(USbackwallHits[lowestIndex].triangleIndex) * 1];
            BackwallVertex = globalVertices[backwallIndex];
            USAverageBackwallVertex = aggregateBackwall / ((float)backwallHitsDetected - Tune); // 0.65
        }

        // Define the Normal as directly down the probe - in this case, make the first Backwall Raycaster
        // align with the probe axis.  Right now, all Backwall Raycasters are parallel - any of them work.
        Transform d = USBackwallRaycasterSet.GetChild(0);
        Transform t = d.GetChild(0);
        Ray r = new Ray(d.position, t.position - d.position);

        USAverageBackwallNormal = r.direction;
    }

    private void NeedleDetection()
    {
        for (int i = 0; i < NeedleBackwallRaycasterSet.childCount; i++)
        {
            Transform detector = NeedleBackwallRaycasterSet.GetChild(i);
            Transform target = detector.GetChild(0);
            Ray ray = new Ray(detector.position, target.position - detector.position);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, Mask))
            {
				if (hit.transform.GetInstanceID() == transform.GetInstanceID())
				{
					NeedlebackwallHits[i] = hit;
					NeedleContact = true;
				}
				else
					NeedlebackwallHits[i] = new RaycastHit();
			}
            else
            {
                NeedlebackwallHits[i] = new RaycastHit();
            }


        }

        float smallestDistance = 10000;
        int lowestIndex = -1;

        int backwallHitsDetected = 0;
        Vector3 aggregateBackwall = Vector3.zero;

        for (int i = 0; i < NeedlebackwallHits.Length; i++)
        {
            // this finds the farthest backwall
            if ((NeedlebackwallHits[i].distance < smallestDistance) && (NeedlebackwallHits[i].distance != 0))
            {
                smallestDistance = NeedlebackwallHits[i].distance;
                lowestIndex = i;
            }

            // this finds the average backwall
            if (NeedlebackwallHits[i].distance != 0)
            {
                aggregateBackwall = aggregateBackwall + NeedlebackwallHits[i].point;
                backwallHitsDetected++;
            }
        }

        // works better if we add a slight bias towards the farthest backwall 
        if (lowestIndex > -1)
            aggregateBackwall = aggregateBackwall - NeedlebackwallHits[lowestIndex].point;

        if (NeedleContact)
        {
            backwallIndex = triangles[(NeedlebackwallHits[lowestIndex].triangleIndex) * 1];
            if (backwallIndex >= globalVertices.Length)
            {
                Debug.Log("YOUR INDEX IS OUT OF BOUND!!!!! WHYYYYY???");
                backwallIndex = globalVertices.Length - 1;
            }
            BackwallVertex = globalVertices[backwallIndex];
            NeedleAverageBackwallVertex = aggregateBackwall / ((float)backwallHitsDetected - 0.65F); 
        }

        // Define the Normal as directly down the probe - in this case, make the first Backwall Raycaster
        // align with the probe axis.  Right now, all Backwall Raycasters are parallel - any of them work.
        Transform d = NeedleBackwallRaycasterSet.GetChild(0);
        Transform t = d.GetChild(0);
        Ray r = new Ray(d.position, t.position - d.position);

        NeedleAverageBackwallNormal = r.direction;
    }
}
