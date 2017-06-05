using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Primitives
{
	public class SimObject
	{
		Vector2 position;
		public Vector2 velocity;

		public GameObject sceneObject;

		private Rigidbody rb;

		public SimObject(Vector2 position, Vector2 velocity, GameObject sceneObject){
			this.position = position;
			this.velocity = velocity;
			this.sceneObject = sceneObject;
			if (sceneObject.GetComponent<Rigidbody> () == null) {
				rb = sceneObject.AddComponent<Rigidbody> ();
				rb.position = new Vector3 (position.x, 0.2f, position.y);
				rb.useGravity = false;

			} else {
				rb = sceneObject.GetComponent<Rigidbody> ();
			}

			if (sceneObject.GetComponent<Collider> () == null) {
				BoxCollider bC = sceneObject.AddComponent<BoxCollider> ();
			}
		}
			

		public void applyVelocity(Vector2 vel){
			//Debug.Log (vel.x + " " + vel.y);
			sceneObject.GetComponent<Rigidbody>().velocity = new Vector3(vel.x, 0, vel.y);
			//sceneObject.transform.position = new Vector3 (position.x + velocity.x, 0, position.y + velocity.y);
		}

		public Vector2 getPosition(){
			return new Vector2 (rb.position.x, rb.position.z);
		}
	}
}

