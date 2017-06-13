using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForcePush : MonoBehaviour {

	public float force = 1.0f;
	public Vector3 forceVector = new Vector3 (1.0f, 0.0f, 0.0f);

	public void Start() {
		gameObject.GetComponentInChildren<Rigidbody> ().useGravity = false;
	}

	public void Update ()
	{
		if (Time.time > 5.0f) gameObject.GetComponentInChildren<Rigidbody> ().useGravity = true;

		if (Input.GetKey (KeyCode.F)) {
			gameObject.GetComponentInChildren<Rigidbody> ().AddForce (forceVector * force);
		}
	}

}
