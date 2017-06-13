using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WholeObject : BreakableObject {

	public GameObject brokenGeo;
	public float collisionThreshold = 2;
	public AudioClip shatterSound;
	public bool initializeOnStart = true;
	private float soundPitch = 1.0f;
	private bool initialized = false;


	// Use this for initialization
	void Start () {
		if (!initialized && initializeOnStart) {
			initialize ();
		}
		brokenGeo.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
		//brokenGeo.transform.position = gameObject.transform.position;
	}
		
	public void initialize(){
		setVolume ();
		if (brokenGeo != null) {
			setupAudio ();
			setupMaterials();
		}
		if (shatterSound != null && GetComponent<AudioSource> () != null) {
			GetComponent<AudioSource> ().clip = shatterSound;
		}
	}

	void setupMaterials(){
		// Make the broken pieces inherit the material and physical material of whole geometry
		Material mat = gameObject.GetComponent<Renderer> ().sharedMaterial;
		PhysicMaterial pMat = gameObject.GetComponent<Collider> ().material;
		foreach (Transform child in brokenGeo.transform) {
			if (mat != null) {
				child.GetComponent<Renderer> ().material = mat;

			}
			if (pMat != null){
				if (child.gameObject.GetComponent<PhysicMaterial> () == null) {
					//Debug.Log ("Setting piece material");
					child.gameObject.GetComponent<Collider> ().material = pMat;

				}
				BreakableObject childScript = child.gameObject.GetComponent<BrokenObject> ();
				if (childScript == null) {
					childScript = child.gameObject.AddComponent<BrokenObject>() as BrokenObject;
				}
				childScript.setVolume ();
				childScript.setVolumeRatio (volume);

			}
		}
	}


	void OnCollisionEnter(Collision collision){
		
		if (collision.relativeVelocity.magnitude >= collisionThreshold) {
			if (brokenGeo != null) {
				breakGeo ();
				playBreakSound ();
			}
		}
	}

	void breakGeo(){
		brokenGeo.SetActive (true);
		brokenGeo.transform.position = gameObject.transform.position;
		brokenGeo.transform.rotation = gameObject.transform.rotation;
		gameObject.SetActive (false);

		Rigidbody rigidbody = gameObject.GetComponent<Rigidbody> ();
		foreach (Transform child in brokenGeo.transform) {
			Component[] childRigidBodies = child.GetComponentsInChildren<Rigidbody> ();
			foreach (Rigidbody c in childRigidBodies) {
				c.velocity = rigidbody.velocity;
				c.mass = rigidbody.mass / brokenGeo.transform.childCount;
			}
		}
	}

	void setupAudio(){
		soundPitch = Random.Range (0.75f, 1.25f);
		AudioSource brokenSource = brokenGeo.GetComponent<AudioSource> ();
		if (brokenSource == null) {
			brokenSource = brokenGeo.AddComponent<AudioSource> ();
		}
		brokenSource.pitch = soundPitch;
		brokenSource.clip = shatterSound;
	}

	void playBreakSound(){
		if (brokenGeo != null && brokenGeo.GetComponent<AudioSource> () != null) {
			brokenGeo.GetComponent<AudioSource> ().Play ();
		}
	}

}
