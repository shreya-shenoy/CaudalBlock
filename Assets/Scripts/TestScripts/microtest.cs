using UnityEngine;
using System;
using System.IO.Ports;
using System.Threading;
using System.Collections;

//namespace SMMARTS_SDK
//{
// THIS IS A SINGLETON:
// we have one microcontroller that many scripts frequently reference
// so it is appropriate to use a public static class for our microcontroller.
// it is also useful to use the property inspector for public variables - 
// and its also useful to use coroutines for serial connection and read cycles (although we use a thread for that).
// But, those two useful things come from : MonoBehavior.
// MonoBehavior classes cannot be static.
// So, we have an argument to use a singleton: 
// a regular MonoBehavior class and make one single instance of the whole class.
// any other script can access public (not public static) variables and funtions like this:
// Microcontroller.ME.SomeFunction();

// WE CANNOT USE THESE: 
// UNITY JIT compiler does not include these functions even though they compile in VB/MONO
// DataReceivedHandler
// ReadExisting
// BytesToRead
// DiscardNull
// DiscardInBuffer 
// DiscardOutBuffer

/// <summary>
/// This class is compatible with, and requires the MicrocontrollerWhiteBoxHandshake arduino code develped by:
/// Andre Bigos
/// 08.16.2017
/// 14:30
/// </summary>
public class microtest : MonoBehaviour
{

    // --------- Singleton Reference to this script --------------------------------------------------------------------------
    //Static instance of this class which allows it to be accessed by any other script. 
    public static microtest ME;

    [Header("----------- COM Port Initialization ------------------------------------------------------------")]
    [SerializeField]
    private string com_Port;
    public string COM_Port { set { com_Port = value; } } // ATTENTION - Set to default port - 5.

    private SerialPort port;

    [Header("----------- WhiteBox Connection Status------------------------------------------------------------")]
    [SerializeField]
    private bool retryConnection;
    [SerializeField]
    private bool connected;
    [SerializeField]
    private bool whiteboxConnectionAlert;
    public bool RetryConnection
    {
        get
        {
            return retryConnection;
        }
    }
    public bool Connected
    {
        get
        {
            return connected;
        }

    }
    public bool WhiteboxConnectionAlert
    {
        get
        {
            return whiteboxConnectionAlert;
        }

    }
    private Thread streamThread;
    private string inString;

    [Header("----------- Sensor Readings ------------------------------------------------------------")]
    // these dont have to be public - but its handy to see it in the inspector.
    [SerializeField]
    private string incoming;

    [SerializeField]
    private bool tui_Button;
    [SerializeField]
    private int usProbePressureNotchSide;
    [SerializeField]
    private int usProbePressureFlatSide;
    [SerializeField]
    private float valvePressure;
    [SerializeField]
    private bool syringeValvePresent = false;

    public bool TUI_Button
    {
        get
        {
            return tui_Button;
        }
    }
    public int USProbePressureNotchSide
    {
        get
        {
            return usProbePressureNotchSide;
        }
    }
    public int USProbePressureFlatSide
    {
        get
        {
            return usProbePressureFlatSide;
        }
    }
    public float ValvePressure
    {
        get
        {
            return valvePressure;
        }
        set//// REMOVE REMOVE REMOVE
        {
            valvePressure = value;
        }
    }
    public bool SyringeValvePresent
    {
        get
        {
            return syringeValvePresent;
        }
    }

    [SerializeField]
    private bool redLight, blueLight, syringeValveOpen; // get and set

    public bool RedLight
    {
        get
        {
            return redLight;
        }
        set
        {
            redLight = value;
        }
    }
    public bool BlueLight
    {
        get
        {
            return blueLight;
        }
        set
        {
            blueLight = value;
        }
    }
    public bool SyringeValveOpen
    {
        get
        {
            return syringeValveOpen;
        }
        set
        {
            syringeValveOpen = value;
        }
    }

    //List of Microcontroller commands
    [HideInInspector]
    public enum MicrocontrollerCommands
    {
        Alive,                      //Unity is Alive and Well
        LPop,                       //Large Tactile Pop
        SPop,                       //Small Tactile Pop
        LORO,                       //Open Syringe Valve
        LORC,                       //Close Syringe Valve
        RedLight,                   //Red Syringe LED
        BlueLight,                  //Blue Syringe LED
        LightsOff,                  //Syringe LED OFF
        ZeroSyringePressure,        //Calibrate Syringe Pressure
        ResetSyringe               //Reset Syring hardware
                                   //SendData					//Tells the microcontroller it's OK to send back data
    }
    // Testing variables
    // private int lastthing;
    // public GameObject CamCube;


    private int awakeTimer;
    private int awakeTimerPeriod = 350; //Hardcoded values

    ArrayList commandQueue = new ArrayList();
    bool waitingForData = false;
    bool canSendAgain = true;
    bool dataReceived = false;
    // --------- Awake Singleton Constructor --------------------------------//
    //  Awake is always called before any Start functions 
    //  Check if instance already exists
    //  If so, then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of this class.
    void Awake()
    {

        if (ME != null)
            GameObject.Destroy(ME);
        else
            ME = this;

        DontDestroyOnLoad(this);

    }

    // --------- Start Connection --------------------------------------//
    //Wait for 50ms and initiate connection
    void Start()
    {
        Thread.Sleep(50);   // 50 ms
        Connect();
    }

    // --------- Connection Sequence--------------------------------------//
    //Connect to Microcontroller from new thread
    public void Connect()
    {
        try
        {
            connected = false;
            streamThread.Abort();
            port.Close();
            port.Dispose();
            Thread.Sleep(50);   // 50 ms

        }
        catch (Exception e)
        {
            //Debug.Log(e);
        }
        streamThread = new Thread(ThreadListener); //Start a new thread and listen to Microcontroller
        streamThread.Start();
    }

    // --------- Listen to MicroController via comport--------------------------------------//
    // Listen to port for whitebox parameters
    // ATTENTION - dont do string comparisons, compare with ints or enums
    // ATTENTION - dont do string + string
    public void ThreadListener()
    {
        retryConnection = false;
        try
        {
            if (String.IsNullOrEmpty(com_Port))
            {
                //Debug.Log("Handshake is being done");
                string[] ports = SerialPort.GetPortNames();
                //Debug.Log("Number of available COM Ports:" + ports.Length);
                foreach (string portname in ports)
                {
                    float portOpenPeriod = 0;

                    //Debug.Log("Attempting to connect to " + portname);
                    port = null;
                    port = new SerialPort(portname, 19200, Parity.None, 8, StopBits.One);
                    port.Open();
                    port.ReadTimeout = 150;
                    port.Write("-");

                    while (port.IsOpen)
                    {

                        byte handShake = (byte)port.ReadByte();
                        if (handShake == '>')
                        {
                            //Debug.Log("CSSALT compatible microcontroller found!");
                            canSendAgain = true;
                            break;
                        }
                        else if (portOpenPeriod > 150)
                        {
                            port.Close();
                            port.Dispose();
                            port = null;
                            //Debug.Log("Not a CSSALT compatible microcontroller!");
                            break;
                        }
                        portOpenPeriod++;
                    }
                    if (port != null)
                        break;


                }
                if (port != null && port.IsOpen)
                {
                    connected = true;
                    retryConnection = false;
                }
                else
                {
                    connected = false;
                }
            }
            else
            {
                float portOpenPeriod = 0;
                string portName = "COM" + com_Port;
                //Debug.Log("Attempting to connect to " + portName);
                port = null;
                port = new SerialPort(portName, 19200, Parity.None, 8, StopBits.One);
                port.Open();
                port.ReadTimeout = 150;
                port.Write("-");

                while (port.IsOpen)
                {

                    byte handShake = (byte)port.ReadByte();
                    if (handShake == '>')
                    {
                        //Debug.Log("CSSALT compatible microcontroller found!");
                        canSendAgain = true;
                        break;
                    }
                    else if (portOpenPeriod > 150)
                    {
                        port.Close();
                        port.Dispose();
                        port = null;
                        //Debug.Log("Not a CSSALT compatible microcontroller!");
                        break;
                    }
                    portOpenPeriod++;
                }
                if (port != null && port.IsOpen)
                {
                    connected = true;
                    retryConnection = false;
                }
                else
                {
                    connected = false;
                }
            }
        }


        catch (Exception)
        {
            retryConnection = true;
            connected = false;
            streamThread.Abort();
        }

        while (port.IsOpen & connected)
        {
            if (waitingForData)
            {
                try
                {
                    inString = "";
                    inString = port.ReadLine();
                    inString = inString.Trim();
                    if (!String.IsNullOrEmpty(inString) && (inString.Substring(0, 1)).CompareTo("$") == 0 && (inString.Substring(inString.Length - 1)).CompareTo("#") == 0)
                    {

                        try
                        {
                            // public variable for property inspector visibility
                            incoming = inString.Substring(1, inString.Length - 2);

                            string[] SplitArray = incoming.Split(',');

                            if (SplitArray.Length == 3)
                            {
                                tui_Button = (int.Parse(SplitArray[0]) > 0);
                                usProbePressureNotchSide = int.Parse(SplitArray[1]);
                                usProbePressureFlatSide = int.Parse(SplitArray[2]);
                                syringeValvePresent = false;
                                dataReceived = true;
                            }
                            if (SplitArray.Length == 4) // this newer whitebox has a valve pressure sensor.
                            {
                                tui_Button = (int.Parse(SplitArray[0]) > 0);
                                usProbePressureNotchSide = int.Parse(SplitArray[1]);
                                usProbePressureFlatSide = int.Parse(SplitArray[2]);
                                valvePressure = float.Parse(SplitArray[3]);
                                syringeValvePresent = true;
                                dataReceived = true;
                            }
                        }
                        catch (Exception e)
                        {
                            dataReceived = true;
                            // //Debug.Log(e);
                            // this catches mostly parseInt errors from bad data that the arduino sometimes spits out after reads.
                            // Note the problem is most likely the poor .NET implementation of Serial.IO.ports
                            // read this blog - its written by a popular embedded systems engineer who posts on stackoverflow alot: 
                            // www.sparxeng.com/blog/software/muse-use-net-system-io-ports-serialport
                        }
                    }
                    else
                    {
                        dataReceived = true;
                    }
                }
                catch (Exception e)
                {
                    retryConnection = true;
                    connected = false;
                    //Debug.Log(e);

                }
                if (dataReceived)
                {
                    waitingForData = false;
                    dataReceived = false;
                    canSendAgain = true;
                }
            }
        }
        waitingForData = false;
        dataReceived = false;
        connected = false;
        retryConnection = true;
    }

    // --------- Every Rendering Frame --------------------------------------//
    void Update()
    {
        if (retryConnection)
        {
            // strobing through the COM ports like this introduces instability.
            // COM_portNumber++;
            // if (COM_portNumber > 20) COM_portNumber = 1;
            Connect();
        }

        whiteboxConnectionAlert = retryConnection;

        awakeTimer += (int)(Time.deltaTime * 1000);
        if (awakeTimer > awakeTimerPeriod)
        {
            awakeTimer = 0;
            Send(MicrocontrollerCommands.Alive); // this lets the arduino know that unity is actively talking to it.
        }

    }


    // ---------  Send commands to the microcontroller --------------------------------------//
    // The commands are simple and few, therefore we can limit them to single bytes for quick parsing.
    public void Send(MicrocontrollerCommands theCommand)
    {
        commandQueue.Add(theCommand);
    }

    void LateUpdate()
    {
        //if (!commandQueue.Contains(MicrocontrollerCommands.SendData))
        //	commandQueue.Add(MicrocontrollerCommands.SendData);
        if (canSendAgain)
            SendData();
    }

    void SendData()
    {
        canSendAgain = false;
        if (port != null && port.IsOpen & connected)
        {
            foreach (MicrocontrollerCommands theCommand in commandQueue)
            {

                try
                {

                    switch (theCommand)
                    {

                        // ------ Unity is Alive and Well --------------------------------- 
                        case (MicrocontrollerCommands.Alive):
                            port.Write("A");    // writes ASCII character 65 "A",
                            awakeTimer = 0;
                            break;

                        // ------ Large Tactile Pop ---------------------------------------
                        case (MicrocontrollerCommands.LPop):
                            port.Write("1");    // writes 0110001 ("1" is ASCII character 49)
                            awakeTimer = 0;
                            break;

                        // ------ Small Tactile Pop ---------------------------------------
                        case (MicrocontrollerCommands.SPop):
                            port.Write("2");    // writes 0110010 ("2" is ASCII character 50)
                            awakeTimer = 0;
                            break;

                        // ------ Open Syringe Valve ---------------------------------------
                        case (MicrocontrollerCommands.LORO):
                            if (syringeValveOpen)
                            {
                                // nothing - the syringe valve is already open.
                            }
                            else
                            {
                                port.Write("3");  // this writes 0110011 ("3" is ASCII character 51)
                                syringeValveOpen = true;
                                awakeTimer = 0;
                            }
                            break;

                        // ------ Close Syringe Valve --------------------------------------
                        case (MicrocontrollerCommands.LORC):
                            if (syringeValveOpen)
                            {
                                port.Write("4");  // this writes 0110011 ("3" is ASCII character 51)
                                syringeValveOpen = false;
                                awakeTimer = 0;
                            }
                            else
                            {
                                // nothing - the valve is already closed
                            }
                            break;

                        // ------ Red Syringe LED ------------------------------------------
                        case (MicrocontrollerCommands.RedLight):
                            if (!redLight)
                            {
                                port.Write("8");
                                redLight = true;
                                awakeTimer = 0;
                            }
                            break;

                        // ------ Blue Syringe LED ------------------------------------------
                        case (MicrocontrollerCommands.BlueLight):
                            if (!blueLight)
                            {
                                port.Write("7");
                                blueLight = true;
                                awakeTimer = 0;
                            }
                            break;

                        // ------ Syringe LED OFF ------------------------------------------
                        case (MicrocontrollerCommands.LightsOff):
                            if (redLight || blueLight)
                            {
                                port.Write("9");
                                redLight = false;
                                blueLight = false;
                                awakeTimer = 0;
                            }
                            break;

                        // ------ Calibrate Syringe Pressure -------------------------------
                        case (MicrocontrollerCommands.ZeroSyringePressure):
                            port.Write("P");
                            awakeTimer = 0;
                            syringeValveOpen = true;
                            break;

                        // ------ Reset Syring hardware -------------------------------
                        case (MicrocontrollerCommands.ResetSyringe):
                            port.Write("9");
                            redLight = false;
                            blueLight = false;
                            awakeTimer = 0;
                            port.Write("3");  // this writes 0110011 ("3" is ASCII character 51)
                            syringeValveOpen = true;
                            break;
                        //case (MicrocontrollerCommands.SendData):
                        //	port.Write("S");
                        //	awakeTimer = 0;
                        //	break;
                        default:
                            break;
                    }

                }
                catch (System.Exception)
                {
                    connected = false;
                    retryConnection = true;
                    //Debug.Log("there is a problem with the arduino.");
                    port.Close();
                    //throw;
                }
            }
            try
            {
                port.Write("S");
                awakeTimer = 0;
            }
            catch (Exception)
            {

            }
        }
        commandQueue = new ArrayList();
        waitingForData = true;
    }
    // ------ Close Connection Sequence ------------------------------------------
    public void OnApplicationQuit()
    {
        streamThread.Abort();
        if (port != null && port.IsOpen)
            port.Close();
        if (port != null)
            port.Dispose();

        connected = false;
        Thread.Sleep(10);


    }

}
//}



/*
// Old Microcontroller code:

using UnityEngine;
using System.Collections;

using System;

using System.IO.Ports;
using System.Threading;


// THIS IS A SINGLETON:
// we have one microcontroller that many scripts frequently reference
// so it is appropriate to use a public static class for our microcontroller.
// it is also useful to use the property inspector for public variables - 
// and its also useful to use coroutines for serial connection and read cycles (although we use a thread for that).
// But, those two useful things come from : MonoBehavior.
// MonoBehavior classes cannot be static.
// So, we have an argument to use a singleton: 
// a regular MonoBehavior class and make one single instance of the whole class.
// any other script can access public (not public static) variables and funtions like this:
// Microcontroller.ME.SomeFunction();

// WE CANNOT USE THESE: 
// UNITY JIT compiler does not include these functions even though they compile in VB/MONO
// DataReceivedHandler
// ReadExisting
// BytesToRead
// DiscardNull
// DiscardInBuffer 
// DiscardOutBuffer


public class Microcontroller : MonoBehaviour
{

    public static Microcontroller ME;

    public int COM_PortNumber;

    public SerialPort Port;

    public bool RetryConnection = false;
    public bool connected = false;
    public bool WhiteboxConnectionAlert = false;

    private Thread StreamThread;

    private string InString;

    // these dont have to be public - but its handy to see it in the inspector.
    public string Incoming;

    public bool TUI_Button;
    public int USProbePressureNotchSide;
    public int USProbePressureFlatSide;
    public float ValvePressure;

    public bool RedLight;
    public bool BlueLight;
    public bool SyringeValveOpen;

    

    // Testing variables
    // private int lastthing;
    // public GameObject CamCube;

    public int AwakeTimer;
    public int AwakeTimerPeriod;

    void Awake()
    {

        if (ME != null)
            GameObject.Destroy(ME);
        else
            ME = this;

        DontDestroyOnLoad(this);

    }

    void Start()
    {
        Thread.Sleep(50);	// 50 ms
        Connect();
    }

    public void Connect()
    {
        // Port = null;
        try
        {

            // UnityEngine.Debug.Log("Restarting Connections");
            connected = false;
            Thread.Sleep(50);   // 50 ms
            StreamThread.Abort();
            Port.Close();
            Port.Dispose();

        }
        catch (Exception e)
        {
             Debug.Log(e);

        }

        connected = true;
        StreamThread = new Thread(ThreadListener);
        StreamThread.Start();

    }


    // dont do string comparisons, compare with ints or enums
    // dont do string + string


    public void ThreadListener()
    {

        try
        {

            Port = null;

            string portname = "COM" + COM_PortNumber.ToString();

            UnityEngine.Debug.Log("Attempting to connect to " + portname);

            Port = new SerialPort(portname, 19200, Parity.None, 8, StopBits.One);

            Port.Open();
            Port.ReadTimeout = 150;

            // Debug.Log("at this point the port is open, readtimeout set");
            connected = true;
            RetryConnection = false;

        }

        catch (Exception e)
        {
            RetryConnection = true;
            connected = false;
            StreamThread.Abort();
        }

        while (Port.IsOpen & connected)
        {
            try
            {
                InString = "";
                byte tempB = (byte)Port.ReadByte();

                if (tempB == '$')
                {

                    tempB = (byte)Port.ReadByte();

                    while (tempB != '\n')
                    {
                        InString += ((char)tempB);
                        tempB = (byte)Port.ReadByte();
                    }


                    try
                    {
                        // public variable for property inspector visibility
                        Incoming = InString;

                        string[] SplitArray = InString.Split(char.Parse(","));

                        if (SplitArray.Length == 3) // this older whitebox has no valve pressure sensor.
                        {
                            TUI_Button = (int.Parse(SplitArray[0]) > 0);
                            USProbePressureNotchSide = int.Parse(SplitArray[1]);
                            USProbePressureFlatSide = int.Parse(SplitArray[2]);
                            ValvePressure = 0;
                        }

                        if (SplitArray.Length == 4) // this newer whitebox has a valve pressure sensor.
                        {
                            TUI_Button = (int.Parse(SplitArray[0]) > 0);
                            USProbePressureNotchSide = int.Parse(SplitArray[1]);
                            USProbePressureFlatSide = int.Parse(SplitArray[2]);
                            ValvePressure = float.Parse(SplitArray[3]);
                        }
                    }
                    catch (Exception e)
                    {
                        // Debug.Log(e);
                        // this catches mostly parseInt errors from bad data that the arduino sometimes spits out after reads.
                        // Note the problem is most likely the poor .NET implementation of Serial.IO.Ports
                        // read this blog - its written by a popular embedded systems engineer who posts on stackoverflow alot: 
                        // www.sparxeng.com/blog/software/muse-use-net-system-io-ports-serialport
                    }
                }

            }
            catch (Exception e)
            {
                // Port.Dispose();
                RetryConnection = true;
                connected = false;
                //StreamThread.Abort();
                //Port.Close();
                // Port.Dispose();
                Debug.Log(e);

            }
        }
    }

    void Update()
    {
        if (RetryConnection)
        {
            // strobing through the COM ports like this introduces instability.
            // COM_PortNumber++;
            // if (COM_PortNumber > 20) COM_PortNumber = 1;
            Connect();
        }

        WhiteboxConnectionAlert = RetryConnection;

        AwakeTimer += (int)(Time.deltaTime * 1000);
        if (AwakeTimer > AwakeTimerPeriod)
        {
            AwakeTimer = 0;
            Send("Alive");
        }
    }



    // send commands to the microcontroller.
    // the commands are simple and few, therefore we can limit them to single bytes for quick parsing.

    public void Send(string theCommand)
    {

        if (Port.IsOpen & connected)
        {

            try
            {

                switch (theCommand)
                {

                    // ------ Unity is Alive and Well ---------------------------------
                    case ("Alive"):
                        Port.Write("A");    // writes ASCII character 65 "A",
                        AwakeTimer = 0;
                        break;

                    // ------ Large Tactile Pop ---------------------------------------
                    case ("LPop"):
                        Port.Write("1");    // writes 0110001 ("1" is ASCII character 49)
                        AwakeTimer = 0;
                        Debug.Log("POP");
                        break;

                    // ------ Large Tactile Pop ---------------------------------------
                    case ("SPop"):
                        Port.Write("2");    // writes 0110010 ("2" is ASCII character 50)
                        AwakeTimer = 0;
                        break;

                    // ------ Open Syringe Valve ---------------------------------------
                    case ("LORO"):
                        if (SyringeValveOpen)
                        {
                            // nothing - the syringe valve is already open.
                        }
                        else
                        {
                            Port.Write("3");  // this writes 0110011 ("3" is ASCII character 51)
                            SyringeValveOpen = true;
                            AwakeTimer = 0;
                        }
                        break;

                    // ------ Close Syringe Valve --------------------------------------
                    case ("LORC"):
                        if (SyringeValveOpen)
                        {
                            Port.Write("4");  // this writes 0110011 ("3" is ASCII character 51)
                            SyringeValveOpen = false;
                            AwakeTimer = 0;
                        }
                        else
                        {
                            // nothing - the valve is already closed
                        }
                        break;

                    // ------ Red Syringe LED ------------------------------------------
                    case ("RedLight"):
                        if (!RedLight)
                        {
                            Port.Write("8");
                            RedLight = true;
                            AwakeTimer = 0;
                        }
                        break;

                    // ------ Blue Syringe LED ------------------------------------------
                    case ("BlueLight"):
                        if (!BlueLight)
                        {
                            Port.Write("7"); 
                            BlueLight = true;
                            AwakeTimer = 0;
                        }
                        break;

                    // ------ Syringe LED OFF ------------------------------------------
                    case ("LightsOff"):
                        if (RedLight || BlueLight)
                        {
                            Port.Write("9");
                            RedLight = false;
                            BlueLight = false;
                            AwakeTimer = 0;
                        }
                        break;

                    // ------ Calibrate Syringe Pressure -------------------------------
                    case ("ZeroSyringePressure"):
                        Port.Write("P");
                        AwakeTimer = 0;
                        break;

                    default:
                        break;
                }

            }
            catch (System.Exception)
            {
                connected = false;
                RetryConnection = true;
                //Debug.Log("there is a problem with the arduino.");
                Port.Close();
                throw;
            }

        }
    }



    public void OnApplicationQuit()
    {
        StreamThread.Abort();
        Port.Close();
        Port.Dispose();
        connected = false;
        Thread.Sleep(10);


    }

}

/*
using UnityEngine;
using System.Collections;
using Uniduino;
using Uniduino.Helpers;
using System.Collections.Generic;
using System;


public class Microcontroller : MonoBehaviour
{

    public static Microcontroller ME;

    List<Arduino.Pin> received_pins;
    Arduino arduino;

    // Pin number values (location)
    private int CameraShutter = 4;
    private int USProbeFSR_NotchSide = 4;
    private int USProbeFSR_FlatSide = 3;
    private int LED_Red = 5;
    private int LED_Green = 6;
    private int LED_Blue = 7;
    private int HUB_Thumper_Subtle = 8;
    private int HUB_Thumper_Obvious = 9;
    private int SyringeValve = 2;
    private int OK_LED = 13;

    // Read value variables

    int TUI_ButtonShutterValue = 0;

    // Variables for this GUI script (won't need for final)
    // public string comPort = "";
    public bool retryConnection = true;
    public bool connected = false;
    public bool WhiteboxConnectionAlert = false;
    public bool TUI_Button = false;
    public int USProbePressureNotchSide = 0;
    public int USProbePressureFlatSide = 0;
    public bool RLED;
    public bool GLED;
    public bool BLED;
    public bool SyringeValveOpen;

    int timeout = 0;

    void Awake()
    {

        if (ME != null)
            GameObject.Destroy(ME);
        else
            ME = this;

        DontDestroyOnLoad(this);

    }

    // Use this for initialization
    void Start()
    {

        // Initialization code
        // !!!!!!!!!!!!!! DO NOT TOUCH !!!!!!!!!!!!!
        if (!UniduinoSetupHelpers.SerialPortAvailable())
        {
#if UNITY_EDITOR

            UniduinoSetupHelpers.OpenSetupWindow();
#else
			Debug.LogError("Uniduino SerialPort Support must be installed: is libMonoPosixHelper on your path?");
#endif
        }

        arduino = Arduino.global;                           // convenience, alias the global arduino singleton


        // arduino.Log = (s) => Debug.Log("Arduino: " + s);    // Attach arduino logs to Debug.Log
                                                            //hookEvents();                                       // set up event callbacks for received data
                                                            // !!!!!!!!!!!!!! END SENSITIVE !!!!!!!!!!!!!!!

        // Get the port name and push it to the console
        // Keep this, it's handy.  however it doesnt work, it implies we can set the port with this script so Dave removed it.
        // comPort = Arduino.guessPortNameWindows();
        // Debug.Log("CONNECTED TO PORT: " + comPort);

        // Call setup once after the Arduino is up and running
        arduino.Setup(setupCalls);
    }

    private void setupCalls()
    {
        //arduino.reportAnalog(0, 1);

        // Setting up all pin modes on the arduino
        arduino.pinMode(OK_LED, PinMode.OUTPUT);
        arduino.pinMode(SyringeValve, PinMode.OUTPUT);
        arduino.pinMode(CameraShutter, PinMode.INPUT);
        arduino.pinMode(HUB_Thumper_Subtle, PinMode.OUTPUT);
        arduino.pinMode(HUB_Thumper_Obvious, PinMode.OUTPUT);
        arduino.pinMode(LED_Blue, PinMode.OUTPUT);
        arduino.pinMode(LED_Green, PinMode.OUTPUT);
        arduino.pinMode(LED_Red, PinMode.OUTPUT);

        ////////// Initial writes below  ////////////

        arduino.digitalWrite(SyringeValve, Arduino.HIGH);
        SyringeValveOpen = true;
        arduino.digitalWrite(HUB_Thumper_Subtle, Arduino.LOW);
        arduino.digitalWrite(HUB_Thumper_Obvious, Arduino.LOW);

        // LED turned off
        arduino.digitalWrite(LED_Red, Arduino.LOW);
        arduino.digitalWrite(LED_Green, Arduino.LOW);
        arduino.digitalWrite(LED_Blue, Arduino.LOW);

        // OK LED keeps turning ON - microcontroller will flash it if too much time has passed 
        InvokeRepeating("KeepConnectivityLightOn", 0, 0.1f);

        arduino.reportAnalog(USProbeFSR_FlatSide, 1);
        arduino.reportAnalog(USProbeFSR_NotchSide, 1);

        // Report all digital data from port 0 [pin 0-7]
        arduino.reportDigital(0, 1);

    }

    void KeepConnectivityLightOn()
    {
        // OK LED switched on
        try
        {
            arduino.digitalWrite(OK_LED, Arduino.HIGH);
            // Debug.Log("Pin 13 HIGH sent!");
            if (arduino.IsOpen && arduino.Connected)
            {
                connected = true;
                WhiteboxConnectionAlert = false;
            }
        }
        catch (Exception)
        {
            Debug.Log("Pin 13 HIGH failed!");
            connected = false;
            WhiteboxConnectionAlert = true;
            try
            {
                arduino.Connect();
                arduino.Setup(setupCalls);
            }
            catch (Exception) { }
        }
    }



    // Update is used to poll data being SENT from the microcontroller, like camera button state and US Probe pressures.
    void Update()
    {

        if (arduino.Connected)
        {

            // TUI Button value label
            if (arduino.digitalRead(CameraShutter) == Arduino.HIGH)
            {
                TUI_Button = true;
            }
            else
            {
                TUI_Button = false;
            }

            // FSR value read label
            USProbePressureNotchSide = arduino.analogRead(USProbeFSR_NotchSide);

            // FSR value read label
            USProbePressureFlatSide = arduino.analogRead(USProbeFSR_FlatSide);


        }


    }


    void OnGUI_DISABLED()
    {

        if (arduino.Connected)
        {
            
            //try
            //{
            //    arduino.digitalWrite(OK_LED, Arduino.HIGH);
            //    Debug.Log("Pin 13 HIGH sent!");
            //}
            //catch (Exception) { Debug.Log("Pin 13 HIGH failed!"); }
            
            string connection_status;
            if (arduino != null && arduino.IsOpen && arduino.Connected)
            {
                connection_status = "Connected to Firmata protocol version " + arduino.MajorVersion + "." + arduino.MinorVersion;
                GUILayout.Label(connection_status);

            }
            else if (arduino != null && arduino.IsOpen)
            {
                GUILayout.Label("Connected but waiting for Firmata protocol version");
            }
            else
            {
                GUILayout.Label("Not connected");
            }

            // TUI Button value label
            if (arduino.digitalRead(CameraShutter) == Arduino.HIGH)
            {
                GUILayout.Label("TUI Button = 1");
                TUI_Button = true;
            }
            else
            {
                GUILayout.Label("TUI Button = 0");
                TUI_Button = false;
            }

            // FSR value read label
            USProbePressureNotchSide = arduino.analogRead(USProbeFSR_NotchSide);
            GUILayout.Label("Notch Side Pressure: " + USProbePressureNotchSide);

            // FSR value read label
            USProbePressureFlatSide = arduino.analogRead(USProbeFSR_FlatSide);
            GUILayout.Label("Flat Side Pressure: " + USProbePressureFlatSide);

            // LED Buttons to toggle on the sim
            if (GUILayout.Button("R LED"))
            {
                RLED = !RLED;
                if (RLED)
                    Send("RedLight");
                else
                    Send("RedLightOff");
            }

            if (GUILayout.Button("G LED"))
            {
                GLED = !GLED;
                if (GLED)
                    arduino.digitalWrite(LED_Green, Arduino.HIGH);
                else
                    arduino.digitalWrite(LED_Green, Arduino.LOW);
            }

            if (GUILayout.Button("B LED"))
            {
                BLED = !BLED;
                if (BLED)
                    Send("BlueLight");
                else
                    Send("BlueLightOff");
            }

            GUILayout.Label("\t");

            // GUI buttons for triggering pops on the sim
            if (GUILayout.Button("Feel SUBTLE pop"))
                Send("SPop");

            if (GUILayout.Button("Feel OBVIOUS pop"))
                Send("LPop");


            GUILayout.Label("\t");

            // GUI buttons for triggering pops on the sim
            if (GUILayout.Button("Syringe Valve"))
            {
                SyringeValveOpen = !SyringeValveOpen;
                if (SyringeValveOpen)
                    Send("LORO");
                else
                    Send("LORC");
            }
        }


    }

    // Call this in Start() to display Uniduino logs in the Unity log perspective
    protected void hookEvents()
    {
        arduino.AnalogDataReceived += delegate (int pin, int value)
        {
            Debug.Log("Analog data received: pin " + pin.ToString() + "=" + value.ToString());
        };

        arduino.DigitalDataReceived += delegate (int portNumber, int portData)
        {
            Debug.Log("Digital data received: port " + portNumber.ToString() + "=" + System.Convert.ToString(portData, 2));
        };

        arduino.VersionDataReceived += delegate (int majorVersion, int minorVersion)
        {
            Debug.Log("Version data received");
            arduino.queryCapabilities();
        };

        arduino.CapabilitiesReceived += delegate (List<Arduino.Pin> pins)
        {
            Debug.Log("Pin capabilities received");
            received_pins = pins; // cache the complete pin list here so we can use it without worrying about it being complete yet
        };

    }

    // Close the connection to the Arduino before leaving!
    void OnDestroy()
    {
        if (arduino != null)
            arduino.Disconnect();
        // Debug.Log("OnDestroy called");
    }

    void OnApplicationQuit()
    {
        stopEverything();
    }

    private void stopEverything()
    {
        ////////// All closes below  ////////////

        arduino.digitalWrite(SyringeValve, Arduino.HIGH);
        SyringeValveOpen = true;
        arduino.digitalWrite(HUB_Thumper_Subtle, Arduino.LOW);
        arduino.digitalWrite(HUB_Thumper_Obvious, Arduino.LOW);

        // LED turned off
        arduino.digitalWrite(LED_Red, Arduino.LOW);
        arduino.digitalWrite(LED_Green, Arduino.LOW);
        arduino.digitalWrite(LED_Blue, Arduino.LOW);
    }


    //IEnumerator FeelSubtlePop()
    //{
    //    arduino.digitalWrite(HUB_Thumper_Subtle, Arduino.HIGH);
    //    yield return new WaitForSeconds(0.12f);
    //    arduino.digitalWrite(HUB_Thumper_Subtle, Arduino.LOW);
    //}

    //IEnumerator FeelObviousPop()
    //{
    //    arduino.digitalWrite(HUB_Thumper_Subtle, Arduino.HIGH);
    //    yield return new WaitForSeconds(0.20f);
    //    arduino.digitalWrite(HUB_Thumper_Subtle, Arduino.LOW);
    //}
    


    public void Send(string theCommand)
    {
        switch (theCommand)
        {

            // ------ Large Tactile Pop ---------------------------------------
            case ("LPop"):
                //StartCoroutine(FeelObviousPop());
                arduino.digitalWrite(HUB_Thumper_Obvious, Arduino.HIGH);
                arduino.digitalWrite(HUB_Thumper_Obvious, Arduino.LOW);
                break;

            // ------ Large Tactile Pop ---------------------------------------
            case ("SPop"):
                //StartCoroutine(FeelSubtlePop());
                arduino.digitalWrite(HUB_Thumper_Subtle, Arduino.HIGH);
                arduino.digitalWrite(HUB_Thumper_Subtle, Arduino.LOW);
                break;

            // ------ Open Syringe Valve ---------------------------------------
            case ("LORO"):
                arduino.digitalWrite(SyringeValve, Arduino.HIGH);
                break;

            // ------ Close Syringe Valve --------------------------------------
            case ("LORC"):
                arduino.digitalWrite(SyringeValve, Arduino.LOW);
                break;

            // ------ Red Syringe LED ------------------------------------------
            case ("RedLight"):
                arduino.digitalWrite(LED_Red, Arduino.HIGH);
                break;

            // ------ Blue Syringe LED ------------------------------------------
            case ("BlueLight"):
                arduino.digitalWrite(LED_Blue, Arduino.HIGH);
                break;

            // ------ Red Syringe LED ------------------------------------------
            case ("RedLightOff"):
                arduino.digitalWrite(LED_Red, Arduino.LOW);
                break;

            // ------ Blue Syringe LED ------------------------------------------
            case ("BlueLightOff"):
                arduino.digitalWrite(LED_Blue, Arduino.LOW);
                break;


            // ------ Syringe LED OFF ------------------------------------------
            case ("LightsOff"):
                arduino.digitalWrite(LED_Red, Arduino.LOW);
                arduino.digitalWrite(LED_Blue, Arduino.LOW);
                break;

            default:
                break;
        }
    }


}
*/


//------------------------------------------------------------------------------------------------------------------------
// This is a test without the Ascension unit
// private int functionStepper= 0;
// private int whichfunction;
// private bool lastTUIbuttonState;
// private int CamClickCounter = 0;
/*
void Update()
{
    if (Port.IsOpen & Running)
    {
        CamCube.SetActive(TUI_Button);

        if (TUI_Button & !lastTUIbuttonState)
            CamClickCounter++;

        lastTUIbuttonState = TUI_Button;

        functionStepper++;
        if (functionStepper > 40)
        {
            functionStepper = UnityEngine.Random.Range(0, 35);
            whichfunction++;
            if (whichfunction > 7)
                whichfunction = 1;

            if (whichfunction == 1) Send("LPop");
            if (whichfunction == 2) Send("SPop");
            if (whichfunction == 3) Send("LORO");
            if (whichfunction == 4) Send("LORC");
            if (whichfunction == 5) Send("RedLight");
            if (whichfunction == 6) Send("BlueLight");
            if (whichfunction == 7) Send("LightsOff");
        }
    }


}
// *-/

/*
void OnGUI()
{

    //if (GUI.Button(new Rect(430, 80,  320, 30), "Camera Clicks received: " + CamClickCounter.ToString())) { }
    //if (GUI.Button(new Rect(430, 120, 320, 30), "reported by microcontroller: " + USProbePressureNotchSide.ToString())) { }

    //if (WhiteboxRestartAlert) {
    //	if (GUI.Button (new Rect (10, 60, 60, 30), "microcontroller")) 	{
    //		OpenConnection();
    //	}
    //}

     if (GUI.Button (new Rect (10, 60, 60, 30), "L Pop")) 	{ Send("LPop"); }
     if (GUI.Button (new Rect (10, 100, 60, 30), "S Pop")) 	{ Send("SPop"); }
     if (GUI.Button (new Rect (10, 140, 60, 30), "LOR O")) 	{ Send("LORO");	}
     if (GUI.Button (new Rect (10, 180, 60, 30), "LOR C"))   { Send("LORC"); }
     if (GUI.Button(new Rect(10, 220, 60, 30), "RED LED")) { Send("RedLight"); }
     if (GUI.Button(new Rect(10, 260, 60, 30), "BLUE LED")) { Send("BlueLight"); }
     if (GUI.Button(new Rect(10, 300, 60, 30), "LEDs OFF")) { Send("LightsOff"); }
}
*-/

// these SerialDataReceivedEventHandler things don't work in Unity's compiler.  FMF!F!
//public void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
//{
//    Debug.Log("I was called.");
//    //SerialPort sp = (SerialPort)sender;
//    //string stuff = sp.ReadLine();
//    //Debug.Log(stuff);
//}


/*
    void NOPEUpdate ()
    {

            if (Port.IsOpen & (!ConnectionError)) {


                    string tempS = "";

                    try {
                            byte tempB = (byte)Port.ReadByte ();
                            //Debug.Log(tempB);
                            while (tempB != '\n') { 
                                    tempS += ((char)tempB);
                                    tempB = (byte)Port.ReadByte ();
                            }

                            if (tempB == '\n') {
                                    Stream = tempS;
                                    Debug.Log(Stream);
                                    Port.DiscardOutBuffer();
                                    Port.DiscardInBuffer();

                            }

                    } catch (Exception e) {
                            //Debug.Log("jack shit found");
                    }


            }
    }
*-/



//-----------------------------------------------------------------------------
//-- Using a Coroutine - probably not useful if we go with a thread.
/*
    private StringBuilder sb = new StringBuilder ();

    IEnumerator StreamListener ()
    {

            while (Port.IsOpen & (!ConnectionError)) {
                    Listening = true; 
                    //try {
                    // -------- read incoming chars -----------------
                    //char incomingbyte = Port.ReadChar();
                    /*
                            int len = Port.BytesToRead;

                            if (len == 0)
                                    return "";

                            // read the buffer
                            byte[] buffer = new byte[len];
                            Port.Read (buffer, 0, len);
                            sb.Append (ASCIIEncoding.ASCII.GetString (buffer));

                            // got End Of Line?
                            if (sb.Length < 2 || sb [sb.Length - 2] != '\r' || sb [sb.Length - 1] != '\n')
                                    return "";

                            // if we are here we received both EOL chars
                            Stream = sb.ToString ();
                            sb.Length = 0;


                    // LETS JUST TRY TO READ ONE SINGLE FUCKING CHAR OK?

                    //} catch (Exception e) {
                    //		Debug.Log ("Error");
                    //}


                    // -------- read incoming chars -----------------

                    yield return null; 
                    Listening = false; 
            }	

    }
    *-/
//-----------------------------------------------------------------------------






//------------------------- EXAMPLE COROUTINE -----------------------------------------
/* Example Coroutine
* calls
* and bool tests for existance

public bool ExampleCoroutineRunning = false;

public void StartExampleCoroutine() {
if (!ExampleCoroutineRunning) {
    StartCoroutine("ExampleCoroutine");
}
}

public void StopExampleCoroutine() {
StopCoroutine("ExampleCoroutine");
ExampleCoroutineRunning = false; // if you stop using StopCoroutine(), code after yield does not run
}


IEnumerator ExampleCoroutine() {
while (ConnectionAttempts < 200) {
        ExampleCoroutineRunning = true; // handy bool to test if this is running
        ConnectionAttempts ++;
        yield return null; // will loop back at while() next frame
        ExampleCoroutineRunning = false; // this only runs when the while() ends.
}
}

*-/
//------------------------- EXAMPLE COROUTINE -----------------------------------------

}


*/
