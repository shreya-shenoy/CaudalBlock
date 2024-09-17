using SMMARTS_SDK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand_Slap_Monitor : MonoBehaviour
{
    public static Hand_Slap_Monitor ME;
    public static bool handWasSlapped;

    public bool SlapDetected { get; private set; } = false;

    int number;

    [SerializeField]
    float minPressureSpikeForSlap = 100;

    [SerializeField]
    float timeSlapAffectsVeins = 3;

    [SerializeField]
    float timeSlapDetected = 0;
    
    float currentPressure = 0;
    float lastMeasuredPressure = 0;
    bool monitoring = false;

    private void Awake()
    {
        if (ME != null)
            Destroy(ME);
        ME = this;
    }

    
    private void Update()
    {
        if (!monitoring)
            return;
        
        if ((Time.time - timeSlapDetected ) > timeSlapAffectsVeins)
        {
            SlapDetected = false;
        }

        currentPressure = Microcontroller_Manager.ME.HandSlapPressure;
        if (currentPressure - lastMeasuredPressure > minPressureSpikeForSlap)
        {
            SlapDetected = true;
            timeSlapDetected = Time.time;
        }
        lastMeasuredPressure = currentPressure;

        //try catch to prevent errors when microcontroller isn't connected
        try
        {
            bool isParsable2 = Int32.TryParse(IVMicrocontroller.ME.MicrocontrollerData[6], out number);
            if (number > 1)
            {
                handWasSlapped = true;
            }
        } catch (Exception e)
        {

        }
        
    }

    public void StartMonitoring()
    {
        SlapDetected = false;
        monitoring = true;
        currentPressure = Microcontroller_Manager.ME.HandSlapPressure;
        lastMeasuredPressure = currentPressure;
    }


    public void EndMonitoring()
    {
        monitoring = false;
    }
}
