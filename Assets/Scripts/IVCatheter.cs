using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IVCatheter : MonoBehaviour {

    // Sets the IV catheter position based on catheter sensor, which is not tied directly to the catheter for a good reason.
    // Sets hierarchy of catheter (initially the needle, then to the gel block when released)


    [SerializeField]
    GameObject ProxyCatheterSensor;

    [SerializeField]
    GameObject TargetPlane;

    [SerializeField]
    LayerMask PlaneMask; 

    [SerializeField]
    GameObject TargetCube;

    [SerializeField]
    Transform BasePosition;

    [SerializeField]
    Transform TipPosition;

    [SerializeField]
    Transform SkinTransform;

    [SerializeField]
    Transform NeedleTransform;

    [Range(0, 100f)]
    public float ProxyDistance;

    [Range(0, 100f)]
    public float MinProxyDistance;

    [Range(0, 100f)]
    public float MaxProxyDistance;

    [Range(0, 1.0f)]
    public float PositionFraction;

    [SerializeField]
    public bool OnNeedle;

    [SerializeField]
    public bool CatheterFullySeated;

    // Use this for initialization
    void Start () {
		
	}
	
    

	// Update is called once per frame
	void Update () {

        /*// shoot a ray from the catheter sensor to the target plane
        RaycastHit hit;
        Vector3 direction = TargetCube.transform.position - transform.position;
        Physics.Raycast(ProxyCatheterSensor.transform.position, direction, out hit, 1500F, PlaneMask);

        if (hit.distance == 0 || hit.distance > MaxProxyDistance)
        {
            OnNeedle = false;
            CatheterFullySeated = false;
            transform.parent = SkinTransform;
        } else
        {
            OnNeedle = true;
            transform.parent = NeedleTransform;
            ProxyDistance = hit.distance;
            PositionFraction = Mathf.InverseLerp(MinProxyDistance, MaxProxyDistance, ProxyDistance);
            CatheterFullySeated = (PositionFraction <= 0.03F); // if more than 1mm forward on needle, cath is not seated.
            transform.position = Vector3.Lerp(BasePosition.position, TipPosition.position, PositionFraction);
            transform.rotation = BasePosition.rotation;



        }
        
       
    */
	}
    
}
