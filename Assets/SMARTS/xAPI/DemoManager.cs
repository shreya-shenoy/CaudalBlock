/*using UnityEngine;
using System.Collections;
using System.IO;
using System;
using UnityEngine.UI;

public class DemoManager : MonoBehaviour {

    /*This is a demo program to demonstrate basic functionality.  Unity_xAPI_Integrator is currently purposefully written to be vague.
     * This passes many of the decisions for implementation to the Activity Provider.
     * The result is a heavy use of input variables for function calls
     * See documentation for more in-depth function explaination 
     */
     /*
    public Slider colorPicker;  //slider to change color value
    public Image colorShower;  //image which changes color

    public InputField nameInputField;   //input field for user name
    public InputField emailInputField;  //input field for user email
    public InputField nameSIF;  //Name input field on the settings canvas
    public InputField emailSIF; //Email input field on the settings canvas
    public InputField lrsEndpointField;  //lrs Endpoint Field on the settings canvas.  Used to grab the string from the input field.
    public InputField lrsNameField;     //lrs Name Field on the settings canvas. Used to grab the string from the input field.
    public InputField lrsPassField;     //lrs Password Field on the settings canvas. Used to grab the string from the input field.
    public InputField sftpHostField;    //name of sftp host on settings page. Used to grab the string from the input field.
    public InputField sftpNameField;    //Username for sftp host on settings page. Used to grab the string from the input field.
    public InputField sftpPassField;    //password for sftp host on settings page. Used to grab the string from the input field.
    public string userEmail;    //user's email address.  Is used to identify user. Used to grab the string from the input field.
    public string userName;     //user's name.
    public string lrsEndpoint;  //Endpoint URL to send the statements to
    public string lrsName;      //Authorization name for the lrs
    public string lrsPass;      //Authorization password for the lrs
    public string sftpHost;     //Sftp host name/IP address
    public string sftpName;     //Username for the sftp
    public string sftpPass;     //Password for the sftp
    public float sliderValue;   //Current value of colorPicker slider
    public double score;    //user's current score, based on sliderValue
    public DateTime saveTime;   //the time at which the user hit the "save" button
    public StreamWriter sr;     //Streamwriter to create a test save file


    public string readTextVerbIn;   //Verb associated with the file you'll want to read from
    public string readTextActivityIn;   //Activity associated with the file you'll want to read from
    public string readTextFileLocation; //location of file to be read

	// Use this for initialization
	void Start () {
        sliderValue = 0;
        userEmail = "placeholder";
        userName = "placeholder";

        readTextVerbIn = "C:/Users/William/Documents/xAPI_Demo/Definitions/saved.txt";
        readTextActivityIn = "C:/Users/William/Documents/xAPI_Demo/Definitions/simulation.txt";
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    //called on "loadButton" click
    public void loadButtonHandler()
    {      
        GameObject.Find("xAPIManager").GetComponent<DownloadxAPIManager>().getLastSaveData(saveTime, "C:/Users/William/Documents/xAPI_Demo/Definitions/saved.txt", "C:/Users/William/Documents/xAPI_Demo/Definitions/simulation.txt");  //calls function that reads most recent save data file
        colorPicker.value =GameObject.Find("xAPIManager").GetComponent<DownloadxAPIManager>().dataf;  //set colorPicker value based upon data read from save
        GameObject.Find("xAPIManager").GetComponent<UploadxAPIManager>().statementCreator("loaded", "data");
    }

    //called on "loadPlaybackButton" click
    public void loadPlaybackHandler()
    {
        GameObject.Find("xAPIManager").GetComponent<DownloadxAPIManager>().getReadTextData(userName, userEmail,readTextVerbIn,readTextActivityIn, "http://vam-dev.anest.ufl.edu/xapi_demo_data/playback.txt");  //calls function that reads most recent playback data from server
    }

    //Example Query.  Called on "gQueryButton" click
    public void generalQueryHandler()
    {
        DateTime since = DateTime.ParseExact("2013-08-29 07:42:10Z", "u", System.Globalization.CultureInfo.InvariantCulture);
        GameObject.Find("xAPIManager").GetComponent<DownloadxAPIManager>().generalQuery(since, 3, userName, userEmail, "C:/Users/William/Documents/xAPI_Demo/Definitions/began.txt", "C:/Users/William/Documents/xAPI_Demo/Definitions/simulation.txt");
    }

    //Downloads file from server and creates xAPI statement. Called on "DownloadSaveButton" click
    public void fileDownloaderHandler()
    {
        GameObject.Find("xAPIManager").GetComponent<DownloadxAPIManager>().FileDownloader("/mnt/webvol/vam.anest.ufl.edu/xapi_demo_data/saved.txt", "C:\\Users\\William\\Documents\\xAPI_Demo\\Definitions\\saved.txt");
        GameObject.Find("xAPIManager").GetComponent<UploadxAPIManager>().statementCreator("downloaded", "data");
    }

    //called on "beginButton" click
    public void beginButtonHandler()
    {
        const string FILE_NAME = "C:/Users/William/Documents/xAPI_Demo/Definitions/saved.txt";

        //if save file doesn't exist, create one
        if (File.Exists(FILE_NAME)==false)
        {
            sr = File.CreateText(FILE_NAME);
            sr.Close();
        }

        //set userEmail variable to the inputed email
        userEmail = emailInputField.text;
        userName = nameInputField.text;
        GameObject.Find("xAPIManager").GetComponent<SettingsxAPIManager>().setActor(userName,userEmail);
        GameObject.Find("xAPIManager").GetComponent<UploadxAPIManager>().statementCreator("began","simulation",0.0);
    }

    //called in "endDemoButtonHandler()"
    public void sliderTracker()
    {
        //writes colorPicker.value to the file
        using (StreamWriter sw = new StreamWriter("C:/Users/William/Documents/xAPI_Demo/Definitions/saved.txt"))
        {
            sw.Write(colorPicker.value);
        }
    }

    //needs rename. Called on "saveButton" click. 
    public void saveSessionButtonHandler()
    {
        saveTime = DateTime.Now;  //saves time save button was clicked
        print(saveTime);
        sliderTracker();  //writes slider value to file
        GameObject.Find("xAPIManager").GetComponent<UploadxAPIManager>().statementCreator("saved", "simulation", score);  //creates statement with verb=saved, activity=simulation, result=score
    }
    
    //Sets settings from inside program.  Called on settingsCanvas's "closeButton" click
    public void saveSettingsHandler()
    {
        //set the userName to equal the input field text
        userName = nameSIF.text;
        userEmail = emailSIF.text;
        lrsEndpoint = lrsEndpointField.text;
        lrsName = lrsNameField.text;
        lrsPass = lrsPassField.text;
        sftpHost = sftpHostField.text;
        sftpName = sftpNameField.text;
        sftpPass = sftpPassField.text;
        
        //if input field isn't blank, set variables
        if ((nameSIF.text != "") && (emailSIF.text != ""))
        {
            userName = nameSIF.text;
            userEmail = emailSIF.text;
            GameObject.Find("xAPIManager").GetComponent<SettingsxAPIManager>().setActor(userName, userEmail);
        }
        if ((lrsEndpoint != "") && (lrsName != "") && (lrsPass != ""))
        {
            GameObject.Find("xAPIManager").GetComponent<SettingsxAPIManager>().setLRS(lrsEndpoint, lrsName, lrsPass);
        }
        if ((sftpHost != "") && (sftpName != "") && (sftpPass != ""))
        {
            GameObject.Find("xAPIManager").GetComponent<SettingsxAPIManager>().setSFTP(sftpHost, sftpName, sftpPass);
        }
    }

    //Called on "savePlaybackButton" click
    public void savePlaybackHandler()
    {
        GameObject.Find("xAPIManager").GetComponent<UploadxAPIManager>().statementCreator("saved", "playback", score); //creates statement with verb=saved, activity=playback, result=score
    }

    //Called on "colorSlider" slider value change. 
    public void colorSlider()
    {
        sliderValue = colorPicker.value;  //sets sliderValue based upon colorPicker.value
        imageColorizer(sliderValue);  //changes image color based upon sliderValue;
    }


    //called in "colorSlider()"
    public void imageColorizer(float colorValue)
    {
        colorShower.color = new Color (sliderValue,0,0,1); //changes image color based upon sliderValue
        score = sliderValue * 100;  //changes user score based upon sliderValue. Multiplied by 100 to make it easier to read
        

        //if the sliderValue reaches 1, the color is completely red and the user gets a score of 100.  Make a statement noting this event.
        if(colorShower.color.r == 1)
        {
            score = 100.0;
            GameObject.Find("xAPIManager").GetComponent<UploadxAPIManager>().statementCreator("set", "bright_red", score);
        }
    }
}
*/