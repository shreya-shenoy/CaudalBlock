using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ShowFPS : MonoBehaviour
{
    public int numberOfFrames;
    public float LastCycleTimestamp;
    public Text FPStext;
    public float averageframeduration;
    public int framecount;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        framecount++;
        if (framecount > numberOfFrames)
        {
            
            float deltaT = Time.time - LastCycleTimestamp; // seconds

            averageframeduration = deltaT / (float)framecount * 1000; // ms
            int fps = (int)(1000 / averageframeduration);
            //FPStext.text = averageframeduration.ToString("00") + "ms"; // this shows ms per frame
            FPStext.text = "FPS: " + fps.ToString(); // this shows fps
            framecount = 0;
            LastCycleTimestamp = Time.time;


        }
    }
}