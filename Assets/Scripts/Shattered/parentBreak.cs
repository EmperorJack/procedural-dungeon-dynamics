using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class parentBreak : MonoBehaviour {

	public GameObject brokenGeo;
	public int collisionThreshold = 2;
	public float volume = 0;

	// Use this for initialization
	void Start () {
		setVolume ();
		if (brokenGeo != null) {
			brokenGeo.SetActive (false);
			setupMaterials();

		}
	}
	
	// Update is called once per frame
	void Update () {
		//brokenGeo.transform.position = gameObject.transform.position;
	}

	void setupMaterials(){
		Material mat = gameObject.GetComponent<Renderer> ().material;
		PhysicMaterial pMat = gameObject.GetComponent<Collider> ().material;
		foreach (Transform child in brokenGeo.transform) {
			if (mat != null) {
				child.GetComponent<Renderer> ().material = mat;

			}
			if (pMat != null){
				if (child.gameObject.GetComponent<PhysicMaterial> () == null) {
					Debug.Log ("Setting piece material");
					child.gameObject.GetComponent<Collider> ().material = pMat;
					//child.gameObject.GetComponent<MeshCollider> ().enabled = false;
					//child.gameObject.GetComponent<MeshCollider> ().enabled = true;

				}
				brokenAttributes childScript = child.gameObject.GetComponent<brokenAttributes> ();
				if (childScript != null) {
					childScript.setVolume ();
					childScript.setRatio (volume);
				}
			}
		}
	}

	void OnCollisionEnter(Collision collision){
		if (collision.relativeVelocity.magnitude >= collisionThreshold) {
			if (brokenGeo != null) {
				//brokenGeo.transform.SetParent (gameObject.transform.parent);
				brokenGeo.SetActive (true);
				brokenGeo.transform.position = gameObject.transform.position;
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
		}
	}

	private void setVolume(){
		Collider collider = GetComponent<Collider> ();
		if (collider != null) {
			Bounds bBox = collider.bounds;
			volume = bBox.size.x * bBox.size.y * bBox.size.z;
		}
	}

}
