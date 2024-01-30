/*using UnityEngine;
using System.Collections;
using System.IO;
using System;
using TinCan;
using TinCan.LRSResponses;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;


public class DownloadxAPIManager : MonoBehaviour {

    RemoteLRS lrs;
    public float dataf;  
    public DateTime qSince;
    public StatementsQuery query;

    [Header("--------------- Query Settings -----------------------------------------------")]
    public int qLimit;
    public string qVerb;
    public string qActivity;
    public string qAgentName;
    public string qAgentEmail;

    [Header("--------------- Read Text File Settings -----------------------------------------------")]
    public string readTextFileLocation;
    public string readTextVerb;
    public string readTextActivity;

    // Use this for initialization
    void Start () {
        //Unity doesn't trust certificates by default, this mutes the error that will be returned
        System.Net.ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => { return true; };

        lrs = new RemoteLRS();
        query = new StatementsQuery();

        qLimit = 1;
        qVerb = "placeholder";
        qActivity = "placeholder";
        qAgentName = "placeholder";
        qAgentEmail = "placeholder";

        qSince = DateTime.Now;
    }
	
	// Update is called once per frame
	void Update () {

    }


    //General Query - - allows developer to create general simple query for statements
    public void generalQuery(DateTime qSince, int qLimitIn, string qAgentIn, string qEmailIn, string qVerbIn, string qActivityIn)
    {
        lrs = GameObject.Find("xAPIManager").GetComponent<SettingsxAPIManager>().lrs;
        query = new StatementsQuery();
        Agent actor = new Agent();

        qLimit = qLimitIn;
        qVerb = qVerbIn;
        qActivity = qActivityIn;
        qAgentName = qAgentIn;
        qAgentEmail = qEmailIn;

        actor.name = qAgentName;
        actor.mbox = "mailto:"+qAgentEmail;
        
        query.since = qSince;
        query.agent = actor;
        query.limit = qLimit;
        query.verbId = new Uri(qVerb);
        query.activityId = new Uri(qActivity).ToString();

        Statement[] statementArray = new Statement[qLimit];

        StatementsResultLRSResponse lrsResponse = lrs.QueryStatements(query);

        print("begining loop");
        
        for (int i = 0; i < qLimit; i++)
        {
            print("loop " + i);
            statementArray[i] = lrsResponse.content.statements[i];
            
        }
        if (lrsResponse.success)
        {
            for (int i = 0; i < qLimit; i++)
            {
                print("Statement[" + i + "]: " + statementArray[i]);
                print(statementArray[i].verb.id.ToString());
                print(statementArray[i].timestamp.ToString());
                print(statementArray[i].actor.mbox.ToString());
            }
        }
        else
        {
            print("error: statements could not be recieved");
        }
    }

    public void getReadTextData(string rAgentName, string rAgentEmail, string readTextVerbIn, string readTextActivityIn, string readTextFileLocation)
    {
        lrs = GameObject.Find("xAPIManager").GetComponent<SettingsxAPIManager>().lrs;

        var query = new StatementsQuery();
        Agent actor = new Agent();

        actor.name = rAgentName;
        actor.mbox = "mailto:" + rAgentEmail;

        readTextVerb = readTextVerbIn;
        readTextActivity = readTextActivityIn;

        query.agent = actor;

        query.verbId = new Uri(readTextVerb);
        query.activityId = new Uri(readTextActivity).ToString();

        var statement = new Statement();

        StatementsResultLRSResponse lrsResponse = lrs.QueryStatements(query);

        if (lrsResponse.success)
        {
            statement = lrsResponse.content.statements[0];
            print(statement.verb.id.ToString());
            print(statement.timestamp.ToString());
            print(statement.actor.mbox.ToString());
            print(statement.target);

            StartCoroutine(readOnlineTxt(readTextFileLocation));

        }
        else
        {
            print("error: statements could not be recieved");
        }
    }

    //Downloads a file from a serer to the computer using sftp.  "serverFile" is the path on the server to the desired file, include the file name.  
    //"destination" is the file path where the file will be downloaded to, include file name.
    public void FileDownloader(string serverFile, string destination)
    {
        FileInfo file = new FileInfo(serverFile);

        string downloadfile = file.FullName; //grabs the name of the file


        var client = GameObject.Find("xAPIManager").GetComponent<SettingsxAPIManager>().client;

        client.Connect();
        if (client.IsConnected)
        {
            print("Client is connected.");
        }
        else
        {
            print("Failed to connect to client.");
        }


        print("Downloading: "+ file);
        print("To location: " + destination);
        using (Stream fileStream = File.OpenWrite(destination))
        {
            client.DownloadFile(file.ToString(), fileStream);
        }
    }

    private IEnumerator readOnlineTxt(string readTextFileLocation)
    {
        // WWW w = new WWW("http://vam-dev.anest.ufl.edu/xapi_demo_data/playback.txt"); This for used for testing. Use it as reference for your readTextFileLocation 
        WWW w = new WWW(readTextFileLocation);
        yield return w;
        if (w.error != null)
        {
            print("Error .. " + w.error);
            // for example, often 'Error .. 404 Not Found'
        }
        else
        {
            print("Found ... ==>" + w.text + "<==");
            // don't forget to look in the 'bottom section'
            // of Unity console to see the full text of
            // multiline console messages.
        }


    }
        
    
    //This is a sample of how to use xAPI statements to check verb/activity id and read in data.
    //In actual use, you would use generalQuery to grab a statement, download the relevant data using fileDownloader and the Activity Provider would
    //be responsible for parsing the downloaded file
    public void getLastSaveData(DateTime saveTime, string verbIdIn, string activityIdIn)
    {
        lrs = GameObject.Find("xAPIManager").GetComponent<SettingsxAPIManager>().lrs;

        var query = new StatementsQuery();

        query.verbId = new Uri(verbIdIn);
        query.activityId = new Uri(activityIdIn).ToString();
        var statement = new Statement();

        StatementsResultLRSResponse lrsResponse = lrs.QueryStatements(query);

        if (lrsResponse.success)
        {
            statement = lrsResponse.content.statements[0];
            print(statement.verb.id.ToString());
            print(statement.timestamp.ToString());
            print(statement.actor.mbox.ToString());
            string data = File.ReadAllText(verbIdIn);
            float result;
            float.TryParse(data, out result);
            dataf = result;
            print(dataf);
        }
        else
        {
            print("error: statements could not be recieved");
        }
    }
}
*/