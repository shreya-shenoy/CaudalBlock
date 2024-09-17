/*using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using SMMARTS_SDK;
using Object = UnityEngine.Object;

[System.Serializable]
public class Record_Mgr : EditorWindow
{
    //Use this if we want to update the status of the window UI dynamically.
    //bool sceneDirty = false;


    static bool isReplayerManagerActive, newReplayerManagerStatus;
    static bool isReplayFromFile, newReplayFromFileStatus;

    static bool isBasicSceneSetUp;


    static UnityEngine.Object replayerManager;
    static UnityEngine.Object newReplayerCanvas;

    static int buttonCountIndex = 0;
    static int newButtonCountIndex = 0;

    static bool isUsingLogIn, newLogInStatus;
    static bool isUsingUsername, newUsernameStatus;
    static bool isUsingPreferedName, newPreferedNameStatus;
    static bool isUsingSwitchButton, newSwitchButtonStatus;

    static UnityEngine.Object loginCanvas;
    static UnityEngine.Object newLoginCanvas;
    static UnityEngine.Object usernameInput;
    static UnityEngine.Object newUsernameInput;
    static UnityEngine.Object preferedNameInput;
    static UnityEngine.Object newPreferedNameInput;
    static UnityEngine.Object anatomicalBlockButton;
    static UnityEngine.Object newAnatomicalBlockButton;
    static UnityEngine.Object button1;
    static UnityEngine.Object newButton1;
    static UnityEngine.Object button2;
    static UnityEngine.Object newButton2;
    static UnityEngine.Object button3;
    static UnityEngine.Object newButton3;

    string[] buttonCountOptions = new string[] { "1", "2", "3" };

    List<int> instanceIDs = new List<int>();

    string warningMessage = "";
    // Add menu named "Preferences" to the CSSALT menu
    [MenuItem("CSSALT/Record Preferences %P")]


    static void Init()
    {
        // Get existing open window or if none, make a new one:
        Record_Mgr window = (Record_Mgr)GetWindow(typeof(Record_Mgr));
        window.Show();
    }

    private void OnHierarchyChange()
    {
        //Debug.Log("Hello from Project change.");
        Repaint();
    }

    private void Awake()
    {
        //Debug.Log("Hello from Awake.");
        Repaint();

    }

    void OnGUI()
    {
        if (Application.isPlaying)
            return;

        GUILayout.Label("Record Settings", EditorStyles.boldLabel);

        GUILayout.BeginVertical();

        GUI.enabled = true;
        GUILayout.Label("------Log In Screen Settings------", EditorStyles.centeredGreyMiniLabel);


        newLogInStatus = EditorGUILayout.Toggle("Log-In Screen", isUsingLogIn);

        GUI.enabled = newLogInStatus;

        newUsernameStatus = EditorGUILayout.Toggle("Use Usernames?", isUsingUsername);
        newPreferedNameStatus = EditorGUILayout.Toggle("Use Prefered Names?", isUsingPreferedName);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("# of buttons");
        newButtonCountIndex = EditorGUILayout.Popup(buttonCountIndex, buttonCountOptions, GUILayout.ExpandWidth(true));
        EditorGUILayout.EndHorizontal();


        if (newLogInStatus != isUsingLogIn)
        {
            if (newLogInStatus)
            {
                if (!GameObject.Find("StartupCanvas"))
                {
                    loginCanvas = PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>("StartupCanvas"));
                    instanceIDs.Add(loginCanvas.GetInstanceID());
                }
            }
            else
            {
                DestroyImmediate(loginCanvas);
                newUsernameStatus = false;
                newPreferedNameStatus = false;
                newSwitchButtonStatus = false;
            }
            isUsingLogIn = newLogInStatus;
            isUsingUsername = newUsernameStatus;
            isUsingPreferedName = newPreferedNameStatus;
            isUsingSwitchButton = newSwitchButtonStatus;
            EditorSceneManager.MarkAllScenesDirty();
        }

        if (newUsernameStatus != isUsingUsername)
        {
            if (newUsernameStatus)
            {
                if (!GameObject.Find("UsernameInputField"))
                {
                    usernameInput = PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>("UsernameInputField"));
                    instanceIDs.Add(loginCanvas.GetInstanceID());
                    GameObject.Find("UsernameInputField").transform.SetParent(GameObject.Find("Startup Panel").transform);
                    GameObject.Find("UsernameInputField").transform.localPosition = new Vector3(294.4f, 174.7f, 0);
                }
            }
            else
            {
                DestroyImmediate(usernameInput);
            }
            isUsingUsername = newUsernameStatus;
            EditorSceneManager.MarkAllScenesDirty();
        }

        if (newPreferedNameStatus != isUsingPreferedName)
        {
            if (newPreferedNameStatus)
            {
                if (!GameObject.Find("PreferedNameInputField"))
                {
                    preferedNameInput = PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>("PreferedNameInputField"));
                    GameObject.Find("PreferedNameInputField").transform.SetParent(GameObject.Find("Startup Panel").transform);
                    GameObject.Find("PreferedNameInputField").transform.localPosition = new Vector3(294.4f, 137.6f, 0);
                    GameObject.Find("PreferedNameInputField").transform.localScale = new Vector3(1f, 1f, 1f);
                    instanceIDs.Add(preferedNameInput.GetInstanceID());
                }
            }
            else
            {
                DestroyImmediate(preferedNameInput);
            }
            isUsingPreferedName = newPreferedNameStatus;
            EditorSceneManager.MarkAllScenesDirty();
        }

        if (newButtonCountIndex != buttonCountIndex)
        {
            EditorSceneManager.MarkAllScenesDirty();
            switch (newButtonCountIndex)
            {
                case 0:
                    {
                        if (GameObject.Find("LogInButton2"))
                        {
                            DestroyImmediate(GameObject.Find("LogInButton2"));
                        }
                        if (GameObject.Find("LogInButton3"))
                        {
                            DestroyImmediate(GameObject.Find("LogInButton3"));
                        }
                        break;
                    }
                case 1:
                    {
                        if (!GameObject.Find("LogInButton2"))
                        {
                            button2 = PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>("LogInButton2"));
                            instanceIDs.Add(button2.GetInstanceID());
                            GameObject.Find("LogInButton2").transform.SetParent(GameObject.Find("Startup Panel").transform);
                            GameObject.Find("LogInButton2").transform.localPosition = new Vector3(277.77f, -133.9f, 0);
                            GameObject.Find("LogInButton2").transform.localScale = new Vector3(1f, 1f, 1f);
                        }
                        if (GameObject.Find("LogInButton3"))
                        {
                            DestroyImmediate(GameObject.Find("LogInButton3"));
                        }
                        break;
                    }
                case 2:
                    {
                        if (!GameObject.Find("LogInButton2"))
                        {
                            button2 = PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>("LogInButton2"));
                            instanceIDs.Add(button2.GetInstanceID());
                            GameObject.Find("LogInButton2").transform.SetParent(GameObject.Find("Startup Panel").transform);
                            GameObject.Find("LogInButton2").transform.localPosition = new Vector3(277.77f, -133.9f, 0);
                            GameObject.Find("LogInButton2").transform.localScale = new Vector3(1f, 1f, 1f);
                        }

                        if (!GameObject.Find("LogInButton3"))
                        {
                            button3 = PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>("LogInButton3"));
                            instanceIDs.Add(button3.GetInstanceID());
                            GameObject.Find("LogInButton3").transform.SetParent(GameObject.Find("Startup Panel").transform);
                            GameObject.Find("LogInButton3").transform.localPosition = new Vector3(277.77f, -78.9f, 0);
                            GameObject.Find("LogInButton3").transform.localScale = new Vector3(1f, 1f, 1f);
                        }
                        break;
                    }
            }
            buttonCountIndex = newButtonCountIndex;
        }
        GUI.enabled = true;


        GUILayout.Label("------File Settings------", EditorStyles.centeredGreyMiniLabel);

        newReplayerManagerStatus = EditorGUILayout.Toggle("Replayer Manager", isReplayerManagerActive);
        if (newReplayerManagerStatus != isReplayerManagerActive)
        {
            if (newReplayerManagerStatus)
            {
                if (!GameObject.Find("ReplayerManager"))
                {
                    replayerManager = PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>("ReplayerManager"));
                }
                isBasicSceneSetUp = true;
            }
            else
            {
                DestroyImmediate(GameObject.Find("ReplayerManager"));
                isBasicSceneSetUp = false;
            }
            isReplayerManagerActive = newReplayerManagerStatus;
            EditorSceneManager.MarkAllScenesDirty();
        }
        GUI.enabled = !isBasicSceneSetUp;

        if (isBasicSceneSetUp)
        {
            GUI.enabled = true;

            newReplayFromFileStatus = EditorGUILayout.Toggle("Replay from File", isReplayFromFile);
            if (newReplayFromFileStatus != isReplayFromFile)
            {
                if (newReplayFromFileStatus)
                {
                    //Replayer.ME.saveRecordToFile = true;
                }
                else
                {
                    //Replayer.ME.saveRecordToFile = false;
                }
                isReplayFromFile = newReplayFromFileStatus;
                EditorSceneManager.MarkAllScenesDirty();
            }
        }

        GUILayout.EndVertical();
    }

}*/