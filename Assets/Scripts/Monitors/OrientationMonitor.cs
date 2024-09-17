using UnityEngine;
using System.Collections;

public class OrientationMonitor {

    // Draws a ray from Gameobject Hub to Tip's direction, starting from Tip.
    // this assumes that TargetZoneShape is a model of the target zone.
    // TargetZoneShape must have a child in its heirarchy - a Gameobject named "Orientation Window"
    // this is the target mesh that we test for collisions with that ray, 
    // and must have a Layermask that will be passed in.  
    // This monitor does not expect any particular shape.  
    // and is designed to be rendered like a guide if desired.

    // This monitor needs data to keep track of how much the needle traveled while in and out of the zone
    // therefore it should be implemented in parallel with a Depth Monitor


    private GameObject Hub;
    private GameObject Tip;

    private GameObject TargetGroup;
    private GameObject TargetSurface;
    private LayerMask TargetMask;

    private bool Active; //  Orientation Monitors are not always applicable for other approaches

    // this monitor is intended to be called with data from a Depth Monitor.

    private float Odometer_Total;
    private float Odometer_InZone;
    public float FractionOdometer_InZone;

    public bool InZone;
    public bool ShowInVisualization;

	// Use this for initialization
    public OrientationMonitor(GameObject aStart, GameObject aTip, GameObject target)
    {

        Hub = aStart;
        Tip = aTip;
        TargetGroup = target;
        TargetSurface = TargetGroup.transform.Find("Orientation Window").gameObject;

        TargetMask = 1 << TargetSurface.layer;

        Reset();

    }

    public void SetActive(bool theState)
    {
        Active = theState;
        if (TargetGroup.activeSelf != Active) TargetGroup.SetActive(Active);
    }

    // This is like an UPDATE.  It aggregates needle travel in and out of the zone. 
    public void AnalyzeAdvancement(float distance) // distance is available from a Depth Monitor 
    {
        Update();
        Odometer_Total += distance;
        if (InZone) Odometer_InZone += distance;
        FractionOdometer_InZone = Odometer_InZone / Odometer_Total;
    }

    public void Update()
    {
        TargetGroup.transform.position = Tip.transform.position; // purposely leaving out rotations

        // some temporary variables
        RaycastHit hit;

        // forward direction
        Vector3 forward = Tip.transform.position - Hub.transform.position;
        Ray lookingForward = new Ray(Hub.transform.position, forward);
        float tipDistance = Vector3.Distance(Hub.transform.position, Tip.transform.position);

        // InZone will be true or false depending on the raycast hit return against TargetMask.
        InZone = Physics.Raycast(lookingForward, out hit, tipDistance, TargetMask);
        
    }

    public void Reset()
    {
        Active = false;
        TargetGroup.SetActive(Active);
        Odometer_Total = 0;
        Odometer_InZone = 0;
        FractionOdometer_InZone = 1;
        InZone = false;
    }
}
