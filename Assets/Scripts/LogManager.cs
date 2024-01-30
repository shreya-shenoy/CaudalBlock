using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class LogManager : MonoBehaviour
{
    public static LogManager ME;
    public string sessionFolderPath;
    public string sessionFilePath;
    public string headerString;

    // Start is called before the first frame update
    void Start()
    {
       
        headerString = "0000-ID|timestamp|ReplayFilename|ProcedureType|VesselDiameter|USused(T/F)|InPlaneAttempt(T/F)|TourniquetApplied(T/F)|GoodCatheterAdvance(T/F)|TargetDeviationFromCenter|NeedleMovementAfterVeinPuncture(mm)|NeedleMovementDuringCatheterInsertion(mm)|BevelUp(T/F)|BevelRotation(deg)|3DView(T/F)|USProbePositionVein(mm)|USProbeRotationVein(deg)|USNeedleAngleVein(deg)|AngleNeedleSkin(deg)|AngleNeedleVein(deg)|NeedleTipViewed(T/F)|AccessVeinTime(s)|CatheterAdvanceTime(s)|TourniquetApplicationTime(s)|TourniquetRemovalTime(s)|TotalTestTime(s)|NeedleBackwalled(T/F)||Comments";

        CreateSessionFolderPath();
        CreateSessionFile();
    }

    private void Awake()
    {
        if (ME != null)
            Destroy(ME);
        ME = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void CreateSessionFolderPath()
    {
        sessionFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\ITData";
        if (!Directory.Exists(sessionFolderPath))
            Directory.CreateDirectory(sessionFolderPath);
        sessionFolderPath += "\\Users";
        if (!Directory.Exists(sessionFolderPath))
            Directory.CreateDirectory(sessionFolderPath);
        sessionFolderPath += "\\IV";
        if (!Directory.Exists(sessionFolderPath))
            Directory.CreateDirectory(sessionFolderPath);
        sessionFolderPath += "\\Sessions";
        if (!Directory.Exists(sessionFolderPath))
            Directory.CreateDirectory(sessionFolderPath);
    }

    public void CreateSessionFile()
    {
        sessionFilePath = sessionFolderPath + "\\SessionData.txt";

        if (!Directory.Exists(sessionFolderPath))
        {
            CreateSessionFolderPath();
        }
        if (!File.Exists(sessionFolderPath + "\\SessionData.txt"))
        {
            File.Create(sessionFolderPath + "\\SessionData.txt").Dispose();
            File.AppendAllText(sessionFilePath, headerString + "\n");
        }
    }

    public void CreateHistoryFile()
    {
        if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\ITData\\Users\\IV\\" + User_Manager.ME.Username))
        {
            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\ITData\\Users\\IV\\" + User_Manager.ME.Username);
        }
        if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\ITData\\Users\\IV\\" + User_Manager.ME.Username + "\\" + User_Manager.ME.Username + ".txt"))
        {
            string historyFilePath;
            string historyFileName;
            historyFilePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\ITData\\Users\\IV\\" + User_Manager.ME.Username + "\\";
            historyFileName = User_Manager.ME.Username + ".txt";
            string startingLogString = "Created: " + DateTime.Now.ToString() + "\r\nUserID: " + User_Manager.ME.Username;
            startingLogString = startingLogString + "\r\nMost Recent Log In: " + DateTime.Now.ToString();
            startingLogString = startingLogString + "\r\nTests Passed: ";
            FileManager.WriteFile(startingLogString, historyFilePath + historyFileName);
            startingLogString = "";
        }


    }
}
