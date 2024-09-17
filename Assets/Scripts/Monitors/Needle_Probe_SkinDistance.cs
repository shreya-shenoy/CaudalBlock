using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Aseptic No-Touch Technique - from Arvin Trippensee
//

public class Needle_Probe_SkinDistance : MonoBehaviour
{

    public LayerMask SkinLayer;
    public Transform Hub, Tip;
    public Transform USProbe_LeftEdge, USProbe_RightEdge, USProbe_Middle, USProbe_Center;
    public LayerMask ProbeLayer;

    public float MinimumAllowableDistance;

    public bool Alert;
    public float AlertRate;
    private float nextAlert;

    public GameObject testsphere, testsphere2, testsphere3;

    public float DistanceToProbe;

    public bool ProbeOnSkin;

    // lets get some temp variables out of the way.
    RaycastHit hit;
    Vector3 forwardVector;
    Ray rayLookingForward;
    float castDistance;
    float SpherecastRadius;


    // Start is called before the first frame update
    void Start()
    {

    }


    // Update is called once per frame
    void Update()
    {

        // first lets detect if the probe is touching the skin. if the probe isnt close to the skin then we wont test at all.
        ProbeOnSkin = ProbeTouchingSkin();

        if (!ProbeOnSkin)
        {
            DistanceToProbe = 9999; // i.e. something very large.
            // Sanity check:
            testsphere.transform.position = Vector3.zero;
            testsphere2.transform.position = Vector3.zero;
            testsphere3.transform.position = Vector3.zero;
        }

        if (ProbeOnSkin)
        {
            // first, lets determine what point on the needle we should consider for the test. this will be Vector3 NeedleTestPoint.
            // if the needle is not poking into the skin, we use the needle tip. NeedleTestPoint = NeedleTip.
            // if the needle is poking into the skin, we use the skin entry point. NeedleTestPoint = SkinPuncturePoint.

            Vector3 NeedleTestpoint = Tip.position;

            // forward direction

            forwardVector = Tip.position - Hub.position;
            rayLookingForward = new Ray(Hub.position, forwardVector);
            castDistance = Vector3.Distance(Hub.position, Tip.position);

            if (Physics.Raycast(rayLookingForward, out hit, castDistance, SkinLayer))
            {
                NeedleTestpoint = hit.point;
            }

            // Sanity check:
            testsphere.transform.position = NeedleTestpoint;

            // after needle test point is established, we find closest point on a line that defines the probe face.
            // we define the line using USProbe Left and Right edge markers. 
            Vector3 closestPointOnProbeCenterline = NearestPointOnFiniteLine(USProbe_LeftEdge.position, USProbe_RightEdge.position, NeedleTestpoint);

            // Sanity check:
            testsphere2.transform.position = closestPointOnProbeCenterline;

            // so far we have a point where the needle disappears under the skin and the nearest point on the centerline of the probe face. 
            // the distance between these two points gets us close, but it doesn't account for the thickness of the probe or tilting the probe.
            // now, lets spherecast from the needle to the closestPointOnProbeCenterline.

            Vector3 closestpointonprobesurface = closestPointOnProbeCenterline;

            forwardVector = closestPointOnProbeCenterline - NeedleTestpoint;
            rayLookingForward = new Ray(NeedleTestpoint, forwardVector);
            castDistance = Vector3.Distance(NeedleTestpoint, closestPointOnProbeCenterline);
            SpherecastRadius = 1.5f;

            if (Physics.SphereCast(rayLookingForward, SpherecastRadius, out hit, castDistance, ProbeLayer)) // spherecast radius 1.5mm worked great here.
            {
                closestpointonprobesurface = hit.point;
            }

            // Sanity check:
            testsphere3.transform.position = closestpointonprobesurface;


            DistanceToProbe = Vector3.Distance(closestpointonprobesurface, NeedleTestpoint);
        }


        // if that distance is less than the MinimumAllowableDistance, toast an alert. 
        // maybe do some other things too.
        if (DistanceToProbe < MinimumAllowableDistance)
        {
            Alert = true;

            if (Time.time > nextAlert)
            {
                nextAlert = Time.time + AlertRate;
                Toast.Show(this, "The probe is too close to the needle!", 5, Toast.Type.BLUE, Toast.Gravity.BOTTOM);
                Debug.Log("Aseptic No Touch Technique: needle too close to probe");
            }


        }
        else
        {
            Alert = false;
        }



    }


    bool ProbeTouchingSkin()
    {
        // first assume its not.
        ProbeOnSkin = false;

        // we'll approximate the probe shape with four spherecasts.  This radius works with a tilted probe as well as one held straight up.
        SpherecastRadius = 2.5f;

        if (!ProbeOnSkin)
        {
            forwardVector = USProbe_Middle.position - USProbe_Center.position;
            rayLookingForward = new Ray(USProbe_Center.position, forwardVector);
            castDistance = Vector3.Distance(USProbe_Center.position, USProbe_Middle.position);
            ProbeOnSkin = Physics.SphereCast(rayLookingForward, SpherecastRadius, castDistance, SkinLayer);
        }

        if (!ProbeOnSkin)
        {
            forwardVector = USProbe_LeftEdge.position - USProbe_Center.position;
            rayLookingForward = new Ray(USProbe_Center.position, forwardVector);
            castDistance = Vector3.Distance(USProbe_Center.position, USProbe_LeftEdge.position);
            ProbeOnSkin = Physics.SphereCast(rayLookingForward, SpherecastRadius, castDistance, SkinLayer);
        }

        if (!ProbeOnSkin)
        {
            forwardVector = USProbe_RightEdge.position - USProbe_Center.position;
            rayLookingForward = new Ray(USProbe_Center.position, forwardVector);
            castDistance = Vector3.Distance(USProbe_Center.position, USProbe_RightEdge.position);
            ProbeOnSkin = Physics.SphereCast(rayLookingForward, SpherecastRadius, castDistance, SkinLayer);
        }

        if (!ProbeOnSkin)
        {
            forwardVector = USProbe_RightEdge.position - USProbe_LeftEdge.position;
            rayLookingForward = new Ray(USProbe_LeftEdge.position, forwardVector);
            castDistance = Vector3.Distance(USProbe_LeftEdge.position, USProbe_RightEdge.position);
            ProbeOnSkin = Physics.SphereCast(rayLookingForward, SpherecastRadius, castDistance, SkinLayer);
        }

        return ProbeOnSkin;
    }


    public static Vector3 NearestPointOnFiniteLine(Vector3 start, Vector3 end, Vector3 pnt)
    {
        var line = (end - start);
        var len = line.magnitude;
        line.Normalize();

        var v = pnt - start;
        var d = Vector3.Dot(v, line);
        d = Mathf.Clamp(d, 0f, len);
        return start + line * d;
    }

}