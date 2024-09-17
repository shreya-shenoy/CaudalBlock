using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Needle_Tip_USPlane_Geometry
{
	static GameObject USP, USP1, USP2;
	static GameObject needleTip;
	static GameObject needleHub;
	static float r = 0.5f;
	static US_NeedleShaftInPlaneMonitor USNSIPM;
	//static bool usePositivePlane = false;
	static bool positivePlaneHit = false;
	static bool negativePlaneHit = false;
	static float minDistanceIntersectionToTip = 0;
	//static GameObject testCube;
	public static GameObject USPlane
	{
		get
		{
			return USP;
		}
		set
		{
			USP = value;
		}
	}
	public static GameObject USPlane1
	{
		get
		{
			return USP1;
		}
		set
		{
			USP1 = value;
		}
	}
	public static GameObject USPlane2
	{
		get
		{
			return USP2;
		}
		set
		{
			USP2 = value;
		}
	}
	public static GameObject NeedleTip
	{
		get
		{
			return needleTip;
		}
		set
		{
			needleTip = value;
		}
	}
	public static GameObject NeedleHub
	{
		get
		{
			return needleHub;
		}
		set
		{
			needleHub = value;
		}
	}
	public static float NeedleDiameter
	{
		get
		{
			return r * 2;
		}
		set
		{
			r = value / 2;
		}
	}
	/*public static GameObject TestCube
	{
		get
		{
			return testCube;
		}
		set
		{
			testCube = value;
		}
	}*/
	public static US_NeedleShaftInPlaneMonitor US_Needle_Shaft_In_Plane_Monitor
	{
		get
		{
			return USNSIPM;
		}
		set
		{
			USNSIPM = value;
		}
	}
	public static bool PositivePlaneHit
	{
		set
		{
			positivePlaneHit = value;
		}
	}
	public static bool NegativePlaneHit
	{
		set
		{
			negativePlaneHit = value;
		}
	}
	public static Vector3 IntersectionPoint1
	{
		get
		{
			return Intersection1();
		}
	}
	public static Vector3 IntersectionPoint2
	{
		get
		{
			return Intersection2();
		}
	}
	public static float MinDistanceIntersectionToTip
	{
		get
		{
			return minDistanceIntersectionToTip;
		}
	}
	
	/*public static bool UsePositivePlane
	{
		set
		{
			usePositivePlane = value;
		}
	}*/
	static /*float*/ void AMinDistanceIntersectionToTip(bool usePositivePlane)
	{
		//if (!(USNSIPM.GetNeedlePixelsInPlane > 0))
		//	return -1;
		Plane_ USPlane_;
		if (usePositivePlane)
			USPlane_ = new Plane_(USP1.transform.position, USP.transform.up);
		else
			USPlane_ = new Plane_(USP2.transform.position, USP.transform.up);
		Line line = new Line(NeedleTip.transform.position,needleHub.transform.position);
		Vector3 intersection = Vector3.zero;
		try
		{
			intersection = Plane_Line_Solutions.LinePlaneIntersection(line, USPlane_);
		}
		catch (System.Exception e)
		{
			Debug.Log(e);
			return;
			//return Plane_Line_Solutions.MinPointPlaneDistance(needleTip.transform.position, USPlane_);
		}
		//testCube.transform.position = intersection;
		//Debug.Log(intersection.ToString());
		//Debug.Log(USPlane_.Normal.ToString() + " " + USPlane_.Constant + " " + USPlane_.Point.ToString() + " " + line.Direction.ToString() + " " + line.Point.ToString());
		float D = Vector3.Distance(intersection, needleTip.transform.position);
		float theta = Vector3.Angle(USPlane_.Normal, line.Direction);
		float thetaR = theta * Mathf.PI / 180;
		float N = D * Mathf.Cos(thetaR);
		float O = Mathf.Sqrt(D * D - N * N);
		float R = r / Mathf.Cos(thetaR);
		float T = O - R;
		float Min = Mathf.Sqrt(T * T + N * N);
		if(D<0||Min<0)
			Debug.Log(D + " " + theta + " " + N + " " + O + " " + R + " " + T + " " + Min);
		float AminDistanceIntersectionToTip = 0;
		if (D > Min)
			//return Min;
			AminDistanceIntersectionToTip = Min;
		//return D;
		AminDistanceIntersectionToTip = D;
		if (usePositivePlane)
			minDistanceIntersectionToTip = AminDistanceIntersectionToTip;
		else if (AminDistanceIntersectionToTip < minDistanceIntersectionToTip)
			minDistanceIntersectionToTip = AminDistanceIntersectionToTip;
	}
	static Vector3 Intersection1()
	{
		minDistanceIntersectionToTip = float.MaxValue;
		Plane_ USPlane_ = new Plane_(USP1.transform.position, USP.transform.up);
		Line line = new Line(NeedleTip.transform.position, needleHub.transform.position);
		Vector3 intersection = Vector3.zero;
		try
		{
			intersection = Plane_Line_Solutions.LinePlaneIntersection(line, USPlane_);
		}
		catch (System.Exception e)
		{
			Debug.Log(e);
			positivePlaneHit = false;
			return needleTip.transform.position;
		}
		positivePlaneHit = true;
		AMinDistanceIntersectionToTip(positivePlaneHit);
		return intersection;
	}
	static Vector3 Intersection2()
	{
		Plane_ USPlane_ = new Plane_(USP2.transform.position, USP.transform.up);
		Line line = new Line(NeedleTip.transform.position, needleHub.transform.position);
		Vector3 intersection = Vector3.zero;
		try
		{
			intersection = Plane_Line_Solutions.LinePlaneIntersection(line, USPlane_);
		}
		catch (System.Exception e)
		{
			Debug.Log(e);
			negativePlaneHit = false;
			return needleTip.transform.position;
		}
		negativePlaneHit = true;
		AMinDistanceIntersectionToTip(!negativePlaneHit);
		return intersection;
	}
}
