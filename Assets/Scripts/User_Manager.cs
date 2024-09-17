using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;

public class User_Manager : MonoBehaviour
{
    public static User_Manager ME;
    private void Awake()
    {
        if (ME != null)
        {
            Destroy(ME);
        }
        ME = this;
    }
    public string Username = "NOT_SET";
    public string Password = "NOT_SET";
    public string PreferredName = "NOT_SET";
    public void SetUsername(string username)
    {
        Username = username;
        Username = Username.ToLower();

        Regex rgx = new Regex("[^a-zA-Z0-9#]");
        Regex rgxFile = new Regex("[^a-zA-Z0-9]");

        Username = rgx.Replace(Username, "");
    }
    public void SetPassword(string password)
    {
        Password = password;
    }
    public void SetPreferredName(string preferredName)
    {
        PreferredName = preferredName;
    }
}