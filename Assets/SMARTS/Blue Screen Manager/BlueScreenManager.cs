using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SMMARTS_SDK;

/// <summary>
/// 
/// </summary>
public class BlueScreenManager : MonoBehaviour {

    GameObject blueScreen;
    GameObject titleGO, troubleshootGO, diagnosticGO, errorImageGO;
    GameObject continueButtonGO;

    bool isSystemBooting=true;
    bool offlineMode = false;

    string titleText = "";
    string troubleshootText = "";
    string diagnosticText = "";
    Sprite errorImage;


    // Use this for initialization
    void Start () {

        if(GameObject.Find("EventSystem")==null)
            blueScreen = Instantiate(Resources.Load<GameObject>("EventSystem"));
        blueScreen = Instantiate(Resources.Load<GameObject>("BSOD"));
        titleGO = GameObject.Find("TitleTextBSOD");
        troubleshootGO = GameObject.Find("TroubleshootText");
        diagnosticGO = GameObject.Find("DiagnosticText");
        errorImageGO = GameObject.Find("ErrorImage");
        continueButtonGO = GameObject.Find("ContinueButtonBSOD");

        blueScreen.SetActive(false);
        StartCoroutine(ExpireBootTimeout());
    }

    private IEnumerator ExpireBootTimeout()
    {
        yield return new WaitForSeconds(30);
        isSystemBooting = false;
    }

    // Update is called once per frame
    void Update () {
        if (!isSystemBooting && !offlineMode)
        {
            if ((ATC.ME.Connected && Microcontroller.ME.Connected) && !ATC.ME.PowerFailure)
            {
                blueScreen.SetActive(false);
                return;

            }

            if (ATC.ME.PowerFailure) 
            {
                Debug.Log("Check Main Power");
                SetupBlueScreen(1);
            }
            else if (!ATC.ME.Connected && Microcontroller.ME.Connected)
            {
                Debug.Log("ATC is not connected");
                SetupBlueScreen(2);
            }
            else if (ATC.ME.Connected && !Microcontroller.ME.Connected)
            {
                Debug.Log("Microcontroller in not connected");
                SetupBlueScreen(3);
            }
            else if (!ATC.ME.Connected && !Microcontroller.ME.Connected)
            {
                Debug.Log("Both ATC and MC are not connected");
                SetupBlueScreen(4);
            }
        }
    }

    void SetupBlueScreen(int errorType)
    {
        switch (errorType){
            case 1://Main Power down
                {
                    titleText = "Check plugs and power.";
                    diagnosticText = "Main Power Failure";
                    troubleshootText= "- Double check that the whitebox is plugged into a wall outlet with power\n" +
                "- Be sure that both USB plugs are connected\n\n\n" +
                "If this message keeps appearing:\n" +
                "disconnect everything, shut down, and restart.";
                    errorImage= Resources.Load<Sprite>("wb-diag");
                    break;
                }
            case 2://ATC not connected
                {
                    titleText = "Check plugs and power.";
                    diagnosticText = "ATC Failure";
                    troubleshootText = "- Be sure that both USB plugs are connected\n" +
                "- Double check that the whitebox is plugged into a wall outlet with power\n\n\n" +"ATTENTION: ATC takes 30s to reconnect after plugging in.\n"+
                "If this message keeps appearing:\n" +
                "disconnect everything, shut down, and restart.";
                    errorImage = Resources.Load<Sprite>("wb-diag");
                    break;
                }
            case 3://MC not connected
                {
                    titleText = "Check plugs and power.";
                    diagnosticText = "Microcontroller Failure";
                    troubleshootText = "- Be sure that both USB plugs are connected\n" +
                "- Double check that the whitebox is plugged into a wall outlet with power\n\n\n" +
                "If this message keeps appearing:\n" +
                "disconnect everything, shut down, and restart.";
                    errorImage = Resources.Load<Sprite>("wb-diag");
                    break;
                }
            case 4://ATC and MC not connected
                {
                    titleText = "Check plugs and power.";
                    diagnosticText = "ATC and Microcontroller Failure";
                    troubleshootText = "- Be sure that both USB plugs are connected\n" +
                "- Double check that the whitebox is plugged into a wall outlet with power\n\n\n" +
                "If this message keeps appearing:\n" +
                "disconnect everything, shut down, and restart.";
                    errorImage = Resources.Load<Sprite>("wb-diag");
                    break;
                }
        }
        
        titleGO.GetComponent<Text>().text = titleText;
        diagnosticGO.GetComponent<Text>().text = diagnosticText;
        troubleshootGO.GetComponent<Text>().text = troubleshootText;
        errorImageGO.GetComponent<Image>().sprite = errorImage;

        if (!blueScreen.activeInHierarchy)
        {
            blueScreen.SetActive(true);
            continueButtonGO.GetComponent<Button>().onClick.AddListener(ContinueWithoutWB);
        }
    }

    private void ContinueWithoutWB()
    {
        offlineMode = true;
        blueScreen.SetActive(false);
    }
}
