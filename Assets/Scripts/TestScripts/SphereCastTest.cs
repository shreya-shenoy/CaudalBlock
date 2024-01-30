using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereCastTest : MonoBehaviour
{
    public GameObject bar;
    void Start()
    {
        bar.transform.position = transform.position;
    }

    void Update()
    {
        //OnDrawGizmos();
    }
    //public static float maxDistance = 10.0f;


    void OnDrawGizmos()
    {
        float maxDistance = 100.0f;
        RaycastHit hit;
        Vector3 temp;

        bool isHit = Physics.BoxCast(transform.position, transform.lossyScale / 2, -transform.up, out hit, transform.rotation, maxDistance);
        if (isHit)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, -transform.up * hit.distance);
            Gizmos.DrawWireCube(transform.position + (-transform.up) * hit.distance, transform.lossyScale);
            Debug.Log("Distance: " + hit.distance);
            temp = transform.position;
            temp.y = transform.position.y - hit.distance -50;
            bar.transform.position = temp;
            //bar.transform.Translate(Vector3.forward*hit.distance);
        }
        else
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, -transform.up * maxDistance);
            bar.transform.position = transform.position;
            //bar.transform.Translate(Vector3.forward * 0);

        }
    }

}

/*
//Attach this script to a GameObject. Make sure it has a Collider component by clicking the Add Component button. Then click Physics>Box Collider to attach a Box Collider component.
//This script creates a BoxCast in front of the GameObject and outputs a message if another Collider is hit with the Collider’s name.
//It also draws where the ray and BoxCast extends to. Just press the Gizmos button to see it in Play Mode.
//Make sure to have another GameObject with a Collider component for the BoxCast to collide with.

public class SphereCastTest : MonoBehaviour
{
    float m_MaxDistance;
    float m_Speed;
    bool m_HitDetect;

    Collider m_Collider;
    RaycastHit m_Hit;

    void Start()
    {
        //Choose the distance the Box can reach to
        m_MaxDistance = 300.0f;
        m_Speed = 20.0f;
        m_Collider = GetComponent<Collider>();
    }

    void Update()
    {
        //Simple movement in x and z axes
        float xAxis = Input.GetAxis("Horizontal") * m_Speed;
        float zAxis = Input.GetAxis("Vertical") * m_Speed;
        transform.Translate(new Vector3(xAxis, 0, zAxis));
    }

    void FixedUpdate()
    {
        //Test to see if there is a hit using a BoxCast
        //Calculate using the center of the GameObject's Collider(could also just use the GameObject's position), half the GameObject's size, the direction, the GameObject's rotation, and the maximum distance as variables.
        //Also fetch the hit data
        m_HitDetect = Physics.BoxCast(m_Collider.bounds.center, transform.localScale, -transform.up, out m_Hit, transform.rotation, m_MaxDistance);
        if (m_HitDetect)
        {
            //Output the name of the Collider your Box hit
            Debug.Log("Hit : " + m_Hit.collider.name);
        }
    }

    //Draw the BoxCast as a gizmo to show where it currently is testing. Click the Gizmos button to see this
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        //Check if there has been a hit yet
        if (m_HitDetect)
        {
            //Draw a Ray up from GameObject toward the hit
            Gizmos.DrawRay(transform.position, -transform.up * m_Hit.distance);
            //Draw a cube that extends to where the hit exists
            Gizmos.DrawWireCube(transform.position + -transform.up * m_Hit.distance, transform.localScale);
        }
        //If there hasn't been a hit yet, draw the ray at the maximum distance
        else
        {
            //Draw a Ray up from GameObject toward the maximum distance
            Gizmos.DrawRay(transform.position, -transform.up * m_MaxDistance);
            //Draw a cube at the maximum distance
            Gizmos.DrawWireCube(transform.position + -transform.up * m_MaxDistance, transform.localScale);
        }
    }
}
*/
/*
public class SphereCastTest : MonoBehaviour
{
    public GameObject fakeUS;
    public GameObject tip;
    Plane m_Plane;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //RaycastHit hit;
        Ray ray = new Ray(transform.position, -transform.up);
        float enter = 0.0f;
        //if (Physics.SphereCast(fakeUS.transform.position, 500, -transform.up, out hit, 100))
        if(m_Plane.Raycast(ray, out enter))
        {
            Debug.Log("hit");
        }
        //public static bool BoxCast(Vector3 center, Vector3 halfExtents, Vector3 direction, Quaternion orientation = Quaternion.identity)
        //BoxCast(pos, new Vector3(1, 2, 3), dir, Quaternion.LookRotation(dir));

        Vector3 directionX = transform.TransformDirection(fakeUS.transform.position - tip.transform.position) * 300;


        if (BoxCast(pos, new Vector3(1, 2, 3), dir, Quaternion.LookRotation(dir)
        {

        }
)
        //Debug.DrawRay();
        Debug.DrawRay(fakeUS.transform.position, (-directionX), Color.green);
        //Vector3 size = (50.0f, 50.0f, 50.0f);
        Gizmos.DrawCube(fakeUS.transform.position, tip.transform.position);
    }
}
*/