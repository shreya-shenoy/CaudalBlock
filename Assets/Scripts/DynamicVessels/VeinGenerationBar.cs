using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// VeinGenerationBar.cs
/// Used to generate a loading bar while veins are being generated
/// Stephen Berkner 1/17/2020
/// </summary>


public class VeinGenerationBar : MonoBehaviour
{
    //static singleton
    public static VeinGenerationBar ME;

    //field for text
    [SerializeField]
    private GameObject veinGenerationTextObject = null;

    //text from gameobject
    private UnityEngine.UI.Text veinGenerationText = null;

    //field for loading bar
    [SerializeField]
    private GameObject veinGenerationTextureObject = null;

    //rawImage from gameobject
    private RawImage veinGenerationRawImage = null;

    //gameobjects with buttons to enable when loading bar is complete
    [SerializeField]
    private GameObject HandUpSelection = null;

    [SerializeField]
    private GameObject HandDownSelection = null;

    //determine when to generate loading bar
    private bool generating = false;
    private float loadedPercentage = 0.00f;

    //determine how fast bar loads
    [SerializeField]
    private float loadIncrement = 0.025f;

    //determine point at which bar will wait for connection to complete
    [SerializeField]
    private float loadLimit = 0.95f;

    //boolean to enable/disable ATC Connection Requirement for non-whitebox connected machines
    [SerializeField]
    private bool DevelopmentMode;


    private void Awake()
    {
        //maintaining the singleton
        if (ME != null)
        {
            Destroy(ME);
        }
        ME = this;

        //get text and texture
        veinGenerationText = veinGenerationTextObject.GetComponent<UnityEngine.UI.Text>();
        veinGenerationRawImage = veinGenerationTextureObject.GetComponent<RawImage>();

        //enable development mode
        //DISABLE BEFORE BUILDING EXECUTABLE
        DevelopmentMode = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       if (generating)
       {
            LoadingBarPercentage(loadedPercentage);

            if (loadedPercentage > 1f)
            {
                generating = false;

                HandUpSelection.GetComponent<UnityEngine.UI.Button>().enabled = true;
                HandDownSelection.GetComponent<UnityEngine.UI.Button>().enabled = true;

            } else if (loadedPercentage > loadLimit && (SMMARTS_SDK.ATC.ME.ConnectionStatus == SMMARTS_SDK.ATC.Connection_Status.Connected)) {
                
                loadedPercentage += loadIncrement;

            } else if (loadedPercentage < loadLimit)
            {
                loadedPercentage += loadIncrement;
            }
       } else
       {
            loadedPercentage = 0f;
       } 
       
       if (DevelopmentMode)
       {
           HandUpSelection.GetComponent<UnityEngine.UI.Button>().enabled = true;
           HandDownSelection.GetComponent<UnityEngine.UI.Button>().enabled = true;
       }
    }

    public void GenerateLoadingBar()
    {
        generating = true;

    }

    public void ZeroLoadingBar()
    {
        Texture2D toApply = new Texture2D(2000, 100);
        toApply.filterMode = FilterMode.Point;

        for (int col = 0; col< 100; col++)
        {
            for (int row = 0; row < 2000; row++)
            {
                toApply.SetPixel(row, col, Color.white);
            }
        }

        toApply.Apply();

        veinGenerationRawImage.texture = toApply;
    }

    public void LoadingBarPercentage(float percentage)
    {
        Texture2D toApply = new Texture2D(2000, 100);
        toApply.filterMode = FilterMode.Point;

        //apply loaded texture
        for (int col = 0; col < 100; col++)
        {
            for (int row = 0; row < (int) (2000 * percentage); row++)
            {
                toApply.SetPixel(row, col, Color.blue);
            }
        }

        //apply unloaded texture
        for (int col = 0; col < 100; col++)
        {
            for (int row = (int) (2000*percentage); row < 2000; row++)
            {
                toApply.SetPixel(row, col, Color.white);
            }
        }
        
        toApply.Apply();

        veinGenerationRawImage.texture = toApply;
        
    }

    
}
   