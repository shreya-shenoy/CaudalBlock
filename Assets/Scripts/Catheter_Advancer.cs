using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Catheter_Advancer : MonoBehaviour {

    static readonly int testValues = 12;
    Vector3[] positions = new Vector3[testValues + 1];
    Vector3[] positions2 = new Vector3[testValues + 1];
    GameObject[] testSpheres = new GameObject[testValues + 1];
    GameObject[] testSegments = new GameObject[testValues];


    [SerializeField]
    GameObject catheterTip = null, catheterHub = null, catheter = null, targetVein = null;
    float catheterAdvancement = 0;
    Vector3 lastCatheterHubPosition = Vector3.zero, lastCatheterHubForward = Vector3.zero;
    float catheterDefaultDistance = 0;
    float assumedDiameter = 1;
	// Use this for initialization
	void Start () {
        catheterHub.transform.forward = catheterTip.transform.position - catheterHub.transform.position;
        //Debug.Log("START");
		for(int x = 0;x<=testValues;x++)
        {
            testSpheres[x] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            testSpheres[x].transform.position = catheterHub.transform.position + (x * 1f) / testValues * (catheterTip.transform.position - catheterHub.transform.position);
            //testSpheres[x].transform.parent = catheter.transform;
            positions[x] = testSpheres[x].transform.position;
            if (x > 0)
            {
                testSegments[x - 1] = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                testSegments[x - 1].transform.position = (positions[x] + positions[x - 1]) / 2;
                testSegments[x - 1].transform.up = positions[x] - positions[x - 1];
                testSegments[x - 1].transform.localScale = new Vector3(1, Vector3.Distance(positions[x], positions[x - 1]) / 2, 1);
            }
        }
        catheterDefaultDistance = Vector3.Distance(positions[0], positions[1]);
        //Debug.Log(catheterDefaultDistance);
        lastCatheterHubForward = catheterHub.transform.forward;
        lastCatheterHubPosition = catheterHub.transform.position;
	}
    int frame = 0;
	// Update is called once per frame
	void Update ()
    {
        //Debug.Log(frame);
        if (frame > 250)
            return;
        if (frame % 10 == 0)
        {
            catheter.transform.position = catheter.transform.position + catheter.transform.TransformDirection(Vector3.back).normalized;
            if (Vector3.Dot(lastCatheterHubForward, catheterHub.transform.position - lastCatheterHubPosition) <= 0)
            {
                catheterAdvancement = 0;
                //Debug.Log("NOT MET");
            }
            else
            {
                //Debug.Log("MET");
                catheterAdvancement = Vector3.Magnitude(Vector3.Project(catheterHub.transform.position - lastCatheterHubPosition, lastCatheterHubForward));
                AdvanceCatheter();
            }
            lastCatheterHubForward = catheterHub.transform.forward;
            lastCatheterHubPosition = catheterHub.transform.position;
        }
        frame++;
	}
    float stiffness = 0.85f;/// 0-1
    void AdvanceCatheter()
    {
        Vector3 direction = Vector3.zero;
        for(int x = 0; x<=testValues;x++)
        {
            if (x == 0)
            {
                positions2[x] = positions[x] + lastCatheterHubForward.normalized * catheterAdvancement;
            }
            else
            {
                direction = positions[x] - (positions2[x - 1] + (positions[x - 1] - positions2[x - 1]) * stiffness);
                positions2[x] = positions2[x - 1] + direction.normalized * catheterDefaultDistance;
            }
        }
        RaycastHit[] hits = new RaycastHit[0];
        float distance = 0;
        Vector3 cross = Vector3.zero;
        Vector3 axis = Vector3.zero;
        Vector3 point = Vector3.zero;
        Vector3 leg1 = Vector3.zero;
        float normalLength = 0;
        float axialLength = 0;
        //Debug.Log("HERE");
        for(int x = 1;x<=testValues;x++)
        {
            direction = positions2[x-1] - positions2[x];
            distance = Vector3.Distance(positions2[x - 1], positions2[x]);
            hits = Physics.RaycastAll(positions2[x], direction, distance);
            foreach(RaycastHit hit in hits)
            {
                //Debug.Log("A HIT");
                if(hit.transform.GetInstanceID()==targetVein.transform.GetInstanceID())
                {
                    //Debug.Log("HAPPENING");
                    //Debug.Log(x + " " + positions2[x] + " " + positions2[x - 1]);
                    cross = Vector3.Cross(direction, hit.normal);
                    axis = Vector3.Cross(cross, hit.normal);
                    point = hit.point - hit.normal.normalized * assumedDiameter;
                    leg1 = Vector3.Project(point - positions2[x - 1], hit.normal);
                    normalLength = leg1.magnitude;
                    if (Vector3.Dot(hit.normal, leg1) < 0)
                        normalLength *= -1;
                    axialLength = Mathf.Sqrt(catheterDefaultDistance * catheterDefaultDistance - normalLength * normalLength);
                    positions2[x] = positions2[x - 1] + normalLength * hit.normal.normalized + axialLength * axis.normalized;
                    //Debug.Log(hit.normal + " " + direction + " " + cross + " " + axis);
                    //Debug.Log(axialLength + " " + normalLength + " " + positions2[x]);
                    AdvanceCatheter(x + 1);
                    break;
                }
            }
        }
        for(int x = 0;x<=testValues;x++)
        {
            positions[x] = positions2[x];
            testSpheres[x].transform.position = positions[x];
            if (x > 0)
            {
                testSegments[x - 1].transform.position = (positions[x] + positions[x - 1]) / 2;
                testSegments[x - 1].transform.up = positions[x] - positions[x - 1];
                testSegments[x - 1].transform.localScale = new Vector3(1, Vector3.Distance(positions[x], positions[x - 1]) / 2, 1);
            }
        }
    }
    void AdvanceCatheter(int index)
    {
        Vector3 direction = Vector3.zero;
        for (int x = index; x <= testValues; x++)
        {
            direction = positions2[x] - positions2[x - 1];
            positions2[x] = positions2[x - 1] + direction.normalized * catheterDefaultDistance;
        }
    }
}
