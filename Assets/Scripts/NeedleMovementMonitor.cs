using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeedleMovementMonitor : MonoBehaviour {

    [SerializeField]
    IVCatheter IVCatheter;

    [SerializeField]
    Transform NeedleTransform;

    [Range(0, 1.0f)]
    public float NeedleMovementIndex;

    [Range(0, 0.5f)]
    public float NeedleMovementSensitivity;

    [Range(0, 0.2f)]
    public float NeedleMovementDampening;

    Vector3 LastNeedlePosition;

    [SerializeField]
    GameObject MovementDisc;



    // Use this for initialization
    void Start () {
		


	}
	
	// Update is called once per frame
	void Update () {

        if (IVCatheter.OnNeedle & !IVCatheter.CatheterFullySeated)
        {

            float dN = Vector3.Distance(NeedleTransform.position, LastNeedlePosition) * NeedleMovementSensitivity;
            NeedleMovementIndex = NeedleMovementIndex + dN - (NeedleMovementIndex * NeedleMovementDampening);
            NeedleMovementIndex = Mathf.Clamp01(NeedleMovementIndex);
            LastNeedlePosition = NeedleTransform.position;

            MovementDisc.SetActive(true);
            MovementDisc.transform.localScale = new Vector3(NeedleMovementIndex, NeedleMovementIndex, NeedleMovementIndex);

        } else
        {
            MovementDisc.SetActive(false);
            NeedleMovementIndex = 0;

        }
    }
}
