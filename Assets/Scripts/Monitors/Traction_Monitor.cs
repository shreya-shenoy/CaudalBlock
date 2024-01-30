using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Traction_Monitor : MonoBehaviour
{
    public static Traction_Monitor ME;
    private void Awake()
    {
        if (ME != null)
            Destroy(ME);
        ME = this;
    }
    public bool TractionDetected { get; private set; } = false;

    [SerializeField]
    float minPressureForAdequateTraction = 35;
    float currentPressure = 0;
    bool monitoring = false;

    
    public void StartMonitoring()
    {
        TractionDetected = false;
        monitoring = true;
        currentPressure = Microcontroller_Manager.ME.TractionPressure;
    }
    private void Update()
    {
        if (!monitoring)
            return;

        TractionDetected = false;
        currentPressure = Microcontroller_Manager.ME.TractionPressure;
        if (currentPressure > minPressureForAdequateTraction)
        {
            TractionDetected = true;
        }
            
    }
    public void EndMonitoring()
    {
        monitoring = false;
    }
}
