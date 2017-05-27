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
		GameObject parent;

		public GridCell (float size, Color color, Vector2 position, GameObject parent)
		{
			this.size = size;
			this.color = color;
			this.position = position;
			this.parent = parent;
			initQuad ();
		}

		public GridCell(float size, GameObject parent){
			this.size = size;
			this.color = Color.black;
			this.position = new Vector2 (position.x, position.y);
			this.parent = parent;
			initQuad ();
		}

		private void initQuad(){
			quad = GameObject.CreatePrimitive(PrimitiveType.Plane);
			Transform t = quad.GetComponent<Transform>();
			t.parent = parent.transform;
			t.position = new Vector3(position.x,0,position.y);
			t.localScale = new Vector3 (size/2, 0, size/2);

			setColor (color);
		}

		public void setColor(Color color){
			Material mat = new Material (Shader.Find ("Specular"));
			Renderer rend = quad.GetComponent<Renderer> ();
			rend.material = mat;
		}

		public void display(){
			quad.SetActive (true);
		}

		public void hide(){
			quad.SetActive (false);
		}

	}
}

