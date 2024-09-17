using UnityEngine;
using SMMARTS_SDK;
using System.Collections;
using System;

public class TUIScript : MonoBehaviour, ITUIProbe
{
    // SINGLETON
    public static TUIScript ME;

    [SerializeField]
    private bool cameraShutter = false;

    // Use this for initialization
    void Start()
    {
        if (ME != null) GameObject.Destroy(ME);
        else ME = this;
        DontDestroyOnLoad(this);
        
    }


    // Update is called once per frame
    void Update()
    {

        if (IVMicrocontroller.ME.Connected)
        {

            try
            {
                cameraShutter = int.Parse(IVMicrocontroller.ME.MicrocontrollerData[0]) > 0;

            } catch (Exception e)
            {
                cameraShutter = false;
            }


            if (cameraShutter)
            {
                Debug.Log("Camera shuttered!");
                Camera.main.orthographic = false;
                Camera.main.transform.position = transform.position;
                Camera.main.transform.rotation = transform.rotation;


            }
        } // Microcontroller_sdk connected


    }

}
