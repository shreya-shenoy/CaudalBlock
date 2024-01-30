using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toggle_About : MonoBehaviour
{
    public GameObject AboutPanel;

    public void OpenAbout()
    {
        if (AboutPanel != null)
        {
            bool isActive = AboutPanel.activeSelf;

            AboutPanel.SetActive(!isActive);
        }
    }
}
