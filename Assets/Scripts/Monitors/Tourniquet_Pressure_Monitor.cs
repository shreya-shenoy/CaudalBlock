using SMMARTS_SDK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tourniquet_Pressure_Monitor : MonoBehaviour
{
    public static Tourniquet_Pressure_Monitor ME;
    
    public float minPressureForPassing = 100;

    [SerializeField]
    public float minPressure = 720;

    [SerializeField]
    public float maxPressure = 920;  
    // DL 1/13/22: the old max pressure of 785 is very close to the min pressure 720, so close that its nearly impossible to get scored correctly. I am increasing the max pressure.



    public bool pressureTooLow = false;
    public bool pressureCorrect = false;
    public bool pressureTooHigh = false;


    public float LastTimeApplied; // { get; private set; } // commented out getter/setters so I can see the values in the inspector - DL
    public float LastTimeRemoved; // { get; private set; } // commented out getter/setters so I can see the values in the inspector - DL
    float timeMonitoringStarted = 0;
    float lastMeasuredPressure = 0;
    public float currentPressure = 0;
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
        currentPressure = Microcontroller_Manager.ME.TourniquetPressure;


        //determine pressure range
        if (currentPressure < minPressure)
        {
            pressureTooLow = true;
            pressureCorrect = false;
            pressureTooHigh = false;
        } else if (currentPressure >= minPressure && currentPressure <= maxPressure)
        {
            pressureTooLow = false;
            pressureCorrect = true;
            pressureTooHigh = false;
        } else if (currentPressure > maxPressure)
        {
            pressureTooLow = false;
            pressureCorrect = false;
            pressureTooHigh = true;
        }

        if (LastTimeApplied <= 0)
        {
            if (currentPressure >= minPressureForPassing)
            {
                Debug.Log("Tourniquet Applied - Mark.");
                LastTimeApplied = Time.time;
            }          
        }
        
        if (LastTimeApplied > 0 & LastTimeRemoved <= 0)
        {
            if (currentPressure == 0)
            {
                Debug.Log("Tourniquet Removed - Mark.");
                LastTimeRemoved = Time.time;
            }
        }

     //   if (currentPressure >= minPressureForPassing && lastMeasuredPressure < minPressureForPassing)
     //       LastTimeApplied = Time.time;
     //   else if (currentPressure < minPressureForPassing && lastMeasuredPressure >= minPressureForPassing)
     //       LastTimeRemoved = Time.time;
     //   lastMeasuredPressure = currentPressure;


    }


    public void StartMonitoring()
    {
        timeMonitoringStarted = Time.time;
        LastTimeApplied = -1;
        LastTimeRemoved = -1;
        monitoring = true;
        currentPressure = Microcontroller_Manager.ME.TourniquetPressure;
        lastMeasuredPressure = currentPressure;
    }

    public void EndMonitoring()
    {
        monitoring = false;
    }
}
