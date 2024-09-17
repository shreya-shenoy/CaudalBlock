/*using UnityEngine;
using System.Collections;
using System;
using TinCan;
using TinCan.LRSResponses;
using System.IO;
using Renci.SshNet;


public class UploadxAPIManager : MonoBehaviour {


   RemoteLRS lrs;


    void Start()
    {
        //Unity doesn't trust certificates by default, this mutes the error that will be returned
        System.Net.ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => { return true; };

        //lrs settings.
        lrs = new RemoteLRS();

    }

    //Uploads file to server  -- uses default filePath and serverPath variables in code. 
    public void FileUploader()
    {
        FileInfo f = new FileInfo(GameObject.Find("xAPIManager").GetComponent<SettingsxAPIManager>().filePath);
        string uploadfile = f.FullName; //grabs the name of the file
        print("uploadfile" + uploadfile); 
        var client = GameObject.Find("xAPIManager").GetComponent<SettingsxAPIManager>().client; 
        client.Connect();
        if (client.IsConnected)
        {
            print("Client is connected.");
        }
        var fileStream = new FileStream(uploadfile, FileMode.Open);
        if (fileStream != null)  //checks to make sure the file to upload isn't empty.
        {
            print("fileStream is not null.");
        }
        client.BufferSize = 4 * 1024;  //sets the buffer size for the file upload
        client.UploadFile(fileStream, GameObject.Find("xAPIManager").GetComponent<SettingsxAPIManager>().serverPath + f.Name, null);
        client.Disconnect();
    }

    //Uploads file to server -- allows for filePath input to specify specific file, uses serverPath variable in code
    //NOTE: file path must use double backslashes to work (ex: "C:\\Users\\Person\\Documents\\xAPI_Demo\\Assets\\saved.txt")
    public void FileUploader(string filePathIn)
    {
        FileInfo f = new FileInfo(filePathIn);
        string uploadfile = f.FullName; //grabs the name of the file
        print(f.Name);
        print("uploadfile" + uploadfile);
        var client = GameObject.Find("xAPIManager").GetComponent<SettingsxAPIManager>().client;
        client.Connect();
        if (client.IsConnected)
        {
            print("Client is connected.");
        }
        var fileStream = new FileStream(uploadfile, FileMode.Open);
        if (fileStream != null)//checks to make sure the file to upload isn't empty.
        {
            print("fileStream is not null.");
        }
        client.BufferSize = 4 * 1024;//sets the buffer size for the file upload
        client.UploadFile(fileStream, GameObject.Find("xAPIManager").GetComponent<SettingsxAPIManager>().serverPath + f.Name, null);
        client.Disconnect();
    }

    //fileUploader -- file path, server path -- specify file to upload by file path, specificy server to upload to via server path
    //Note: double backslashes are required for filePath. 
    //Note: Make sure you have proper permissions set for the server path. Do not inlcude file name in server path, that is grabbed from the filePath.
    //example: To write the file "saved.txt", filePath "C:\\Users\\Person\\Documents\\xAPI_Demo\\Assets\\saved.txt", to the server location "xapi_demo_data" 
    //type the serverPath "/mnt/webvol/vam.anest.ufl.edu/xapi_demo_data/"
    public void FileUploader(string filePathIn, string serverPathIn)
    {
        FileInfo f = new FileInfo(filePathIn);
        string uploadfile = f.FullName;//grabs the name of the file
        print(f.Name);
        print("uploadfile" + uploadfile);
        var client = GameObject.Find("xAPIManager").GetComponent<SettingsxAPIManager>().client;
        client.Connect();
        if (client.IsConnected)
        {
            print("Client is connected.");
        }
        var fileStream = new FileStream(uploadfile, FileMode.Open);
        if (fileStream != null)//checks to make sure the file to upload isn't empty.
        {
            print("fileStream is not null.");
        }
        client.BufferSize = 4 * 1024;//sets the buffer size for the file upload
        client.UploadFile(fileStream, serverPathIn + f.Name, null);
        client.Disconnect();
    }

    //basic statementCreator -- verb, activity
    public void statementCreator(string inputVerb, string inputActivity)
    {
        //Initializes statement variables
        var verb = new Verb();
        var activity = new Activity();

        //Defines the verb ID by inserting the given verb name into a string for the URI
        verb.id = new Uri("C:/Users/William/Documents/xAPI_Demo/Definitions/" + inputVerb + ".txt");
        verb.display = new LanguageMap();
        verb.display.Add("en-US", inputVerb);

        //Defines the activity ID by inserting the given activity name into a string for the URI
        activity.id = new Uri("C:/Users/William/Documents/xAPI_Demo/Definitions/" + inputActivity + ".txt").ToString();
        activity.definition = new ActivityDefinition();
        activity.definition.name = new LanguageMap();
        activity.definition.name.Add("en-US", inputActivity);

        //sends the created variables to the pushStatement method to finish statement creation and upload
        pushStatement(verb, activity);

    }

    //statementCreator -- verb, activity, result
    public void statementCreator(string inputVerb, string inputActivity, double inputResult)
    {
        //Initializes statement variables
        var verb = new Verb();
        var activity = new Activity();
        var result = new Result();

        //Defines the verb ID by inserting the given verb name into a string for the URI
        verb.id = new Uri("C:/Users/William/Documents/xAPI_Demo/Definitions/" + inputVerb+".txt");
        verb.display = new LanguageMap();
        verb.display.Add("en-US", inputVerb);

        //Defines the activity ID by inserting the given activity name into a string for the URI
        activity.id = new Uri("C:/Users/William/Documents/xAPI_Demo/Definitions/" + inputActivity+".txt").ToString();
        activity.definition = new ActivityDefinition();
        activity.definition.name = new LanguageMap();
        activity.definition.name.Add("en-US", inputActivity);

        //Sets the score variabe to equal the given inputResult
        result.score = new Score();
        result.score.raw = inputResult;

        //sends the created variables to the pushStatement method to finish statement creation and upload
        pushStatement(verb, activity, result);

    }

    //statementCreator -- verb, activity, file
    public void statementCreator(string inputVerb, string inputActivity, string filePathIn)
    {
        //Initializes statement variables
        var verb = new Verb();
        var activity = new Activity();

        //Defines the verb ID by inserting the given verb name into a string for the URI
        verb.id = new Uri("C:/Users/William/Documents/xAPI_Demo/Definitions/" + inputVerb + ".txt");
        verb.display = new LanguageMap();
        verb.display.Add("en-US", inputVerb);

        //Defines the activity ID by inserting the given activity name into a string for the URI
        activity.id = new Uri("C:/Users/William/Documents/xAPI_Demo/Definitions/" + inputActivity + ".txt").ToString();
        activity.definition = new ActivityDefinition();
        activity.definition.name = new LanguageMap();
        activity.definition.name.Add("en-US", inputActivity);

        //Uploads the file given as input
        FileUploader(filePathIn);

        //sends the created variables to the pushStatement method to finish statement creation and upload
        pushStatement(verb, activity);
    }

    //statementCreator -- verb, activity, file, result
    public void statementCreator(string inputVerb, string inputActivity, string filePath, double inputResult)
    {
        //Initializes statement variables
        var verb = new Verb();
        var activity = new Activity();
        var result = new Result();

        //Defines the verb ID by inserting the given verb name into a string for the URI
        verb.id = new Uri("C:/Users/William/Documents/xAPI_Demo/Definitions/" + inputVerb + ".txt");
        verb.display = new LanguageMap();
        verb.display.Add("en-US", inputVerb);

        //Defines the activity ID by inserting the given activity name into a string for the URI
        activity.id = new Uri("C:/Users/William/Documents/xAPI_Demo/Definitions/" + inputActivity + ".txt").ToString();
        activity.definition = new ActivityDefinition();
        activity.definition.name = new LanguageMap();
        activity.definition.name.Add("en-US", inputActivity);

        //Sets the score variabe to equal the given inputResult
        result.score = new Score();
        result.score.raw = inputResult;

        //Uploads the file given as input
        FileUploader(filePath);

        //sends the created variables to the pushStatement method to finish statement creation and upload
        pushStatement(verb, activity, result);
    }

    //statementCreator -- verb, activity, file, serverPath
    public void statementCreator(string inputVerb, string inputActivity, string filePathIn, string serverPathIn)
    {
        //Initializes statement variables
        var verb = new Verb();
        var activity = new Activity();

        //Defines the activity ID by inserting the given activity name into a string for the URI
        verb.id = new Uri("C:/Users/William/Documents/xAPI_Demo/Definitions/" + inputVerb + ".txt");
        verb.display = new LanguageMap();
        verb.display.Add("en-US", inputVerb);

        //Defines the activity ID by inserting the given activity name into a string for the URI
        activity.id = new Uri("C:/Users/William/Documents/xAPI_Demo/Definitions/" + inputActivity + ".txt").ToString();
        activity.definition = new ActivityDefinition();
        activity.definition.name = new LanguageMap();
        activity.definition.name.Add("en-US", inputActivity);

        //Uploads the file given as input to the server path given
        FileUploader(filePathIn, serverPathIn);

        //sends the created variables to the pushStatement method to finish statement creation and upload
        pushStatement(verb, activity);
    }

    //statementCreator -- verb, activity, file, severPath, result
    public void statementCreator(string inputVerb, string inputActivity, string filePathIn, string serverPathIn, double inputResult)
    {
        //Initializes statement variables
        var verb = new Verb();
        var activity = new Activity();
        var result = new Result();

        //Defines the activity ID by inserting the given activity name into a string for the URI
        verb.id = new Uri("C:/Users/William/Documents/xAPI_Demo/Definitions/" + inputVerb + ".txt");
        verb.display = new LanguageMap();
        verb.display.Add("en-US", inputVerb);

        //Defines the activity ID by inserting the given activity name into a string for the URI
        activity.id = new Uri("C:/Users/William/Documents/xAPI_Demo/Definitions/" + inputActivity + ".txt").ToString();
        activity.definition = new ActivityDefinition();
        activity.definition.name = new LanguageMap();
        activity.definition.name.Add("en-US", inputActivity);

        //Sets the score variabe to equal the given inputResult
        result.score = new Score();
        result.score.raw = inputResult;

        //Uploads the file given as input to the server path given
        FileUploader(filePathIn, serverPathIn);

        //sends the created variables to the pushStatement method to finish statement creation and upload
        pushStatement(verb, activity, result);
    }



    //basic pushStatement -- verb, activity
    public void pushStatement(Verb verbIn, Activity activityIn)
    {
        lrs = GameObject.Find("xAPIManager").GetComponent<SettingsxAPIManager>().lrs; //LRS at which to upload statement

        //set variables based upon inputs
        Verb verb = verbIn;
        Activity activity = activityIn;

        //sets time stamp to current time on machine
        var timestamp = new DateTime();
        timestamp = DateTime.Now;

        //creates new statement and assignes created variables to statement instance
        var statement = new Statement();
        statement.actor = GameObject.Find("xAPIManager").GetComponent<SettingsxAPIManager>().actor;
        statement.verb = verb;
        statement.timestamp = timestamp;
        statement.target = activity;

        //upload created statement to lrs
        StatementLRSResponse lrsResponse = lrs.SaveStatement(statement);

        if (lrsResponse.success)
        {
            // if upload is successful, print success message with statement's id
            print("Save statement: " + lrsResponse.content.id);
            print("Verb: " + lrsResponse.content.verb.id + ", objectType :" + lrsResponse.content.target.ObjectType + ", Actor : " + lrsResponse.content.actor.name + ", Email: " + lrsResponse.content.actor.mbox);
        }
        else
        {
            // if upload not successful, print failure message.  Note: failure is almost always due to incorrect variable formatting (ex: having actor.mbox = "test@test.com" instead of "mailto:test@test.com")
            print("didn't get lrsResponse.");
        }
    }

    //pushStatement -- verb, activity, result
    public void pushStatement(Verb verbIn, Activity activityIn, Result resultIn)
    {
        lrs = GameObject.Find("xAPIManager").GetComponent<SettingsxAPIManager>().lrs; //LRS at which to upload statement

        //set variables based upon inputs
        Verb verb = verbIn;
        Activity activity = activityIn;
        Result result = resultIn;

        //sets time stamp to current time on machine
        var timestamp = new DateTime();
        timestamp = DateTime.Now;

        //creates new statement and assignes created variables to statement instance
        var statement = new Statement();
        statement.actor = GameObject.Find("xAPIManager").GetComponent<SettingsxAPIManager>().actor;
        statement.verb = verb;
        statement.result = result;
        statement.timestamp = timestamp;
        statement.target = activity;

        //upload created statement to lrs
        StatementLRSResponse lrsResponse = lrs.SaveStatement(statement);

        if (lrsResponse.success)
        {
            // if upload is successful, print success message with statement's id
            print("Save statement: " + lrsResponse.content.id);
            print("Verb: " + lrsResponse.content.verb.id + ", objectType :" + lrsResponse.content.target.ObjectType+", Actor : "+lrsResponse.content.actor.name+", Email: "+lrsResponse.content.actor.mbox);
        }
        else
        {
            // if upload not successful, print failure message.  Note: failure is almost always due to incorrect variable formatting (ex: having actor.mbox = "test@test.com" instead of "mailto:test@test.com")
            print("didn't get lrsResponse.");
        }
    }
}
*/