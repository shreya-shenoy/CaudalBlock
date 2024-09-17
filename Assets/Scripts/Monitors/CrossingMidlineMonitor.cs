using UnityEngine;
using System.Collections;

// Basic detector for: is the needle tip under a specific object, and how far away is it in ONE direction from another object?
// You have to define what "under" is since the software doesnt know human anatomy
// Origionally written for the neuraxial complication detection for RA simulator.
// keeps track of the smallest approach since Reset()

public class CrossingMidlineMonitor
{

    // Units = mm (default, 1 world unit = 1mm)

    private GameObject Tip;                 // tip of needle

    private GameObject Hub;                 // needed to define how long the needle is - a sensible test depth
    private float ThresholdAmount;          // you have to cross midline by this many mm for it to count.

    private Vector3 MidlineNormalDirection; // so we can test what "under this ligament" means
    private GameObject Midline;     // like an anatomical midline, or something on the anatomical midline like a spinal cord


    private Vector3 SkinPokeLocation;

    public float Distance;          // Distance FROM REFERENCE OBJECT in REFERENCEOBJECTNORMAL direction - i.e. how far from midline
    public float SmallestDistance;  // smallest distance from midline.  useful for testing how close to causing midline puncture.
    public bool Cross;           // this is continuously updated 
    public bool CrossedMidline;  // this occurs once a session, i.e. did you cause a pnuemothorax or not?  Its good for triggering some other test when access is first established.
    public bool InstanceOfMidlineCross; // this occurs every time the line crosses midline.  Its good for tactile thumps and turning LEDs on.


    // Overloaded method for initialization - this one doesn't have a wall
    public CrossingMidlineMonitor(GameObject aStart, GameObject aTip, float threshold, GameObject aRefObject, Vector3 aRefObjectDirection) //, GameObject aReference, Vector3 aDir)
    {
        Tip = aTip;
        ThresholdAmount = threshold;
        Hub = aStart;
        Midline = aRefObject;
        MidlineNormalDirection = aRefObjectDirection;
        Reset();
    }

    public void NoteSkinPunctureLocation(Vector3 alocation)
    {
        SkinPokeLocation = alocation;
    }

    // Called externally - this class is NOT a monobehavior. Update() calls Measure() without having to return something.
    public void Update()
    {
        if (SkinPokeLocation != Vector3.zero)
        {
            float temp = Measure();
        }
    }

    public float Measure()
    {
        

        Distance = Mathf.Infinity;

        // we are looking to see if the hub and tip exist in oposite sides of the midline object in MidlineNormalDirection.

        // first lets look at the distances from midline to hub and tip in realtime.
        Vector3 tipvector = Tip.transform.position - Midline.transform.position;
        Vector3 hubvector = Hub.transform.position - Midline.transform.position;

        // in the direction of midline.  these numbers will have the same sign if they are on the same side of the midline.
        float tipdirection          = Vector3.Dot(tipvector, MidlineNormalDirection);
        float initialtipdirection   = Vector3.Dot(SkinPokeLocation, MidlineNormalDirection);

        // we are also going to note the distance to midline and keep track of the smallest distance. 
        Distance = Mathf.Abs(tipdirection);
        if (SmallestDistance > Distance) SmallestDistance = Distance;

        tipdirection = Mathf.Sign(tipdirection);
        initialtipdirection = Mathf.Sign(initialtipdirection);


        if ((tipdirection != initialtipdirection) & (Distance > ThresholdAmount))
        {
            // crossed midline.  by how much?
            InstanceOfMidlineCross = (Cross == false);
            Cross = true;
            CrossedMidline = true;
        } else
        {
            // no midline crossing.
            Cross = false;
            InstanceOfMidlineCross = false;
        }


        return (Distance);

    }



    public void Reset()
    {
        Distance = Mathf.Infinity;
        SmallestDistance = Mathf.Infinity;
        CrossedMidline = false;
        Cross = false;
        InstanceOfMidlineCross = false;
        SkinPokeLocation = Vector3.zero;
    }
}
