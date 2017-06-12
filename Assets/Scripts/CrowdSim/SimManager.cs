﻿using System.Collections;
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
		public int groupId =-1;
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


		public void reset ()
		{
			foreach (SimObject simObject in simObjects) {
				Object.Destroy (simObject.sceneObject);
			}

			groups = new List<GroupGrid>();
			groupId = 0;


		}

		public void togglePause(){
			foreach (GroupGrid group in groups) {
				group.paused = !group.paused;
				foreach (SimObject simObject in group.simObjects) {
					simObject.applyVelocity (Vector2.zero);
					simObject.toggleKinematic ();
				}
			}
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
			Debug.Log("Default: "+defaultSetup);
			if (defaultSetup) {
				defaultInit ();
			} else {
				addGroup ();
			}
			helper = new Helper<Cell> (groups [0].grid, cellWidth, gridRatio);
		}

		public void update (float time)
		{

			if (simObjects.Count > 0) {
				//sharedGrid.assignAgents (simObjects);
				sharedGrid.update (time);

				foreach (GroupGrid group in groups) {
					group.update (time);
				}
			}

		}

		public int[] selectCell (Vector2 pos, bool justAdd)
		{
			Cell cell = helper.getCell (pos);
			int[] index = helper.getCellIndex (pos);

			if (cell != null) {
				if (cell.isGoal && justAdd == false) {
					cell.isGoal = false;
				} else {
					cell.isGoal = true;
				}
			} 
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

		public void addDungeonObjects(List<GameObject> objects){
			foreach(GameObject gameObject in objects){
				
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

		private Vector2 getObjectPos(GameObject gameObject){
			Transform t = gameObject.GetComponent<Transform> ();
			Debug.Log (t.position.x + " " + t.position.z);
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
				//Debug.Log ("Adding slime agent at :" + pos.x + ", " + pos.y);
				//Debug.Log ("Grid index: [" + index [0] + ", " + index [1] + "]");
				if (moveable) {
					sceneObject = GameObject.Instantiate (sceneObject);
					if (groupId > 0) {
						Material mat = new Material (Shader.Find ("Diffuse"));
						Renderer rend = sceneObject.GetComponent<Renderer> ();
						mat.color = groupColors [groupId];
						rend.material = mat;
					}
					initGameObject (pos, sceneObject);
				}

				simObject = new SimObject (pos, new Vector2 (0, 0), sceneObject,moveable);
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

		public void defaultInit(){

			addGroup ();


			for (int i = 0; i < groupGrid.grid.GetLength (0); i++) {
				groupGrid.grid [0, i].isGoal = true;
			}

			addGroup ();

			for (int i = 0; i < groupGrid.grid.GetLength (0); i++) {
				groupGrid.grid [groupGrid.grid.GetLength(0)-1, i].isGoal = true;
			}
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

		public void setUpdateField(bool updateField){
			foreach (GroupGrid groupGrid in groups) {
				groupGrid.updateField = updateField;
			}
		}
	}
}

