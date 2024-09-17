using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* Make this button not interactive until ACT is connected. 
 * Used to solve initialization problems that require ACT connection before loading scripts.
 * 
 */


public class WaitForATC_Connect : MonoBehaviour
{

    Button myButton;

    // Start is called before the first frame update
    void Start()
    {
        myButton = GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (SMMARTS_SDK.ATC.ME.Connected)
        //{
            myButton.interactable = true;
        //} else
       // {
           // myButton.interactable = false;
        //}
    }
}
