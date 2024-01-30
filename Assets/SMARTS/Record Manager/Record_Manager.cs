using System;
using UnityEngine;
using SMMARTS_SDK;
using System.Collections.Generic;
using System.IO;
using System.Threading;

/// <summary>
/// 
/// Class OVerview:
/// The Record_Manager class has functions that allow for the easy recording and replaying of critical data (ATC and Microcontroller outputs).
/// It forces a single instance and can be referenced through the static Record_Manager instance ME. This allows the class to have update
/// functionality while forcing a single instance. For this to work properly, a single instance of the script must exist in the scene. Without
/// an instance in the scene, the static ME will not have been initialized, and, therefore, no references to it can be successfully made.
/// Should multiple instances of the script exist in the scene, all but one will be destroyed, with the last one being the instance that
/// defines ME.
/// 
/// Dependencies:
/// This class requires use of the SMMARTS_SDK. From the SMMARTS_SDK, the following class references are required:
/// 	ATC:
/// 	The ATC class handles all the tracking of tools. It has been written to provide easily usable data strings that can be stored and 
/// 	returned to the ATC class for simple replaying.
/// 	Microcontroller:
/// 	Similar in structure to the ATC, the Microcontroller also allows for easy replay/record strings that can be saved and replayed with
/// 	ease.
/// 	
/// Developer:
/// Andre Kazimierz Bigos
/// 2018.08.07
/// YYYY.MM.DD
/// 10:42 EST (24h)
/// 
/// </summary>
public class Record_Manager : MonoBehaviour
{
	/// This static reference to the Record_Manager allows for easy access by other scripts to start/end replays/recordings. It also allows
	/// for update functionality when found in the scene.
	public static Record_Manager ME;

	/// This list contains all the recorded data from the start of a replay to the end. 
	List<string> recordedData;

	/// This int maintains reference to the current frame being recorded.
	int recordFrame = 0;

	/// This StreamWriter is used to save recorded data to files.
	StreamWriter streamWriter;

	/// This StreamReader is used to read recorded data from files.
	StreamReader streamReader;

	/// A boolean that determines whether or not to save to a file. Is set true/false based on a given optional file save location input.
	bool saveRecordFile = false;

	/// The current operation mode is used internally by the class to preform different functionalities. Is set based on user method calls.
    public Mode CurrentMode { get; private set; } = Mode.Standby;

	/// This thread allows for faster operation. Loading files is a notoriously difficult and arduous processing task. To minimize the effect
	/// of loading in potentially large files, a new thread is dispatches to load records and allows the program to operate more efficiently
	/// while loading recorded data.
	Thread recordLoader;

	/// The replayFilePath is the path replay data is read from. It is serialized and thus visible in the inspector. It cannot be set (and 
	/// should not be set from the exposed inspector value). It is set upon replay start from the user provided optional file path.
	[SerializeField]
	string replayFilePath = "";
	public string ReplayFilePath { get { return replayFilePath; } }
	
	/// This inspector visible value displays the currently replaying frame's frame number during a replay.
	[SerializeField]
	int replayFrameNumber = 0;
	public int ReplayFrameNumber { get { return replayFrameNumber; } }

	/// The replay frame time displays the currently replaying frame's recorded time.
	[SerializeField]
	float replayFrameTime = 0;
	public float ReplayFrameTime { get { return replayFrameTime; } }

	/// The maxReplayFrameLoaded value is updated by the StreamReader as it works on a separate thread and can be used to determine how
	/// much of a file has been read in.
	[SerializeField]
	int maxReplayFrameLoaded = 0;
	public int MaxReplayFrameLoaded { get { return maxReplayFrameLoaded; } }

	/// Similarly to the maxReplayFrameLoaded, this value gives the time of the highest read frame the StreamReader has read in.
	[SerializeField]
	float maxReplayFrameTime = 0;
	public float MaxReplayFrameTime { get { return maxReplayFrameTime; } }

	/// Used internally, this value updates the value of the current frame being replayed
	int currentReplayFrame = 0;

	/// A boolean set to true when a record is completely loaded in by the StreamReader
	[SerializeField]
	bool recordLoaded = false;
	public bool RecordLoaded { get { return recordLoaded; } }

    [SerializeField]
    List<string> headerNameList = new List<string>();
    public List<string> HeaderNameList { get { return headerNameList; } }

    [SerializeField]
    List<Vector3> headerPositionList = new List<Vector3>();
    public List<Vector3> HeaderPositionList { get { return headerPositionList; } }

    [SerializeField]
    List<Vector3> headerOrientationList = new List<Vector3>();
    public List<Vector3> HeaderOrientationList { get { return headerOrientationList; } }

    /// <summary>
    /// In awake we create the static Record_Manager ME and destroy all undesired instances of the Record_Manager in the scene.
    /// </summary>
    private void Awake()
	{
		if (ME != null)
			Destroy(ME);
		ME = this;
	}

	/// <summary>
	/// The mode enum is used internally to determine which state the manager is in. When recording, certain operations are run to facilitate
	/// recording processes. The same is true when in replay mode, and when in standby mode, most operations are not run.
	/// </summary>
	public enum Mode
	{
		Record,
		Replay,
		Standby
	}

	/// <summary>
	/// This method (when called by a user) starts the recording process. It makes sure Microcontroller and ATC are both in their default
	/// operation modes, sets the appropriate save parameters, sets this manager into Record Mode, and saves header data.
	/// </summary>
	/// <param name="saveFilePath">The saveFilePath parameter is the file path to write data to (if specified).</param>
	public void StartRecord(string saveFilePath = "")
	{
		IVMicrocontroller.ME.CurrentInputMode = IVMicrocontroller.InputMode.Microcontroller;
		ATC.ME.CurrentInputMode = ATC.InputMode.ATC;
		SetSaveParameters(saveFilePath);
		recordedData = new List<string>();
		CurrentMode = Mode.Record;
		SaveHeaderData();
		recordFrame = 0;
	}

	/// <summary>
	/// The EndRecord method (when called by a user) closes the StreamWriter, and set the current operation mode of the Record_Manager to
	/// Standby.
	/// </summary>
	public void EndRecord()
	{
		CurrentMode = Mode.Standby;
		CloseWriteStream();
	}

	/// <summary>
	/// The LoadRecord method is a method called by a user that begins the process of loading in a record. It launches a new thread intended
	/// to dynamically read in record data in the background. This data gets queued in the recordedData list and can then be replayed once
	/// the ReplayRecordedData method is called. Once the record has been fully loaded, the replay method can be called and will replay the
	/// recorded data in its entirety.
	/// When called, the LoadRecord method clears any current recordedData, and halts any data loading that is already occurring.
	/// </summary>
	/// <param name="recordPath">The file path of the record one wishes to queue for replay.</param>
	public void LoadRecord(string recordPath)
	{
		if (!File.Exists(recordPath))
			throw new FileNotFoundException(recordPath);
        AbortRecordLoader();
        replayFilePath = recordPath;
		recordLoader = new Thread(DynamicRecordLoader);
        recordLoader.Start();
	}
	
	/// <summary>
	/// This method is launched as a new thread from within the LoadRecord method (should that method have taken as an input an appropriate
	/// file path). It creates a new StreamReader and uses it to read in a record. This record is then added to the recordedData list.
	/// </summary>
	void DynamicRecordLoader()
	{
		streamReader = new StreamReader(replayFilePath);
		string currentLine = "";
		while (!streamReader.EndOfStream)
		{
			try { currentLine = streamReader.ReadLine(); }
			catch (Exception) { break; }
			recordedData.Add(currentLine);
			try
			{
				maxReplayFrameLoaded = int.Parse(currentLine.Split(':')[0]);
				maxReplayFrameTime = float.Parse(currentLine.Split(':')[1]);
			}
			catch (Exception) { }
		}
		recordLoaded = true;
        //Debug.Log("HERE");
	}

	/// <summary>
	/// Used to replay from a file (when filePath is specified) or from currently stored recordedData. If the given filePath is specified
	/// and valid, this method will call the LoadRecord() method and begin loading data from the specified file. This method, also initiates
	/// the actual replay functionality. Whether from a file, or from stored data, this method sets the mode to replay and starts the replay.
	/// </summary>
	/// <param name="filePath">The file path of the desired replay file.</param>
	public void ReplayRecordedData(string filePath = "")
	{
		ATC.ME.CurrentInputMode = ATC.InputMode.Replay_Data;
		IVMicrocontroller.ME.CurrentInputMode = IVMicrocontroller.InputMode.Replay_Data;
		if(!string.IsNullOrEmpty(filePath))
		{
            //Debug.Log("FP: " + filePath);
			LoadRecord(filePath);
            ReadInitialPositions();
        }
		CurrentMode = Mode.Replay;
		currentReplayFrame = 0;
	}


    public void EndReplay()
    {
        if (CurrentMode != Mode.Replay)
            return;
        CurrentMode = Mode.Standby;
        ATC.ME.CurrentInputMode = ATC.InputMode.ATC;
        IVMicrocontroller.ME.CurrentInputMode = IVMicrocontroller.InputMode.Microcontroller;
        AbortRecordLoader();
    }


	/// <summary>
	/// This method is called from Update() when the script is set to replay mode. This method looks at the recordedData list and sends
	/// data to the ATC and Microcontroller alike. Once the recordedData list runs out of data to replay, this method also switches the
	/// mode to standby and returns ATC and Microcontroller back to their respective tracked states.
	/// </summary>
	/// <param name="replayFrame">The integer value of which line stored in recordedData to be replayed.</param>
	void AnalyzeRecordLine(int replayFrame)
	{
		if(replayFrame>=recordedData.Count)
		{
			CurrentMode = Mode.Standby;
			ATC.ME.CurrentInputMode = ATC.InputMode.ATC;
			IVMicrocontroller.ME.CurrentInputMode = IVMicrocontroller.InputMode.Microcontroller;
			return;
		}
		string[] splitLine = recordedData[replayFrame].Split(':');
		if (splitLine.Length!=3)
		{
			return;	
		}
		replayFrameNumber = int.Parse(splitLine[0]);
		replayFrameTime = float.Parse(splitLine[1]);
		if (splitLine[2].Substring(0, 8).Equals("ATC DATA"))
			ATC.ME.ReplayDataInput = splitLine[2];
		else if (splitLine[2].Substring(0, 20).Equals("MICROCONTROLLER DATA"))
			IVMicrocontroller.ME.ReplayDataInput = splitLine[2];
	}

    void ReadInitialPositions()
    {
        print("starting to read in INITIAL positions");
        string headerLine = recordedData[0];
        print("headerLine = " + headerLine);
        string[] headerSplit = headerLine.Split(':');
        int totalHeaderLines = int.Parse(headerSplit[1]);
        int actualTotalHeaderLines = totalHeaderLines * 2; //doing this because the lines are double spaced

        print("hL: " + totalHeaderLines+", record Length: " + recordedData.Count);
        for(int i = 0; i < actualTotalHeaderLines; i++)
        {
            print(recordedData[i]);
            if(recordedData[i].Contains("SET INITIAL FOR"))
            {
                string[] lineSplit = recordedData[i].Split('|');
                string objName = lineSplit[0];
                string[] objSplit = objName.Split(':');
                objName = objSplit[1];
                Vector3 position = StringToVector3(lineSplit[1]);
                Vector3 orientation = StringToVector3(lineSplit[2]);

                print("objName: " + objName);
                print("position: " + position);
                print("orientation" + orientation);

                //GameObject objToMove = GameObject.Find("6 mm vessels");
                GameObject objToMove = GameObject.Find(objName);

                //GameObject.Find("6 mm vessels").transform.position = position;
                //GameObject.Find("6 mm vessels").transform.eulerAngles = orientation;

                GameObject.Find(objName).transform.position = position;
                GameObject.Find(objName).transform.eulerAngles = orientation;
            }
        }

        print("initial positions set");

    }

    public static Vector3 StringToVector3(string sVector)
    {
        // Remove the parentheses
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        // split the items
        string[] sArray = sVector.Split(',');

        // store as a Vector3
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

        return result;
    }

    /// <summary>
    /// This method aborts the record loader thread, zeroes the recordedData list, and resets everything to allow for a new record to be
    /// loaded in.
    /// </summary>
    void AbortRecordLoader()
	{
		try { recordLoader.Abort(); } catch (Exception) { };
		recordedData = new List<string>();
		replayFilePath = "";
		recordLoaded = false;
	}

	/// <summary>
	/// In update, if recording data, we call the SaveLine() method and save the ATC and Microcontroller data.
	/// If replaying, we call the AnalyzeRecordLine() method to send the ATC and Microcontroller saved data to replay.
	/// </summary>
	private void Update()
	{
		if (CurrentMode == Mode.Record)
		{
			recordFrame++;
			SaveLine(recordFrame + ":" + Time.time.ToString("0.00000") + ":" + IVMicrocontroller.ME.ReplayFormattedMicrocontrollerInputString);
			SaveLine(recordFrame + ":" + Time.time.ToString("0.00000") + ":" + ATC.ME.ReplayFormattedATCInputString);
		}
		else if (CurrentMode == Mode.Replay)
		{
			AnalyzeRecordLine(currentReplayFrame);
			currentReplayFrame++;
			AnalyzeRecordLine(currentReplayFrame);
			currentReplayFrame++;
		}
	}

    /// <summary>
    /// If a file does not already exist, we create it.
    /// </summary>
    /// <param name="filePath">The file path of the file we wish to create.</param>
    void CreateFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            FileStream f = File.Create(filePath);
            f.Flush();
            f.Close();
            f.Dispose();
        }
    }

    /// <summary>
    /// This method saves the given data to the recordedData list. If the data recording was started with a non-empty file path for saving data,
    /// this method also uses the streamWriter to write to the specified file.
    /// </summary>
    /// <param name="dataLine"></param>
    void SaveLine(string dataLine)
	{
		if (saveRecordFile)
			streamWriter.WriteLine(dataLine + "\n");
		recordedData.Add(dataLine);
	}

	/// <summary>
	/// Here we specify the save parameters when we start a new record. We close any existing StreamWriter, and define whether or not we are
	/// saving to a file, or locally based on the file path input. If the file path is not empty, we create a StreamWriter and will dynamically
	/// record data to that file. If it remains unspecified, we will only save data to the recordedData list.
	/// </summary>
	/// <param name="saveFilePath">The optional file path to save to.</param>
	void SetSaveParameters(string saveFilePath)
	{
		CloseWriteStream();
		saveRecordFile = false;
		if (!string.IsNullOrEmpty(saveFilePath))
		{
			saveRecordFile = true;
			CreateFile(saveFilePath);
			streamWriter = new StreamWriter(saveFilePath);
		}
	}

	/// <summary>
	/// Properly closes the StreamWriter (if open).
	/// </summary>
	void CloseWriteStream()
	{
		try { streamWriter.Flush(); } catch { }
		try { streamWriter.Close(); } catch { }
		try { streamWriter.Dispose(); } catch { }
	}

    /// <summary>
    /// Add objects whose initial position must be set before starting replay
    /// Pass exact gameobject name, gameobject position, gameobject orientation.  
    /// Only pass WORLD position/rotation.  Method must be called BEFORE StartRecord()
    /// </summary>
    public void AddInitialPosition(string gameObjectName, Vector3 gameObjectPosition, Vector3 gameObjectOrientation)
    {
        headerNameList.Add(gameObjectName);
        headerPositionList.Add(gameObjectPosition);
        headerOrientationList.Add(gameObjectOrientation);

    }

	/// <summary>
	/// At the beginning of any record, this stores important header data. While not really used over the course of replaying,
	/// this data can make reading the saved data file more useful as it lists certain useful pieces of data.
	/// </summary>
	void SaveHeaderData()
	{
        int headerLength = 2;
		string headerLengthLine = "Header Length: ";
		string recordStartDateTime = "RECORD START DATE (yyyy.MM.dd.hh.mm.ss.fffff):" + DateTime.Now.ToString("yyyy.MM.dd.hh.mm.ss.fffff");
		string baseObject = "Base Object:";
        if (ATC.ME.BaseObject != null)
        {
            baseObject += ATC.ME.BaseObject.name;
            headerLength++;
        }
        else
        {
            baseObject += "NOT ASSIGNED";
            headerLength++;
        }
		string trackedObject1 = "Tracked Object 1:";
        if (ATC.ME.TrackedObject1 != null) {
            trackedObject1 += ATC.ME.TrackedObject1.name;
            headerLength++;
        }
        else
        {
            trackedObject1 += "NOT ASSIGNED";
            headerLength++;
        }
		string trackedObject2 = "Tracked Object 2:";
        if (ATC.ME.TrackedObject2 != null)
        {
            trackedObject2 += ATC.ME.TrackedObject2.name;
            headerLength++;
        }
        else
        {
            trackedObject2 += "NOT ASSIGNED";
            headerLength++;
        }
		string trackedObject3 = "Tracked Object 3:";
        if (ATC.ME.TrackedObject3 != null)
        {
            trackedObject3 += ATC.ME.TrackedObject3.name;
            headerLength++;
        }
        else
        {
            trackedObject3 += "NOT ASSIGNED";
            headerLength++;
        }
		string trackedObject4 = "Tracked Object 4:";
        if (ATC.ME.TrackedObject4 != null)
        {
            trackedObject4 += ATC.ME.TrackedObject4.name;
            headerLength++;
        }
        else
        {
            trackedObject4 += "NOT ASSIGNED";
            headerLength++;
        }

        headerLength += headerNameList.Count;

        headerLengthLine += headerLength;
        SaveLine(headerLengthLine);
        SaveLine(recordStartDateTime);
        SaveLine(baseObject);
        SaveLine(trackedObject1);
        SaveLine(trackedObject2);
        SaveLine(trackedObject3);
        SaveLine(trackedObject4);

        if (headerNameList.Count > 0)
        {
            for(int i = 0; i < headerNameList.Count; i++)
            {
                SaveLine("SET INITIAL FOR: " + headerNameList[i] + "|" + headerPositionList[i] + "|" + headerOrientationList[i]);
            }
        }



	}
}