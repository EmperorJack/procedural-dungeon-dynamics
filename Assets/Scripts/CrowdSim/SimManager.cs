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
		private int groupId =-1;
		public GroupGrid groupGrid;
		public List<GroupGrid> groups;

		public float cellWidth;
		public int dim;
		Helper<Cell> helper;

		List<SimObject> simObjects;
		GameObject simObjectsParent;

		List<Color> groupColors;

		DungeonGeneration.Cell[,] dungeon;
		int gridRatio;

		public void reset ()
		{
			foreach (SimObject simObject in simObjects) {
				Object.Destroy (simObject.sceneObject);
			}
		}

		public SimManager (float cellWidth, int dim, GameObject simObjectsParent, DungeonGeneration.Cell[,] dungeon, int gridRatio)
		{
			this.cellWidth = cellWidth;
			this.dim = dim;
			this.dungeon = dungeon;
			this.gridRatio = gridRatio;

			sharedGrid = new SharedGrid (cellWidth, dim, dungeon, gridRatio);	
			
			this.simObjectsParent = simObjectsParent;
			simObjects = new List<SimObject> ();

			groupColors = new List<Color> ();
			groups = new List<GroupGrid> ();

			//groupGrid = new GroupGrid (cellWidth, dim, sharedGrid, dungeon,gridRatio);
			addGroup ();

			helper = new Helper<Cell> (groups [0].grid, cellWidth, gridRatio);
		}

		public void update ()
		{

			if (simObjects.Count > 0) {
				//Debug.Log (simObjects [0].sceneObject.GetComponent<Rigidbody> ().velocity);
			}
			//sharedGrid.assignAgents (simObjects);
			sharedGrid.update ();

			foreach (GroupGrid group in groups) {
				group.update ();
			}

		}

		public int[] selectCell (Vector2 pos)
		{
			Cell cell = helper.getCell (pos);
			int[] index = helper.getCellIndex (pos);

			if (cell != null) {
				if (cell.isGoal) {
					cell.isGoal = false;
				} else {
					cell.isGoal = true;
				}
			} 
			return index;
		}

		public int[] getCell (Vector2 pos)
		{
			Cell cell = helper.getCell (pos);
			int[] index = helper.getCellIndex (pos);

			return index;
		}

		public Cell[,] getGrid ()
		{
			return groupGrid.grid;
		}

		public int addAgent (Vector2 pos, GameObject sceneObject)
		{
			SimObject simObject = null;
			int[] index = helper.getLeft (pos);

			if (sceneObject == null) {
				sceneObject = createDummyAgent (pos);
				simObject = new SimObject (pos, new Vector2 (0, 0), sceneObject);
			} else {
				Debug.Log ("Adding slime agent at :" + pos.x + ", " + pos.y);
				Debug.Log ("Grid index: [" + index [0] + ", " + index [1] + "]");
				sceneObject = GameObject.Instantiate (sceneObject);
				if (groupId > 0) {
					Material mat = new Material (Shader.Find ("Diffuse"));
					Renderer rend = sceneObject.GetComponent<Renderer> ();
					mat.color =  groupColors [groupId];
					rend.material = mat;
				}
				initGameObject (pos, sceneObject);

				simObject = new SimObject (pos, new Vector2 (0, 0), sceneObject);
			}
			simObjects.Add (simObject);
			groupGrid.simObjects.Add (simObject);
			sharedGrid.addAgent (simObject);

			return simObjects.Count;
		}

		private void initGameObject (Vector2 pos, GameObject customObject)
		{
			Transform t = customObject.transform;
			t.parent = simObjectsParent.transform;
			t.localScale = new Vector3 (0.5f, 0.5f, 0.5f);
			t.position = new Vector3 (pos.x, 0, pos.y);
		}

		private GameObject createDummyAgent (Vector2 pos)
		{
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

		public float getMax ()
		{
			return groupGrid.getMax ();
		}

		public float getMaxDensity ()
		{
			return groupGrid.maxCalcDensity;
		}

		public void trigger ()
		{
			if (groupGrid.trigger) {
				groupGrid.trigger = false;
			} else {
				groupGrid.trigger = true;
			}
			Debug.Log ("Trigger: " + groupGrid.trigger);
		}

		Color[] colors = new Color[]{ Color.white, Color.blue, Color.green, Color.yellow, Color.red };
		int colorId = 0;

		public void addGroup ()
		{
			colorId++;
			if (colorId >= colors.GetLength (0)) {
				colorId = 0;
			}

			Color color = colors [colorId];
			GroupGrid newGroup = new GroupGrid (cellWidth, dim, sharedGrid, dungeon, gridRatio);
			groupColors.Add (color);
			groups.Add (newGroup);
			groupGrid = newGroup;
			groupId++;
			helper = new Helper<Cell> (groups [groupId].grid, cellWidth, gridRatio);

			Debug.Log ("Now editing group: " + groupId);
		}

		public void swapGroup ()
		{
			groupId++;
			if (groupId >= groups.Count) {
				groupId = 0;
				groupGrid = groups [0];
			} else {
				groupGrid = groups [groupId];
			}
			helper = new Helper<Cell> (groups [groupId].grid, cellWidth, gridRatio);
			Debug.Log ("Now editing group: " + groupId);
		}

	}
}

