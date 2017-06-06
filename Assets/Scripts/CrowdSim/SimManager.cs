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
		public GroupGrid groupGrid;

		public float cellWidth;
		public int dim;
		Helper<Cell> helper;

		List<SimObject> simObjects;
		GameObject simObjectsParent;

		public SimManager (float cellWidth, int dim, GameObject simObjectsParent, DungeonGeneration.Cell[,] dungeon, int gridRatio)
		{
			this.cellWidth = cellWidth;
			this.dim = dim;

			sharedGrid = new SharedGrid (cellWidth, dim, dungeon,gridRatio);	
			
			this.simObjectsParent = simObjectsParent;
			simObjects = new List<SimObject> ();

			groupGrid = new GroupGrid (cellWidth, dim, sharedGrid, dungeon,gridRatio);
			helper = new Helper<Cell>(groupGrid.grid, cellWidth);
		}

		public void update(){

			if (simObjects.Count > 0) {
				//Debug.Log (simObjects [0].sceneObject.GetComponent<Rigidbody> ().velocity);
			}
			//sharedGrid.assignAgents (simObjects);
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

		public int[] getCell(Vector2 pos){
			Cell cell = helper.getCell (pos);
			int[] index = helper.getCellIndex(pos);

			return index;
		}

		public Cell[,] getGrid(){
			return groupGrid.grid;
		}

		public int addAgent(Vector2 pos, GameObject sceneObject){
			SimObject simObject = null;
			if (sceneObject == null) {
				sceneObject = createDummyAgent (pos);
				simObject = new SimObject (pos, new Vector2 (0, 0), sceneObject);
			} else {
				Debug.Log ("Adding slime agent at :" + pos.x + ", " + pos.y);
				sceneObject = GameObject.Instantiate (sceneObject);
				initGameObject (pos,sceneObject);

				simObject = new SimObject (pos, new Vector2 (0, 0), sceneObject);
			}
			simObjects.Add (simObject);
			groupGrid.simObjects.Add (simObject);
			sharedGrid.addAgent (simObject);

			return simObjects.Count;
		}

		private void initGameObject(Vector2 pos, GameObject customObject){
			Transform t = customObject.transform;
			t.parent = simObjectsParent.transform;
			t.localScale = new Vector3 (0.5f, 0.5f, 0.5f);
			t.position = new Vector3(pos.x,0,pos.y);
		}

		private GameObject createDummyAgent(Vector2 pos){
			GameObject dummy = GameObject.CreatePrimitive (PrimitiveType.Cylinder);

			Collider c = dummy.GetComponent<Collider> ();
			c.enabled = true;

			initGameObject (pos, dummy);

			Material mat = new Material (Shader.Find ("Diffuse"));
			Renderer rend = dummy.GetComponent<Renderer> ();
			mat.color = Color.white;
			rend.material = mat;

			return dummy;
		}

		public float getMax(){
			return groupGrid.getMax();
		}

		public void trigger(){
			if (groupGrid.trigger) {
				groupGrid.trigger = false;
			} else {
				groupGrid.trigger = true;
			}
			Debug.Log ("Trigger: " + groupGrid.trigger);
		}

	}
}

