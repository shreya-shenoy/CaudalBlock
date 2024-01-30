using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SMMARTS_SDK;

public class Microcontroller_Manager : MonoBehaviour
{
    public static Microcontroller_Manager ME;

    void Update()
    {
        //Debug.Log("hi: " + float.Parse(IVMicrocontroller.ME.MicrocontrollerData[microcontrollerDataSetIndexOfTourniquetPressure]));
    }
    private void Awake()
    {
        if (ME != null)
            Destroy(ME);
        ME = this;
    }


    //data indexes
    int microcontrollerDataSetIndexOfTourniquetPressure = 3;
    int microcontrollerDataSetIndexOfHandSlapPressure = 4;
    int microcontrollerDataSetIndexOfTractionPressure = 5;


    public float TourniquetPressure
    {
        get
        {
            if (IVMicrocontroller.ME.Connected && IVMicrocontroller.ME.MicrocontrollerData.Length>=microcontrollerDataSetIndexOfTourniquetPressure + 1)
                return float.Parse(IVMicrocontroller.ME.MicrocontrollerData[microcontrollerDataSetIndexOfTourniquetPressure]); //IVMicrocontroller.ME.TourniquetPressure;//
            else
                return 0;
        }
    }
    public float HandSlapPressure
    {
        get
        {
            if (IVMicrocontroller.ME.Connected && IVMicrocontroller.ME.MicrocontrollerData.Length >= microcontrollerDataSetIndexOfHandSlapPressure + 1)
                return float.Parse(IVMicrocontroller.ME.MicrocontrollerData[microcontrollerDataSetIndexOfHandSlapPressure]); //IVMicrocontroller.ME.SlapPressure;// 
           else
                return 0;
        }
    }
    public float TractionPressure
    {
        get
        {
            if (IVMicrocontroller.ME.Connected && IVMicrocontroller.ME.MicrocontrollerData.Length >= microcontrollerDataSetIndexOfTractionPressure + 1)
                return float.Parse(IVMicrocontroller.ME.MicrocontrollerData[microcontrollerDataSetIndexOfTractionPressure]); //IVMicrocontroller.ME.TractionPressure;// 
            else
                return 0;
        }
    }

    public enum COMMAND
    {
        ZeroSyringePressure,
        ResetSyringe,
        LightsOff,
        BlueLight,
        RedLight,
        LORC,
        LORO,
        LPop,
        SPop
    }
    public void SendCommand(COMMAND command)
    {
        if (command == COMMAND.ZeroSyringePressure)
        {
            IVMicrocontroller.ME.SendCommand(IVMicrocontroller.MicrocontrollerCommand.Microcontroller_Command_P);
        }
        else if (command == COMMAND.ResetSyringe)
        {
            IVMicrocontroller.ME.SendCommand(IVMicrocontroller.MicrocontrollerCommand.Microcontroller_Command_9);
            IVMicrocontroller.ME.SendCommand(IVMicrocontroller.MicrocontrollerCommand.Microcontroller_Command_3);
        }
        else if (command == COMMAND.LightsOff)
        {
            IVMicrocontroller.ME.SendCommand(IVMicrocontroller.MicrocontrollerCommand.Microcontroller_Command_9);
        }
        else if (command == COMMAND.BlueLight)
        {
            IVMicrocontroller.ME.SendCommand(IVMicrocontroller.MicrocontrollerCommand.Microcontroller_Command_7);
        }
        else if (command == COMMAND.RedLight)
        {
            IVMicrocontroller.ME.SendCommand(IVMicrocontroller.MicrocontrollerCommand.Microcontroller_Command_8);
        }
        else if (command == COMMAND.LORC)
        {
            IVMicrocontroller.ME.SendCommand(IVMicrocontroller.MicrocontrollerCommand.Microcontroller_Command_4);
        }
        else if (command == COMMAND.LORO)
        {
            IVMicrocontroller.ME.SendCommand(IVMicrocontroller.MicrocontrollerCommand.Microcontroller_Command_3);
        }
        else if (command == COMMAND.SPop)
        {
            IVMicrocontroller.ME.SendCommand(IVMicrocontroller.MicrocontrollerCommand.Microcontroller_Command_2);
        }
        else if (command == COMMAND.LPop)
        {
            IVMicrocontroller.ME.SendCommand(IVMicrocontroller.MicrocontrollerCommand.Microcontroller_Command_1);
        }
    }
}
