using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdScript : MonoBehaviour {

	public GameObject agent;
	private Rigidbody rb;

	// Use this for initialization
	void Start () {
		rb = agent.GetComponent<Rigidbody> ();
	}
	
	// Update is called once per frame
	void Update () {
		rb.AddForce (new Vector3 (1, 0, 0));
	}
}
