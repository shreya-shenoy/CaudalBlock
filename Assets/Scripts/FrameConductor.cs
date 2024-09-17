using UnityEngine;
using System.Collections;

public class FrameConductor : MonoBehaviour {
	
	public static bool UltrasoundFrame;
	public static bool DeformableAnatomyFrame;
	public static bool DataTextureFrame;

    public bool UltrasoundFrameOff;
    public bool DeformableAnatomyFrameOff;
    public bool DataTextureFrameOff;

	private int Framecounter;
	
	// Use this for initialization
	void Start () {
		Framecounter = 0;
	}
	
	// Update is called once per frame
	void LateUpdate () {
		
		Framecounter ++;
		
		if (Framecounter > 3) Framecounter = 1;

        UltrasoundFrame         = (Framecounter == 1);
        DeformableAnatomyFrame  = (Framecounter == 2);
        DataTextureFrame 		= (Framecounter == 3);
		

        if (UltrasoundFrameOff) UltrasoundFrame = false;
        if (DataTextureFrameOff) DataTextureFrame = false;
        if (DeformableAnatomyFrameOff) DeformableAnatomyFrame = false;

	}
}
