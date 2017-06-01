using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class brokenAttributes : MonoBehaviour {

	public float volumeRatio = 0;
	public float volume = 0;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void setVolume(){
		Collider collider = GetComponent<Collider> ();
		if (collider != null) {
			Bounds bBox = collider.bounds;
			volume = bBox.size.x * bBox.size.y * bBox.size.z;
		}
	}

	public void setRatio(float parentVolume){
		if (volume > 0) {
			volumeRatio = parentVolume / volume;
		}
	}
}
