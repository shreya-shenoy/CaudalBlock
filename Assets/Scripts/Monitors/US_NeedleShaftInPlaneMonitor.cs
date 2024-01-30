using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// -------------- PURPOSE and ORIGIONAL PROBLEM -----------------------
// The 2017 CVA scoring algorithm and IT needs to grade your in-plane approach.
// This means it needs to know how much of the needle is in the insonating plane (which is easy) and 
// compare it with how much of the needle isn't under the insonating plane, but could be (a little harder.)
// We are going to use image processing to do the trick.  We set up a camera that looks down the ultrasound probe's 
// insonating plane, and make it wide enough to see each side of the insonating plane.  
// The needle is already in a Layer "Needle" so we set the cameara mask to see only the needle and nothing else.
// if we know where the center of the image is, we can do a pixel comparison of how many pixels appear in the total image 
// and compare that number to how many pixels are lined up with the middle of the probe.
// This makes the monitor agnostic to partial needle insertions, anisotropy, other anatomy, depth, etc.
// it only cares about how well you can line the needle and probe up.  
// It doesnt care about the tip either - another monitor already does that.
// There are anisotropy issues on the ends of the needle, but I've played with this for about an hour: 
// I believe that if GetFractionInPlane() is greater than 0.66, you can see the shaft.
// It may not be 1 but anything greater than 2/3 is just fine.
// The camera and render texture is set up so that the camera sees 6.5mm away from either side of the US probe.
// thats very close to the width of the probe.  if your needle is farther off than that, the score will default to zero.
// The render texture dimensions are a power of two. They line up with the cameara's width and depth
// to give a 6.5mm sensitive range off to either side of the midline while being as wide as the insonating plane.
// This means about 16,300 pixel comparisons per test, which is doable. 
// The sensitive centerline is six pixels high; to adjust, line the needle up with the US probe in the Scene.
// that centerline thickness should change if we go with a smaller texture for performance reasons.
// - Dave Lizdas 6/5/17

// -------------- DEPENDENCIES ----------------------------------------
// 1) An Ultrasound Probe.  Specifically, the RA style 20MHz probe. 
//      I expect this script to require minor but careful changes for other style probes.
// 2) An orthographic camera attached to the probe.  Mine is a child of "Ultrasound Probe" GO.
//      Clear Flags to solid color - a White background. Culling Mask is Needle.
//      The following parameters line it up perfectly for the 20MHz Wide RA Probe:
//      Transform position is 0, 0, -25.4; rotation is 180 degrees about Y axis.  
//      Size is 6.72, Clipping Planes 0 and 65
//      Target Texture is "USInPlaneNeedlingRenderTexture"
// 3) A render texture called USInPlaneNeedlingRenderTexture:
//      256 x 64 pixels, ARGB32 bit (smaller may work faster?)
//      no depth buffer, clamped, point filter.  No aniso.   


// --------------- HOW OTHER SCRIPTS REFERENCE ------------------------
// on initialization: this script should be a component of the GO that has the CVA scorer
// US_NeedleShaftInPlaneMonitor US_NeedleShaftInPlaneMonitor = GetComponent<US_NeedleShaftInPlaneMonitor>();
// 
// during use - say we detected the needle was pushed farther in:
// if (US_NeedleShaftInPlaneMonitor.GetFractionInPlane() > 0.6F ) {

public class US_NeedleShaftInPlaneMonitor : MonoBehaviour
{
    [Header("------------ Initialization ------------ ")]
    // InPlaneCamera is a special camera that renders to "USInPlaneNeedlingRenderTexture"
    // and it has been sized to work with the RA wide probe.  
    // This script should be packaged with with the camera and render texture.
    // Remember, the CAMERA MASK sees the ONLY the needle and a white background.
    [SerializeField]
    Camera inPlaneCamera;

    // This is the reference to the "USInPlaneNeedlingRenderTexture" render texture
    [SerializeField]
    RenderTexture renderTexture;

    // this is the width and height of the render texture
    private int width, height;

    // this is a copy of the texure that we need to pull pixels from 
    // render textures are GPU objects, and GetPixel is a CPU method. 
    // this is why there are extra steps in referenceing pixels from a render texture
    private Texture2D tex;

    [Header("------------ Live Outputs ------------ ")]

    // this is the main point of the script - how much of the needle can we see?
    [SerializeField]
    private float fractionInPlane; // dimensionless fraction. 0 means you suck, 1 means you rock.

    // these keep count of the pixels seen by the camera.  This is pretty cool to see in the inspector
    [SerializeField]
    private int allNeedlePixels, needlePixelsInPlane;

    // Use this for initialization
    void Start()
    {
        // Null checks:
        if (inPlaneCamera == null) Debug.LogAssertion("inPlaneCamera is Null.");
        if (renderTexture == null) Debug.LogAssertion("renderTexture is Null.");

        width = Mathf.FloorToInt(renderTexture.width);
        height = Mathf.FloorToInt(renderTexture.height);
        
        tex = new Texture2D(width, height);
    }

    // --------------------------- Accessor Methods ---------------------------
    public float GetFractionInPlane
    {
        get
        {
            return fractionInPlane;
        }
    }

    public int GetNeedlePixelsInPlane
    {
        get
        {
            return needlePixelsInPlane;
        }
    }

    public int GetAllNeedlePixels
    {
        get
        {
            return allNeedlePixels;
        }
    }
    // -------------------------- No Mutator Methods --------------------------

    // Update is called once per frame
    void Update()
    {

        // The next few hoops are what you have to do to read pixels from a render texture.

        // Remember currently active render texture
        RenderTexture currentActiveRT = RenderTexture.active;

        // Set the supplied RenderTexture as the active one
        RenderTexture.active = renderTexture;

        // OLD CODE:
        // Create a new Texture2D - must do this every frame
        // tex = new Texture2D(width, height);  
        // NEW CODE 9/20/17:
        // we moved the new Texture2D into start().
        // doing it here created many new textures every second and fills up memory.  
        // its obvious under profiler memory tab, look at the total objects number climb.  you can't treat textures like this.
        // if you want to display the texture to the screen, take a look at Ultrasound Data Texture script, that's what its meant for.
        // tried Destroy(tex); but this slows down frame rate.  
        // "must do this every frame."  I needed to do this every frame to SEE the texture UPDATE on the screen during development.
        // however, this monitor is not SEEN by the user: we use the texture as a means for analysis. -DAVE


        // and read the RenderTexture image into it
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);

        // Restore previously active render texture
        RenderTexture.active = currentActiveRT;

        // Test Getpixel
        allNeedlePixels = 0;
        needlePixelsInPlane = 0;

        // The width of the needle is about four pixels in the texture.
        // if we treat is like six, the algorithm works better.
        int dx1 = 3;
        int dx2 = 3;

        // Sweeps across the texture - across the width of the beam.
        for (int i = 0; i < tex.width; i++)
        {
            // sweeps up and down across the entire height of the texture
            for (int j = 0; j < tex.height; j++)
            {
                // Any non-background white pixel has to be the needle.  add them up.
                if (tex.GetPixel(i, j) != Color.white) allNeedlePixels++;
            }

            // sweeps up and down across a narrow section of texture height aligned with the center of the probe
            for (int j = tex.height / 2 - dx1; j < tex.height / 2 + dx2; j++)
            {
                // Any non-background white pixel has got to be the needle. add them up.
                if (tex.GetPixel(i, j) != Color.white) needlePixelsInPlane++;
            }

        }

        // calculate the fraction of needle pixels that line up with the middle - i.e. the insonating plane
        if (allNeedlePixels > 0)
            fractionInPlane = needlePixelsInPlane / (float)allNeedlePixels; // dimenionless fraction.
        else
            fractionInPlane = 0;
    }

}
