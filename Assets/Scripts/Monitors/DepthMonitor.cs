using UnityEngine;
using System.Collections;

// detector for: how many times has a needle-like object been poked into a mesh?
// written for skin pokes and attempt counts
// uses the needle hub and tip analogy. The mesh is the skin
// if it seems a little complex, thats because its designed with a buffer to reject small needle movements introduced by system noise.
// keeps track of the smallest approach since Reset()
// also can be used to detect if the needle is in the mesh (beyond a threshold amount) using the Inside() method
// and when it is, can be used to monitor if the needle is going in or being pulled out using the MovingDeeper() method 

public class DepthMonitor
{

    private GameObject Hub;
    private GameObject Tip;
    private float ScaleFactor; // so we can use real world units internally
    private LayerMask TargetMask;
    private float TipDistance;
    private bool NewCountDetected;
    private Vector3 LastHubPosition, LastTipPosition;

    // if the system (or the user's hand) has a small bit of jitter, and the user pauses at the depth threshold for a second, 
    // the skinpoke or attempt count should reflect the user's intention - not increment over and over.
    // these variables keep track of that buffer.
    private float OuterGateThreshold; // world units
    private float InnerGateThreshold; // world units
    private int Gate;
    private int Zone;
    public int Count;
    public Vector3 PuncturePoint;
    private float PokeDepth, LastPokeDepth;
    private bool GoingIn; // as opposed to going out
    private float GoingInByHowMuch; // its now necessary to ask how much its going in by
    private bool GoingOut;
    private float GoingOutByHowMuch;
    public float TipOdometer;

    public float MaxDepth { get; private set; }

    // constructor without a buffer depth
    public DepthMonitor(GameObject aStart, GameObject aTip, float scale, LayerMask meshtarget, float depthThreshold)
    {

        Hub = aStart;
        Tip = aTip;
        ScaleFactor = scale;
        TargetMask = meshtarget;

        TipDistance = Vector3.Distance(Hub.transform.position, Tip.transform.position);

        InnerGateThreshold = depthThreshold * (1 / ScaleFactor);
        OuterGateThreshold = 0;

        Reset();
    }

    // constructor with a buffer depth
    public DepthMonitor(GameObject aStart, GameObject aTip, float scale, LayerMask meshtarget, float depthThreshold, float bufferThickness)
    {

        Hub = aStart;
        Tip = aTip;
        ScaleFactor = scale;
        TargetMask = meshtarget;
        TipDistance = Vector3.Distance(Hub.transform.position, Tip.transform.position);
        InnerGateThreshold = depthThreshold * (1 / ScaleFactor);
        OuterGateThreshold = (depthThreshold - bufferThickness) * (1 / ScaleFactor);

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
            RaycastHit hit;

            // forward direction
            Vector3 forward = Tip.transform.position - Hub.transform.position;
            Ray lookingForward = new Ray(Hub.transform.position, forward);

            // this allows NewCountDetected to persist until the needle is moved again - 
            // the scoring algorightm should be able to call this function several times per frame without it being cleared.
            NewCountDetected = false;

            float PokeDepth = 0;

            if (Physics.Raycast(lookingForward, out hit, TipDistance, TargetMask))
            {

                PokeDepth = TipDistance - hit.distance;

                if (PokeDepth > InnerGateThreshold)
                {
                    if (Gate == 1 | Gate == 0)  // if Gate is 0, user plunged the needle into the mesh so quickly that gate 1 was not detected.
                    {
                        NewCountDetected = true;
                        Count++;
                        PuncturePoint = hit.point;
                    }
                    Gate = 2;
                    Zone = 2;
                }
                else if (PokeDepth > OuterGateThreshold)
                {
                    if (Gate == 0) Gate = 1;
                    Zone = 1;
                }
                else
                {
                    Gate = 0;
                    Zone = 0;
                }

            }
            else
            {

                Zone = 0;
                Gate = 0;

            }


            GoingIn = (PokeDepth > LastPokeDepth);

            if (GoingIn)
                GoingInByHowMuch = PokeDepth - LastPokeDepth;
            else
                GoingInByHowMuch = 0;

            if (GoingIn) TipOdometer = TipOdometer + GoingInByHowMuch;

            GoingOut = (PokeDepth < LastPokeDepth);

            if (GoingOut)
                GoingOutByHowMuch = LastPokeDepth - PokeDepth;
            else
                GoingOutByHowMuch = 0;

            if (GoingIn) TipOdometer = TipOdometer + GoingInByHowMuch;

            // allows us to keep track of how much the needle is going in per frame
            LastPokeDepth = PokeDepth;

            if (MaxDepth < (PokeDepth * ScaleFactor))
                MaxDepth = PokeDepth * ScaleFactor; 

        }

        // allows us to keep track of movement as opposed to the function being called several times per frame for readability
        LastTipPosition = Tip.transform.position;
        LastHubPosition = Hub.transform.position;

    }

    public float GetGoingInAmount()
    {
        return GoingInByHowMuch;
    }

    public float GetGoingOutAmount()
    {
        return GoingOutByHowMuch;
    }

    // are we inside the surface? (is the needle poking in the skin?)
    public bool Inside()
    {
        bool isInside = (Zone == 2);
        return isInside;
    }

    // how far we we inside the surface?  (How much of the needle has been pushed into the skin?)
    public float GetDepth()
    {
        float d = 0;
        if (Inside()) d = LastPokeDepth * ScaleFactor; // cm
        return d;
    }


    // how far have we traveled inside the surface?  (How much of the needle has been pushed into the skin - TOTAL?)
    public float GetOdometer()
    {
        float d = 0;
        d = (TipOdometer - InnerGateThreshold); // * ScaleFactor; // cm
        //if (d < 0) d = 0;
        return d;
    }

    // are we pushing farther, or pulling out?
    public bool MovingDeeper()
    {
        bool goingIn = ((Zone == 2) && GoingIn);
        return goingIn;
    }

    // we are pushing farther, by how much?  (by how much has the needle been pushed into the skin since last measure?)
    public float GetMovingDeeperDistance()
    {
        float d = 0;
        if (Inside()) d = GoingInByHowMuch; // * ScaleFactor; // cm
        return d;

    }

    
    public void Reset()
    {
        Gate = 0;
        Zone = 0;
        Count = 0;
        GoingInByHowMuch = 0;
        PuncturePoint = Vector3.zero;
        NewCountDetected = false;
        TipOdometer = 0;
        MaxDepth = 0;
    }

}
