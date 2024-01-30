using UnityEngine;
using System.Collections;

// Basic detector for: is the needle tip under a specific object, and how far away is it in ONE direction from another object?
// You have to define what "under" is since the software doesnt know human anatomy
// Origionally written for the neuraxial complication detection for RA simulator.
// keeps track of the smallest approach since Reset()

public class BelowZoneMonitor
{


    private GameObject Tip;                 // tip of needle
    private float ScaleFactor;              // useful if we use cm while world units are mm
    private LayerMask TargetMask;           // probably something named "neuraxial zone"
    private Vector3 Up;                     // so we can test what "under this ligament" means
    private GameObject ReferenceObject;     // like an anatomical midline
    private Vector3 ReferenceObjectNormal;  // defines the midline direction
    private GameObject Hub;                 // needed to define how long the needle is - a sensible test depth


    public float Distance;          // Distance FROM REFERENCE OBJECT in REFERENCEOBJECTNORMAL direction - i.e. how far from midline
    public float SmallestDistance;  // smallest distance from midline
    public bool Puncture;           // this is continuously updated 
    public bool InstantOfPuncture;  // this occurs once a session, i.e. did you cause a pnuemothorax or not?  Its good for triggering some other test when access is first established.
    // public bool InstanceOfPuncture; // this occurs every time the line goes through the target mesh.  Its good for tactile thumps and turning LEDs on.


    // Overloaded method for initialization - this one doesn't have a wall
    public BelowZoneMonitor(GameObject aStart, GameObject aTip, float scale, LayerMask target, Vector3 whatUP, GameObject aRefObject, Vector3 aRefObjectDirection) //, GameObject aReference, Vector3 aDir)
    {
        Tip = aTip;
        ScaleFactor = scale;
        TargetMask = target;
        Up = whatUP;
        Hub = aStart;
        ReferenceObject = aRefObject;
        ReferenceObjectNormal = aRefObjectDirection;
        Reset();
    }

    // Called externally - this class is NOT a monobehavior. Update() calls Measure() without having to return something.
    public void Update()
    {
        float temp = Measure();
    }

    public float Measure()
    {
        // some temporary variables
        RaycastHit hit;
        bool lastPuncture = Puncture;

        float length = Vector3.Distance(Hub.transform.position, Tip.transform.position);

        // looking from the needle tip, straight up.   
        Ray lookingUp = new Ray(Tip.transform.position, Up);

        // looking straight down at the needle tip from above.  
        Vector3 AbovePoint = lookingUp.GetPoint(length);
        Ray lookingDown = new Ray(AbovePoint, (Up * -1));

        // Distance to any mesh in the Target Mask
        // we test both looking straight up and down so the target normal directions do not matter
        bool isUnder = false;
        if (Physics.Raycast(lookingUp, out hit, length, TargetMask))
            isUnder = true;

        if (Physics.Raycast(lookingDown, out hit, length, TargetMask))
            isUnder = true;

        // we also test looking along the needle tip.
        // if the needle is deep enough into a small danger zone, the tip may not be straight below it anymore.
        Vector3 forward = Tip.transform.position - Hub.transform.position;
        Ray lookingForward = new Ray(Hub.transform.position, forward);

        if (Physics.Raycast(lookingForward, out hit, length, TargetMask))
            isUnder = true;


        // set the public variable for other scripts to see
        Puncture = isUnder;

        // if tip is in the zone, measure how far it is from the reference object in a particular direction
        if (Puncture)
        {
            Vector3 vectorToMidline = Tip.transform.position - ReferenceObject.transform.position;
            Distance = Mathf.Abs(Vector3.Dot(vectorToMidline, ReferenceObjectNormal) * ScaleFactor);
            if (SmallestDistance == Mathf.Infinity) InstantOfPuncture = true;
            if (Distance < SmallestDistance) SmallestDistance = Distance;
        }

        return Distance;
    }

    // Called externally as needed.
    /*
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
            Distance = (hit.distance - tipDistance) * ScaleFactor;

        // Readable way to determine inside or outside.
        Puncture = (Distance <= 0);

        // Mininum distance is zero, then we track the deepest puncture distance.
        //if (Distance < 0)
        //{
        //    if (-Distance > DeepestDistance) DeepestDistance = -Distance;
        //    Distance = 0;
        //}

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
    */

    public void Reset()
    {
        Distance = Mathf.Infinity;
        SmallestDistance = Mathf.Infinity;
        // DeepestDistance = 0;
        Puncture = false;
        InstantOfPuncture = false;
        //InstanceOfPuncture = false;
    }
}
