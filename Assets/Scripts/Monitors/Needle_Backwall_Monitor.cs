using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Needle_Backwall_Monitor : MonoBehaviour
{
    public static Needle_Backwall_Monitor ME;
    private void Awake()
    {
        if (ME != null)
            Destroy(ME);
        ME = this;
    }
    [SerializeField]
    GameObject needleTip = null, needleHub = null;
    bool monitoring = false;
    public bool BackwallDetected { get; private set; } = false;
    [SerializeField]
    LayerMask vessel = 0;//Made Vein/Artery in inspector.
    Vector3 direction = Vector3.zero;
    float distance = 0;
    public void StartMonitoring()
    {
        monitoring = true;
        BackwallDetected = false;
        distance = Vector3.Distance(needleHub.transform.position, needleTip.transform.position);
        Debug.Log(distance);
    }
    private void Update()
    {
        
        if ((!monitoring)||BackwallDetected)
            return;
        //Debug.DrawLine(needleTip.transform.position, needleHub.transform.position, Color.red, 2.0f);

        if (Physics.Raycast(needleTip.transform.position, needleHub.transform.position - needleTip.transform.position, distance, vessel)){
            Debug.Log("BACKWALL");
            BackwallDetected = true;
        }
            
        
    }
    public void EndMonitoring()
    {
        monitoring = false;
    }
}
