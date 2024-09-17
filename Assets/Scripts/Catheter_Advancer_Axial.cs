using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Catheter_Advancer_Axial : MonoBehaviour
{
    public static Catheter_Advancer_Axial ME;
    private void Awake()
    {
        if (ME != null)
            Destroy(ME);
        ME = this;
    }
    static readonly int testValues = 12;
    Vector3[] positions = new Vector3[testValues + 1];
    //Vector3[] positions2 = new Vector3[testValues + 1];
    GameObject[] testSpheres = new GameObject[testValues + 1];
    GameObject[] testSegments = new GameObject[testValues];
    [SerializeField]
    GameObject catheterTip = null, catheterHub = null, catheter = null, catheterBasePosition = null, targetVein = null, needleTip = null, catheterShaft = null;
    float catheterDistance = 0;
    Vector3 lastCatheterHubPosition = Vector3.zero, lastCatheterHubForward = Vector3.zero;
    float catheterDefaultDistance = 0;
    float minimumCatheterDistance = 0;
    float catheterHubTipDistance = 0;
    float entryAngleMinimumThresholdFromVertical = 1;//degrees
    // Use this for initialization
    bool scoring = false;
    public void StartScoring(GameObject TargetVein)
    {
        targetVein = TargetVein;
        catheterHub.transform.forward = catheterTip.transform.position - catheterHub.transform.position;
        //Debug.Log("START");
        for (int x = 0; x <= testValues; x++)
        {
            testSpheres[x] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            testSpheres[x].transform.position = catheterHub.transform.position + (x * 1f) / testValues * (catheterTip.transform.position - catheterHub.transform.position);
            testSpheres[x].transform.parent = catheter.transform;
            positions[x] = testSpheres[x].transform.position;
            if (x > 0)
            {
                testSegments[x - 1] = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                testSegments[x - 1].transform.position = (positions[x] + positions[x - 1]) / 2;
                testSegments[x - 1].transform.up = positions[x] - positions[x - 1];
                testSegments[x - 1].transform.localScale = new Vector3(1, Vector3.Distance(positions[x], positions[x - 1]) / 2, 1);
                testSegments[x - 1].transform.parent = catheter.transform;
            }
        }
        catheterDefaultDistance = Vector3.Distance(positions[0], positions[1]);
        //Debug.Log(catheterDefaultDistance);
        lastCatheterHubForward = catheterHub.transform.forward;
        lastCatheterHubPosition = catheterHub.transform.position;
        minimumCatheterDistance = Vector3.Distance(catheterHub.transform.position, catheterBasePosition.transform.position);
        catheterHubTipDistance = Vector3.Distance(catheterTip.transform.position, catheterHub.transform.position);
        segments = targetVein.GetComponent<DynamicVessel>().Segments;
        centerLineLocal = new Vector3[segments.Length];
        for (int x = 0; x < segments.Length; x++)
        {
            centerLineLocal[x] = segments[x].CenterPoint;
            //Debug.Log("CP: " + segments[x].CenterPoint);
        }
        centerLine = new Vector3[segments.Length];
        ///TESTING
        //Update();
        scoring = true;
        catheterShaft.SetActive(false);
        //Debug.Log("STARTING SCORING");///TESTING REMOVE
    }
    private void Update()
    {
        if (!scoring)
            return;
        //Debug.Log("SCORING?");///TESTING REMOVE
        UpdateCatheter();
    }
    /*int frame = -1;
    // Update is called once per frame
    void Update()
    {
        //Debug.Log(frame);
        if (frame > 250)
            return;
        if (frame % 10 == 0)
        {
            catheter.transform.position = catheter.transform.position + catheter.transform.TransformDirection(Vector3.back).normalized;
            UpdateCatheter();
        }
        frame++;
    }
    */
    Vector3[] centerLineLocal = { };
    Vector3[] centerLine = { };
    DynamicVessel.VesselSegment[] segments = { };
    void UpdateCatheter()
    {
        //bool inVein = false;
        Vector3 direction = catheterTip.transform.position - catheterHub.transform.position;
        RaycastHit[] hits = Physics.RaycastAll(catheterHub.transform.position, direction, catheterHubTipDistance);
        RaycastHit veinIn = new RaycastHit();
        foreach(RaycastHit hit in hits)
        {
            if(hit.transform.GetInstanceID()==targetVein.transform.GetInstanceID())
            {
                veinIn = hit;
                //inVein = true;
                break;
            }
        }
        if(Vector3.Angle(-direction,veinIn.normal)<entryAngleMinimumThresholdFromVertical)
        {
            //Debug.Log("TOO PERPENDICULAR");
            return;
        }
        segments = targetVein.GetComponent<DynamicVessel>().Segments;
        centerLineLocal = new Vector3[segments.Length];
        for (int x = 0; x < segments.Length; x++)
        {
            centerLineLocal[x] = segments[x].CenterPoint;
           // Debug.Log("CP: " + segments[x].CenterPoint);
        }
        centerLine = new Vector3[segments.Length];
        catheterDistance = Vector3.Distance(catheterBasePosition.transform.position, catheterHub.transform.position)-minimumCatheterDistance;
        if (catheterDistance < 0)
            catheterDistance = 0;
        for(int x = 0;x<centerLine.Length;x++)
        {
            centerLine[x] = centerLineLocal[x] + targetVein.transform.position;
            //Debug.Log("CL: " + centerLine[x]);
        }
        int indexOfClosestCenter = 0;
        float closestCenter = float.MaxValue;
        float distance = 0;
        for(int x = 0;x<centerLine.Length;x++)
        {
            distance = Vector3.Distance(needleTip.transform.position, centerLine[x]);
            if (distance<closestCenter)
            {
                closestCenter = distance;
                indexOfClosestCenter = x;
            }
        }
        int indexOfNext = 0;
        if (indexOfClosestCenter == 0)
            indexOfNext = 1;
        else if (indexOfClosestCenter == centerLine.Length - 1)
            indexOfNext = centerLine.Length - 2;
        else
        {
            direction = centerLine[indexOfClosestCenter + 1] - centerLine[indexOfClosestCenter];
            if (Vector3.Dot(direction, needleTip.transform.position - centerLine[indexOfClosestCenter]) >= 0)
                indexOfNext = indexOfClosestCenter + 1;
            else
                indexOfNext = indexOfClosestCenter - 1;
        }
        Vector3 direction2 =  centerLine[indexOfNext] - centerLine[indexOfClosestCenter];
        direction = catheterTip.transform.position - catheterHub.transform.position;
        //bool flipped = false;
        if(Vector3.Dot(direction2,direction)<0)
        {
            int t = indexOfNext;
            indexOfNext = indexOfClosestCenter;
            indexOfClosestCenter = t;
            direction2 = -direction2;
            //flipped = true;
        }
        distance = Mathf.Abs(((needleTip.transform.position - centerLine[indexOfClosestCenter]) - Vector3.Project(needleTip.transform.position - centerLine[indexOfClosestCenter], direction2)).magnitude);
        float theta = 90 - Vector3.Angle(veinIn.normal, -direction);
        float distance2 = Mathf.Abs(distance / Mathf.Atan(Mathf.Rad2Deg * theta));
        float distance3 = Mathf.Sqrt(distance * distance + distance2 * distance2);
        Vector3 point = centerLine[indexOfClosestCenter] + direction2.normalized * ((Vector3.Project(needleTip.transform.position - centerLine[indexOfClosestCenter], direction2)).magnitude + distance2);
        int indexP = indexOfNext - indexOfClosestCenter;
        //Debug.Log(distance + " " + distance2 + " " + distance3);
        distance = Vector3.Project(needleTip.transform.position - centerLine[indexOfClosestCenter], direction2).magnitude + distance2;
        bool overshotNext = false;
        int ignoreIndex = 0;
        if(Vector3.Distance(centerLine[indexOfClosestCenter],centerLine[indexOfClosestCenter+indexP])<distance)
        { 
            overshotNext = true;
            ignoreIndex = 2;
        }
        Vector3[] points;
        //Debug.Log(theta + " " /*+ flipped + " "*/ + indexOfClosestCenter + " " + indexOfNext);

        
        if (indexP > 0)
            points = new Vector3[3 + centerLine.Length - indexOfNext - ignoreIndex];
        else
            points = new Vector3[3 + indexOfClosestCenter - ignoreIndex];
        float[] distances = new float[points.Length];
        points[0] = catheterHub.transform.position;
        points[1] = needleTip.transform.position;
        if (!overshotNext)
        {
            points[2] = point;
            //points[3] = centerLine[indexOfNext];
            for (int x = 3; x < points.Length; x++)
            {
                points[x] = centerLine[indexOfNext + (x - 3) * indexP];
            }
        }
        else
        {
            for(int x = 2; x < points.Length; x++)
            {
                points[x] = centerLine[(indexOfNext + indexP) + (x - 2) * indexP];
            }
        }
        distances[0] = 0;
        for(int x = 1;x<distances.Length;x++)
        {
            distances[x] = Vector3.Distance(points[x], points[x - 1]);
        }
        /*for(int x = 0;x<points.Length;x++)
        {
            Debug.Log(points[x]);
            GameObject GO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GO.transform.localScale = Vector3.one * 0.5f;
            GO.transform.position = points[x];
        }*/
        float cumulativeDistance = distances[0];
        float nextCumulativeDistance = distances[1];
        distance = 0;
        int distanceIndex = 0;
        bool maxAttained = false;
        positions[0] = points[0];
        for (int x = 1; x <= testValues; x++)
        {
            distance = x * catheterDefaultDistance;
            if (!maxAttained)
            {
                while (distance > nextCumulativeDistance)
                {
                    distanceIndex++;
                    if (distanceIndex >= distances.Length - 2)
                    {
                        cumulativeDistance = nextCumulativeDistance;
                        maxAttained = true;
                        break;
                    }
                    cumulativeDistance = nextCumulativeDistance;
                    nextCumulativeDistance += distances[distanceIndex + 1];
                }
            }
            //Debug.Log(x + " " + distance + " " + distanceIndex + " " + cumulativeDistance + " " + nextCumulativeDistance);
            positions[x] = (points[distanceIndex + 1] - points[distanceIndex]).normalized * (distance - cumulativeDistance) + points[distanceIndex];
            testSegments[x - 1].transform.position = (positions[x] + positions[x - 1]) / 2;
            testSegments[x - 1].transform.up = positions[x] - positions[x - 1];
            testSegments[x - 1].transform.localScale = new Vector3(1, Vector3.Distance(positions[x], positions[x - 1]) / 2, 1);
            testSpheres[x].transform.position = positions[x];
        }
    }
    public void EndScoringMaintainPosition()
    {
        scoring = false;
        //RemoveElements();
    }
    public void EndScoringResetCatheter()
    {
        //scoring = false;
        RemoveElements();
    }
    void RemoveElements()
    {
        for(int x = 0;x<testSegments.Length;x++)
        {
            Destroy(testSegments[x]);
        }
        for(int x = 0;x<testSpheres.Length;x++)
        {
            Destroy(testSpheres[x]);
        }
        catheterShaft.SetActive(true);
    }
}
