using UnityEngine;
using System.Collections;

// detector for: When the needle pierces a tubular mesh, how centered is the trajectory of the needle?
// this uses the needle hub and tip analogy and tests backwall distance.
// if the backwall distance is small, then its a glancing blow, which is unlikely to lead to a good cannulation.

public class RobustPunctureMonitor {
	
	
	private GameObject Hub;
	private GameObject Tip;
	private LayerMask TargetMask;
	
	public float MeshDiameter;
	public float ExtrapolatedBackwallDistance;
	public float RobustnessIndex;
	
	public RobustPunctureMonitor (GameObject aStart, GameObject aTip, LayerMask target){
		
		Hub = aStart;
		Tip = aTip;
		TargetMask = target;
		

	}
	
	// this is intended to be called only once, when the needle penetrates a vessel mesh.
	// it does not need to be polled.
	public float Measure () {
	
		RaycastHit hit;
	
		// forward direction
		Vector3 forward = Tip.transform.position - Hub.transform.position;	
		Ray lookingForward = new Ray (Hub.transform.position, forward);
		float tipDistance = Vector3.Distance (Hub.transform.position, Tip.transform.position);
		
		// if we are indeed piercing the vein, then:
		// measure its diameter at the point of penetration, and
		// measure the extrapolated backwall depth
		// compare these two.
		
		// if we can't measure either of those due to mesh edge or strange errors, then dont run the algorithm
		bool isValid = true;
		
		if (Physics.Raycast (lookingForward, out hit, tipDistance, TargetMask)) {
			
			// how far from puncture to backwall?  
			Vector3 backwalltestPoint = lookingForward.GetPoint(100F);
			Ray lookingBack_needleAxis = new Ray (backwalltestPoint, (Tip.transform.position - backwalltestPoint));
			RaycastHit backwallhit;
			if (Physics.Raycast (lookingBack_needleAxis, out backwallhit, Mathf.Infinity, TargetMask))
			{
				ExtrapolatedBackwallDistance = Vector3.Distance(hit.point, backwallhit.point);
			} else {
				isValid = false;	
			}
			

			// What is vessel diameter at point of penetration?
			// use raycast to draw a ray back up the normal to find vessel diameter
			Vector3 behindNormaltestPoint = new Ray (hit.point, hit.normal).GetPoint(-100F);
			Ray lookingBack_normal = new Ray (behindNormaltestPoint, hit.normal);
			RaycastHit caliperhit;
			if (Physics.Raycast (lookingBack_normal, out caliperhit, Mathf.Infinity, TargetMask)) 
			{
				MeshDiameter = Vector3.Distance(caliperhit.point, hit.point);
			} else {
				isValid = false;
			}

		}
	
		// if we have enough information, then calculate a score.
		// this compares the thickness of the vessel against the distance to backwall.
		// if the backwall distance is less than the vessel diameter, which is an offset hit,
		// 		then the robustness index will be less than one.
		// if the backwall distance is greater than the diameter its one.
		// if its zero, then we were not able to get a diameter or a backwall distance for some reason 
		// perhaps we are at the end of a mesh looking out the tube or something
		// and the robustness index is zero, which should be interpreted as undetermined.
		if (isValid) {
			RobustnessIndex = ExtrapolatedBackwallDistance / MeshDiameter;
			if (RobustnessIndex > 1) RobustnessIndex = 1;
		} else {
			RobustnessIndex = 0;	
		}
		
		return RobustnessIndex;
	}
	
		
	
	
	public void Reset () {
		
		MeshDiameter = 0;
	  	ExtrapolatedBackwallDistance = 0;
	  	RobustnessIndex = 0;
		
	}
	
	
}
