using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class logic : MonoBehaviour {

	public int dim;
	public double cell_width;

	public float left;
	public float top;

	private Cell[,] grid;
	private bool redraw = true;



	// Use this for initialization
	void Start() {
		grid = new Cell[dim, dim];
		float d = 1f;
		print ("Hello World");
		for (int i = 0; i < dim; i++) {
			for (int j = 0; j < dim; j++) {
				grid [i, j] = new Cell(d);
			}
			d -= 0.1f;
		}
	}

	// Update is called once per frame
	void Update () {

	}

	void drawGrid(){
		for (int i = 0; i < dim; i++) {
			for (int j = 0; j < dim; j++) {
				float width = 1;
				float height = 1;

				float density = grid[i,j].getDensity();
				Color d_Color = new Color (density, density, density);

				setColor (d_Color);
				fillRect (i, j, width, height);
				setColor (Color.red);
				drawRect (i, j, width, height);
			}
		}
	}
		
	void OnDrawGizmosSelected(){
		Start ();
			drawGrid ();
	}

	void drawRect(float x, float y, float width, float height){
		Vector2 pos = new Vector2 (x, y);
		drawRect (pos, width, height);

	}

	void fillRect(float x, float y, float width, float height){
		Vector2 pos = new Vector2 (x, y);
		fillRect (pos, width, height);
	}

	void drawRect(Vector2 pos, float width, float height){
		
		Vector3 top_l = new Vector3 (pos.x , 0, pos.y);
		Vector3 top_r = new Vector3 (pos.x + width, 0, pos.y );
		Vector3 bot_r = new Vector3 (pos.x + width, 0, pos.y + height );
		Vector3 bot_l = new Vector3 (pos.x , 0, pos.y + height);

		Gizmos.DrawLine (top_l, top_r);
		Gizmos.DrawLine (top_r, bot_r);
		Gizmos.DrawLine (bot_r, bot_l);
		Gizmos.DrawLine (bot_l, top_l);
	}

	void fillRect(Vector2 pos, float width, float height){
		Vector3 pos_3 = new Vector3 (pos.x + width/2, 0,pos.y + height/2);
		Gizmos.DrawCube (pos_3, new Vector3(width,0.0f,height));
	}

	void setColor(Color color){
		Gizmos.color = color;
	}

	class Cell{

		private float density;

		public Cell(float density){
			this.density = density;
		}

		public float getDensity(){
			return density;
		}

	}
		
}


