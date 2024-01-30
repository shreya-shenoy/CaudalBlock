using SMMARTS_SDK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class usVeinPress : MonoBehaviour
{
    public GameObject bar;
    public GameObject UScenter;
    public static float press;
    public static bool tournIsOn;

    void Start()
    {
        bar.transform.position = transform.localPosition;

    }

    void Update()
    {
        //this block will throw an array index out of bounds exception if the whitebox is not plugged in
        int num;
        try
        {
           Int32.TryParse(IVMicrocontroller.ME.MicrocontrollerData[3], out num);

        } catch (Exception e)
        {
            num = 0;
        }
        if (num > 10)
        {
            tournIsOn = true;
        }
        else
        {
            tournIsOn = false;
        }

        //end block
        int number1;
        int number2;
        int USpress;
        try
        {
            Int32.TryParse(IVMicrocontroller.ME.MicrocontrollerData[1], out number1);
            Int32.TryParse(IVMicrocontroller.ME.MicrocontrollerData[2], out number2);
            USpress = number1 + number2;

        } catch (Exception e){
            USpress = 0;
        }

        if (USpress > 1300)
        {
            press = 0.33f;
        }
        else if (USpress > 1000)
        {
            press = 0.66f;
        }
        else if(USpress > 10)
        {
            press = 1;
        }
        else
        {
            press = 4;//2;
        }
        
    }
    //public static float maxDistance = 10.0f;


    void OnDrawGizmos()
    {
        float maxDistance = 100.0f;
        RaycastHit hit;
        Vector3 temp;
        int layer_mask = LayerMask.GetMask("Vein");
        

        bool isHit = Physics.BoxCast(UScenter.transform.position, bar.transform.lossyScale / 2, bar.transform.up, out hit, bar.transform.rotation, maxDistance, layer_mask);
        if (isHit)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(UScenter.transform.position, UScenter.transform.up * hit.distance);
            Gizmos.DrawWireCube(UScenter.transform.position + (UScenter.transform.up) * hit.distance, bar.transform.lossyScale);
            //temp = hit.point + ((UScenter.transform.position - hit.point).normalized) * press;
            temp = UScenter.transform.position;

            //Debug.Log((UScenter.transform.position-hit.point).normalized);
            //temp.y = UScenter.transform.localPosition.y - hit.distance - number;//(109 / 2);//(116/2);
            bar.transform.position = temp ;
            bar.transform.rotation = UScenter.transform.rotation;
            //bar.transform.Translate(Vector3.forward*hit.distance);
        }
        else
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(UScenter.transform.position, UScenter.transform.up * maxDistance);
            //temp = hit.point + ((UScenter.transform.position - hit.point).normalized) * press;
            //bar.transform.position = temp;

            bar.transform.position = UScenter.transform.position;
            bar.transform.rotation = UScenter.transform.rotation;

            //bar.transform.Translate(Vector3.forward * 0);

        }
    }
}
