using UnityEngine;
using System.Collections;

// Basic detector for: is the needle pointing at a specific layer, and how far away is it?  
//    and - as an option - is there a wall in the way?
// keeps track of the smallest approach since Reset()
// and uses the needle hub and tip analogy.

public class LinearTrajectoryMonitor
{

    // Trajectory goes from Gameobject Hub to Tip's direction, starting from Tip.
    // The test actually starts from Hub and considers the distance from Hub to Tip
    // this way, penetrations can be detected; a negative distance is a penetration.
    // We can optionally consider an unpenetratable Wall; if a wall is between the tip and the mesh,
    // we will consider the mesh unreachable.  this is intended for ribs being in-between a needle tip and the lung.
    // Note, if the tip is in the mesh, we do not test for a wall.

    private GameObject Hub;
    private GameObject Tip;
    private float ScaleFactor;
    private LayerMask TargetMask;
    private LayerMask WallMask;
    private bool WallExists;

    public float Distance;
    public float SmallestDistance;
    public bool Puncture;           // this is continuously updated 
    public bool InstantOfPuncture;  // this occurs once a session, i.e. did you cause a pnuemothorax or not?  Its good for triggering some other test when access is first established.
    public bool InstanceOfPuncture; // this occurs every time the line goes through the target mesh.  Its good for tactile thumps and turning LEDs on.
    public float DeepestDistance;   // this is the largest distance the line ever went into the target - pay attention to how this works, im not sure how it will behave if the needle backwalls or brushes the side of the vein  
    public GameObject PuncturedObject; // this is the object with the layermask that is currently being punctured, if none, then null.
    public float BoundryCorrectionFactor = 0; // Allows a correction factor for a line monitor, if needed.
    public Vector3 hitPoint;


    // Overloaded method for initialization - this one has a wall
    public LinearTrajectoryMonitor(GameObject aStart, GameObject aTip, float scale, LayerMask target, LayerMask wall)
    {

        Hub = aStart;
        Tip = aTip;
        ScaleFactor = scale;
        TargetMask = target;

        WallExists = true;
        WallMask = wall;

        Reset();

    }

    // Overloaded method for initialization - this one doesn't have a wall
    public LinearTrajectoryMonitor(GameObject aStart, GameObject aTip, float scale, LayerMask target)
    {
        Hub = aStart;
        Tip = aTip;
        ScaleFactor = scale;
        TargetMask = target;

        WallExists = false;

        Reset();
    }

    // Called externally - this class is NOT a monobehavior. Update() calls Measure() without having to return something.
    public void Update()
    {
        float temp = Measure();
    }

    // Called externally as needed.
    public float Measure()
    {

        // some temporary variables
        RaycastHit hit;
        bool lastPuncture = Puncture;

        // forward direction
        Vector3 forward = Tip.transform.position - Hub.transform.position;
        Ray lookingForward = new Ray(Hub.transform.position, forward);
        float tipDistance = Vector3.Distance(Hub.transform.position, Tip.transform.position) + BoundryCorrectionFactor;

        // Distance to any mesh in the Target Mask
        Distance = Mathf.Infinity;
        if (Physics.Raycast(lookingForward, out hit, Mathf.Infinity, TargetMask))
        {
            Distance = (hit.distance - tipDistance) * ScaleFactor;
            hitPoint = hit.point;
        }

        // if we have a Distance and are not puncturing the TargetMask, check for the wall.  The wall will block Distance and SmallestDistance from tracking.
        if (WallExists & Distance > 0)
        { 
            float wallDistance = Mathf.Infinity;
            if (Physics.Raycast(lookingForward, out hit, Mathf.Infinity, WallMask))
            {
                wallDistance = (hit.distance - tipDistance) * ScaleFactor;
                hitPoint = hit.point;
                if (wallDistance < Distance) { Distance = Mathf.Infinity; }
            }
        }

        // Readable way to determine inside or outside.
        Puncture = (Distance <= 0);

        // Mininum distance is zero, then we track the deepest puncture distance.
        if (Distance < 0)
        {
            if (-Distance > DeepestDistance) DeepestDistance = -Distance;
            Distance = 0;
        }

        // Reference the object being punctured.
        if (Distance <= 0)
        {
            PuncturedObject = hit.collider.gameObject;
        } else
        {
            PuncturedObject = null;
        }

        // Mark the instant of puncture.  This can happen only one time after every Reset()
        InstantOfPuncture = false;
        if (Distance <= 0 && SmallestDistance > 0) InstantOfPuncture = true;

        // Mark the event of crossing the puncture threshold.  This can happen as often as necessary; consider a timeout debouncer here.
        InstanceOfPuncture = false;
        if (Puncture && !lastPuncture) InstanceOfPuncture = true;

        // remember the smallest distance ever encountered since the last Reset().
        if (Distance < SmallestDistance) SmallestDistance = Distance;

        return Distance;
    }

    public void Reset()
    {
        Distance = Mathf.Infinity;
        SmallestDistance = Mathf.Infinity;
        DeepestDistance = 0;
        Puncture = false;
        InstantOfPuncture = false;
        InstanceOfPuncture = false;
    }
}
