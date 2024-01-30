using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// DL: end monitoring does nothing. 

public class Needle_Entry_Angle_Monitor : MonoBehaviour
{
    //public static Needle_Entry_Angle_Monitor ME;
    /*
    private void Awake()
    {
        if (ME != null)
            Destroy(ME);
        ME = this;
    }
    */
    public float needleEntryAngle = -1; // { get; private set; } 
    bool measuring = false;
    bool lastFrameNeedleInVein = false;
    bool needleInVein = false;
    [SerializeField]
    GameObject needleTip = null, needleHub = null;
    [SerializeField]
    LayerMask layerMask = 0;

    public Needle_Entry_Angle_Monitor(GameObject thisTip, GameObject thisHub, LayerMask thisLayerMask)
    {
        needleTip = thisTip;
        needleHub = thisHub;
        layerMask = thisLayerMask;
    }

    public void StartMonitoring()
    {
        measuring = true;
        needleEntryAngle = -1;
    }
    private void Update()
    {
        if ((!measuring)||needleEntryAngle>-1)
            return;
        needleInVein = IV_Manager.ME.NeedleInVein();
        if ((!lastFrameNeedleInVein) && needleInVein)  // lastFrameNeedleInVein will always be false because its never set to true.
        {
            RaycastHit layerHit;
            if (!Physics.Raycast(needleHub.transform.position, needleTip.transform.position -
                needleHub.transform.position, out layerHit,
                Vector3.Distance(needleTip.transform.position,
                needleHub.transform.position), layerMask))
                return;
            needleEntryAngle = Vector3.Angle(needleHub.transform.position -
                needleTip.transform.position, layerHit.normal);
        }
    }
    public void EndMonitoring()
    {
        measuring = false; 
    }
}
