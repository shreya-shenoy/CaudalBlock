using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SMMARTS_SDK;
using System;
using System.IO;

public class Record_Manager_Tester : MonoBehaviour
{
	[SerializeField]
	bool startRecord = false;
	[SerializeField]
	bool endRecord = false;
	[SerializeField]
	bool replayRecord = false;
    // Use this for initialization
    string testFolderPath = "";
    
	void Start ()
	{
        testFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\TESTING_REMOVE_WHENEVER";
        CreateFilePath(testFolderPath);
        testFolderPath += "\\Chest_Tube\\Replay_Records\\" + Time.time.ToString("0") + ".txt";
        //Record_Manager.ME.StartRecord(testFolderPath);
    }
	// Update is called once per frame
	void Update ()
	{
		if(startRecord)
		{
			startRecord = false;
			Record_Manager.ME.StartRecord(testFolderPath);
		}
		if(endRecord)
		{
			endRecord = false;
			Record_Manager.ME.EndRecord();
		}
		if(replayRecord)
		{
			replayRecord = false;
			Record_Manager.ME.ReplayRecordedData(testFolderPath);
		}
    }
    
    public void ReplayingRecordedCustomString(string s)
    {
        Debug.Log("CUSTOM STRING FOUND IN RECORD:\n" + s);
    }
    public void CreateFilePath(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        path += "\\Chest_Tube";
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        path += "\\Replay_Records";
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }
}
