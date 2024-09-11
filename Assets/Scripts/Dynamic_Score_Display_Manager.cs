using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Dynamic_Score_Display_Manager : MonoBehaviour
{
    public static Dynamic_Score_Display_Manager ME;
    private void Awake()
    {
        if (ME != null)
            Destroy(ME);
        ME = this;
    }
    
    //[Header("-----Static Replay Screen Toggles-----")]
    [Header("-----Toggles-----")]
    [SerializeField]
    Toggle tourniquetToggle = null;
    [SerializeField]
    Toggle handSlapToggle = null, tractionToggle = null,
        needleEntryToggle = null, backwallToggle = null, entryAngleToggle = null,
        goodCatheterAdvancementToggle = null, tourniquetRemovedToggle = null;

    private string spreadsheetLine = "";
    public bool tourniquetUsed, handSlapUsed, handTractionUsed, needleAccessedVein, needleBackwalled, goodEntryAngle, canReset, goodCathAdvancement, tourniquetRemoved, cathInVeinScoreMe;
    public float needleEntryAngle; //For vein specifically
    public float needleSkinEntryAngle; //for skin specifically
    public float needleBevelAngle; //the bevel's rotation. In this case using the local z axis of the needle.
    public bool needleBevelUp; //whether the bevel is pointed "up". currently taken at time of veinous access. Set to be if angle is between +/-30 degrees currently.
    public bool inPlaneAttempt;
    public bool view3Dused; // 12-15-2020 TJ - This is flipped to TRUE anytime the ultrasound is SMALL (thus showing the 3D model)
    public float veinAccessTime, cathStartedAdvanceTime, tourniquetAppliedTime, tourniquetRemovedTime, scoreMeButtonTime;
    public float moveDistanceCath, moveDistanceAccess;
    public string needleVeinLocationAtCathAdvance;
    public float entryAngleDisplay; 
    public Text entryAngleText;


    //[Header("-----Active Replay Screen Toggles-----")]
    //[SerializeField]
    //Toggle activeTourniquetToggle = null;

    //[SerializeField]
    //Toggle activeHandSlapToggle = null, activeTractionToggle = null,
    //    activeNeedleEntryToggle = null, activeBackwallToggle = null, activeEntryAngleToggle = null,
    //    activeGoodCatheterAdvancementToggle = null, activeTourniquetRemovedToggle = null;


    bool scoring = false;
    public void StartScoring()
    {
        Debug.Log("START SCORING");
        scoring = true;
        tourniquetToggle.isOn = false;
        handSlapToggle.isOn = false;
        tractionToggle.isOn = false;
        needleEntryToggle.isOn = false;
        backwallToggle.isOn = false;
        entryAngleToggle.isOn = false;
        goodCatheterAdvancementToggle.isOn = false;
        tourniquetRemovedToggle.isOn = false;

        tourniquetUsed = false;
        handSlapUsed = false;
        handTractionUsed = false;
        needleAccessedVein = false;
        needleBackwalled = false;
        goodCathAdvancement = false;
        goodEntryAngle = false;
        canReset = true;
        tourniquetRemoved = false;
        needleEntryAngle = -1;
        needleBevelAngle = -1;
        view3Dused = false;
        needleVeinLocationAtCathAdvance = "No Access";
        veinAccessTime = -1;
        cathStartedAdvanceTime = 0;
        tourniquetAppliedTime = -1;
        tourniquetRemovedTime = -1;
        scoreMeButtonTime = 0;
        moveDistanceAccess = 0;
        moveDistanceCath = 0;


    //activeTourniquetToggle.isOn = tourniquetToggle.isOn;
    //activeHandSlapToggle.isOn = handSlapToggle.isOn;
    //activeTractionToggle.isOn = tractionToggle.isOn;
    //activeNeedleEntryToggle.isOn = needleEntryToggle.isOn;
    //activeBackwallToggle.isOn = backwallToggle.isOn;
    //activeEntryAngleToggle.isOn = entryAngleToggle.isOn;
    //activeGoodCatheterAdvancementToggle.isOn = goodCatheterAdvancementToggle.isOn;

    }
    private void Update()
    {
        //if (!scoring)
        //    return;
        if (tourniquetRemovedTime > veinAccessTime && IV_Manager.ME.pressureDuringPuncture)
        {
            tourniquetToggle.isOn = true;
            tourniquetUsed = true;
            tourniquetRemovedToggle.isOn = true;
            tourniquetRemoved = true;
        }
        else if (tourniquetRemovedTime < veinAccessTime && IV_Manager.ME.pressureDuringPuncture)
        {
            tourniquetToggle.isOn = true;
            tourniquetUsed = true;
            tourniquetRemovedToggle.isOn = false;
        }
        else
        {
            tourniquetToggle.isOn = false;
            tourniquetRemovedToggle.isOn = false;
        }
        Debug.Log("Move Distance Access:" + moveDistanceAccess.ToString());

        // if (IV_Manager.ME.properTourniquetRemoval)  //Tourniquet_Pressure_Monitor.ME.LastTimeApplied > -1 && veinAccessTime < tourniquetRemovedTime)
        // {
        //     tourniquetToggle.isOn = true;
        //     tourniquetUsed = true;
        // }
        // else
        // {
        //     //tourniquetToggle.isOn = false;
        //     tourniquetUsed = false;
        // }

        // if(IV_Manager.ME.properTourniquetRemoval) //Tourniquet_Pressure_Monitor.ME.LastTimeRemoved > -1 && needleAccessedVein && 
        // {
        //     tourniquetRemovedToggle.isOn = true;
        //     tourniquetRemoved = true;
        // }
        // else
        // {
        //     tourniquetRemoved = false;
        // }

        if (Hand_Slap_Monitor.ME.SlapDetected)
        {
            //handSlapToggle.isOn = true;
            handSlapUsed = true;
        }
        else
        {
            //handSlapToggle.isOn = false;
            handSlapUsed = false;
        }

        if (Traction_Monitor.ME.TractionDetected)
        {
            //tractionToggle.isOn = true;
            handTractionUsed = true;
        }
        else
        {
            //tractionToggle.isOn = false;
            handTractionUsed = false;
        }

        if(needleEntryToggle.isOn || IV_Manager.ME.NeedleInVein())
        {
            //needleEntryToggle.isOn = true;
            needleAccessedVein = true;
        }
        else
        {
            //needleEntryToggle.isOn = false;
            needleAccessedVein = false;
        }

        //Added check for good entry angle checkbox 10/24/22 -CS
        
        if(goodEntryAngle)
        {
            canReset = false;
            entryAngleToggle.isOn = true;
            Debug.Log("Toggle:" + entryAngleToggle.isOn);
        }
      
        else
        {
            if(canReset){
                 // case where it doesn't work: put needle in, take out, and put it in again
                Debug.Log("Toggle:" + entryAngleToggle.isOn);
                entryAngleToggle.isOn = false;
            }
        }
        Debug.Log("RESET" + canReset);
        // in skin: angle is good, toggle is true
        // leave the skin: angle is bad, toggle should still be true
        // if angle is bad, toggle should be false only if it has never been true

        if (!Needle_Backwall_Monitor.ME.BackwallDetected)
        {
            backwallToggle.isOn = true;
            needleBackwalled = false;
        }
        else
        {
            backwallToggle.isOn = false;
            Debug.Log("BACKWALL DETECTED");
            needleBackwalled = true;
        }
          
        if(goodCathAdvancement)
        {
            goodCatheterAdvancementToggle.isOn = true;
        }
        else
        {
            goodCatheterAdvancementToggle.isOn = false;
        }
        
        if(UltrasoundManagerSource.ME.UltrasoundScreenSize == UltrasoundManagerSource.SCREEN_DIMENSIONS.SMALL)
        {
            view3Dused = true;
        }

        //tourniquetToggle.isOn = Tourniquet_Pressure_Monitor.ME.LastTimeApplied > -1;

        handSlapToggle.isOn = Hand_Slap_Monitor.ME.SlapDetected;

        tractionToggle.isOn = Traction_Monitor.ME.TractionDetected;

        needleEntryToggle.isOn = needleEntryToggle.isOn || IV_Manager.ME.NeedleInVein();

        backwallToggle.isOn = !Needle_Backwall_Monitor.ME.BackwallDetected;

        //tourniquetRemovedToggle.isOn = Tourniquet_Pressure_Monitor.ME.LastTimeRemoved > -1; 
        entryAngleDisplay = IV_Manager.ME.needleSkinAngleAtPuncture;
        
        entryAngleText.text = "Entry Angle: " + entryAngleDisplay.ToString("0.0");
    
                
        goodEntryAngle = IV_Manager.ME.needleSkinAngleAtPuncture > 40f && IV_Manager.ME.needleSkinAngleAtPuncture < 50f;
        
        Debug.Log("Entry Angle:" + goodEntryAngle + IV_Manager.ME.needleSkinAngleAtPuncture);

        //activeTourniquetToggle.isOn = tourniquetToggle.isOn;
        //activeHandSlapToggle.isOn = handSlapToggle.isOn;
        //activeTractionToggle.isOn = tractionToggle.isOn;
        //activeNeedleEntryToggle.isOn = needleEntryToggle.isOn;
        //activeBackwallToggle.isOn = backwallToggle.isOn;
        //activeEntryAngleToggle.isOn = entryAngleToggle.isOn;
        //activeGoodCatheterAdvancementToggle.isOn = goodCatheterAdvancementToggle.isOn;
    }
    public void EndScoring()
    {
        string username = User_Manager.ME.Username.ToLower();

        if (username == "demo")
        {
            spreadsheetLine = "DEMO-DEMO" + "|";
        }
        else if ((username[0] == 's' || username[0] == 'l') && IV_Manager.ME.testmode == 2) //mode = 2
        {
            spreadsheetLine = "Practice-" + username + "|";
        }
        else if ((username[0] == 's' || username[0] == 'l') && IV_Manager.ME.testmode == 3)
        {
            spreadsheetLine = "Study-" + username + "|";
        }
        else
        {
            spreadsheetLine = username + "|";
        }
        spreadsheetLine = spreadsheetLine + DateTime.Now.ToString() + "|";
        spreadsheetLine = spreadsheetLine + IV_Manager.ME.replayFileName + "|";
        if(ProcedureTestManager.ME.currentTest == ProcedureTestManager.testType.longaxis2)
        {
            spreadsheetLine = spreadsheetLine  + "longAxis|2mm|";
        }
        else if (ProcedureTestManager.ME.currentTest == ProcedureTestManager.testType.longaxis4)
        {
            spreadsheetLine = spreadsheetLine + "longAxis|4mm|";
        }
        else if (ProcedureTestManager.ME.currentTest == ProcedureTestManager.testType.longaxis6)
        {
            spreadsheetLine = spreadsheetLine + "longAxis|6mm|";
        }
        else if (ProcedureTestManager.ME.currentTest == ProcedureTestManager.testType.shortaxis2)
        {
            spreadsheetLine = spreadsheetLine + "shortAxis|2mm|";
        }
        else if (ProcedureTestManager.ME.currentTest == ProcedureTestManager.testType.shortaxis4)
        {
            spreadsheetLine = spreadsheetLine + "shortAxis|4mm|";
        }
        else if (ProcedureTestManager.ME.currentTest == ProcedureTestManager.testType.shortaxis6)
        {
            spreadsheetLine = spreadsheetLine + "shortAxis|6mm|";
        }
        spreadsheetLine = spreadsheetLine + IV_Manager.ME.usProbeUsed + "|";
        spreadsheetLine = spreadsheetLine + IV_Manager.ME.inPlaneAttempt + "|";
        /*  5-5-2021 TJ - IDK if this is needed for the study. Users will be only accessing the vein intentionally. Maybe want a check to see if they hit artery
        if (IV_Manager.ME.targetVein) {
            spreadsheetLine = spreadsheetLine + "vein|";
         }
        else
        {
            spreadsheetLine = spreadsheetLine + "artery|";

        }*/
        spreadsheetLine = spreadsheetLine + tourniquetUsed.ToString() + "|";
        //spreadsheetLine = spreadsheetLine + cathInVeinScoreMe.ToString() + "|";
        spreadsheetLine = spreadsheetLine + goodCathAdvancement.ToString() + "|";
        spreadsheetLine = spreadsheetLine + needleVeinLocationAtCathAdvance.ToString() + "|";
        spreadsheetLine = spreadsheetLine + moveDistanceAccess.ToString() + "|";
        spreadsheetLine = spreadsheetLine + moveDistanceCath.ToString() + "|";
        Debug.Log("Move Distance Access:" + moveDistanceAccess.ToString());
        if (Mathf.Abs(needleBevelAngle) > 30)
        {
            spreadsheetLine = spreadsheetLine + "True|";
        }
        else
        {
            spreadsheetLine = spreadsheetLine + "False|";
        }
        spreadsheetLine = spreadsheetLine + needleBevelAngle.ToString("0.0") + "|";
        spreadsheetLine = spreadsheetLine + view3Dused.ToString() + "|";
        //spreadsheetLine = spreadsheetLine + handSlapUsed.ToString() + "|";
        //spreadsheetLine = spreadsheetLine + handTractionUsed.ToString() + "|";
        //spreadsheetLine = spreadsheetLine + needleAccessedVein.ToString() + "|";
        //spreadsheetLine = spreadsheetLine + needleEntryAngle.ToString() + "|";
        //spreadsheetLine = spreadsheetLine + tourniquetRemoved.ToString() + "|";
        spreadsheetLine = spreadsheetLine + IV_Manager.ME.probePositionAccess.ToString() + "|";
        spreadsheetLine = spreadsheetLine + IV_Manager.ME.probeRotationAccess.ToString() + "|";
        spreadsheetLine = spreadsheetLine + IV_Manager.ME.needleUSAngleAtPuncture.ToString() + "|";
        spreadsheetLine = spreadsheetLine + IV_Manager.ME.needleSkinAngleAtPuncture.ToString() + "|";
        spreadsheetLine = spreadsheetLine + IV_Manager.ME.needleVeinAngleAtPuncture.ToString() + "|";
        spreadsheetLine = spreadsheetLine + UltrasoundManagerSource.ME.NeedleTipViewed.ToString() + "|"; //SMMARTS_SDK.Ultrasound.UltrasoundManager.ME.NeedleTipViewed.ToString() + "|";
        spreadsheetLine = spreadsheetLine + veinAccessTime.ToString() + "|";
        spreadsheetLine = spreadsheetLine + cathStartedAdvanceTime.ToString() + "|";
        spreadsheetLine = spreadsheetLine + tourniquetAppliedTime.ToString() + "|";
        spreadsheetLine = spreadsheetLine + tourniquetRemovedTime.ToString() + "|";
        spreadsheetLine = spreadsheetLine + scoreMeButtonTime.ToString() + "|";
        spreadsheetLine = spreadsheetLine + needleBackwalled.ToString() + "|";

        FileManager.AppendLine(spreadsheetLine, LogManager.ME.sessionFilePath);

        scoring = false;

        ProcedureTestManager.ME.SaveCheckpoint();

    }
}
