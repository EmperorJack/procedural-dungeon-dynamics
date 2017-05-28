using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Visualization;
using Primitives;
using Utilities;

namespace CrowdSim
{
	public class SimManager
	{
		SharedGrid sharedGrid;
		public float cellWidth;
		public int dim;
		Helper<Cell> helper;

		List<SimObject> simObjects;
		GameObject simObjectsParent;

		public SimManager (float cellWidth, int dim, GameObject simObjectsParent)
		{
			this.cellWidth = cellWidth;
			this.dim = dim;
			sharedGrid = new SharedGrid(cellWidth,dim);	
			helper = new Helper<Cell>(sharedGrid.grid, cellWidth);
			this.simObjectsParent = simObjectsParent;
			simObjects = new List<SimObject> ();
		}

		public int[] selectCell(Vector2 pos){

			Cell cell = helper.getCell (pos);
			int[] index = helper.getCellIndex(pos);
			if (cell != null) {
				if (cell.isGoal) {
					cell.isGoal = false;
				} else {
					cell.isGoal = true;
				}
			} 
			return index;
		}

		public Cell[,] getGrid(){
			return sharedGrid.grid;
		}

		public void addAgent(Vector2 pos){
			GameObject dummyAgent = createDummyAgent (pos);
			SimObject simObject = new SimObject (pos, new Vector2 (0, 0), dummyAgent);
			simObjects.Add (simObject);
		}

		private GameObject createDummyAgent(Vector2 pos){
			GameObject dummy = GameObject.CreatePrimitive (PrimitiveType.Cylinder);

			Collider c = dummy.GetComponent<Collider> ();
			c.enabled = false;

			Transform t = dummy.transform;
			t.parent = simObjectsParent.transform;
			t.position = new Vector3(pos.x,0,pos.y);
			t.localScale = new Vector3 (0.2f, 0.01f, 0.2f);

			Material mat = new Material (Shader.Find ("Diffuse"));
			Renderer rend = dummy.GetComponent<Renderer> ();
			mat.color = Color.white;
			rend.material = mat;

			return dummy;
		}

	}
}

