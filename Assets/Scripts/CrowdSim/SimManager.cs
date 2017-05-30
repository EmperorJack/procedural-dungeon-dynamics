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
		GroupGrid groupGrid;

		public float cellWidth;
		public int dim;
		Helper<Cell> helper;

		List<SimObject> simObjects;
		GameObject simObjectsParent;

		public SimManager (float cellWidth, int dim, GameObject simObjectsParent, DungeonGeneration.Cell[,] dungeon)
		{
			this.cellWidth = cellWidth;
			this.dim = dim;

			sharedGrid = new SharedGrid (cellWidth, dim, dungeon);	
			
			this.simObjectsParent = simObjectsParent;
			simObjects = new List<SimObject> ();

			groupGrid = new GroupGrid (cellWidth, dim, sharedGrid, dungeon);
			helper = new Helper<Cell>(groupGrid.grid, cellWidth);
		}

		public void update(){
			sharedGrid.update ();
			groupGrid.update ();
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
			return groupGrid.grid;
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

		public float getMax(){
			return groupGrid.getMax();
		}

	}
}

