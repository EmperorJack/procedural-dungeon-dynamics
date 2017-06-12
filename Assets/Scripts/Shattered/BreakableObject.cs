using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BreakableObject : MonoBehaviour {

	protected float volume = 1;
	protected float volumeRatio =1;

	public float getVolume(){
		return volume;
	}

	public void setVolume(float vol){
		volume = vol;
	}

	public void setVolume(){
		Collider collider = GetComponent<Collider> ();
		if (collider != null) {
			Bounds bBox = collider.bounds;
			volume = bBox.size.x * bBox.size.y * bBox.size.z;
		} else {
			volume = 1;
		}
	}

	public float getVolumeRatio(){
		return volumeRatio;
	}
		
	public void setVolumeRatio(float parentVolume){
		if (volume > 0) {
			volumeRatio = parentVolume / volume;
		} else {
			volumeRatio = 1;
		}
	}


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
