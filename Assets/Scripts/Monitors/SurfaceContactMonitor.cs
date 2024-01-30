using UnityEngine;
using System.Collections;

// detector for: how many times has a needle-like object contacted the surface of a mesh?
// written for bone contact counting.
// uses the needle hub and tip analogy. The mesh is the bone.
// it is designed to accept a small amount of misaglinment (<0.2mm) 
// is also designed with a buffer to reject small needle movements introduced by system noise.

public class SurfaceContactMonitor
{

    private GameObject Hub;
    private GameObject Tip;
    private float ScaleFactor; // so we can use real world units internally
    private LayerMask TargetMask;
    private float TipDistance;
    private bool NewCountDetected;
    private Vector3 LastHubPosition, LastTipPosition;

    public Vector3 ContactPoint;

    // if the system (or the user's hand) has a small bit of jitter, and the user pauses at the depth threshold for a second, 
    // the skinpoke or attempt count should reflect the user's intention - not increment over and over.
    // these variables keep track of that buffer.
    private Vector3 LastContactPoint;
    private bool ContactZoneSet;
    private float SensitivityDistance; // world units
    private float RecontactZoneSize; // world units

    public int Count;

    private float PokeDepth, LastPokeDepth;



    // constructor with a buffer zone diameter
    public SurfaceContactMonitor(GameObject aStart, GameObject aTip, float scale, LayerMask meshtarget, float aDepthThreshold, float buffersize)
    {

        Hub = aStart;
        Tip = aTip;
        ScaleFactor = scale;
        TargetMask = meshtarget;
        TipDistance = Vector3.Distance(Hub.transform.position, Tip.transform.position);
        SensitivityDistance = aDepthThreshold * (1 / ScaleFactor);
        RecontactZoneSize = (buffersize) * (1 / ScaleFactor);

        Reset();
    }

    // returns true if a new count happened; good for polling.
    public bool CheckforNew()
    {
        Measure();
        return NewCountDetected;
    }

    // Measure updates Count, NewCountDetected, GoingIn, and GoingInByHowMuch.  It can be called as often as is needed by the scoring algorithm.
    public void Measure()
    {
        // Measure is often called several times per frame.  This allows the analysis to be run once per new position. 
        if ((Hub.transform.position != LastHubPosition) & (Tip.transform.position != LastTipPosition))
        {

            // test to see if the tip has moved outside of the ReContactZone
            float d = Vector3.Distance(Tip.transform.position, LastContactPoint);
            if (d > RecontactZoneSize)
            {
                // clear the last contact zone.  whatever hit happens now will be a fresh strike.
                LastContactPoint = Vector3.zero;
                ContactZoneSet = false;

                RaycastHit hit;

                // forward direction
                Vector3 forward = Tip.transform.position - Hub.transform.position;
                Ray lookingForward = new Ray(Hub.transform.position, forward);

                // this allows NewCountDetected to persist until the needle is moved again - 
                // the scoring algorithm should be able to call this function several times per frame without it being cleared.
                NewCountDetected = false;

                float pokeDepth = 0;

                if (Physics.Raycast(lookingForward, out hit, TipDistance, TargetMask))
                {

                    pokeDepth = Mathf.Abs(TipDistance - hit.distance);

                    if (pokeDepth < SensitivityDistance)
                    {
                        if (!ContactZoneSet)
                        {
                            NewCountDetected = true;
                            ContactZoneSet = true;
                            Count++;
                            LastContactPoint = hit.point;
                        }
                    }

                }

            } else
            {
                // the tip is close enough to the LastContactPoint that we will consider any bone contact as the same previously noted strike.
            }

            

        } // else, the tip and hub didn't move

        // allows us to keep track of movement as opposed to the function being called several times per frame for readability
        LastTipPosition = Tip.transform.position;
        LastHubPosition = Hub.transform.position;

    }





    public void Reset()
    {
        Count = 0;
        NewCountDetected = false;
        LastContactPoint = Vector3.zero;
        ContactZoneSet = false;
    }

}
