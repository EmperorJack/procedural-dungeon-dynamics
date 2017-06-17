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

		bool revive = false;

		public void reset ()
		{
			foreach (SimObject simObject in simObjects) {
				Object.DestroyImmediate (simObject.sceneObject);
			}

			groups = new List<GroupGrid> ();
			groupId = 0;


		}

		public string togglePause ()
		{
			pause = !pause;
			if (pause) {
				return "Paused";
			} else {
				return "Running";
			}
		}

		public string toggleRevive()
		{
			revive = !revive;
			if (revive) {
				return "True";
			} else {
				return "False";
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
			if (defaultSetup) {
				defaultInit ();
			} else {
				//addGroup ();
			}
			helper = new Helper<Cell> (sharedGrid.grid, cellWidth, gridRatio);

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

		public void swapAgent(SimObject toSwap, int groupId){
			// change color
			//
			if (groups.Count <= 1) {
				return;
			}
			int randomGroup = groupId;
			GameObject sceneObject = toSwap.sceneObject;
			while (randomGroup == groupId) {
				randomGroup = Random.Range (0, groups.Count);
			}

			Material mat = sceneObject.GetComponent<Renderer> ().material;

			mat.color = groups[randomGroup].color;

			groups [groupId].simObjects.Remove (toSwap);
			groups [randomGroup].simObjects.Add (toSwap);
		}

		public void removeAgent (GameObject toRemove, int groupId)
		{
			Rigidbody rb = toRemove.GetComponent<Rigidbody> ();
			Vector2 pos = new Vector2 (rb.position.x, rb.position.z);

			foreach (SimObject simObject in groups[groupId].simObjects) {
				if (simObject.getPosition () == pos) {
					if (revive) {
						if (groups.Count <= 1) {
							return;
						}
						swapAgent (simObject, groupId);
					} else {
						sharedGrid.removeAgent (simObject);
						simObjects.Remove (simObject);

						Object.Destroy (simObject.sceneObject);
					}

					groups [groupId].simObjects.Remove (simObject);

					return;

				}
			}
	
		}

		public bool addGoal (Vector2 pos, GameObject goalObject)
		{
			if (groups.Count > 0) {
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
					return true;
				}
			}
			return false;
		}

		public int[] getCell (Vector2 pos)
		{
			return helper.getCellIndex (pos);
		}

		public void addDungeonObjects (List<GameObject> objects)
		{
			foreach (GameObject gameObject in objects) {
				addDungeonObject(gameObject);
			}
		}

		void addDungeonObject(GameObject gameObject){
			if (gameObject.transform.name.Contains ("_whole")) {
				addAgent (getObjectPos (gameObject), gameObject, false);
			} else {
				foreach (Transform childT in gameObject.transform) {
					if (childT.name.Contains ("_broken") == false) {
						GameObject child = childT.gameObject;
						addDungeonObject (child);

					} else if (childT.name.Contains ("chair") || childT.name.Contains("table") || childT.name.Contains("bench")) {
						// for non breakable objects
						addAgent (getObjectPos (childT.gameObject), childT.gameObject, false);
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

		public int getGroupId(){
			return groupId;
		}

		public bool getPaused(){
			return pause;
		}
			
		public int addAgent (Vector2 pos, GameObject sceneObject, bool moveable)
		{
			if (groups.Count <= 0 && moveable) {
				return 0;
			}

			SimObject simObject = null;
			int[] index = helper.getLeft (pos);

			if (helper.accessGridCell (index) == null || helper.accessGridCell (index).exists == false) {
				return simObjects.Count;
			}

			if (sceneObject == null) {
				sceneObject = createDummyAgent (pos);
				simObject = new SimObject (pos, new Vector2 (0, 0), sceneObject, moveable);
			} else {

				if (moveable) {
					sceneObject = GameObject.Instantiate (sceneObject);
					if (groupId > 0) {
						Material mat = sceneObject.GetComponent<Renderer> ().material;

						mat.color = groupGrid.color;
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


		public string increaseLaneFormation(){
			return sharedGrid.increasePathAvoidance ();
		}

		public string decreaseLaneFormation(){
			return sharedGrid.decreasePathAvoidance ();
		}

		public string increaseDensityExp(){
			return sharedGrid.increaseDensityExp ();
		}

		public string decreaseDensityExp(){
			return sharedGrid.decreaseDensityExp ();
			
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
			if (groupGrid != null) {
				return groupGrid.getMax ();
			}
			return 0.0f;
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
		}

		Color[] colors = new Color[]{ Color.green, Color.blue, Color.yellow, Color.red };
		int colorId = -1;

		public Color addGroup ()
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


			return colors [groupId];
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

		}

		public Color swapGroup ()
		{
			groupId++;

			if (groupId >= groups.Count) {
				groupId = 0;
				groupGrid = groups [0];
			} else {
				groupGrid = groups [groupId];
			}
			helper = new Helper<Cell> (groups [groupId].grid, cellWidth, gridRatio);

			return groupGrid.color;
		}

		public void setUpdateField (bool updateField)
		{
			foreach (GroupGrid groupGrid in groups) {
				groupGrid.updateField = updateField;
			}
		}

		public string increaseAvoidance(){
			avoidance += 0.2f;
			if (avoidance > 2.0f) {
				avoidance = 1.0f;
			}


			sharedGrid.setAvoidance (avoidance);
			return avoidance.ToString ("#.##");
		}

		public string decreaseAvoidance(){
			avoidance -= 0.2f;
			if (avoidance < 0f) {
				avoidance = 0f;
			}
				
			sharedGrid.setAvoidance (avoidance);
			return avoidance.ToString ("#.##");
		}

		public void setDistanceWeight(float distanceWeight){
			sharedGrid.setDistanceWeight (distanceWeight);
		}

		public void setTimeWeight(float timeWeight){
			sharedGrid.setTimeWeight (timeWeight);
		}

		public void setMaxDensity(float max){
			sharedGrid.setMaxDensity (max);
		}

		public void setMinDensity(float min){
			sharedGrid.setMinDensity (min);
		}

		public void setPathAvoidance(float pA){
			sharedGrid.setPathAvoidance (pA);
		}

		public void setDensityExp(float densityExp){
			sharedGrid.setDensityExponent (densityExp);
		}

		public void setObjectAvoidance(float oA){
			sharedGrid.setObjectAvoidance (oA);
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

