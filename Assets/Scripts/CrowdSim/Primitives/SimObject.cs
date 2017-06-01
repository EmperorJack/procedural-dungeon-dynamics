using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Primitives
{
	public class SimObject
	{
		public Vector2 position;
		public Vector2 velocity;

		GameObject sceneObject;

		public SimObject(Vector2 position, Vector2 velocity, GameObject sceneObject){
			this.position = position;
			this.velocity = velocity;
			this.sceneObject = sceneObject;
		}

		public void updatePosition(){
			//Debug.Log (position.x + " " + position.y);
			sceneObject.transform.position = new Vector3(position.x, sceneObject.transform.position.y, position.y);
		}
	}
}

