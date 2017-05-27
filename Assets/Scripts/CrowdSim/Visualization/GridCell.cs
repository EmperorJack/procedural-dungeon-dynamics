using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Visualization
{
	public class GridCell
	{
		private float size;
		private Color color;
		private Vector2 position;

		GameObject quad;

		public GridCell (float size, Color color, Vector2 position)
		{
			this.size = size;
			this.color = color;
			this.position = position;
		}

		public GridCell(float size){
			this.size = size;
			this.color = Color.black;
			this.position = new Vector2 (0, 0);
		}

		private void initQuad(){
			quad = GameObject.CreatePrimitive(PrimitiveType.Plane);
			Rigidbody rb = quad.GetComponent<Rigidbody>();
			rb.position = new Vector3(position.x, 0, position.y);

			setColor (color);
		}

		public void setColor(Color color){
			Material mat = quad.GetComponent<Material> ();
			mat.color = color;
		}

		public void display(){
			quad.SetActive (true);
		}

		public void hide(){
			quad.SetActive (false);
		}

	}
}

