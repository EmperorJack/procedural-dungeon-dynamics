using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Visualization;
using DungeonGeneration;

using UnityEngine.UI;

namespace CrowdSim
{
	public class SimAccess : MonoBehaviour
	{
		public SimManager simManager;
		// class which runs the simulation
		GridGraphics[] groupGraphics;
		private Vector2 pos = new Vector2 (0, 0);
		private bool visilbe = false;

		GameObject gridParent;
		GameObject crowdSim;
		public GameObject simObject;
		public GameObject goalPrefab;
		GameObject simObjects;

		// Grid fields
		public float cellWidth = 1;
		// width of cells before scaling, 1 is a good value
		public int dim = 30;
		public float gridRatio = 1;
		// how much to expand each dungeon cell by
		public int frameLimit = 10;
		// how many frames to wait between updating sim

		public bool visualizeGrids = false;
		// when false, grids will not be visualized

		public bool defaultSetup = false;
		// default setup for a plane grid, for testing
		 
		Primitives.Cell leftSelected;

		string action = "select";
		private int displayField = 0;
		private bool updateFields = true;
		int frames = 1;


		DungeonGeneration.Cell[,] dungeon;

		public GameObject avoidanceText;
		public GameObject laneText;
		public GameObject densityText;

		public GameObject groupText;
		public GameObject actionText;
		public GameObject pausedText;
			

		public void reset ()
		{
			if (simManager != null) {
				simManager.reset ();
			}
			DestroyImmediate (GameObject.Find ("Sim Objects"));
			DestroyImmediate (GameObject.Find ("Graphics Grid"));
			DestroyImmediate (GameObject.Find ("Components"));

		}
			
		public void setAvoidance(float avoidance){
			if (simManager == null || simManager.sharedGrid == null) {
				return;
			}

			simManager.sharedGrid.setAvoidance (avoidance);

			avoidanceText.GetComponent<Text>().text = avoidance.ToString ("#.##");
		}

		public void setTimeWeight(float timeWeight){
			if (simManager == null || simManager.sharedGrid == null) {
				return;
			}

			simManager.sharedGrid.setTimeWeight (timeWeight);		}

		public void setDistanceWeight(float distanceWeight){
			if (simManager == null || simManager.sharedGrid == null) {
				return;
			}

			simManager.sharedGrid.setDistanceWeight (distanceWeight);		}

		public void setMaxDensity(float maxDensity){
			if (simManager == null || simManager.sharedGrid == null) {
				return;
			}

			simManager.sharedGrid.setMaxDensity (maxDensity);		}

		public void setMinDensity(float minDensity){
			if (simManager == null || simManager.sharedGrid == null) {
				return;
			}

			simManager.sharedGrid.setMinDensity (minDensity);		}

		public void setMaxVelocity(float maxVelocity){
			if (simManager == null || simManager.sharedGrid == null) {
				return;
			}

			simManager.sharedGrid.setMaxVelocity (maxVelocity);		}

		void Start ()
		{

			Collider c = GetComponent<Collider> ();
			c.transform.position = pos;
			c.transform.localScale = new Vector3 (cellWidth * dim * gridRatio, 0, cellWidth * dim * gridRatio);
			c.transform.position = c.transform.position + new Vector3 ((cellWidth * gridRatio * (dim - 1)) / 2, 0, (cellWidth * gridRatio * (dim - 1)) / 2);

			groupGraphics = new GridGraphics[10];

//			if (simObject == null) {
//				simObject = Resources.Load ("Prefabs/slime", GameObject) as GameObject;
//				if (simObject == null) {
//					Debug.Log ("Failed to find default prefab: Prefabs/slime");
//				}
//			}
		}

		public void addDungeonObjects(List<GameObject> gameObjects){
			if (simManager != null) {
				simManager.addDungeonObjects (gameObjects);
			}
		}

		public void swapGroups ()
		{
			if (simManager != null) {
				simManager.swapGroup ();
			}

			foreach (GridGraphics graphics in groupGraphics) {
				hideGrid (graphics);
			}

			groupText.GetComponent<Text> ().text = simManager.getGroupId().ToString();
		}

		public void addGroup ()
		{
			if (simManager != null) {
				simManager.addGroup ();

				foreach (GridGraphics graphics in groupGraphics) {
					hideGrid (graphics);
				}
			}
			groupText.GetComponentInParent<Text> ().text = simManager.getGroupId ().ToString();
		}

		public SimAccess ()
		{
			//init (null);
		}

		public void init (DungeonGeneration.Cell[,] dungeon)
		{
			this.dungeon = dungeon;
			Debug.Log ("Creating Sim on grid: " + dim + " x " + dim);
			crowdSim = new GameObject ();
			crowdSim.name = "Components";
			crowdSim.transform.position = new Vector3 (0, 0, 0);

			gridParent = new GameObject ();
			gridParent.name = "Graphics Grid";
			gridParent.transform.parent = crowdSim.transform;

			simObjects = new GameObject ();
			simObjects.name = "Sim Objects";
			simObjects.transform.parent = crowdSim.transform;

			this.simManager = new SimManager (cellWidth / gridRatio, (int)(dim * gridRatio), simObjects, dungeon, (int)gridRatio, defaultSetup);
		}

		public void OnDrawGizmos ()
		{
			if (leftSelected != null) {
				Vector2 pos = leftSelected.position;
				Gizmos.color = Color.blue;
				Gizmos.DrawCube(new Vector3(pos.x, 0.01f, pos.y), new Vector3(0.1f, 0.1f,0.1f));
			}

			if (displayField > 0) {
				Primitives.Cell[,] grid = simManager.groupGrid.grid;
				for (int i = 0; i < grid.GetLength (0); i++) {
					for (int j = 0; j < grid.GetLength (0); j++) {
						if (grid [i, j].exists) {
							Vector2 norm = new Vector2 (0f, 0f);
							if (displayField == 1) {
								norm = 0.25f * grid [i, j].avgVelocity.normalized;
							} else if (displayField == 2) {
								norm = 0.25f * grid [i, j].groupVelocity.normalized;
							}
							Vector2 gPos = grid [i, j].position;
							Vector2 from = gPos - norm * 0.5f;
							Vector2 to = gPos + norm * 0.5f;
							Gizmos.color = Color.white;
							Gizmos.DrawLine (new Vector3 (from.x, 0.01f, from.y), new Vector3 (to.x, 0.01f, to.y));
							Gizmos.color = Color.blue;
							Gizmos.DrawCube (new Vector3 (to.x, 0f, to.y), new Vector3 (0.05f, 0.05f, 0.05f));
						}
					}
				}
			}
		}

		public void setAction (string action)
		{
			actionText.GetComponent<Text> ().text = action;
			this.action = action;
		}

		public void resetValues(){
			setAction ("select");
			setAvoidanceText ("0.7");
			setDensityText ("0.4");
			setLaneText ("2.0");

			groupText.GetComponent<Text> ().text = "0";
			pausedText.GetComponent<Text> ().text = "Paused";
		}

		public void toggleTextUI(){
			actionText.SetActive (!actionText.activeSelf);
			densityText.SetActive (!densityText.activeSelf);
			avoidanceText.SetActive (!avoidanceText.activeSelf);
			laneText.SetActive (!laneText.activeSelf);
			groupText.SetActive (!groupText.activeSelf);
			pausedText.SetActive (!pausedText.activeSelf);

		}

		public void setDisplayFields ()
		{
			displayField++;
			if (displayField >= 3) {
				displayField = 0;
			}

			if (displayField == 1) {
				Debug.Log ("Displaying potential Gradients");
			}

			if (displayField == 2) {
				Debug.Log ("Displaying velocities");
			}
		}

		void FixedUpdate(){
			if (simManager == null) return;

			updateSim (Time.deltaTime);
		}

		void Update ()
		{
			if (simManager == null) return;

			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast (ray, out hit)) {
				Vector3 hitPosition = hit.point;

				 if (Input.GetMouseButtonDown (0) && action != null) {
					// set grid cell to a goal

					int[] index = simManager.helper.getLeft (new Vector2 (hitPosition.x, hitPosition.z));
					leftSelected = simManager.helper.accessGridCell (index);

					if (action.Equals ("goal")) {
						addGoal (new Vector2 (hitPosition.x, hitPosition.z));
					}

					if (action.Equals ("select")) {
						selectCell (new Vector2 (hitPosition.x, hitPosition.z));
					}

					// add an agent
					if (action.Equals ("agent")) {
						addAgent (new Vector2 (hitPosition.x, hitPosition.z));
					}

				} else if (Input.GetMouseButton (1) && action != null) {
					// add an agent
					if (action.Equals ("agent")) {
						addAgent (new Vector2 (hitPosition.x, hitPosition.z));
					}

					if (action.Equals ("goal")) {
						addGoal (new Vector2 (hitPosition.x, hitPosition.z));

					}
				}
			}				

		}

		public void updateSim (float time)
		{
			if (frames >= frameLimit) {
				updateFields = true;
				setGroupUpdateField (true);
				frames = 0;
			} else {
				updateFields = false;
				setGroupUpdateField (false);
				frames++;
			}
			executeUpdate (time);
		}

		private void setGroupUpdateField (bool updateField)
		{
			if (simManager != null) {
				simManager.setUpdateField (updateField);
			}
		}


		public void togglePause ()
		{
			if (simManager != null) {
				pausedText.GetComponent<Text>().text= simManager.togglePause ();
			}
		}

		private void executeUpdate (float time)
		{
			if (simManager != null) {
				simManager.update (time);
				float max = simManager.getMax ();
				if (groupGraphics [simManager.groupId] != null && updateFields) {
					groupGraphics [simManager.groupId].updatePotentialColors (max);
				}
			}
		}

		public void addGoal (Vector2 pos)
		{
			GameObject goalObject = Instantiate (goalPrefab);;
			if (simManager.addGoal (pos, goalObject) == false) {
				Object.Destroy (goalObject);
			}
		}

		public void selectCell (Vector2 pos)
		{
			int[] index = simManager.getCell (pos);
			Primitives.Cell selected = simManager.groupGrid.grid [index [0], index [1]];
			if (selected != null) {
				leftSelected = selected;
				Primitives.Cell sharedCell = leftSelected.sharedCell;
				Debug.Log (selected.index [0] + ", " + selected.index [1] + ": Potential: " + selected.potential+" vel: [" + selected.groupVelocity.x + ", " + selected.groupVelocity.y + "] Density: "+ sharedCell.density);

				Debug.Log ("Faces: ");

				for (int i = 0; i < 4; i++) {
					Primitives.Face face = selected.faces [i];
					Primitives.Face sharedFace = sharedCell.faces[i];
					if (face != null) {
						Debug.Log (i + " Cost: " + face.cost + " grad: " + face.potentialGrad + "vel: "+face.velocity+" groupVel: "+face.groupVelocity);
					}
				}
				Debug.Log ("-------------");
			}
		}

		public void addAgent (Vector2 pos)
		{
			simManager.addAgent (pos, simObject, true);
		}

		public void addAgent(Vector2 pos, int id){
			simManager.addAgent (pos, simObject, true, id);
		}

		private void displayGrid (GridGraphics graphics)
		{
			if (graphics != null) {
				graphics.display ();
			}
		}

		private void hideGrid (GridGraphics graphics)
		{
			if (graphics != null) {
				graphics.hide ();
			}
		}

		private void addGraphics ()
		{
			Debug.Log ("Creating graphical grids");

			GameObject newParent = new GameObject ();
			newParent.name = "Graphics Grid: " + simManager.groupId;
			newParent.transform.parent = crowdSim.transform;

			GridGraphics newGraphics = new GridGraphics (cellWidth / gridRatio, simManager.groups [simManager.groupId].grid, newParent, (int)gridRatio);
			groupGraphics [simManager.groupId] = newGraphics;
		}

		public void displaySim ()
		{
			if (visilbe) {
				hideGrid (groupGraphics [simManager.groupId]);
				visilbe = false;
			} else {
				Debug.Log ("Displaying group grid: " + simManager.groupId);
				if (groupGraphics [simManager.groupId] == null){
					addGraphics ();
				}
				displayGrid (groupGraphics [simManager.groupId]);
				visilbe = true;					
			}
		}

		public void trigger ()
		{
			simManager.trigger ();
		}

		void setAvoidanceText(string toAdd){
			avoidanceText.GetComponent<Text> ().text = toAdd;
		}

		public void increaseAvoidance(){
			if (simManager == null) {
				return;
			}
			setAvoidanceText(simManager.increaseAvoidance ());
		}

		public void decreaseAvoidance(){
			if (simManager == null) {
				return;
			}
			setAvoidanceText(simManager.decreaseAvoidance ());
		}

		void setDensityText(string toAdd){
			densityText.GetComponent<Text> ().text = toAdd;
		}
		public void decreaseDensityExp(){
			if (simManager == null) {
				return;
			}

			setDensityText(simManager.decreaseDensityExp ());
		}

		public void increaseDensityExp(){
			if (simManager == null) {
				return;
			}

			setDensityText(simManager.increaseDensityExp ());

		}

		void setLaneText(string toAdd){
			laneText.GetComponent<Text> ().text = toAdd;
		}

		public void decreaseLaneFormation(){
			if (simManager == null) {
				return;
			}

			setLaneText(simManager.decreaseLaneFormation ());
		}

		public void increaseLaneFormation(){
			if (simManager == null) {
				return;
			}

			setLaneText(simManager.increaseLaneFormation ());
		}
			
	}
}

