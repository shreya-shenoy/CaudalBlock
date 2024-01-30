using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// -------------- PURPOSE and ORIGIONAL PROBLEM -----------------------
// So, the new CVA scoring algorithm and IT needs to know if you are doing a short axis or long axis approach.
// What is short and long axis?  It depends on anatomy.  We note a few examples of short and long axis transforms,
// and compare the ultrasound probe's rotation to those examples.  The closest match should indicate which view you are using.
// This works independend of the way you hold the probe (could be backwards 180 degrees) so every example is part of a pair.  
// This script doesn't - and shouldn't - know what backwards is; it could be intentional and the user may have flipped the US screen.
// On the CVA this works very well - independent of tilt and rock and it works on both anatomical blocks. I can't seem to confuse it.
// We anticipate other approches (like oblique) can be added later but for now we focus on short & long axis.
// Also, note this monitor is a MonoBehaviour.  Most of the other monitors are not.  
// The easiest way to load the example transforms into the code is via the property inspector;
// I dont see the need to force this script to look like the other monitors simply for the sake of it.
// This means that the monitor will be referenced by the scoring algorithm a little differently.  
// Hope that isn't too vexing.
// - Dave Lizdas 6/2/17

// -------------- DEPENDENCIES ----------------------------------------
// 1) An Ultrasound Probe.  Hopefully thats obvious...
// 2) Four transforms that represent Short Axis and Long Axis probe orientations; parent is Anatomical Model under Anatomy.
//      To get these, I copied the US Probe; I deleted every child but the renderer and moved it 
//      in the scene view over the anatomy, 
//      lined it up over the IJ vein across the neck looking straight down like an ideal short axis position.
//      then I copied that GO and rotated it 180 degrees to get an example of holding the probe the other way.
//      and then I did the same for the long axis views.
//      Then I deleted all the children and components and moved them under the "Anatomy" GO, 
//      so they will move with the anatomy if it rotates with different anatomic blocks.
//      I put them in a prefab named "Short Long Axis Examples"

// --------------- HOW OTHER SCRIPTS REFERENCE ------------------------
// on initialization: this script should be a component of the GO that has the CVA scorer
// AxisMonitor AxisMonitor = GetComponent<AxisMonitor>();
// 
// during use: 
// if (AxisMonitor.IsShort()) {


public class AxisMonitor : MonoBehaviour
{
    [Header("------------ Initialization ------------ ")]

    // private serialized fields initialized through the propery inspector 
    [SerializeField]
    Transform ShortAxis1;

    [SerializeField]
    Transform ShortAxis2;

    [SerializeField]
    Transform LongAxis1;

    [SerializeField]
    Transform LongAxis2;

    [SerializeField]
    GameObject UltrasoundProbe;

    [Header("------------ Live Outputs ------------ ")]

    // this is nice to see in the propery inspector - verification that it works
    [SerializeField]
    private Axis View;

    // handy way to encode axis 
    private enum Axis { Short, Long, Oblique, Unknown };


    // Use this for initialization
    void Start()
    {
        // Null checks:
        if (ShortAxis1 == null) Debug.LogAssertion("ShortAxis1 is Null.");
        if (ShortAxis2 == null) Debug.LogAssertion("ShortAxis2 is Null.");
        if (LongAxis1 == null) Debug.LogAssertion("LongAxis1 is Null.");
        if (LongAxis2 == null) Debug.LogAssertion("LongAxis2 is Null.");
        if (UltrasoundProbe == null) Debug.LogAssertion("UltrasoundProbe is Null.");

        View = Axis.Unknown;
    }

    // --------------------------- Accessor Methods ---------------------------
    public bool IsShort
    {
        get
        {
            return View == Axis.Short;
        }
    }

    public bool IsLong
    {
        get
        {
            return View == Axis.Long;
        }
    }

    // -------------------------- No Mutator Methods --------------------------


    // Called every frame
    void Update()
    {
        // how close is the probe to two short axis example rotations?  
        float ShortAxis1OffsetAngle = Mathf.Abs(Quaternion.Angle(UltrasoundProbe.transform.rotation, ShortAxis1.rotation)); // degrees
        float ShortAxis2OffsetAngle = Mathf.Abs(Quaternion.Angle(UltrasoundProbe.transform.rotation, ShortAxis2.rotation)); // degrees

        // which short axis example is closer to the probe?
        float ShortAxisOffsetAngle = ShortAxis1OffsetAngle; // degrees
        if (ShortAxis2OffsetAngle < ShortAxis1OffsetAngle) ShortAxisOffsetAngle = ShortAxis2OffsetAngle; // degrees

        // how close is the probe to two long axis example rotations?
        float LongAxis1OffsetAngle = Mathf.Abs(Quaternion.Angle(UltrasoundProbe.transform.rotation, LongAxis1.rotation)); // degrees
        float LongAxis2Angle = Mathf.Abs(Quaternion.Angle(UltrasoundProbe.transform.rotation, LongAxis2.rotation)); // degrees

        // which long axis example is closer to the probe?
        float LongAxisAngle = LongAxis1OffsetAngle; // degrees
        if (LongAxis2Angle < LongAxis1OffsetAngle) LongAxisAngle = LongAxis2Angle; // degrees

        // we can now judge the view of the probe.  
        // Note its ">" instead of "<".  Thats the opposite of what I expected but the script works perfectly. 
        if (ShortAxisOffsetAngle > LongAxisAngle) // degrees
            View = Axis.Short;
        if (LongAxisAngle > ShortAxisOffsetAngle) // degrees
            View = Axis.Long;
        if (ShortAxisOffsetAngle == LongAxisAngle) // degrees
            View = Axis.Unknown;

    }

}
