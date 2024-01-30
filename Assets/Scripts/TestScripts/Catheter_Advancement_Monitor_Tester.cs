using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Catheter_Advancement_Monitor_Tester : MonoBehaviour
{
    //[SerializeField]
    //Catheter_Advancement_Monitor CAM = null;
    [SerializeField]
    bool start = false;
    [SerializeField]
    bool end = false;
    [SerializeField]
    GameObject targetVein = null;
    // Start is called before the first frame update
  //  void Start()
 //   {
  //      CAM = this.GetComponent<Catheter_Advancement_Monitor>();
  //  }

    // Update is called once per frame
    void Update()
    {
        if (start)
        {
            Catheter_Advancement_Monitor.ME.StartAdvancementMeasurement(4, targetVein);
            start = false;
        }
        if (end)
        {
            Debug.Log(Catheter_Advancement_Monitor.ME.EndAdvancmentMeasurement());
            end = false;
        }
    }
}
