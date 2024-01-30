using UnityEngine;
using SMMARTS_SDK;
public class Blind_Advancement : MonoBehaviour
{
	[SerializeField]
	bool score = false;
	[SerializeField]
	GameObject needleHub, needleTip, USPlaneCenterPoint, USProbeTip, USProbeHub;
	GameObject /*needleHub, needleTip,*/ planePoint1, planePoint2, planePoint3, planePoint4, planePoint5, planePoint6, planePoint7, planePoint8;//, USPTip, USPHub;
	[SerializeField]
	LayerMask skinLayer;
	[SerializeField]
	float scorePenalty = 5f;
	float penalty = 5f;
	[SerializeField]
	float totalPenalty = 0;
	float overallPenalty = 0;
	[SerializeField]
	float totalOutOfLinePenalty = 0;
	int totalOffAdvancements = 0;
	[SerializeField]
	int[] depths = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
	int[] theDepths = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
	bool needleInSkin = false;
	Vector3 skinPoint;
	Vector3 skinDirection;
	float lastTipDepth = 0;
	[SerializeField]
	bool clear = false;
	int incrementMeasurement = 1;
	bool initialized = false;
	float distanceBeyondInsonatingPlane = 0;
	float distanceFromInsonatingPlane = 0;
	bool inLineWithUSPlane = false;
	float ignoreDist = 0;//mm
						 //bool lastSidePast = false;
	public int[] Advancements
	{
		get
		{
			return theDepths;
		}
	}
	public float CurrentDepth
	{
		get
		{
			return Depth();
		}
	}
	public int TotalOffAdvancements
	{
		get
		{
			return totalOffAdvancements;
		}
	}
	public float DistanceBeyondInsonatingPlane
	{
		get
		{
			return distanceBeyondInsonatingPlane;
		}
	}
	public void InitializeValues(GameObject needle, GameObject probe, int IncrementMeasurement, LayerMask skin, float ignoreDistance = 0)
	{
		needleHub = GameObject.Find("NeedleHubMarker");
		needleTip = GameObject.Find("NeedleTipMarker");
		USPlaneCenterPoint = GameObject.Find("US Probe Center Point");
		USProbeTip = GameObject.Find("US Probe Tip");
		USProbeHub = GameObject.Find("US Probe Hub");
		incrementMeasurement = IncrementMeasurement;
		skinLayer = skin;
		PlaneGeometry();
		initialized = true;
		lastTipDepth = Depth();
		lastTotDepth = Depth();
		ignoreDist = ignoreDistance;
	}
	void Update()
	{
		if (initialized)
		{
			if (score)
			{
				score = false;
				Score();
			}
			if (clear)
			{
				clear = false;
				Clear();
			}
			Score();
			penalty = scorePenalty;
			depths = theDepths;
			totalPenalty = overallPenalty;
			totalOutOfLinePenalty = totalOffAdvancements;
			//Debug.Log(Needle_Tip_USPlane_Geometry.US_Needle_Shaft_In_Plane_Monitor.GetNeedlePixelsInPlane);
		}
	}
	void PlaneGeometry()
	{
		planePoint1 = USPlaneCenterPoint.transform.GetChild(0).gameObject;
		planePoint1.transform.localPosition = planePoint1.transform.localPosition + new Vector3(0, SMMARTS_SDK.Ultrasound.UltrasoundManager.ME.BeamThickness / 2 + Needle_Tip_USPlane_Geometry.NeedleDiameter / 2, 0);
		planePoint2 = USPlaneCenterPoint.transform.GetChild(1).gameObject;
		planePoint2.transform.localPosition = planePoint2.transform.localPosition + new Vector3(0, SMMARTS_SDK.Ultrasound.UltrasoundManager.ME.BeamThickness / 2 + Needle_Tip_USPlane_Geometry.NeedleDiameter / 2, 0);
		planePoint3 = USPlaneCenterPoint.transform.GetChild(2).gameObject;
		planePoint3.transform.localPosition = planePoint3.transform.localPosition + new Vector3(0, SMMARTS_SDK.Ultrasound.UltrasoundManager.ME.BeamThickness / 2 + Needle_Tip_USPlane_Geometry.NeedleDiameter / 2, 0);
		planePoint4 = USPlaneCenterPoint.transform.GetChild(3).gameObject;
		planePoint4.transform.localPosition = planePoint4.transform.localPosition + new Vector3(0, SMMARTS_SDK.Ultrasound.UltrasoundManager.ME.BeamThickness / 2 + Needle_Tip_USPlane_Geometry.NeedleDiameter / 2, 0);
		planePoint5 = USPlaneCenterPoint.transform.GetChild(4).gameObject;
		planePoint5.transform.localPosition = planePoint5.transform.localPosition + new Vector3(0, -(SMMARTS_SDK.Ultrasound.UltrasoundManager.ME.BeamThickness / 2 + Needle_Tip_USPlane_Geometry.NeedleDiameter / 2), 0);
		planePoint6 = USPlaneCenterPoint.transform.GetChild(5).gameObject;
		planePoint6.transform.localPosition = planePoint6.transform.localPosition + new Vector3(0, -(SMMARTS_SDK.Ultrasound.UltrasoundManager.ME.BeamThickness / 2 + Needle_Tip_USPlane_Geometry.NeedleDiameter / 2), 0);
		planePoint7 = USPlaneCenterPoint.transform.GetChild(6).gameObject;
		planePoint7.transform.localPosition = planePoint7.transform.localPosition + new Vector3(0, -(SMMARTS_SDK.Ultrasound.UltrasoundManager.ME.BeamThickness / 2 + Needle_Tip_USPlane_Geometry.NeedleDiameter / 2), 0);
		planePoint8 = USPlaneCenterPoint.transform.GetChild(7).gameObject;
		planePoint8.transform.localPosition = planePoint8.transform.localPosition + new Vector3(0, -(SMMARTS_SDK.Ultrasound.UltrasoundManager.ME.BeamThickness / 2 + Needle_Tip_USPlane_Geometry.NeedleDiameter / 2), 0);
	}
	float lastTotDepth = 0;
	float currentTotDepth = 0;
	[SerializeField]
	int totalTotAdvancements = 0;
	public int TotalAdvancements
	{
		get
		{
			return totalTotAdvancements;
		}
	}
	void Score()
	{
		//if (CVA_Scorer.ME.NeedleInSkin)
		//{
		//if (!needleInSkin)
		if (!SetSkinEntryPoint())
			return;
		//else
		//	CheckNeedleAngle();
		///MAKE CALCULATE BLIND DEPTH HERE
		float currentDepth = Depth();
		if (currentDepth < ignoreDist)
		{
			lastTipDepth = ignoreDist;
			lastTotDepth = ignoreDist;
			return;
		}
		//Debug.Log(currentDepth);
		if (true) //Should be needle tip in view. Changed to this so that I could run the program
		{
			//Debug.Log("TIP IN VIEW");
			lastTipDepth = currentDepth;
			distanceBeyondInsonatingPlane = 0;
			distanceFromInsonatingPlane = 0;
		}
		else if (currentDepth - lastTipDepth >= incrementMeasurement)
		{
			if (PlaneColliderHit())
			{
				inLineWithUSPlane = true;
				ApplyInLinePenalty();
			}
			else
			{
				inLineWithUSPlane = false;
				distanceFromInsonatingPlane = 0;
				distanceBeyondInsonatingPlane = 0;
				ApplyOffPenalty();
			}
			lastTipDepth = currentDepth;
		}
		else if (currentDepth < lastTipDepth)
			lastTipDepth = currentDepth;
		//}
		currentTotDepth = currentDepth;
		if (currentTotDepth - lastTotDepth >= incrementMeasurement)
		{
			//Debug.Log(currentTotDepth + " " + lastTotDepth + " " + incrementMeasurement);
			totalTotAdvancements++;
			lastTotDepth = currentTotDepth;
		}
		else if (currentTotDepth < lastTotDepth)
			lastTotDepth = currentTotDepth;
	}
	bool SetSkinEntryPoint()
	{
		RaycastHit[] hits = Physics.RaycastAll(needleHub.transform.position, needleTip.transform.position - needleHub.transform.position,
		Vector3.Distance(needleTip.transform.position, needleHub.transform.position), skinLayer);
		if (hits.Length <= 0)
			return false;
		RaycastHit skinHit = hits[0];
		skinPoint = skinHit.point;
		skinDirection = needleTip.transform.position - needleHub.transform.position;
		return true;
	}
	void CheckNeedleAngle()
	{
		if (Vector3.Angle(skinDirection, needleTip.transform.position - needleHub.transform.position) > 5)
		{
			SetSkinEntryPoint();
		}
	}
	float Depth()
	{
		return Vector3.Distance(skinPoint, needleTip.transform.position);
	}
	void ApplyInLinePenalty()
	{
		float distanceFromUSPlane = Needle_Tip_USPlane_Geometry.MinDistanceIntersectionToTip;
		// Debug.Log(distanceFromUSPlane);
		int dist = (int)distanceFromUSPlane;
		dist /= incrementMeasurement;
		if (dist >= theDepths.Length / 2 - 1)
			dist = theDepths.Length / 2 - 1;
		if (dist < 0)
		{
			//Debug.Log(dist);
			dist = 0;
		}
		if (Needle_Tip_USPlane_Geometry.US_Needle_Shaft_In_Plane_Monitor.GetNeedlePixelsInPlane > 0) ///Should be beyond the plane, not just shown by this to be beyond the plane
		{
			theDepths[dist + theDepths.Length / 2]++;
			distanceBeyondInsonatingPlane = dist * incrementMeasurement;
			distanceFromInsonatingPlane = 0;
			//Debug.Log("PAST");
			overallPenalty += dist * dist * penalty / 100f;

			//Debug.Log(dist * dist * penalty / 100);
		}
		else
		{
			distanceFromInsonatingPlane = dist * incrementMeasurement;
			distanceBeyondInsonatingPlane = 0;
			theDepths[theDepths.Length / 2 - dist - 1]++;
			//Debug.Log("NOT");
		}
	}
	void ApplyOffPenalty()
	{
		totalOffAdvancements++;
	}
	bool PlaneColliderHit()
	{
		bool negativeHit = false;
		bool positiveHit = false;
		/*RaycastHit[] hits = Physics.RaycastAll(needleHub.transform.position, needleTip.transform.position - needleHub.transform.position, 1000f);
		foreach(RaycastHit hit in hits)
		{
			if (hit.transform.name == plane.transform.name)
				return true;
		}
		return false;*/
		Vector3 d12 = planePoint2.transform.position - planePoint1.transform.position;
		Vector3 d14 = planePoint4.transform.position - planePoint1.transform.position;
		Vector3 d32 = planePoint2.transform.position - planePoint3.transform.position;
		Vector3 d34 = planePoint4.transform.position - planePoint3.transform.position;
		Vector3 intersection = Needle_Tip_USPlane_Geometry.IntersectionPoint1;
		Vector3 d1i = intersection - planePoint1.transform.position;
		Vector3 d3i = intersection - planePoint3.transform.position;
		float angle1214 = Vector3.Angle(d12, d14);
		float angle3234 = Vector3.Angle(d32, d34);
		float angle121i = Vector3.Angle(d12, d1i);
		float angle141i = Vector3.Angle(d14, d1i);
		float angle323i = Vector3.Angle(d32, d3i);
		float angle343i = Vector3.Angle(d34, d3i);
		//Debug.Log(intersection.ToString() + " " + angle1214 + " " + angle3234 + " " + angle121i + " " + angle141i + " " + angle323i + " " + angle343i);
		if ((angle1214 > angle141i && angle1214 > angle121i) && (angle3234 > angle343i && angle3234 > angle323i))
			//	return true;
			//return false;
			positiveHit = true;


		Vector3 d56 = planePoint6.transform.position - planePoint5.transform.position;
		Vector3 d58 = planePoint8.transform.position - planePoint5.transform.position;
		Vector3 d76 = planePoint6.transform.position - planePoint7.transform.position;
		Vector3 d78 = planePoint8.transform.position - planePoint7.transform.position;
		Vector3 intersection2 = Needle_Tip_USPlane_Geometry.IntersectionPoint2;
		Vector3 d5i = intersection2 - planePoint5.transform.position;
		Vector3 d7i = intersection2 - planePoint7.transform.position;
		float angle5658 = Vector3.Angle(d56, d58);
		float angle7678 = Vector3.Angle(d76, d78);
		float angle565i = Vector3.Angle(d56, d5i);
		float angle585i = Vector3.Angle(d58, d5i);
		float angle767i = Vector3.Angle(d76, d7i);
		float angle787i = Vector3.Angle(d78, d7i);
		//Debug.Log(intersection2.ToString() + " " + angle5658 + " " + angle7678 + " " + angle565i + " " + angle585i + " " + angle767i + " " + angle787i);
		if ((angle5658 > angle585i && angle5658 > angle565i) && (angle7678 > angle787i && angle7678 > angle767i))
			//	return true;
			//return false;
			negativeHit = true;

		if (positiveHit || negativeHit)
		{
			return true;
		}
		return false;
	}
	public void Clear()
	{
		theDepths = new int[theDepths.Length];
		overallPenalty = 0;
		totalOffAdvancements = 0;
		needleInSkin = false;
		skinPoint = Vector3.zero;
		skinDirection = Vector3.zero;
		lastTipDepth = ignoreDist;
		lastTotDepth = ignoreDist;
		totalTotAdvancements = 0;
	}
}