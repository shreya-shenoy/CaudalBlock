using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.UI;

public class ProcedureTestManager : MonoBehaviour
{

    public static ProcedureTestManager ME;

    public enum testType {notstarted, shortaxis6, shortaxis4, shortaxis2, longaxis6, longaxis4, longaxis2, completed};
    public testType currentTest;
    public GameObject vesselSet6mm, vesselSet4mm, vesselSet2mm;
    public Button shortaxis6button, shortaxis4button, shortaxis2button, longaxis6button, longaxis4button, longaxis2button;
    public bool longAxisCompleted, shortAxisCompleted, isStudy;
    public bool longAxisStudy = false, shortAxisStudy = false;
    public GameObject completedMessage;
    //public int mode = 3;
    public int testmode = 0;


    private void Awake()
    {
        if (ME != null)
        {
            Destroy(ME);
        }
        ME = this;

    }

    // Start is called before the first frame update
    void Start()
    {
        currentTest = testType.notstarted;
        DisableButtons(currentTest);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AdvanceTestByOne()
    {
        if(currentTest != testType.completed)
        {
            if(currentTest == testType.shortaxis2 && longAxisCompleted)
            {
                currentTest = testType.completed;
            }
            else if ((currentTest == testType.longaxis2) && !shortAxisCompleted)
            {
                currentTest = testType.shortaxis6;
            }
            else {
                currentTest++;
            }
        }
        SetVessels(currentTest);
        DisableButtons(currentTest);
    }


    //Written to be called by button press. Buttons on TestSelectionScreen preconfigured in inspector to pass int associated with button. 
    public void SetTestType(int testSelected)
    {
        if(shortAxisCompleted && longAxisCompleted)
        {
            currentTest = testType.completed;
        }

        if (currentTest != testType.completed)
        {
            if (testSelected == 0)
            {
                currentTest = testType.shortaxis6;
            }
            else if (testSelected == 1)
            {
                currentTest = testType.shortaxis4;
            }
            else if (testSelected == 2)
            {
                currentTest = testType.shortaxis2;
            }
            else if (testSelected == 3)
            {
                currentTest = testType.longaxis6;
            }
            else if (testSelected == 4)
            {
                currentTest = testType.longaxis4;
            }
            else if (testSelected == 5)
            {
                currentTest = testType.longaxis2;
            }

            SetVessels(currentTest);
            DisableButtons(currentTest);
        }           
    }


    //Switches active vessels based upon test type selected
    public void SetVessels(testType type)
    {
        if(type == testType.longaxis6 || type == testType.shortaxis6)
        {
            vesselSet2mm.SetActive(false);
            vesselSet4mm.SetActive(false);
            vesselSet6mm.SetActive(true);

            // VPD controls the vein compression, and it needs to initialize. it knows to not initialize more than once. 
            VPD v = vesselSet6mm.GetComponentInChildren<VPD>();
            v.ClearAll(); // clear out the old vertex movers because they reference the old locations in space
            IV_Manager.ME.RandomizeVesselLocation(); // move the vessel
            v.positionSet(); // make new vertex movers that reference the new location.

        }
        else if (type == testType.longaxis4 || type == testType.shortaxis4)
        {
            vesselSet2mm.SetActive(false);
            vesselSet4mm.SetActive(true);
            vesselSet6mm.SetActive(false);
            // VPD controls the vein compression, and it needs to initialize. it knows to not initialize more than once. 
            VPD v = vesselSet4mm.GetComponentInChildren<VPD>();
            v.ClearAll(); // clear out the old vertex movers because they reference the old locations in space
            IV_Manager.ME.RandomizeVesselLocation(); // move the vessel
            v.positionSet(); // make new vertex movers that reference the new location.

        }
        else if (type == testType.longaxis2 || type == testType.shortaxis2)
        {
            vesselSet2mm.SetActive(true);
            vesselSet4mm.SetActive(false);
            vesselSet6mm.SetActive(false);
            // VPD controls the vein compression, and it needs to initialize. it knows to not initialize more than once. 
            VPD v = vesselSet2mm.GetComponentInChildren<VPD>();
            v.ClearAll(); // clear out the old vertex movers because they reference the old locations in space
            IV_Manager.ME.RandomizeVesselLocation(); // move the vessel
            v.positionSet(); // make new vertex movers that reference the new location.

        }
    }
    
    //Disables buttons assuming that only 1 test should be available at a time and that Users must do short axis first
    public void DisableButtons(testType type, int mode = 3)
    {
        Debug.Log("Test: " + mode);
        isStudy = false;
        shortAxisStudy = false;
        longAxisStudy = false;
        testmode = IV_Manager.ME.testmode;
        //testmode = mode;

        if(User_Manager.ME.Username == "")
        {
            User_Manager.ME.Username = "DEMO";
        }
        else if (User_Manager.ME.Username.ToLower()[0] == 's' && Char.IsDigit(User_Manager.ME.Username[1]) && User_Manager.ME.Username.Length == 2 || User_Manager.ME.Username[0] == 's' && Char.IsDigit(User_Manager.ME.Username[1]) && Char.IsDigit(User_Manager.ME.Username[2]) && User_Manager.ME.Username.Length == 3)
        {
            shortAxisStudy = true;
            if (testmode == 3)
            {
                isStudy = true;
            }
        }
        else if (User_Manager.ME.Username.ToLower()[0] == 'l' && Char.IsDigit(User_Manager.ME.Username[1]) && User_Manager.ME.Username.Length == 2 || User_Manager.ME.Username[0] == 'l' && Char.IsDigit(User_Manager.ME.Username[1]) && Char.IsDigit(User_Manager.ME.Username[2]) && User_Manager.ME.Username.Length == 3)
        {
            longAxisStudy = true;
            if (testmode == 3)
            {
                isStudy = true;
            }
        }

        if (shortAxisCompleted && !longAxisCompleted)
        {
            shortaxis6button.interactable = false;
            shortaxis4button.interactable = false;
            shortaxis2button.interactable = false;
            longaxis6button.interactable = true;
            longaxis4button.interactable = false;
            longaxis2button.interactable = false;
        }
        else if (longAxisCompleted && !shortAxisCompleted)
        {
            shortaxis6button.interactable = true;
            shortaxis4button.interactable = false;
            shortaxis2button.interactable = false;
            longaxis6button.interactable = false;
            longaxis4button.interactable = false;
            longaxis2button.interactable = false;
        }
      
        if(currentTest == testType.completed)
        {
            completedMessage.SetActive(true);
        }
        else
        {
            completedMessage.SetActive(false);
        }

        // if (type == testType.notstarted) 
        // {
        //     shortaxis6button.interactable = true;
        //     shortaxis4button.interactable = false;
        //     shortaxis2button.interactable = false;
        //     longaxis6button.interactable = true;
        //     longaxis4button.interactable = false;
        //     longaxis2button.interactable = false;
        // }
        if (type == testType.shortaxis6)
        {
            shortaxis6button.interactable = true;
            shortaxis4button.interactable = false;
            shortaxis2button.interactable = false;
            longaxis6button.interactable = false;
            longaxis4button.interactable = false;
            longaxis2button.interactable = false;
        }
        else if (type == testType.shortaxis4)
        {
            shortaxis6button.interactable = false;
            shortaxis4button.interactable = true;
            shortaxis2button.interactable = false;
            longaxis6button.interactable = false;
            longaxis4button.interactable = false;
            longaxis2button.interactable = false;
        }
        else if (type == testType.shortaxis2)
        {
            shortaxis6button.interactable = false;
            shortaxis4button.interactable = false;
            shortaxis2button.interactable = true;
            longaxis6button.interactable = false;
            longaxis4button.interactable = false;
            longaxis2button.interactable = false;
        }
        else if (type == testType.longaxis6)
        {
            longaxis6button.interactable = true;
            longaxis4button.interactable = false;
            longaxis2button.interactable = false;
            shortaxis6button.interactable = false;
            shortaxis4button.interactable = false;
            shortaxis2button.interactable = false;
        }
        else if (type == testType.longaxis4)
        {
            longaxis6button.interactable = false;
            longaxis4button.interactable = true;
            longaxis2button.interactable = false;
            shortaxis6button.interactable = false;
            shortaxis4button.interactable = false;
            shortaxis2button.interactable = false;
        }
        else if (type == testType.longaxis2)
        {
            longaxis6button.interactable = false;
            longaxis4button.interactable = false;
            longaxis2button.interactable = true;
            shortaxis6button.interactable = false;
            shortaxis4button.interactable = false;
            shortaxis2button.interactable = false;
        }
        if (type == testType.completed)
        {
            longaxis6button.interactable = false;
            longaxis4button.interactable = false;
            longaxis2button.interactable = false;
            shortaxis6button.interactable = false;
            shortaxis4button.interactable = false;
            shortaxis2button.interactable = false;
        }

        if(currentTest == testType.notstarted)
        {
            //s# or s## can be used as username for a study where a participant will be doing short axis testing
            if (shortAxisStudy)
            {
                if(mode == 2)
                {
                    Debug.Log("Short Axis Demo");
                    shortaxis6button.interactable = true;
                    shortaxis4button.interactable = true;
                    shortaxis2button.interactable = true;
                    longaxis6button.interactable = true;
                    longaxis4button.interactable = true;
                    longaxis2button.interactable = true;
                }
            
                else if(mode == 3)
                {
                    isStudy = true;
                    Debug.Log("Short Axis Study");
                    shortaxis6button.interactable = true;
                    shortaxis4button.interactable = false;
                    shortaxis2button.interactable = false;
                    longaxis6button.interactable = false;
                    longaxis4button.interactable = false;
                    longaxis2button.interactable = false;
                }
            
            }

            //l# or l## can be used as username for a study where a participant will be doing long axis testing
            else if (longAxisStudy)
            {
                if(mode == 2)
                {
                    Debug.Log("Long Axis Demo");
                    shortaxis6button.interactable = true;
                    shortaxis4button.interactable = true;
                    shortaxis2button.interactable = true;
                    longaxis6button.interactable = true;
                    longaxis4button.interactable = true;
                    longaxis2button.interactable = true;
                }
                
                else if(mode == 3)
                {
                    isStudy = true;
                    Debug.Log("Long Axis Study");
                    shortaxis6button.interactable = false;
                    shortaxis4button.interactable = false;
                    shortaxis2button.interactable = false;
                    longaxis6button.interactable = true;
                    longaxis4button.interactable = false;
                    longaxis2button.interactable = false;
                }
            }

            // Demo mode makes everything available 
            else if ((User_Manager.ME.Username.ToLower() == "demo") || (mode == 2 && !(longAxisStudy || shortAxisStudy)))
            {
                Debug.Log("Demo mode");
                longaxis6button.interactable = true;
                longaxis4button.interactable = true;
                longaxis2button.interactable = true;
                shortaxis6button.interactable = true;
                shortaxis4button.interactable = true;
                shortaxis2button.interactable = true;
            }
            else{
                shortaxis6button.interactable = true;
                shortaxis4button.interactable = false;
                shortaxis2button.interactable = false;
                longaxis6button.interactable = true;
                longaxis4button.interactable = false;
                longaxis2button.interactable = false;
            }
        }
    }

    //Reads in user history file.  Testing has a check point between short and long axis tests. We load this in here in case user had to leave and come back later.
    public void LoadCheckpoint(int mode)
    {
        if ((User_Manager.ME.Username.ToLower() == "demo") || (mode == 2))
        {
            Debug.Log("Demo (Skip checkpoints)");
        }
        else
        {
            Debug.Log("Loading");
            StreamReader sr = File.OpenText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\ITData\\Users\\IV\\" + User_Manager.ME.Username + "\\" + User_Manager.ME.Username + ".txt");
            string fileTime = sr.ReadLine();
            string userID = sr.ReadLine();
            string recentLogIn = sr.ReadLine();
            string testsCompleted = sr.ReadLine();
        
            if((testsCompleted.Contains("Long") || testsCompleted.Contains("long")) && (testsCompleted.Contains("Short") || testsCompleted.Contains("short")))
            {
                currentTest = testType.completed;
                shortAxisCompleted = true;
                longAxisCompleted = true;
            }
            else if (testsCompleted.Contains("Short") || testsCompleted.Contains("short"))
            {
                currentTest = testType.longaxis6;
                shortAxisCompleted = true;
            }
            else if (testsCompleted.Contains("Long") || testsCompleted.Contains("long"))
            {
                currentTest = testType.shortaxis6;
                longAxisCompleted = true;
            }
            else
            {
                currentTest = testType.notstarted;
            }

            DisableButtons(currentTest);
            sr.Close();
            }
        
    }

    public void SaveCheckpoint()
    {
        print("saving");

        if ((currentTest == testType.longaxis2 && shortAxisCompleted && isStudy) || (currentTest == testType.shortaxis2 && longAxisCompleted && isStudy))
        {
            print("saving3");

            FileManager.ME.UpdateFileLine("Tests", "Tests Passed: Short Axis Tests, Long Axis Tests", Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\ITData\\Users\\IV\\" + User_Manager.ME.Username + "\\" + User_Manager.ME.Username + ".txt");
        }
        else if(currentTest == testType.shortaxis2 && isStudy && testmode == 3)
        {
            print(User_Manager.ME.Username);
            //shortAxisCompleted = true;

            print("saving2");
            FileManager.ME.UpdateFileLine("Tests Passed:", "Tests Passed: Short Axis Tests", Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\ITData\\Users\\IV\\" + User_Manager.ME.Username + "\\" + User_Manager.ME.Username + ".txt");
            //FileManager.ME.UpdateFileLine("Tests", "Tests Completed: Short Axis Tests", Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\ITData\\Users\\IV\\flowtest2\\flowtest2.txt");
        }
        else if (currentTest == testType.longaxis2 && isStudy && testmode == 3)
        {
            print(User_Manager.ME.Username);
            //longAxisCompleted = true;

            print("saving1");
            FileManager.ME.UpdateFileLine("Tests Passed:", "Tests Passed: Long Axis Tests", Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\ITData\\Users\\IV\\" + User_Manager.ME.Username + "\\" + User_Manager.ME.Username + ".txt");
            //FileManager.ME.UpdateFileLine("Tests", "Tests Completed: Short Axis Tests", Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\ITData\\Users\\IV\\flowtest2\\flowtest2.txt");
        }

    }

}