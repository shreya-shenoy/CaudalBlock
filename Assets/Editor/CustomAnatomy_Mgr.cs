/*using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using SMMARTS_SDK;
using Object = UnityEngine.Object;

[System.Serializable]
public class CustomAnatomy_Mgr : EditorWindow
{
    //Use this if we want to update the status of the window UI dynamically.
    //bool sceneDirty = false;

    static bool isIndexPlateActive, newindexPlateStatus;

    static bool isBasicSceneSetUp;

    static GameObject communicator;

    static UnityEngine.Object indexPlate;
    static UnityEngine.Object newIndexPlate;


    static GameObject atcTrackedObj;


    List<int> instanceIDs = new List<int>();
    static int needleIndex = 0;
    static int newNeedleIndex = 0;

    string warningMessage = "";
    // Add menu named "Preferences" to the CSSALT menu
    [MenuItem("CSSALT/Custom Anatomy Importer %P")]


    static void Init()
    {
        // Get existing open window or if none, make a new one:
        CustomAnatomy_Mgr window = (CustomAnatomy_Mgr)GetWindow(typeof(CustomAnatomy_Mgr));
        ReadPrefsFromScene();
        window.Show();
    }

    private void OnHierarchyChange()
    {
        //Debug.Log("Hello from Project change.");
        ReadPrefsFromScene();
        Repaint();
    }

    private void Awake()
    {
        //Debug.Log("Hello from Awake.");
        ReadPrefsFromScene();
        Repaint();

        //Check updates last. TBS (To be safe :P)
        UpdateChecker.CheckUpdates();
    }

    void OnGUI()
    {
        if (Application.isPlaying)
            return;

        GUILayout.Label("SMARTS Tracking Settings", EditorStyles.boldLabel);

        GUILayout.BeginVertical();

        GUI.enabled = true;

          
            GUILayout.Label("------Select Components------", EditorStyles.centeredGreyMiniLabel);
            newindexPlateStatus = EditorGUILayout.Toggle("Standard Index Plate", newindexPlateStatus);
            isIndexPlateActive = GameObject.Find("IndexPlate");
            if (isIndexPlateActive && !newindexPlateStatus)
            {
                Debug.Log("Destroy Index Plate");
                DestroyAllOfType(typeof(IBasePlate));
            }
            else if (!isIndexPlateActive && newindexPlateStatus)
            {
                Debug.Log("Make Index Plate");
                indexPlate = PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>("IndexPlate"));
                instanceIDs.Add(indexPlate.GetInstanceID());
                communicator.GetComponent<ATC>().BaseObject = (GameObject)indexPlate;
            }
            else { Debug.Log("weird logic issue"); }

            Debug.Log("button: " + newindexPlateStatus + ", indexplate: " + isIndexPlateActive);
     
        
        GUILayout.EndVertical();
    }



    private void DestroyAllOfType(Type type)
    {
        //Yes yes its a nested for loop, but they're always gonna be small.
        // #NoFret

        foreach (Type t in InterfaceHelper.GetTypesInheritedFromInterface(type))
        {
            //Debug.Log("Removing all of type: " + t.FullName);
            foreach (UnityEngine.Object o in FindObjectsOfType(t))
            {
                MonoBehaviour current = (MonoBehaviour)o;
                if (instanceIDs.Contains(current.gameObject.GetInstanceID()))
                {
                    instanceIDs.Remove(current.gameObject.GetInstanceID());
                    DestroyImmediate(current.gameObject);

                }

            }
        }
    }

    private static bool CheckIfAnyExists(Type type)
    {
        foreach (Type t in InterfaceHelper.GetTypesInheritedFromInterface(type))
        {
            //Debug.Log("Scanning all of type: " + t.FullName);
            if (FindObjectOfType(t) != null)
            {
                return true;
            }
        }
        return false;
    }


    private static void ReadPrefsFromScene()
    {
        if (communicator == null)
        {
            if (GameObject.Find("WhiteBoxCommunicator") != null)
            {
                communicator = GameObject.Find("WhiteBoxCommunicator");
                isBasicSceneSetUp = true;
            }
            else
                isBasicSceneSetUp = false;

        }

        isIndexPlateActive = CheckIfAnyExists(typeof(IBasePlate));
        Debug.LogWarning("Set Index Plate to " + isIndexPlateActive);

    }

    private void HandleCustomSelection(Type type, ref Object obj, ref Object newObj, String objTypeName, int atcTrackobjNum)
    {
        newObj = EditorGUILayout.ObjectField(obj, typeof(GameObject), true);
        if (newObj != null)
        {
            if (CheckIfAnyExists(type))
            {
                if (newObj != obj)
                {
                    obj = newObj;
                    if (((GameObject)newObj).GetComponent(type) != null)
                    {
                        warningMessage = "Scene contains " + objTypeName + "!";
                        DestroyAllOfType(type);
                        try
                        {
                            newObj = PrefabUtility.InstantiatePrefab(newObj);

                        }
                        catch (UnityException e)
                        {
                            Debug.Log(e);
                        }
                        if (newObj == null)
                        {
                            atcTrackedObj = (GameObject)obj;
                        }
                        else
                        {
                            instanceIDs.Add(newObj.GetInstanceID());
                            atcTrackedObj = (GameObject)newObj;
                        }

                    }
                    else if (((GameObject)newObj).GetComponent(type) == null)
                    {
                        warningMessage = "Selected Object is not of type " + type.ToString() + "!";
                    }
                }
            }
            else
            {
                if (newObj != obj)
                {
                    obj = newObj;
                    if (((GameObject)newObj).GetComponent(type) == null)
                    {
                        warningMessage = "Selected Object is not of type " + type.ToString() + "!";
                    }
                    else if (((GameObject)newObj).GetComponent(type) != null)
                    {
                        newObj = PrefabUtility.InstantiatePrefab(newObj);
                        instanceIDs.Add(newObj.GetInstanceID());
                        atcTrackedObj = (GameObject)newObj;
                    }
                }


            }
            assignTrackObj(atcTrackobjNum);
        }
        if (!String.IsNullOrEmpty(warningMessage))
            EditorGUILayout.HelpBox(warningMessage, MessageType.Warning);
    }

    private void assignTrackObj(int num)
    {
        switch (num)
        {
            case 1:
                communicator.GetComponent<ATC>().TrackedObject1 = atcTrackedObj;
                break;
            case 2:
                communicator.GetComponent<ATC>().TrackedObject2 = atcTrackedObj;
                break;
            case 3:
                communicator.GetComponent<ATC>().TrackedObject3 = atcTrackedObj;
                break;
            case 4:
                communicator.GetComponent<ATC>().TrackedObject4 = atcTrackedObj;
                break;
            default:
                break;
        }

    }
}*/