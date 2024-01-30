using UnityEngine;
using System.Collections;

public class Stopwatch {
	
	public float Seconds;
	public bool Running;
	
	
	// Use this for initialization
	public Stopwatch () {
		Reset();
	}
	
	public void Start () {
		Running = true;
	}
	
	public void Stop () {
		Running = false;
	}
	
	public float Update() {
		if (Running)
			Seconds = Seconds + Time.deltaTime;	
		return Seconds;
	}
	
	
	
	// Update is called once per frame
	public void Reset () {
		Seconds = 0;
		Running = false;
	}
}
