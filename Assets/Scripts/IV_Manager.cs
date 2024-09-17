using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using SMMARTS_SDK;
using UnityEngine.UI;

public class IV_Manager : MonoBehaviour
{
    public static IV_Manager ME;
    [SerializeField]
    Canvas startupCanvas = null, mainUICanvas = null;
    public GameObject optionsButton, optionsCanvas, replayProcedureButton, ultraSoundRenderedTexture, leftDot, rightDot;
    [SerializeField]
    public GameObject modeSelection = null, armOrientationSelection = null, targetSelection = null;
    public GameObject userIDInput;
    //public GameObject nameInput;
    [SerializeField]
    GameObject background = null, completeScreen = null, returnToMenuOverlay = null, activeReplayOverlay = null, testSelectionScreen;
    [SerializeField]
    GameObject needleTip = null, needleHub = null, bevelDirectionObj, cathTip, cathHub, usProbe;
    [SerializeField]
    GameObject[] handVesselArray = new GameObject[0], armVesselArray = new GameObject[0];
    [SerializeField]
    GameObject catheter = null, catheterHub, proxyTracker = null, needle = null;
    public bool usSizeLarge = true;
    public GameObject usScreenSize;
    public RectTransform usLarge, usSmall;
    public GameObject[] handUpVeins;
    public GameObject[] handDownVeins;
    public string replayFileName;
    public bool usProbeUsed; //as of 12/7/2020, "used" means it was activated at any point
    public bool properTourniquetRemoval, tourniquetApplied, targetVein, targetArtery, flipped = false;
    public Collider testCollider;
    public string veinHitName;
    public Vector3 probePositionAccess, probeRotationAccess; // 12-15-2020 TJ - Making this set at time of needle venous acess. 
    private LinearTrajectoryMonitor ArteryLineMonitor;
    private LinearTrajectoryMonitor VeinLineMonitor;
    public LinearTrajectoryMonitor SkinLineMonitor;
    public US_NeedleTipVisibleMonitor TipInUSMonitor;
    public GameObject usProbeCenter, usProbeOffset;
    public bool inPlaneAttempt, pressureDuringPuncture;
    public bool needlePunctureSkin;

    public float needleSkinAngleAtPuncture;
    public float needleVeinAngleAtPuncture;
    public float needleUSAngleAtPuncture;

    private LayerMask veinLayer;
    private LayerMask skinLayer;

    public float maxCathStartDistance, lastMeasuredPressure;
    public GameObject cathStartDistanceWarning;

    public Stopwatch procedureTimer;
    private Vector3 needleTipPosAtCathStart, needleTipPosAtCathEnd, needleTipPosAtAccess;
    public GameObject vein6mm, vein4mm, vein2mm;
    public GameObject vessels6mm, vessels4mm, vessels2mm;
    private GameObject ProtoVein6mm, ProtoVein4mm, ProtoVein2mm;
    public float needleDepth = 0;
    public Vector3 needleCurrentPos;
    public Vector3 needleAtStart;

    public float deviationUSMidline;

    public int testmode = 0;

    private void Awake()
    {
        if(ME!=null)
        {
            Destroy(ME);
        }
        ME = this;

    }
    private void Start()
    {
        procedureTimer = new Stopwatch();

        float conversion = 0.1F;

        veinLayer = 1 << LayerMask.NameToLayer("Vein");
        skinLayer = 1 << LayerMask.NameToLayer("Skin");

        CreateITDataFilePath();
        startupCanvas.enabled = true;
        background.SetActive(false);
        armOrientationSelection.SetActive(false);
        completeScreen.SetActive(false);
        optionsButton.SetActive(false); 
        mainUICanvas.enabled = false;
        returnToMenuOverlay.SetActive(false);
        activeReplayOverlay.SetActive(false);

        print(SystemInfo.graphicsDeviceName);

        ProtoVein6mm = GameObject.Find("vein6mm"); // guys, its better to declare a public or serializable variable so its visible in the inspector for drag-n-drop initialization. - DL, Jan 2022
        ProtoVein4mm = GameObject.Find("vein4mm");
        ProtoVein2mm = GameObject.Find("vein2mm");

        //ProtoVein6mm.SetActive(false);
        //ProtoVein4mm.SetActive(false);
        //ProtoVein2mm.SetActive(false);


        // Artery Linear Distance Monitor
        //ArteryLineMonitor = new LinearTrajectoryMonitor(needleHub, needleTip, conversion, ArteryLayer, BoneLayer); come back to later

        // Vein Linear Distance Monitor
        /////CHANGED FROM NeedleTipFurther (THE BULK RESCORE TIP) TO NeedleTip for demo on 04.18.2018
        VeinLineMonitor = new LinearTrajectoryMonitor(needleHub, needleTip, conversion, veinLayer); 
        VeinLineMonitor.BoundryCorrectionFactor = 0.6f;
        SkinLineMonitor = new LinearTrajectoryMonitor(needleHub, needleTip, conversion, skinLayer);

        ProcedureTestManager.ME.currentTest = ProcedureTestManager.testType.notstarted;
    }
    ///
    /// 
    /// 
    /// 
    /// LOGIN UI MANAGEMENT
    ///
    ///
    ///
    ///

    public void StartTimer()
    {
        procedureTimer.Start();
    }

    public void SetUsername(string userName)
    {
        User_Manager.ME.SetUsername(userName);
    }
    public void SetPreferredName(string preferredName)
    {
        User_Manager.ME.SetPreferredName(preferredName);
    }

    //TJ - This method is added here for convenience but not currently used
    //DL - Thanks for the note, TJ! since this method will destroy the vein collapse functionality as currently implemented, I'm commenting out so its not accidentally used!
    /* public void ResetVeins()
    {
        if (vein6mm != null) Destroy(vein6mm);
        if (vein4mm != null) Destroy(vein4mm);
        if (vein2mm != null) Destroy(vein2mm);

        GameObject p;
        p = ProtoVein6mm;
        vein6mm = Instantiate(p, p.transform.position, p.transform.rotation, p.transform.parent);
        p = ProtoVein4mm; 
        vein4mm = Instantiate(p, p.transform.position, p.transform.rotation, p.transform.parent);
        p = ProtoVein2mm;
        vein2mm = Instantiate(p, p.transform.position, p.transform.rotation, p.transform.parent);

        vein6mm.GetComponent<VPD>().enabled = true;
        vein4mm.GetComponent<VPD>().enabled = true;
        vein2mm.GetComponent<VPD>().enabled = true;
    }
    */

    //This should be called right after current test has been selected.  
    //Goal is to randomize vein/artery position to avoid users being able to use same needle position in multiple procedure tests
    //Right now setting the random to be equal to +/- radius to avoid any overlap.  May be adjusted if this is too large in future
    public void RandomizeVesselLocation()
    {
        System.Random r = new System.Random();
        int randomDistance;
        if (ProcedureTestManager.ME.currentTest == ProcedureTestManager.testType.longaxis6 || ProcedureTestManager.ME.currentTest == ProcedureTestManager.testType.shortaxis6)
        {
            //reset position in case method called multiple times
            vessels6mm.transform.localPosition = new Vector3(0, vessels6mm.transform.localPosition.y, vessels6mm.transform.localPosition.z);
            randomDistance = r.Next(-6, 6);
            vessels6mm.transform.localPosition = new Vector3(randomDistance, vessels6mm.transform.localPosition.y, vessels6mm.transform.localPosition.z);
        }
        else if (ProcedureTestManager.ME.currentTest == ProcedureTestManager.testType.longaxis4 || ProcedureTestManager.ME.currentTest == ProcedureTestManager.testType.shortaxis4)
        {
            vessels4mm.transform.localPosition = new Vector3(0, vessels4mm.transform.localPosition.y, vessels4mm.transform.localPosition.z);
            randomDistance = r.Next(-4, 4);
            vessels4mm.transform.localPosition = new Vector3(randomDistance, vessels4mm.transform.localPosition.y, vessels4mm.transform.localPosition.z);
        }
        else if (ProcedureTestManager.ME.currentTest == ProcedureTestManager.testType.longaxis2 || ProcedureTestManager.ME.currentTest == ProcedureTestManager.testType.shortaxis2)
        {
            vessels2mm.transform.localPosition = new Vector3(0, vessels2mm.transform.localPosition.y, vessels2mm.transform.localPosition.z);
            randomDistance = r.Next(-2, 2);
            vessels2mm.transform.localPosition = new Vector3(randomDistance, vessels2mm.transform.localPosition.y, vessels2mm.transform.localPosition.z);
        }
        else
        {
            print("No veins active!");
        }
    }


    public GameObject loading;
    public Text loadingStatus;
    public static bool start;
    public void BeginSimulation(int mode)
    {       
        //Mode_Manager.ME.CurrentMode = (Mode_Manager.MODE)mode;
        startupCanvas.enabled = false;
        mainUICanvas.enabled = true;
        //optionsButton.SetActive(true); no longer necessary.  Set in inspector
        //background.SetActive(true);

        //modeSelection.SetActive(true);
        
        SetUsername(userIDInput.GetComponent<InputField>().text);
        testmode = mode;
        ProcedureTestManager.ME.longAxisCompleted = false;
        ProcedureTestManager.ME.shortAxisCompleted = false;

        if (mode == 2)
        {
            //ProcedureTestManager.ME.DisableButtons(ProcedureTestManager.testType.notstarted, mode);
            ProcedureTestManager.ME.DisableButtons(0, mode);
            CreateITDataFilePath();
            LogManager.ME.CreateHistoryFile();
        }

        // Mode = 2 is demo mode. the user pressed "Begin Demo"  Comment added by DL 1/13/22
        else if (mode != 2)
        {
            ProcedureTestManager.ME.DisableButtons(ProcedureTestManager.ME.currentTest, mode);
            CreateITDataFilePath();
            LogManager.ME.CreateHistoryFile();
        }
        
        ProcedureTestManager.ME.LoadCheckpoint(mode);
        testSelectionScreen.SetActive(true);

    }

    public void SelectArmOrientation()
    {
        
        background.SetActive(false);

        armOrientationSelection.SetActive(true);
        returnToMenuOverlay.SetActive(true);
        start = true;

    }
    ///
    ///
    ///
    /// 
    /// END LOGIN UI
    /// 
    /// 
    /// 
    /// 


    ///
    ///
    ///
    ///
    /// ARM ORIENTATION SELECTION
    ///
    ///
    ///
    /// 

    public enum ARM_ORIENTATION
    {
        NOT_SELECTED = 0,
        HAND_UP = 1,
        HAND_DOWN = 2
    }

    [SerializeField]
    public static ARM_ORIENTATION currentOrientation = ARM_ORIENTATION.NOT_SELECTED;

    public GameObject handUp;
    public GameObject handDown;
    public GameObject handDownVessels;
    public GameObject handUpVessels;
    public GameObject handDownTargetVessels; //These vessels should be used as the "center" of vessels. Currently designated to be 1/3rd the radius
    public GameObject handUpTargetVessels;

    public GameObject usScreen;
    //public GameObject tourniquetApplied;
    //public GameObject handSlap;
    public static bool tournOnOff;

    

    public void SelectArmOrientation(int orientation)
    {
       
        currentOrientation = (ARM_ORIENTATION)orientation;
        //BeginProcedure();
        armOrientationSelection.SetActive(false);

        targetSelection.SetActive(true);

    }

    public void SelectTarget(int target)
    {
        if(target == 0)
        {
            targetVein = true;
            targetArtery = false;
        }
        else
        {
            targetVein = false;
            targetArtery = true;
        }

    }

    ///
    ///
    ///
    ///
    /// END ARM ORIENTATION SELECTION
    ///
    ///
    ///
    /// 


    ///
    ///
    ///
    ///
    /// PROCEDURE
    ///
    ///
    ///
    ///
    public enum STEP
    {
        NOT_SET,
        INSERT_NEEDLE_SKIN,
        INSERT_NEEDLE_VEIN,
        ADVANCE_CATHETER,
        REMOVE_TOURNIQUET,
        REMOVE_NEEDLE,
        COMPLETED
    }
    public STEP currentStep = STEP.NOT_SET;

    public void BeginProcedure()
    {
        // optionsButton.SetActive(true); 
        //currentStep = STEP.INSERT_NEEDLE_SKIN;
        procedureTimer = new Stopwatch();
        currentStep = STEP.INSERT_NEEDLE_VEIN;
        Debug.Log("IN BEGIN PROCEDURE");
        //Toast.Dismiss();
       // Toast.Show(this, "Place the IV catheter into the vein.", 10, Toast.Type.MESSAGE, 30, Toast.Gravity.BOTTOM, "Place the IV catheter into the vein.");
       // Toast.Show(this, "When finished press spacebar for assessment.", 500, Toast.Type.MESSAGE, 30);
        start = false;

        Mode_Manager.ME.CurrentMode = (Mode_Manager.MODE)3; //Mode 3 is study mode. Currently being set here to get recordmanager to begin recording

        usScreen.SetActive(true);

        //Makes options buttons available at start of procedure 10/24/22 -CS
        if (testmode == 3)
        {
            optionsButton.SetActive(false);
            UltrasoundManagerSource.ME.UltrasoundScreenSize = UltrasoundManagerSource.SCREEN_DIMENSIONS.LARGE;
        }
        else
        {
            optionsButton.SetActive(true);
        }
        
        
        completeScreen.SetActive(false);
        background.SetActive(false);
        targetSelection.SetActive(false);

        if (ProcedureTestManager.ME.currentTest  == ProcedureTestManager.testType.longaxis6 || ProcedureTestManager.ME.currentTest == ProcedureTestManager.testType.shortaxis6)
        {
            Record_Manager.ME.AddInitialPosition("6 mm vessels", vessels6mm.transform.position, vessels6mm.transform.rotation.eulerAngles);      
        }
        else if (ProcedureTestManager.ME.currentTest == ProcedureTestManager.testType.longaxis4 || ProcedureTestManager.ME.currentTest == ProcedureTestManager.testType.shortaxis4)
        {
            Record_Manager.ME.AddInitialPosition("4 mm vessels", vessels4mm.transform.position, vessels4mm.transform.rotation.eulerAngles);
        }
        else if (ProcedureTestManager.ME.currentTest == ProcedureTestManager.testType.longaxis2 || ProcedureTestManager.ME.currentTest == ProcedureTestManager.testType.shortaxis2)
        {
            Record_Manager.ME.AddInitialPosition("2 mm vessels", vessels2mm.transform.position, vessels2mm.transform.rotation.eulerAngles);
        }

        properTourniquetRemoval = false;
        Tourniquet_Pressure_Monitor.ME.LastTimeApplied = -1;
        Tourniquet_Pressure_Monitor.ME.LastTimeRemoved = -1;
        needleVeinAngleAtPuncture = -1;
        needleSkinAngleAtPuncture = -1;
        needlePunctureSkin = false;
        tourniquetApplied = false;
        pressureDuringPuncture = false;

        lastMeasuredPressure = 0;

        print("Begin Procedure");
        StartRecord();
    }

    bool veinsActivated = false;
    bool veinsCreated = false;
    int frame = 0;
    //bool E = false;
    int E = -1;
    //public GameObject temp1;
    //public GameObject temp2;
    bool up1 = true;
    int frames1 = 0;
    bool down1 = true;
    int frames2 = 0;
    public static Text handSlapT;
    public static Text tournText;

    public void Update()
    {
        VeinLineMonitor.Update();
        SkinLineMonitor.Update();
        procedureTimer.Update();


        
        // if(Tourniquet_Pressure_Monitor.ME.currentPressure >= Tourniquet_Pressure_Monitor.ME.minPressureForPassing)
        // {   
        //     properTourniquetRemoval = false;
        //     if (!tourniquetApplied)
        //     {
        //         Dynamic_Score_Display_Manager.ME.tourniquetAppliedTime = procedureTimer.Seconds;
            
        //         Dynamic_Score_Display_Manager.ME.tourniquetRemoved = false;
                
        //         tourniquetApplied = true;
        //     }
        // }
        // else
        // {
        //     tourniquetApplied = false;
        //     //Dynamic_Score_Display_Manager.ME.tourniquetRemoved = true;
        // }

        // if (Tourniquet_Pressure_Monitor.ME.currentPressure < Tourniquet_Pressure_Monitor.ME.minPressureForPassing && Dynamic_Score_Display_Manager.ME.tourniquetAppliedTime > 0)
        // {
        //     if(!Dynamic_Score_Display_Manager.ME.tourniquetRemoved)
        //     {
        //         Dynamic_Score_Display_Manager.ME.tourniquetRemovedTime = procedureTimer.Seconds;
        //         Dynamic_Score_Display_Manager.ME.tourniquetRemoved = true;
        //     }

        //     // Dynamic_Score_Display_Manager.ME. = Tourniquet_Pressure_Monitor.ME.LastTimeRemoved;
        
        //     if (Dynamic_Score_Display_Manager.ME.veinAccessTime < Dynamic_Score_Display_Manager.ME.tourniquetRemovedTime)
        //     {
        //         properTourniquetRemoval = true;
        //     }
        //    else
        //     {
        //         properTourniquetRemoval = false; 
        //     }
        // }
        

        if (Tourniquet_Pressure_Monitor.ME.currentPressure >= Tourniquet_Pressure_Monitor.ME.minPressureForPassing && lastMeasuredPressure < Tourniquet_Pressure_Monitor.ME.minPressureForPassing)
        {
            Dynamic_Score_Display_Manager.ME.tourniquetAppliedTime = procedureTimer.Seconds;
        }
        if (Tourniquet_Pressure_Monitor.ME.currentPressure < Tourniquet_Pressure_Monitor.ME.minPressureForPassing && lastMeasuredPressure > Tourniquet_Pressure_Monitor.ME.minPressureForPassing)
        {
            Dynamic_Score_Display_Manager.ME.tourniquetRemovedTime = procedureTimer.Seconds;
        }

        lastMeasuredPressure = Tourniquet_Pressure_Monitor.ME.currentPressure;


        if (VeinLineMonitor.InstanceOfPuncture)
        {
            if (Tourniquet_Pressure_Monitor.ME.currentPressure >= Tourniquet_Pressure_Monitor.ME.minPressureForPassing)
            {
                pressureDuringPuncture = true;
            }
            probePositionAccess = usProbe.transform.position;
            probeRotationAccess = usProbe.transform.eulerAngles;
            Dynamic_Score_Display_Manager.ME.veinAccessTime = procedureTimer.Seconds;
            Dynamic_Score_Display_Manager.ME.needleBevelAngle = 180 - needle.transform.localEulerAngles.z;
            needleTipPosAtAccess = needleTip.transform.position;

            needleUSAngleAtPuncture = Mathf.Abs(Vector3.Angle(Vector3.ProjectOnPlane(-needle.transform.forward, usProbe.transform.right), -usProbe.transform.forward));
            Vector3 targetDir = needleTip.transform.position - needleHub.transform.position;
            Vector3 dir2 = usProbeCenter.transform.position - usProbeOffset.transform.position;
            float angleInPlane = AngleBetweenVectors(targetDir, dir2, 2);

            if (angleInPlane > 134 || angleInPlane < 45)
            {
                inPlaneAttempt = true;
            }
            else
            {
                inPlaneAttempt = false;
            }
        }
        if (SkinLineMonitor.InstanceOfPuncture)
        {
            needleSkinAngleAtPuncture = NeedleSkinAngle();
            Debug.Log("Entry Skin Angle: " + needleSkinAngleAtPuncture);
            print("cath needle distance: " + Vector3.Distance(catheterHub.transform.position, needleHub.transform.position));
            if(Vector3.Distance(catheterHub.transform.position, needleHub.transform.position) > maxCathStartDistance)
            {
                cathStartDistanceWarning.SetActive(true); // TEST THIS
            }
            else
            {
                // cathStartDistanceWarning.SetActive(false); // Dave Lizdas 12/13/21: resetting this message here, at skin puncture, is annoying.
                // the message stays on and overlaps many user interface elements unintentionally. 
            }
        }

        if (!SkinLineMonitor.Puncture) // Dave Lizdas 12/13/21
        {
            cathStartDistanceWarning.SetActive(false); // Dave Lizdas: just turn the warning off when the needle isn't in the skin. 
        }
         

        if (currentOrientation == (ARM_ORIENTATION)1)//hand up
        {
            //small delay before veins are initialized to allow for atc to connect
            if (up1 == true && frames1>5)
            {
                //temp1 = GameObject.Find("Deformable Vein 1");
                handUpVeins = GameObject.FindGameObjectsWithTag("handUp");
                
                up1 = false;

                //TJ 12/2/2020 - Added since anatomy had to be rotated due to MRT tracking being reversed compared to SRT tracking
                handUpVessels.transform.Rotate(0, 180f, 0, Space.Self);
                handUpTargetVessels.transform.Rotate(0, 180f, 0, Space.Self);

            }
            frames1++;

            handUp.SetActive(true);
            foreach (GameObject handDownVein in handDownVeins)
            {
                handDownVein.SetActive(false);
            }
            foreach (GameObject handUpVein in handUpVeins)
            {
                handUpVein.SetActive(true);
            }            
        }
        if (currentOrientation == (ARM_ORIENTATION)2)//hand down
        {
            if (down1 == true && frames2>5)
            {
                handDownVeins = GameObject.FindGameObjectsWithTag("handDown");
                
                down1 = false;

                //TJ 12/2/2020 - Added since anatomy had to be rotated due to MRT tracking being reversed compared to SRT tracking
                handDownVessels.transform.Rotate(0, 180f, 0, Space.Self);
                handDownTargetVessels.transform.Rotate(0, 180f, 0, Space.Self);


            }
            frames2++;

            handDown.SetActive(true);


            foreach (GameObject handUpVein in handUpVeins)
            {
                handUpVein.SetActive(false);
            }
            foreach (GameObject handDownVein in handDownVeins)
            {
                handDownVein.SetActive(true);
            }
        }
        
        //usScreen.SetActive(true);

        //for detection ultrasound probe pressure
        //add (int.Parse(IVMicrocontroller.ME.MicrocontrollerData[1]) > 0) || to use other fsr
        try
        {
            if ((int.Parse(IVMicrocontroller.ME.MicrocontrollerData[1]) > 10) || (int.Parse(IVMicrocontroller.ME.MicrocontrollerData[2]) > 10))
            {
              //   usScreen.SetActive(true); US should always be on for study. Moving to outside the check
                usProbeUsed = true;
            }

        } catch (Exception e)
        {
          //  usScreen.SetActive(false);
        }
        
        if (currentStep==STEP.INSERT_NEEDLE_VEIN)
        {
            //Toast.Dismiss();
            if (NeedleInVein())
            {
                //Microcontroller_Manager.ME.SendCommand(Microcontroller_Manager.COMMAND.BlueLight);
                
                AdvanceCatheter();
                
                Debug.Log("Confirmed Hit!!!!");
                //BeginProcedure();

            }
        }
        if(currentStep==STEP.ADVANCE_CATHETER)
        Dynamic_Score_Display_Manager.ME.moveDistanceAccess = Vector3.Distance(needleTipPosAtAccess, needleTipPosAtCathStart);
        //Debug.Log("Catheter Distance" + Dynamic_Score_Display_Manager.ME.moveDistanceAccess);
        needleDepth = 0;
        //needleTipPosAtCathStart = needleTip.transform.position;
        //Debug.Log("NeedleTipPosAtAccess: " + needleTipPosAtAccess + "NeedleTipPosAtCathStart" + needleTipPosAtCathStart);
        needleCurrentPos = needleTip.transform.position;
        needleDepth = Vector3.Distance(needleAtStart, needleCurrentPos);
<<<<<<< HEAD
        {   if(Catheter_Advancement_Monitor.ME.CatheterAdvancementDistance < 1)
            {
                needleTipPosAtCathStart = needleTip.transform.position;
=======
        Debug.Log("Needle Depth" + needleDepth);
        {   if(Catheter_Advancement_Monitor.ME.CatheterAdvancementDistance < 1)
            {
                needleTipPosAtCathStart = needleTip.transform.position;
                Debug.Log("NeedleTipPosAtAccess: " + needleTipPosAtAccess + "NeedleTipPosAtCathStart" + needleTipPosAtCathStart);
>>>>>>> a2898942c5e8ada7fa02d08044f9537b47a45cd5
                Dynamic_Score_Display_Manager.ME.moveDistanceAccess = Vector3.Distance(needleTipPosAtAccess, needleTipPosAtCathStart);
                 if (NeedleInInnerThird())
                {
                    Dynamic_Score_Display_Manager.ME.needleVeinLocationAtCathAdvance = "Center";
                }
                else if (NeedleInMiddleThird())
                {
                    Dynamic_Score_Display_Manager.ME.needleVeinLocationAtCathAdvance = "Middle";
                }
                else
                {
                    Dynamic_Score_Display_Manager.ME.needleVeinLocationAtCathAdvance = "Outer";
                }
            }

            if (Catheter_Advancement_Monitor.ME.CatheterAdvancementDistance > 1 && Catheter_Advancement_Monitor.ME.CatheterAdvancementDistance < 2)
                {
                    if (NeedleInVein())
                    {
                        Dynamic_Score_Display_Manager.ME.goodCathAdvancement = true;
                    }
                    else
                    {
                        Dynamic_Score_Display_Manager.ME.goodCathAdvancement = false;
                    }
                }

            if(Catheter_Advancement_Monitor.ME.CatheterAdvancementDistance > 100000000)// 4)//15) //this is the distance the cath travels to be considered "off" the needle
            {
                RemoveTourniquet();
                needleTipPosAtCathEnd = needleTip.transform.position;
                Dynamic_Score_Display_Manager.ME.moveDistanceCath = Vector3.Distance(needleTipPosAtCathStart, needleTipPosAtCathEnd);
                Dynamic_Score_Display_Manager.ME.cathStartedAdvanceTime = procedureTimer.Seconds;

                Catheter_Advancement_Monitor.ME.EndAdvancmentMeasurement();
                Catheter_Advancer_Axial.ME.EndScoringMaintainPosition();
                ProxyCatheterToCatheterTransform.ME.DisableTrackingTransform();
            }
        }

        if (currentStep == STEP.REMOVE_TOURNIQUET)
        {
            RemoveNeedle();            
        }

        if (Input.GetKeyDown(KeyCode.Space) && completeScreen.activeSelf == false && testSelectionScreen.activeSelf == false && startupCanvas.GetComponent<Canvas>().enabled == false)
        {
            //Made spacebar/complete process canvas screen only reachable if spacebar is pressed while the options button is available 10/24/22 -CS

            Dynamic_Score_Display_Manager.ME.cathInVeinScoreMe = CathInVein();
           // testCollider = GameObject.Find("Dynamic Vessel Target Vein 9").GetComponent<Collider>();
            //print(Vector3.Distance(testCollider.ClosestPoint(needleTip.transform.position), needleTip.transform.position));
            //print(testCollider.ClosestPointOnBounds(needleTip.transform.position));
            //print(needleTip.transform.position);
            
            CompleteProcess();

        }

        if (Input.GetKeyDown(KeyCode.R) && completeScreen.activeSelf == false && testSelectionScreen.activeSelf == false && startupCanvas.GetComponent<Canvas>().enabled == false)
        {
            if (!flipped)
            {

                ultraSoundRenderedTexture.transform.localScale = new Vector3(1, 1, 1);

                leftDot.SetActive(false);

                rightDot.SetActive(true);

                flipped = true;

            }
            else
            {

                ultraSoundRenderedTexture.transform.localScale = new Vector3(-1, 1, 1);

                leftDot.SetActive(true);

                rightDot.SetActive(false);

                flipped = false;

            }         
        }

        /*
        if (currentStep == STEP.REMOVE_NEEDLE)
        {
            if(!NeedleInSkin())
            {
                CompleteProcess();
            }
        }*/

        //turn on blue flashback if in vein

        
        if (NeedleInVein())
        {
            IVMicrocontroller.ME.SendCommand(IVMicrocontroller.MicrocontrollerCommand.Microcontroller_Command_7);  // Turn on Blue LED
        }
        else if (NeedleInArtery())
        {
            IVMicrocontroller.ME.SendCommand(IVMicrocontroller.MicrocontrollerCommand.Microcontroller_Command_8); // Turn on red LED
        }
        else
        {
            IVMicrocontroller.ME.SendCommand(IVMicrocontroller.MicrocontrollerCommand.Microcontroller_Command_9); // Turn off both blue and red LEDs
        }

        /*
         * Uncomment to debug pressure and hand slap
         */


        //if (Hand_Slap_Monitor.ME.SlapDetected)
        //{

        //    Debug.Log("Slap detected!");
        //}

        //if (Hand_Slap_Monitor.handWasSlapped)
        //{
        //    Debug.Log("Hand was slapped!");
        //}

        //if (Tourniquet_Pressure_Monitor.ME.pressureTooLow)
        //{
        //    Debug.Log("Pressure too low!");
        //} else if (Tourniquet_Pressure_Monitor.ME.pressureCorrect)
        //{
        //    Debug.Log("Correct Pressure");
        //} else if (Tourniquet_Pressure_Monitor.ME.pressureTooHigh)
        //{
        //    Debug.Log("Pressure is too high");
        //}

        //left shift escape to quit
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Escape))
        {

            if (!Application.isEditor) System.Diagnostics.Process.GetCurrentProcess().Kill();

        }
    }

    public bool NeedleInVein()
    {
        RaycastHit hit;

        if((Physics.Raycast(needleHub.transform.position, needleTip.transform.position - needleHub.transform.position, out hit,
            Vector3.Distance(needleTip.transform.position, needleHub.transform.position), 1 << LayerMask.NameToLayer("Epidural"))) &&
            (!(Physics.Raycast(needleTip.transform.position, needleHub.transform.position - needleTip.transform.position,
            Vector3.Distance(needleTip.transform.position, needleHub.transform.position), 1 << LayerMask.NameToLayer("Epidural")))))
        {
            veinHitName = hit.collider.name;
            needleVeinAngleAtPuncture = Vector3.Angle(needleHub.transform.position - needleTip.transform.position, hit.normal);
            return true;
        }
        else
        {
         //   print("false");
            return false;
        }
    }

    public bool NeedleInInnerThird()
    {
        RaycastHit hit;

        if ((Physics.Raycast(needleHub.transform.position, needleTip.transform.position - needleHub.transform.position, out hit,
            Vector3.Distance(needleTip.transform.position, needleHub.transform.position), 1 << LayerMask.NameToLayer("InnerThird"))) &&
            (!(Physics.Raycast(needleTip.transform.position, needleHub.transform.position - needleTip.transform.position,
            Vector3.Distance(needleTip.transform.position, needleHub.transform.position), 1 << LayerMask.NameToLayer("InnerThird")))))
        {
            veinHitName = hit.collider.name;
            //veinHit = true;
            return true;
        }
        else
        {
            //   print("false");
            return false;
        }
    }

    public bool NeedleInMiddleThird()
    {
        RaycastHit hit;

        if ((Physics.Raycast(needleHub.transform.position, needleTip.transform.position - needleHub.transform.position, out hit,
            Vector3.Distance(needleTip.transform.position, needleHub.transform.position), 1 << LayerMask.NameToLayer("MiddleThird"))) &&
            (!(Physics.Raycast(needleTip.transform.position, needleHub.transform.position - needleTip.transform.position,
            Vector3.Distance(needleTip.transform.position, needleHub.transform.position), 1 << LayerMask.NameToLayer("MiddleThird")))))
        {
            veinHitName = hit.collider.name;
            //veinHit = true;
            return true;
        }
        else
        {
            //   print("false");
            return false;
        }
    }

    public bool NeedleInArtery()
    {
        RaycastHit hit;

        if((Physics.Raycast(needleHub.transform.position, needleTip.transform.position - needleHub.transform.position, out hit,
            Vector3.Distance(needleTip.transform.position, needleHub.transform.position), 1 << LayerMask.NameToLayer("Artery"))) &&
            (!(Physics.Raycast(needleTip.transform.position, needleHub.transform.position - needleTip.transform.position,
            Vector3.Distance(needleTip.transform.position, needleHub.transform.position), 1 << LayerMask.NameToLayer("Artery")))))
        {
            veinHitName = hit.collider.name;
            //arteryHit = true;

            return true;
        }
        else
        {
           // print("false");

            return false;
        }
    }

    public bool CathInVein()
    {
        return (Physics.Raycast(cathHub.transform.position, cathTip.transform.position - cathHub.transform.position,
            Vector3.Distance(cathTip.transform.position, cathHub.transform.position), 1 << LayerMask.NameToLayer("Vein"))) &&
            (!(Physics.Raycast(cathTip.transform.position, cathHub.transform.position - cathTip.transform.position,
            Vector3.Distance(cathTip.transform.position, cathHub.transform.position), 1 << LayerMask.NameToLayer("Vein"))));
    }

    public bool CathInArtery()
    {
        return (Physics.Raycast(cathHub.transform.position, cathTip.transform.position - cathHub.transform.position,
            Vector3.Distance(cathTip.transform.position, cathHub.transform.position), 1 << LayerMask.NameToLayer("Artery"))) &&
            (!(Physics.Raycast(cathTip.transform.position, cathHub.transform.position - cathTip.transform.position,
            Vector3.Distance(cathTip.transform.position, cathHub.transform.position), 1 << LayerMask.NameToLayer("Artery"))));
    }

    void AdvanceCatheter()
    {
        Debug.Log("AdvacenCath");///TESTING REMOVE
        needleAtStart = needleTip.transform.position;
        currentStep = STEP.ADVANCE_CATHETER;
        //Toast.Dismiss();
        //Toast.Show(this, "Advance catheter.", 5, Toast.Type.MESSAGE, 30, Toast.Gravity.BOTTOM, "Advance catheter.");
        GameObject veinHit = VeinHit();
       // Catheter_Advancer_Axial.ME.StartScoring(veinHit);
        Catheter_Advancement_Monitor.ME.StartAdvancementMeasurement(10, veinHit);
        Debug.Log(veinHit.name);
        if (!NeedleInVein())
        {
            Debug.Log("Needle has been detected to not be in vein!");
            //NeedleOutCatheterAdvance();
            currentStep = STEP.INSERT_NEEDLE_VEIN;
            BeginProcedure();
        } else
        {
            Debug.Log("Needle must be in vein!");
        }
        
    }

    void NeedleOutCatheterAdvance()
    {
        currentStep = STEP.INSERT_NEEDLE_VEIN;
        Catheter_Advancer_Axial.ME.EndScoringMaintainPosition();
    }

    void RemoveTourniquet()
    {
        currentStep = STEP.REMOVE_TOURNIQUET;

        //Toast.Dismiss();
        //Toast.Show(this, "Remove Tourniquet.", 5, Toast.Type.MESSAGE, 30, Toast.Gravity.BOTTOM, "Remove needle.");

    }

    void RemoveNeedle()
    {
        currentStep = STEP.REMOVE_NEEDLE;

        //Toast.Dismiss();
        //Toast.Show(this, "Remove Needle.", 5, Toast.Type.MESSAGE, 30, Toast.Gravity.BOTTOM, "Remove needle.");

        Catheter_Advancement_Monitor.ME.EndAdvancmentMeasurement();
        Catheter_Advancer_Axial.ME.EndScoringMaintainPosition();
        ProxyCatheterToCatheterTransform.ME.DisableTrackingTransform();
        //Catheter_Advancer_Axial.ME.EndScoring();
    }
    
    public bool NeedleInSkin()
    {
        // needlePunctureSkin = true;
        return Physics.Raycast(needleHub.transform.position, needleTip.transform.position - needleHub.transform.position,
              Vector3.Distance(needleTip.transform.position, needleHub.transform.position), 1 << LayerMask.NameToLayer("Skin"));
        
   
    }

    public float NeedleSkinAngle()
    {
        RaycastHit layerHit;

        if (!Physics.Raycast(needleHub.transform.position, needleTip.transform.position -
            needleHub.transform.position, out layerHit,
            Vector3.Distance(needleTip.transform.position,
            needleHub.transform.position), skinLayer))
            {
            return -1;
            }
        else
            {
                needlePunctureSkin = true;
                Debug.Log("Needle Punctured Skin");
                Debug.Log("Puncture Angle:" + Vector3.Angle(needleHub.transform.position -
                needleTip.transform.position, layerHit.normal));
                return Vector3.Angle(needleHub.transform.position -
                needleTip.transform.position, layerHit.normal);
            }
    }

    void CompleteProcess()
    {
        //Removes options button upon process completion 10/24/22 -CS
        usScreen.SetActive(false);
        optionsButton.SetActive(false);
        Catheter_Advancer_Axial.ME.EndScoringResetCatheter();
        Dynamic_Score_Display_Manager.ME.scoreMeButtonTime = procedureTimer.Seconds;
        currentStep = STEP.COMPLETED;  // DL - STEP.COMPLETED is not in the code anywhere else (nothing checks for this value specifically)
        //Toast.Dismiss();
        //Toast.Show(this, "Well done.", 5, Toast.Type.MESSAGE, 30, Toast.Gravity.BOTTOM, "Well done.");
        Record_Manager.ME.EndRecord();
        Tourniquet_Pressure_Monitor.ME.EndMonitoring();
        Hand_Slap_Monitor.ME.EndMonitoring();
        Traction_Monitor.ME.EndMonitoring();
        Needle_Backwall_Monitor.ME.EndMonitoring();
        Dynamic_Score_Display_Manager.ME.EndScoring();
        inPlaneAttempt = false;
        completeScreen.SetActive(true);
        background.SetActive(true);
        returnToMenuOverlay.SetActive(false);
        activeReplayOverlay.SetActive(false);
        ProxyCatheterToCatheterTransform.ME.EnableTrackingTransform();
        
        if (testmode == 3)
        {
            if (User_Manager.ME.Username.ToLower()[0] == 's' && Char.IsDigit(User_Manager.ME.Username[1]) && User_Manager.ME.Username.Length == 2 || User_Manager.ME.Username[0] == 's' && Char.IsDigit(User_Manager.ME.Username[1]) && Char.IsDigit(User_Manager.ME.Username[2]) && User_Manager.ME.Username.Length == 3)
            {
                replayProcedureButton.SetActive(false);
            }
            else if (User_Manager.ME.Username.ToLower()[0] == 'l' && Char.IsDigit(User_Manager.ME.Username[1]) && User_Manager.ME.Username.Length == 2 || User_Manager.ME.Username[0] == 'l' && Char.IsDigit(User_Manager.ME.Username[1]) && Char.IsDigit(User_Manager.ME.Username[2]) && User_Manager.ME.Username.Length == 3)
            {
                replayProcedureButton.SetActive(false);
            }
            else if (User_Manager.ME.Username.ToLower()[0] == 's' && Char.IsDigit(User_Manager.ME.Username[1]) && User_Manager.ME.Username.Length == 2 || User_Manager.ME.Username[0] == 's' && Char.IsDigit(User_Manager.ME.Username[1]) && Char.IsDigit(User_Manager.ME.Username[2]) && User_Manager.ME.Username.Length == 3)
            {
                replayProcedureButton.SetActive(false);   
            }
            else if (User_Manager.ME.Username.ToLower()[0] == 'l' && Char.IsDigit(User_Manager.ME.Username[1]) && User_Manager.ME.Username.Length == 2 || User_Manager.ME.Username[0] == 'l' && Char.IsDigit(User_Manager.ME.Username[1]) && Char.IsDigit(User_Manager.ME.Username[2]) && User_Manager.ME.Username.Length == 3)
            {
                replayProcedureButton.SetActive(false); 
            }
        }
        else
        {
                replayProcedureButton.SetActive(true); 
        }

        //Record_Manager.ME.ReplayRecordedData();
    }

    GameObject VeinHit()
    {
        RaycastHit veinHit;
        Physics.Raycast(needleHub.transform.position, needleTip.transform.position - needleHub.transform.position, out veinHit,
            Vector3.Distance(needleTip.transform.position, needleHub.transform.position), 1 << LayerMask.NameToLayer("Epidural"));
        return veinHit.transform.gameObject;
    }


    //used to increase radius of vessels
    void IncreaseVesselRadii()
    {
        bool tourniquetMultiplier = Tourniquet_Pressure_Monitor.ME.currentPressure >= Tourniquet_Pressure_Monitor.ME.minPressureForPassing;
        bool slapMultiplier = Hand_Slap_Monitor.ME.SlapDetected;

        float handMultiplier = slapMultiplier && tourniquetMultiplier ? 1.44f : slapMultiplier || tourniquetMultiplier ? 1.2f : 1f;
        float armMultiplier = tourniquetMultiplier ? 1.2f : 1f;

        foreach(GameObject GO in handVesselArray)
        {
            GO.GetComponent<DynamicVessel>().radiusMultiplier = handMultiplier;
        }
        foreach(GameObject GO in armVesselArray)
        {
            GO.GetComponent<DynamicVessel>().radiusMultiplier = armMultiplier;
        }
    }

    ///
    ///
    ///
    ///
    /// END PROCEDURE
    ///
    ///
    ///
    ///


    ///
    ///
    ///
    ///
    /// RECORDING
    ///
    ///
    ///
    ///


    string folderPath = "";
    void StartRecord()
    {
        print("recording called");
        print("username: " + User_Manager.ME.Username);
        print("mode: " + Mode_Manager.ME.CurrentMode);

        replayFileName = User_Manager.ME.Username + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt";

        print("replayer file: " + replayFileName);

        if (Mode_Manager.ME.CurrentMode == Mode_Manager.MODE.STUDY)
        {
            Record_Manager.ME.StartRecord(folderPath + "\\" + User_Manager.ME.Username + "\\" + replayFileName);
        }
        else
        {
            Record_Manager.ME.StartRecord();
        }

        Tourniquet_Pressure_Monitor.ME.StartMonitoring();
        Hand_Slap_Monitor.ME.StartMonitoring();
        Traction_Monitor.ME.StartMonitoring();
        Needle_Backwall_Monitor.ME.StartMonitoring();
        Dynamic_Score_Display_Manager.ME.StartScoring();


    }

    public void CreateITDataFilePath()
    {
        folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\ITData";
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);
        folderPath += "\\Users";
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);
        folderPath += "\\IV";
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);
    }

    

    void EndRecord()
    {
        Record_Manager.ME.EndRecord();
    }

    ///
    ///
    ///
    ///
    /// END RECORDING
    ///
    ///
    ///
    ///

    ///
    ///
    ///
    ///
    /// REPLAYING
    ///
    ///
    ///
    ///

    public void ReplayRecord()
    {
        catheter.transform.position = Vector3.zero;
        needle.transform.position = Vector3.zero;
        //proxyTracker.transform.position = Vector3.zero;
        Record_Manager.ME.ReplayRecordedData("C:\\Users\\light\\Desktop\\ITData\\Users\\IV\\bigtest3\\bigtest3_2021-04-07_10-19-03.txt");
        background.SetActive(false);
        returnToMenuOverlay.SetActive(true);
        activeReplayOverlay.SetActive(true);
        completeScreen.SetActive(false);

        Tourniquet_Pressure_Monitor.ME.StartMonitoring();
        Hand_Slap_Monitor.ME.StartMonitoring();
        Traction_Monitor.ME.StartMonitoring();
        Needle_Backwall_Monitor.ME.StartMonitoring();

        Dynamic_Score_Display_Manager.ME.StartScoring();
        Catheter_Advancer_Axial.ME.EndScoringResetCatheter();

        currentStep = STEP.INSERT_NEEDLE_VEIN;
        //BeginProcedure();
        //Toast.Dismiss();
    }

    public void ReplayAgain()
    {
        if(Record_Manager.ME.CurrentMode != Record_Manager.Mode.Replay)
        {
            ReplayRecord();
            return;
        }
        else
        {
            Record_Manager.ME.EndReplay();
            //Record_Manager.ME.ReplayRecordedData();
            ReplayRecord();
        }
    }

    ///
    ///
    ///
    ///
    /// END REPLAY
    ///
    ///
    ///
    ///

    ///
    ///
    ///
    ///
    /// PROCEDURE COMPLETE SCREEN
    ///
    ///
    ///
    ///

    public void ReturnToMenu()
    {
        if (Record_Manager.ME.CurrentMode == Record_Manager.Mode.Record)
            Record_Manager.ME.EndRecord();
        else if (Record_Manager.ME.CurrentMode == Record_Manager.Mode.Replay)
            Record_Manager.ME.EndReplay();
        Start();
        Catheter_Advancer_Axial.ME.EndScoringResetCatheter();
        //Removes options buttons upon clicking logout 10/24/22 CS
        optionsButton.SetActive(false);

       // Toast.Dismiss();
    }

    public void ReturnToProcedureSelect()
    {
        if (Record_Manager.ME.CurrentMode == Record_Manager.Mode.Record)
            Record_Manager.ME.EndRecord();
        else if (Record_Manager.ME.CurrentMode == Record_Manager.Mode.Replay)
            Record_Manager.ME.EndReplay();


        if ((User_Manager.ME.Username.ToLower() == "demo") || (testmode == 2)) //&& !(ProcedureTestManager.ME.longAxisStudy || ProcedureTestManager.ME.shortAxisStudy)
        {
            Debug.Log("demo(ProcedureSelect)");
            ProcedureTestManager.ME.currentTest = ProcedureTestManager.testType.notstarted;
            ProcedureTestManager.ME.DisableButtons(ProcedureTestManager.testType.notstarted, 2);
            //ProcedureTestManager.ME.DisableButtons(ProcedureTestManager.testType.notstarted, testmode);
        }
        else if (Dynamic_Score_Display_Manager.ME.goodCathAdvancement && Dynamic_Score_Display_Manager.ME.tourniquetUsed && Dynamic_Score_Display_Manager.ME.tourniquetRemoved && Dynamic_Score_Display_Manager.ME.needleAccessedVein)
        {
            if (ProcedureTestManager.ME.currentTest == ProcedureTestManager.testType.shortaxis2 && ProcedureTestManager.ME.isStudy)
            {
                Debug.Log("Completed short axis");
                ProcedureTestManager.ME.shortAxisCompleted = true;
            }
            else if (ProcedureTestManager.ME.currentTest == ProcedureTestManager.testType.longaxis2 && ProcedureTestManager.ME.isStudy)
            {
                Debug.Log("Completed long axis");
                ProcedureTestManager.ME.longAxisCompleted = true;
            }
            ProcedureTestManager.ME.AdvanceTestByOne();
            ProcedureTestManager.ME.SaveCheckpoint();
        }
        else if (ProcedureTestManager.ME.shortAxisStudy && testmode == 3)
        {
            if(ProcedureTestManager.ME.shortAxisCompleted)
            {
                ProcedureTestManager.ME.currentTest = ProcedureTestManager.testType.longaxis6;
                ProcedureTestManager.ME.DisableButtons(ProcedureTestManager.testType.longaxis6, testmode);
            }
            else
            {
                ProcedureTestManager.ME.currentTest = ProcedureTestManager.testType.shortaxis6;
                ProcedureTestManager.ME.DisableButtons(ProcedureTestManager.testType.shortaxis6, testmode);
            }
        }
        else if (ProcedureTestManager.ME.longAxisStudy && testmode == 3)
        {
            if(ProcedureTestManager.ME.longAxisCompleted)
            {
                ProcedureTestManager.ME.currentTest = ProcedureTestManager.testType.shortaxis6;
                ProcedureTestManager.ME.DisableButtons(ProcedureTestManager.testType.shortaxis6, testmode);
            }
            else
            {
                ProcedureTestManager.ME.currentTest = ProcedureTestManager.testType.longaxis6;
                ProcedureTestManager.ME.DisableButtons(ProcedureTestManager.testType.longaxis6, testmode);
            }
        }

        Catheter_Advancer_Axial.ME.EndScoringResetCatheter();
        completeScreen.SetActive(false);
        testSelectionScreen.SetActive(true);

        //Toast.Dismiss();
    }

    private float AngleBetweenVectors(Vector3 v1, Vector3 v2, int mode)
    {
        switch (mode)
        {
            case 1:
                v1.x = 0;
                v2.x = 0;
                break;
            case 2:
                v1.y = 0;
                v2.y = 0;
                break;
            case 3:
                v1.z = 0;
                v2.z = 0;
                break;    
            default:
                break;
        }
        return Vector3.Angle(v1, v2);
    }

    ///
    ///
    ///
    ///
    /// END PROCEDURE COMPLETE SCREEN
    ///
    ///
    ///
    ///

    public void spaceBarPress()
    {
        //Made spacebar/complete process canvas screen only reachable if spacebar is pressed while the options button is available 10/24/22 -CS

        Dynamic_Score_Display_Manager.ME.cathInVeinScoreMe = CathInVein();
        // testCollider = GameObject.Find("Dynamic Vessel Target Vein 9").GetComponent<Collider>();
        //print(Vector3.Distance(testCollider.ClosestPoint(needleTip.transform.position), needleTip.transform.position));
        //print(testCollider.ClosestPointOnBounds(needleTip.transform.position));
        //print(needleTip.transform.position);
        
        CompleteProcess();
    }
}