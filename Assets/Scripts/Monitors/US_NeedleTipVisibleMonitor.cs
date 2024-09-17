using UnityEngine;
using System.Collections;
using SMMARTS_SDK;

// Basic detector for: is the needle tip in the insonating plane?
// This is difficult to accomplish without the ultrasound scripts themselves.
// The UltrasoundManager's minions of TransducerElements look specifically for a hit.collider.tag name of "Needle Tip"
// this script just looks at the UltrasoundManager for that variable.

public class US_NeedleTipVisibleMonitor {
	
	public bool TipIsVisible;

	// Use this for initialization
	public US_NeedleTipVisibleMonitor () {

	}
	
	// Update is called externally as needed 
	public bool Update () {
        TipIsVisible = SMMARTS_SDK.Ultrasound.UltrasoundManager.ME.NeedleTipInView;
        //TipIsVisible = SUltrasoundManager.ME.IsTipInView();
        return TipIsVisible;
	}

    
}
