using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
 * This script was used to disable the vein compression when the tourniquet was applied. 
 * It did this by turning off the compressible vein completely, and turning on a duplicate "dumb" vein that had no compressible controllers.
 * the problem is that it incorrectly teaches that if you apply a tourniquet your vein wont compress at all, 
 * and thats not true as per Dr. Acar on 1/21/22. It actually made the simulator very difficult to use at a workshop last Friday.
 * The learners correctly obeyed Dr. Trippensee's instructions and could not use US pressure to differentiate between artery and vein, not even a little. 
 * so I am removing this script. Be gone, demons of negative teaching! I cast thee out in the name of MDs!
 *
 * I'm modifying the code for vein compression to not squish flat with the tourniquet - over in VMover.cs
 * - DL 1/28/22


public class VesselResizeScript : MonoBehaviour
{
    public GameObject standardSize, largerSize;

    // Start is called before the first frame update
    void Start()
   
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Tourniquet_Pressure_Monitor.ME.pressureTooHigh || Tourniquet_Pressure_Monitor.ME.pressureCorrect)
        {
            standardSize.SetActive(false);
            largerSize.SetActive(true);
        }
        else
        {
            standardSize.SetActive(true);
            largerSize.SetActive(false);
        }
    }

}


 */