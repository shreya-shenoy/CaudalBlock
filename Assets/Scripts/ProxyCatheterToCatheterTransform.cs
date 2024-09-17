using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProxyCatheterToCatheterTransform : MonoBehaviour
{
    public static ProxyCatheterToCatheterTransform ME;
    private void Awake()
    {
        if (ME != null)
            Destroy(ME);
        ME = this;
    }

    [SerializeField]
    GameObject proxyCatheterSensor = null, catheter = null, needle = null, needleTip = null, anatomy = null,
        needleHub = null, catheterBase = null;
    Vector3 needleBasePosition, catheterBasePosition, catheterSensorPosition, axialDirection, catheterDirection;
    float maxTrackedCatheterAdvancementAxialDistance = 42;//mm
    float catheterAxialDistanceFromBase = 0;
    float needleBaseToCatheterBaseDistance = 0;
    float distanceNormalToAxis = 0;
    float maxDistanceFromAxis = 20;//mm
    bool disableTrackingTransform = false;
    private void Start()
    {
        UpdateData();
        needleBaseToCatheterBaseDistance = Vector3.Distance(needleBasePosition, catheterBasePosition);
    }
    private void Update()
    {
        if (disableTrackingTransform)
            return;
        UpdateData();
        //Debug.Log(Vector3.Dot(axialDirection, catheterDirection));
        if(Vector3.Dot(axialDirection, catheterDirection)>=0)
        {
            catheterAxialDistanceFromBase = Vector3.Project(catheterDirection, axialDirection).magnitude;
            if (catheterAxialDistanceFromBase < needleBaseToCatheterBaseDistance)
                catheterAxialDistanceFromBase = 0;
            else
                catheterAxialDistanceFromBase -= needleBaseToCatheterBaseDistance;
            if(catheterAxialDistanceFromBase < maxTrackedCatheterAdvancementAxialDistance)
            {
                distanceNormalToAxis = Vector3.Cross(axialDirection, -catheterDirection).magnitude / axialDirection.magnitude;
                if(distanceNormalToAxis < maxDistanceFromAxis)
                {
                    catheter.transform.position = catheterBasePosition + axialDirection.normalized * catheterAxialDistanceFromBase;
                    catheter.transform.rotation = catheterBase.transform.rotation;
                    catheter.transform.parent = needle.transform;
                    return;
                }
            }
        }
        catheter.transform.parent = null;
    }
    public void DisableTrackingTransform()
    {
        disableTrackingTransform = true;
        catheter.transform.parent = anatomy.transform;
    }
    public void EnableTrackingTransform()
    {
        disableTrackingTransform = false;
    }
    void UpdateData()
    {
        needleBasePosition = needleHub.transform.position;
        catheterBasePosition = catheterBase.transform.position;
        catheterSensorPosition = proxyCatheterSensor.transform.position;
        axialDirection = needleTip.transform.position - needleBasePosition;
        catheterDirection = catheterSensorPosition - needleBasePosition;
    }
}