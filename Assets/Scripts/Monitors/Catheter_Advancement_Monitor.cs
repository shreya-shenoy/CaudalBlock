using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Catheter_Advancement_Monitor : MonoBehaviour
{
    public static Catheter_Advancement_Monitor ME;
    private void Awake()
    {
        if (ME != null)
        {
            Destroy(ME);
        }
        ME = this;
    }
    [SerializeField]
    GameObject catheter = null, /*catheterTip = null,
        catheterHub = null,*/ needleHub = null, needleTip = null, targetVein = null;
    Vector3 initialNeedleTipPosition = Vector3.zero;
    public bool NeedleMovedWhileMeasuring = false;
    public bool NeedleLeftVeinDuringMeasurement = false;
    public float MaximumAllowedTipMovement = 0;
    float hubToTipDistance = 0;
    bool measuring = false;
    public float CatheterAdvancementDistance = 0; //{ get; private set; } = 0;
    Vector3 initialCatheterPosition = Vector3.zero;
    public void StartAdvancementMeasurement(float maximumAllowedTipMovement, GameObject targetVein)
    {
        NeedleMovedWhileMeasuring = false;
        NeedleLeftVeinDuringMeasurement = false;
        MaximumAllowedTipMovement = maximumAllowedTipMovement;
        initialNeedleTipPosition = needleTip.transform.position;
        this.targetVein = targetVein;
        hubToTipDistance = Vector3.Distance(needleTip.transform.position, needleHub.transform.position);
        CatheterAdvancementDistance = 0;
        initialCatheterPosition = needleHub.transform.InverseTransformPoint(catheter.transform.position);
        measuring = true;
    }
    public bool EndAdvancmentMeasurement()
    {
        measuring = false;
        //Debug.Log(NeedleLeftVeinDuringMeasurement + " " + NeedleMovedWhileMeasuring);
        return !(NeedleMovedWhileMeasuring || NeedleLeftVeinDuringMeasurement);
    }
    private void Update()
    {
        if (measuring)
        {
            if (Vector3.Distance(needleTip.transform.position, initialNeedleTipPosition) > MaximumAllowedTipMovement)
            {
                NeedleMovedWhileMeasuring = true;
            }
            if (!NeedleLeftVeinDuringMeasurement)
            {
                RaycastHit[] hits = Physics.RaycastAll(needleHub.transform.position, needleTip.transform.position - needleHub.transform.position, hubToTipDistance);
                RaycastHit[] hits2 = Physics.RaycastAll(needleTip.transform.position, needleHub.transform.position - needleTip.transform.position, hubToTipDistance);
                if (hits.Length == 0)
                {
                    NeedleLeftVeinDuringMeasurement = true;
                    //Debug.Log("1");
                }
                for (int x = 0; x < hits.Length; x++)
                {
                    //Debug.Log(hits[x].transform.name);
                    if (hits[x].transform.GetInstanceID() == targetVein.transform.GetInstanceID())
                    {
                        break;
                    }
                    if (x == hits.Length - 1)
                    {
                        NeedleLeftVeinDuringMeasurement = true;
                        //Debug.Log("2");
                    }
                }
                foreach (RaycastHit hit in hits2)
                {
                    if (hit.transform.GetInstanceID() == targetVein.transform.GetInstanceID())
                    {
                        NeedleLeftVeinDuringMeasurement = true;
                        //Debug.Log("3");
                    }
                }
            }
            CatheterAdvancementDistance = Vector3.Distance(needleHub.transform.InverseTransformPoint(catheter.transform.position), initialCatheterPosition);
            //Debug.Log("Distance: "+CatheterAdvancementDistance);
        }
    }
}
