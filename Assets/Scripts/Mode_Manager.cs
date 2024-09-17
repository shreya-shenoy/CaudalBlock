using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mode_Manager : MonoBehaviour
{
    public static Mode_Manager ME;

    //determine if in guided mode or not
    [SerializeField]
    private bool isGuided;

    private void Awake()
    {
        if (ME != null)
        {
            Destroy(ME);
        }
        ME = this;

    }
    public enum MODE
    {
        NOT_SET = 0,
        DEV = 1,
        DEMO = 2,
        STUDY = 3
    }
    public MODE CurrentMode { get; set; }

    public void SetGuidedMode()
    {
        isGuided = true;

        //turn off mode selection
        IV_Manager.ME.modeSelection.SetActive(false);

        //turn on arm selection
        IV_Manager.ME.armOrientationSelection.SetActive(true);

        //start vein generation bar
        VeinGenerationBar.ME.GenerateLoadingBar();

    }

    public void SetUnguidedMode()
    {
        isGuided = false;

        //turn off mode selection
        IV_Manager.ME.modeSelection.SetActive(false);

        //turn on arm selection
        IV_Manager.ME.armOrientationSelection.SetActive(true);

        //start vein generation bar
        VeinGenerationBar.ME.GenerateLoadingBar();

    }


}