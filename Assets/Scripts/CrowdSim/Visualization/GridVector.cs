using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Visualization
{
	public class GridVector
	{
		private float magnitude;
		private float direction;
		private Color color;
		private Vector2 position;

		GameObject vector;
		GameObject point;
		GameObject parent;



		public GridVector (float magnitude, float direction, Color color, Vector2 position, GameObject parent)
		{
			this.magnitude = magnitude;
			this.direction = direction;
			this.color = color;
			this.position = position;
			this.parent = parent;
			initPolygon ();
			hide ();
		}

		public void initPolygon(){
			vector = GameObject.CreatePrimitive(PrimitiveType.Cube);
			Collider c = vector.GetComponent<Collider> ();
			c.enabled = false;

			Transform tV = vector.transform;
			tV.parent = parent.transform;
			//tV.position = new Vector3(position.x,0,position.y);
			tV.localScale = new Vector3 (magnitude, 0.01f, 0.1f);

			point = GameObject.CreatePrimitive (PrimitiveType.Cylinder);
			c = vector.GetComponent<Collider> ();
			c.enabled = false;

			Transform tP = vector.transform;
			tP = vector.transform;
			tP.parent = parent.transform;
			//tP.position = new Vector3 (position.x, 0, position.y);

			tV.localRotation = new Quaternion (0f, direction, 0f, 1f);
			tP.localRotation = new Quaternion (0f, direction, 0f, 1f);


		}

		public void display(){
			point.SetActive (true);
			vector.SetActive (true);

		}

		public void hide(){
			point.SetActive (false);
			vector.SetActive (false);
		}

	}
}

