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
		public SharedGrid sharedGrid;
		public int groupId = -1;
		public GroupGrid groupGrid;
		public List<GroupGrid> groups;

		public float cellWidth;
		public int dim;
		public Helper<Cell> helper;

		List<SimObject> simObjects;
		GameObject simObjectsParent;

		List<Color> groupColors;

		DungeonGeneration.Cell[,] dungeon;
		int gridRatio;

		float avoidance = 0.0f;

		bool pause = true;

		public void reset ()
		{
			foreach (SimObject simObject in simObjects) {
				Object.DestroyImmediate (simObject.sceneObject);
			}

			groups = new List<GroupGrid> ();
			groupId = 0;


		}

		public void togglePause ()
		{
			pause = !pause;
		}

		public SimManager (float cellWidth, int dim, GameObject simObjectsParent, DungeonGeneration.Cell[,] dungeon, int gridRatio, bool defaultSetup)
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
			Debug.Log ("Default: " + defaultSetup);
			if (defaultSetup) {
				defaultInit ();
			} else {
				addGroup ();
			}
			helper = new Helper<Cell> (groups [0].grid, cellWidth, gridRatio);

			sharedGrid.setAvoidance (avoidance);
		}

		public void update (float time)
		{
			if (!pause) {
				resetGrids ();

				if (simObjects.Count > 0) {
					//sharedGrid.assignAgents (simObjects);
					sharedGrid.update (time);

					foreach (GroupGrid group in groups) {
						group.update (time);
					}
				}
			}
		}

		public void removeAgent (GameObject toRemove, int groupId)
		{
			Rigidbody rb = toRemove.GetComponent<Rigidbody> ();
			Vector2 pos = new Vector2 (rb.position.x, rb.position.z);

			foreach (SimObject simObject in groups[groupId].simObjects) {
				if (simObject.getPosition () == pos) {
					groups [groupId].simObjects.Remove (simObject);
					sharedGrid.removeAgent (simObject);
					simObjects.Remove (simObject);

					Object.Destroy (simObject.sceneObject);
					return;
				}
			}
	
		}

		public int[] addGoal (Vector2 pos, bool justAdd, GameObject goalObject)
		{
			Cell cell = helper.getCell (pos);
			int[] index = helper.getCellIndex (pos);

			if (cell.isGoal == false) {

				goalObject.transform.parent = simObjectsParent.transform;
				goalObject.transform.name = "GroupGoal" + groupId;

				Light goalLight = goalObject.GetComponentInChildren<Light> ();
				goalLight.color = groupGrid.color / 2.0f;

				goalObject.transform.position = new Vector3 (cell.position.x, 0.001f, cell.position.y);
				colliderScript goalScript = goalObject.GetComponent<colliderScript> ();
				goalScript.setManager (this);
				goalScript.setGroupId (groupId);
				cell.isGoal = true;
				groupGrid.addGoal (cell);
			}

//			if (cell != null) {
//				if (cell.isGoal && justAdd == false) {
//					cell.isGoal = false;
//				} else {
//					cell.isGoal = true;
//				}
//				groupGrid.addGoal (cell);
//			} 
			return index;
		}

		public int[] getCell (Vector2 pos)
		{
			return helper.getCellIndex (pos);
//			Cell cell = helper.getCell (pos);
//			int[] leftIndex = helper.getLeft (pos);
//			Cell leftCell = helper.accessGridCell (leftIndex);
//			if (leftCell == null) {
//				return new int[]{ -1, -1 };
//			} else {
//				float deltaX = pos.x - leftCell.position.x;
//				float deltaY = pos.y - leftCell.position.y;
//
//				if (deltaX >= cellWidth / 2) {
//					leftIndex [0]++;
//				}
//
//				if (deltaY >= cellWidth / 2) {
//					leftIndex [1]++;
//				}
//				return leftIndex;
//			}

		}

		public void addDungeonObjects (List<GameObject> objects)
		{
			foreach (GameObject gameObject in objects) {
				
				//Debug.Log (gameObject.name);
				Transform parentTransform = gameObject.GetComponent<Transform> ();
				Vector2 parentPos = new Vector2 (parentTransform.position.x, parentTransform.position.z);

				if (gameObject.name.Contains ("table")) {
					addAgent (getObjectPos (gameObject), gameObject, false);
				} else {
					foreach (Transform childT in gameObject.transform) {
						GameObject child = childT.gameObject;
						if (child.name.Contains ("whole")) {
							addAgent (getObjectPos (child), child, false);
						} else {
							//table 
						}
					}
				}
			}
		}

		private Vector2 getObjectPos (GameObject gameObject)
		{
			Transform t = gameObject.GetComponent<Transform> ();
			return new Vector2 (t.position.x, t.position.z);
		}

		public Cell[,] getGrid ()
		{
			return groupGrid.grid;
		}

		public int addAgent (Vector2 pos, GameObject sceneObject, bool moveable)
		{
			SimObject simObject = null;
			int[] index = helper.getLeft (pos);

			if (sceneObject == null) {
				sceneObject = createDummyAgent (pos);
				simObject = new SimObject (pos, new Vector2 (0, 0), sceneObject, moveable);
			} else {

				if (moveable) {
					sceneObject = GameObject.Instantiate (sceneObject);
					if (groupId > 0) {
						Material mat = sceneObject.GetComponent<Renderer> ().material;

						mat.color = groupColors [groupId];
						//rend.material = mat;
					}
					initGameObject (pos, sceneObject);
				}

				simObject = new SimObject (pos, new Vector2 (0, 0), sceneObject, moveable);
			}
			simObjects.Add (simObject);
			if (moveable) {
				groupGrid.addAgent (simObject);
				simObject.groupId = groupId;
			}
			sharedGrid.addAgent (simObject);

			return simObjects.Count;
		}

		public int addAgent(Vector2 pos, GameObject sceneObject, bool moveable, int id){
			setGroup (id);
			return addAgent (pos, sceneObject, moveable);
		}

		private void initGameObject (Vector2 pos, GameObject customObject)
		{
			Transform t = customObject.transform;
			//t.parent = simObjectsParent.transform;
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

		Color[] colors = new Color[]{ Color.green, Color.blue, Color.yellow, Color.red };
		int colorId = -1;

		public void addGroup ()
		{
			colorId++;
			if (colorId >= colors.GetLength (0)) {
				colorId = 0;
			}
			groupId++;

			GameObject groupTransform = new GameObject ();
			groupTransform.transform.parent = simObjectsParent.transform;
			groupTransform.transform.name = "GroupNum" + groupId;

			Color color = colors [colorId];
			GroupGrid newGroup = new GroupGrid (groupTransform,color, cellWidth, dim, sharedGrid, dungeon, gridRatio);
			groupColors.Add (color);
			groups.Add (newGroup);
			sharedGrid.groups.Add (newGroup);
			groupGrid = newGroup;
			groupGrid.groupId = groupId;
			helper = new Helper<Cell> (groups [groupId].grid, cellWidth, gridRatio);

			Debug.Log ("Now editing group: " + groupId);
		}

		public void defaultInit ()
		{

			addGroup ();


			for (int i = 0; i < groupGrid.grid.GetLength (0); i++) {
				groupGrid.grid [0, i].isGoal = true;
			}

			addGroup ();

			for (int i = 0; i < groupGrid.grid.GetLength (0); i++) {
				groupGrid.grid [groupGrid.grid.GetLength (0) - 1, i].isGoal = true;
			}
		}

		public void setGroup(int id){
			if (id < 0 || id >= groups.Count) {
				return;
			}
			helper = new Helper<Cell> (groups [groupId].grid, cellWidth, gridRatio);

			groupGrid = groups [id];
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

		public void setUpdateField (bool updateField)
		{
			foreach (GroupGrid groupGrid in groups) {
				groupGrid.updateField = updateField;
			}
		}

		public void increaseAvoidance(){
			avoidance += 0.2f;
			if (avoidance > 2.0f) {
				avoidance = 1.0f;
			}


			sharedGrid.setAvoidance (avoidance);
			Debug.Log ("Avoidance set: " + avoidance);
		}

		public void decreaseAvoidance(){
			avoidance -= 0.2f;
			if (avoidance < 0f) {
				avoidance = 0f;
			}
				
			sharedGrid.setAvoidance (avoidance);
			Debug.Log ("Avoidance set: " + avoidance);
		}

		public void resetGrids ()
		{
			for (int i = 0; i < dim; i++) {
				for (int j = 0; j < dim; j++) {
					sharedGrid.grid [i, j].reset ();
					foreach (GroupGrid groupGrid in groups) {
						if (groupGrid.updateField) {
							groupGrid.grid [i, j].reset ();
						}
					}
				}
			}
		}
	}
}

