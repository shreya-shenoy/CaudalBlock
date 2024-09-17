using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsManager : MonoBehaviour
{
    public GameObject optionsToggleButton;
    public GameObject optionsMenu;

    public MeshRenderer armRender;

    public void ToggleOptionsMenu()
    {
        // if (IV_Manager.ME.testmode == 3)
        // {
        //     IV_Manager.ME.optionsButton.SetActive(false);
        //     UltrasoundManagerSource.ME.UltrasoundScreenSize = UltrasoundManagerSource.SCREEN_DIMENSIONS.LARGE;
        // }
        // else
        // {
        //     IV_Manager.ME.optionsButton.SetActive(true);
        // }
    }

    public void ToggleUSSize()
    {
        if (UltrasoundManagerSource.ME.UltrasoundScreenSize == UltrasoundManagerSource.SCREEN_DIMENSIONS.LARGE)
        {
            UltrasoundManagerSource.ME.UltrasoundScreenSize = UltrasoundManagerSource.SCREEN_DIMENSIONS.SMALL;
        }
        else
        {
            UltrasoundManagerSource.ME.UltrasoundScreenSize = UltrasoundManagerSource.SCREEN_DIMENSIONS.LARGE;
        }
    }

    public void ToggleAnatomicVisualization()
    {
        if (armRender.enabled)
        {
            armRender.enabled = false;
        }
        else
        {
            armRender.enabled = true;
        }
    }

    public void CycleAnatomicDifficulty()
    {
        ProcedureTestManager.ME.AdvanceTestByOne();
    }
}
