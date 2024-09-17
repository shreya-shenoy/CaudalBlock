using UnityEngine;
using UnityEngine.UI;
using TMPro;

// This updates a version number automatically when you run the program in Editor. 
// Now we can build .exes or WebGLs without having to remember to update the version number. 
// The version is automatically generated based on date and time.
// It writes the text of the version number to a UI Text Asset. Here's the cool part:
// Changes to Text Assets persist after the program stops! Really convenient.
// Note, you may have to hard code major changes to the version numbers, like "V1." before the generated version number.
// YOU'RE WELCOME. - Dave L, 0110221003

[ExecuteInEditMode] // this means that the changes made in runtime act as though you made them manually with the inspector before running.
public class BuildDate : MonoBehaviour
{

    //public Text BuildDateTextAsset;

    public TMP_Text BuildDateTextAsset;
    // this Awake() runs only in Editor.

    public GameObject AboutVersion;


    void Awake()
    {

#if UNITY_EDITOR 

        if(AboutVersion.name == "AboutVersion")
        {
            string builddate = "V1." + System.DateTime.Now.ToString("MMddyyHHmm"); // check it out, the capital HH means 24 hour format! Did you know?
            Debug.Log("Updating Version: " + builddate);

            // Changes to Text Assets persist after stops! How convenient.
            BuildDateTextAsset.text = builddate;
        }

        else
        {
            string builddate = "V1." + System.DateTime.Now.ToString("MMddyyHHmm"); // check it out, the capital HH means 24 hour format! Did you know?
        Debug.Log("Updating Version: " + builddate);

        // Changes to Text Assets persist after stops! How convenient.
        BuildDateTextAsset.text = builddate;

        //This was an old implementation that saved to a text file in Assets - no longer needed but good to note. 
        //string outfile = "Assets/BuildDateTextFile.txt";
        //System.IO.File.WriteAllText(outfile, builddate + "\n");
        }


#endif

    }



}



