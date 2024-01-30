using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestNeedleInteraction : MonoBehaviour {

    public Camera cam;

    public RaycastHit hit;

   // public int HitTriangleIndex;

    public LayerMask VesselLayermask;

    public Transform TestHub, TestTip;

    [Range(0, 2)]
    public float TestPressure;

    [Range(-1, 1)]
    public float TestSlide;

    [Range(0, 10)]
    public int TestSpreadIndex;



    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void LateUpdate () {

        // test from screen
        Ray ray1 = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray1, out hit, VesselLayermask))
        {
          //  hit.collider.gameObject.GetComponent<DynamicVessel>().OutsidePressAgainst(hit, 1.0F, 0, 0.25F);
        }

        // test from testhub/tip
        Ray ray2 = new Ray(TestHub.position, (TestTip.position - TestHub.position));
        if (Physics.Raycast(ray2, out hit, VesselLayermask))
        {
           // hit.collider.gameObject.GetComponent<DynamicVessel>().OutsidePressAgainst(hit, TestPressure, TestSlide, TestSpreadIndex);
        }

        Debug.DrawRay(TestHub.position, (TestTip.position - TestHub.position));
    }
}
