using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Needle_Tip_USPlane_Geometry_Mono_Assist : MonoBehaviour {
	[SerializeField]
	GameObject USPlane, USPlane1, USPlane2, needleTip, needleHub, USProbe; //testCube;
	US_NeedleShaftInPlaneMonitor USNSIPM;
	[SerializeField]
	bool test = false;
	// Use this for initialization
	void Start () {
		Needle_Tip_USPlane_Geometry.USPlane = USPlane;
		Needle_Tip_USPlane_Geometry.NeedleTip = needleTip;
		Needle_Tip_USPlane_Geometry.NeedleHub = needleHub;
		Needle_Tip_USPlane_Geometry.NeedleDiameter = 1f;
		Needle_Tip_USPlane_Geometry.USPlane1 = USPlane1;
		Needle_Tip_USPlane_Geometry.USPlane2 = USPlane2;
		//Needle_Tip_USPlane_Geometry.TestCube = testCube;
		USNSIPM = USProbe.GetComponent<US_NeedleShaftInPlaneMonitor>();
		Needle_Tip_USPlane_Geometry.US_Needle_Shaft_In_Plane_Monitor = USNSIPM;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(test)
		{
			test = false;
			Debug.Log(Needle_Tip_USPlane_Geometry.MinDistanceIntersectionToTip);
		}
	}
}
