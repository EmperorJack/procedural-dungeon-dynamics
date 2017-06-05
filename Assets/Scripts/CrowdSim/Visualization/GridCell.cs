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
		GridVector vector;
		GameObject parent;

		public GridCell (float size, Color color, Vector2 position, GameObject parent)
		{
			this.size = size;
			this.color = color;
			this.position = position;
			this.parent = parent;
			initQuad ();

			//this.vector = new GridVector (0.8f, 0f, Color.black, this.position, quad);

			hide ();
		}

		public GridCell(float size, GameObject parent){
			this.size = size;
			this.color = Color.black;
			this.position = new Vector2 (position.x, position.y);
			this.parent = parent;
			initQuad ();
			hide ();
		}

		private void initQuad(){

			quad = GameObject.CreatePrimitive(PrimitiveType.Cube);
			Collider c = quad.GetComponent<Collider> ();
			c.enabled = false;

			Transform t = quad.transform;
			t.parent = parent.transform;
			t.position = new Vector3(position.x,0,position.y);
			t.localScale = new Vector3 (size, 0.01f, size);
			setColor (color);


		}

		public void setColor(Color color){
			this.color = color;
			Material mat = new Material (Shader.Find ("Diffuse"));
			Renderer rend = quad.GetComponent<Renderer> ();
			mat.color = color;
			rend.material = mat;
		}

		public Color getColor(){
			return color;
		}

		public void display(){
			
			quad.SetActive (true);
		}

		public void hide(){
			quad.SetActive (false);
		
		}

	}
}

