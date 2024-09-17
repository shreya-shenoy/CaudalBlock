using UnityEngine;
using System.Collections;

// Basic detector for: is the needle pointing at a specific layer, and how far away is it? 
// in addition, given an direction UP, is it UNDER the layer without poking THROUGH it? 

// keeps track of the smallest approach since Reset()
// and uses the needle hub and tip analogy.

public class LinearTrajectoryUnderMonitor
{

    // Trajectory goes from Gameobject Hub to Tip's direction, starting from Tip.
    // The test actually starts from Hub and considers the distance from Hub to Tip
    // this way, penetrations can be detected; a negative distance is a penetration.
    // We can optionally consider an unpenetratable Wall; if a wall is between the tip and the mesh,
    // we will consider the mesh unreachable.  this is intended for ribs being in-between a needle tip and the lung.
    // Note, if the tip is in the mesh, we do not test for a wall.

    private GameObject Tip;
    private GameObject Hub;
    private float ScaleFactor;
    private LayerMask TargetMask;
    private LayerMask WallMask;
    private Vector3 Up;

    public float Distance;
    public float SmallestDistance;
    public bool Puncture;           // this is continuously updated 
    public bool InstantOfPuncture;  // this occurs once a session, i.e. did you cause a pnuemothorax or not?  Its good for triggering some other test when access is first established.
    public bool InstanceOfPuncture; // this occurs every time the line goes through the target mesh.  Its good for tactile thumps and turning LEDs on.
    public float DeepestDistance;   // this is the largest distance the line ever went into the target - pay attention to how this works, im not sure how it will behave if the needle backwalls or brushes the side of the vein  
    public GameObject PuncturedObject; // this is the object with the layermask that is currently being punctured, if none, then null.
    public float BoundryCorrectionFactor = 0; // Allows a correction factor for a certina line monitor, if needed.
 


    // Overloaded method for initialization - this one doesn't have a wall
    public LinearTrajectoryUnderMonitor(GameObject aStart, GameObject aTip, float scale, LayerMask target, Vector3 anUpdirection)
    {
        Tip = aStart;
        Hub = aTip;
        ScaleFactor = scale;
        TargetMask = target;
        Up = anUpdirection;

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
        Vector3 forward = Hub.transform.position - Tip.transform.position;
        Ray lookingForward = new Ray(Tip.transform.position, forward);
        float tipDistance = Vector3.Distance(Tip.transform.position, Hub.transform.position) + BoundryCorrectionFactor;

        // dorsal direction - looking up.
        Ray dorsalNeedleRay = new Ray(Tip.transform.position, Up);
        // Debug.DrawRay(Tip.transform.position, (Up * 10), Color.red);


        // Distance to any mesh in the Target Mask
        Distance = Mathf.Infinity;

        // Now we can either poke through a target or go under it - where "under" is a defined direction.

        // First, lets see if we are poking the object:
        if (Physics.Raycast(lookingForward, out hit, Mathf.Infinity, TargetMask))
        {
            // we are in-line with the object; a distance of less than zero will be interpreted as poking through.
            Distance = (hit.distance - tipDistance) * ScaleFactor;
            // Debug.Log("Distance through: " + Distance.ToString("0.00"));
        }

        // If we are not poking the object, are we UNDER it?
        if (Distance > 0)
        {
            if (Physics.Raycast(dorsalNeedleRay, out hit, Mathf.Infinity, TargetMask))
            {
                // we are under the object. could be a long way off, but a distance less then the shaft means the tip is under it.
                Distance = (hit.distance - tipDistance) * ScaleFactor;
                // Debug.Log("Distance under: " + Distance.ToString("0.00"));
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
