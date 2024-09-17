/*using UnityEngine;
using System.Collections;
using TinCan;
using TinCan.LRSResponses;
using System.IO;
using Renci.SshNet;

public class SettingsxAPIManager : MonoBehaviour {


    public RemoteLRS lrs;  //holds lrs variables
    public Agent actor;     //holds user variables
    public SftpClient client;  //holds sftp variables

    [Header("--------------- Actor Settings -----------------------------------------------")]
    public string actorName;    //the user's name
    public string actorEmail;   //the user's email address

    [Header("--------------- LRS Settings -----------------------------------------------")]
    public string lrsEndpoint;  //endpoint to use for xAPI statements
    public string lrsAuthName;  //name for lrs.  Not Username, the authorization id
    public string lrsAuthPassword;  //password for lrs.  Not lrs account password, the authorization password associated with the authorization id used

    [Header("--------------- SFTP Settings -----------------------------------------------")]
    public string sftpHost;     //IP of file host
    public string sftpUsername;     //username for host authorization
    public string sftpPassword;     //password associated with host authorization username

    [Header("--------------- File/Server Path Settings -----------------------------------------------")]
    public string filePath;  //the filepath at which data is written
    public string serverPath;   //the server path at which the data is stored

    

    // Use this for initialization
    void Start()
    {        
        
        lrs = new RemoteLRS();

        actor = new Agent();
        // default placeholders for sftpclient
        client = new SftpClient("host", 22, "UserName", "UserPassword");
    }

    //grabs the newest versions of these variables
    void Update()
    {
        actor.name = actorName;
        actor.mbox = "mailto:" + actorEmail;

        lrs = lrs = new RemoteLRS(lrsEndpoint, lrsAuthName, lrsAuthPassword);
        client = new SftpClient(sftpHost, 22, sftpUsername, sftpPassword);
    }

    //Sets up the LRS with the given inputs
    public void setLRS(string lrsEndpointIn, string authorizationNameIn, string authorizationPasswordIn)
    {
        lrsEndpoint = lrsEndpointIn;
        lrsAuthName = authorizationNameIn;
        lrsAuthPassword = authorizationPasswordIn;
        lrs = new RemoteLRS(lrsEndpoint, lrsAuthName, lrsAuthPassword);
    }

    //Sets the user to the one defined by the inputs.  Note: it is the responsibilty of the activity provider to manage users (such as user passwords)
    public void setActor(string actorNameIn, string actorEmailIn)
    {
        actorName = actorNameIn;
        actorEmail = actorEmailIn;

        actor.name = actorName;
        actor.mbox = "mailto:" + actorEmail;
    }

    //Sets up the sftp connection with the given variables
    public void setSFTP(string sftpHostIn,string sftpNameIn, string sftpPassIn)
    {
        sftpHost = sftpHostIn;
        sftpUsername = sftpNameIn;
        sftpPassword = sftpPassIn;

        client = new SftpClient(sftpHost, 22, sftpUsername, sftpPassword);
    }

    //Set the file path for local files
    public void setFilePath(string filePathIn)
    {
        filePath = filePathIn;
    }

    //Sets the file path for online files (on the given server)
    public void setServerPath(string serverPathIn)
    {
        serverPath = serverPathIn;
    }
}
*/