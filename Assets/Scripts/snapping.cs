using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class snapping : MonoBehaviour
{
    public GameObject ideal1;
    public GameObject ideal2;

    public GameObject current1;
    public GameObject current2;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            snap();
        }

        Debug.Log("Ideal: " + ideal1.transform.position);
        Debug.Log("Curr: " + current1.transform.position);
        //Debug.Log("Ideal: " + ideal1.transform.position);

    }

    void snap()
    {
        //ideal1.transform.position = current1.transform.position;
        current1.transform.position = ideal1.transform.position;
        current1.transform.rotation = ideal1.transform.rotation;
    }
}
