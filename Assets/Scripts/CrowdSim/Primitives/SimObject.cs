using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Primitives
{
	public class SimObject
	{
		Vector2 position;
		public Vector2 velocity;
		public float densityWeight = 1.0f;

		public GameObject sceneObject;

		private Rigidbody rb;

		public bool moveable = false;

		public SimObject (Vector2 position, Vector2 velocity, GameObject sceneObject, bool moveable)
		{
			this.position = position;
			this.velocity = velocity;
			this.sceneObject = sceneObject;
			this.moveable = moveable;

			if (moveable == false) {
				densityWeight = 1.0f;
			}

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
				
			if (moveable) {
				rb.isKinematic = false;
			}

		}

		public void destroy ()
		{
			Object.Destroy (sceneObject);
		}

		public void toggleKinematic ()
		{
			if (moveable) {
				rb.isKinematic = !rb.isKinematic;
			}
		}

		public void setVelocity (Vector2 vel)
		{
			if (moveable) {
				sceneObject.GetComponent<Rigidbody> ().velocity -= new Vector3 (velocity.x, 0, velocity.y);
				this.velocity = vel;
				sceneObject.GetComponent<Rigidbody> ().velocity += new Vector3 (vel.x, 0, vel.y);
			}
		}

		public void applyVelocity (Vector2 vel)
		{
			if (moveable) {
				sceneObject.GetComponent<Rigidbody> ().velocity = new Vector3 (vel.x, 0, vel.y);
			}

		}

		public Rigidbody getBody(){
			return rb;
		}

		public Vector2 getPosition ()
		{
			return new Vector2 (rb.position.x, rb.position.z);
		}


	}
}

