using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class parentBreak : MonoBehaviour {

	public GameObject brokenGeo;

	// Use this for initialization
	void Start () {
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
		PhysicMaterial mat = gameObject.GetComponent<Collider> ().material;
		if (mat != null){
			foreach (Transform child in brokenGeo.transform) {
				if (child.gameObject.GetComponent<PhysicMaterial> () == null) {
					Debug.Log ("Setting piece material");
					child.gameObject.GetComponent<Collider> ().material = mat;
					//child.gameObject.GetComponent<MeshCollider> ().enabled = false;
					//child.gameObject.GetComponent<MeshCollider> ().enabled = true;

				}
			}
		}
	}

	void OnCollisionEnter(Collision collision){
		if (brokenGeo != null) {
			//brokenGeo.transform.SetParent (gameObject.transform.parent);
			brokenGeo.SetActive (true);
			brokenGeo.transform.position = gameObject.transform.position;
			gameObject.SetActive (false);

			Rigidbody rigidbody = gameObject.GetComponent<Rigidbody> ();
			foreach (Transform child in brokenGeo.transform){
				Component[] childRigidBodies = child.GetComponentsInChildren<Rigidbody> ();
				foreach (Rigidbody c in childRigidBodies) {
					c.velocity = rigidbody.velocity;
					c.mass = rigidbody.mass / brokenGeo.transform.childCount;

				}

			}



		}
	}

}
