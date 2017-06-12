using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class ModelPostProcessor : AssetPostprocessor {


	void OnPostprocessModel (GameObject g) {
		//if (assetPath.EndsWith (".fbx")) {
		string modelName = Path.GetFileNameWithoutExtension (assetPath);
		if (modelName.EndsWith("_broken") || modelName.EndsWith("_whole")){
			physicsSetup (g);
			audioSetup (g);
		}
	}
		

	void physicsSetup(GameObject g){
		Debug.Log ("Setting physics for " + g.name);
		//Mesh objectMesh = g.GetComponent<Mesh> ();
		MeshFilter objectMeshFilter = g.GetComponent<MeshFilter>();
		if (objectMeshFilter != null) {
			// If no rigidbody exists, attach one
			if (g.GetComponent<Rigidbody> () == null) {
				Debug.Log ("\t - Adding Rigidbody");
				g.AddComponent<Rigidbody> ();

			}
			// If no collider exists, make one from the mesh
			if (g.GetComponent<Collider> () == null) {
				Debug.Log ("\t - Adding MeshCollider");
				g.AddComponent<BoxCollider>();
			}
		} else {
			Debug.Log (g.name + " has no mesh object");
		}
		foreach ( Transform child in g.transform){
			physicsSetup (child.gameObject);
		}
	}

	void audioSetup(GameObject g){
		Debug.Log ("Setting audio for " + g.name);
		AudioSource objectAudio = g.GetComponent<AudioSource> ();
		if (objectAudio == null) {
			g.AddComponent<AudioSource> ();
			objectAudio = g.GetComponent<AudioSource> ();
		}
		objectAudio.playOnAwake = false;
	}

}