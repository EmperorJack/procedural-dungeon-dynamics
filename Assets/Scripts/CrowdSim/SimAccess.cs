﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Visualization;
using DungeonGeneration;

namespace CrowdSim
{
	public class SimAccess : MonoBehaviour
	{
		public SimManager simManager; // class which runs the simulation
		GridGraphics sharedGraphics;
		private Vector2 pos = new Vector2(0,0);
		private bool visilbe = false;

		GameObject gridParent;
		GameObject crowdSim ;
		public GameObject simObject;
		GameObject simObjects;

		// Grid fields
		public float cellWidth = 1; // width of cells before scaling, 1 is a good value
		public int dim = 30;
		public float gridRatio = 1; // how much to expand each dungeon cell by
		public int frameLimit = 10; // how many frames to wait between updating sim



		string action = "select";
		private int displayField = 0;
		private bool updateFields = true;
		int frames = 1;

		DungeonGeneration.Cell[,] dungeon;

		public void reset(){
			if (simManager != null) {
				simManager.reset ();
			}
			DestroyImmediate (GameObject.Find ("Sim Objects"));
			DestroyImmediate (GameObject.Find ("Graphics Grid"));
			DestroyImmediate (GameObject.Find ("Components"));

		}

		void Start(){
			Collider c = GetComponent<Collider> ();
			c.transform.position = pos;
			c.transform.localScale = new Vector3 (cellWidth * dim * gridRatio, 0, cellWidth * dim * gridRatio);
			c.transform.position = c.transform.position + new Vector3 ((cellWidth * gridRatio * (dim - 1)) / 2, 0, (cellWidth * gridRatio * (dim - 1)) / 2);

//			if (simObject == null) {
//				simObject = Resources.Load ("Prefabs/slime", GameObject) as GameObject;
//				if (simObject == null) {
//					Debug.Log ("Failed to find default prefab: Prefabs/slime");
//				}
//			}
		}

		public SimAccess(){
			//init (null);
		}
			
		public void init(DungeonGeneration.Cell[,] dungeon){
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

			this.simManager = new SimManager(cellWidth/gridRatio,(int)(dim*gridRatio), simObjects, dungeon, (int)gridRatio);
		}

		public void OnDrawGizmos ()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine (new Vector3 (0, 0.1f, 0), new Vector3 (0, 0.1f, 10f));
			Gizmos.DrawLine (new Vector3 (0, 0.1f, 0), new Vector3 (10f, 0.1f, 0));
			if (displayField > 0) {
					Primitives.Cell[,] grid = simManager.groupGrid.grid;
					for (int i = 0; i < grid.GetLength (0); i++) {
						for (int j = 0; j < grid.GetLength (0); j++) {
							if (grid [i, j].exists) {
								Vector2 norm = new Vector2 (0f, 0f);
								if (displayField == 1) {
									norm = 0.25f * grid [i, j].potGrad.normalized;
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

		public void setAction(string action){
			this.action = action;
		}

		public void setDisplayFields(){
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

		void Update ()
		{

			updateSim ();
				Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				RaycastHit hit;

				if (Physics.Raycast (ray, out hit)) {
					Vector3 hitPosition = hit.point;

				if (Input.GetMouseButtonDown (0) && action != null) {
						// set grid cell to a goal
						if (action.Equals ("goal")) {
							int[] selectedIndex = addGoal (new Vector2 (hitPosition.x, hitPosition.z));
							if (selectedIndex != null) {
								print ("Selected cell: " + selectedIndex [0] + " " + selectedIndex [1]);
							} else {
								print ("Failed to select cell at: " + hitPosition.x + " " + hitPosition.z);
							}
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
					}
				}

		}

		public void updateSim(){
			executeUpdate ();
			if (frames >= frameLimit) {
				updateFields = true;
				setGroupUpdateField (true);
				frames = 0;
			} else {
				updateFields = false;
				setGroupUpdateField (false);
				frames++;
			}
		}

		private void setGroupUpdateField(bool updateField){
			if (simManager != null && simManager.groupGrid != null) {
				simManager.groupGrid.updateField = updateField;
			}
		}

		private void executeUpdate(){
			if (simManager != null) {
				simManager.update ();
				float max = simManager.getMax ();
				if (sharedGraphics != null && updateFields) {
					sharedGraphics.updatePotentialColors (max);
				}
			}
		}

		public int[] addGoal(Vector2 pos){
			int[] index = simManager.selectCell (pos);
			if (sharedGraphics != null) {
				GridCell graphicCell = sharedGraphics.getDispCell (index);

				if (graphicCell != null) {
					if (graphicCell.getColor ().Equals (Color.green)) {
						graphicCell.setColor (Color.black);
					} else {
						graphicCell.setColor (Color.green);
					}
				} 
			}
			return index;
		}

		public void selectCell(Vector2 pos){
			int[] index = simManager.getCell (pos);
			Primitives.Cell selected = simManager.groupGrid.grid[index [0], index [1]];
			if (selected != null) {
				Debug.Log (selected.index [0] + ", " + selected.index [1] + ": Potential: " + selected.potential);
				Debug.Log ("Faces: ");
				for(int i = 0; i < 4; i++){
					Primitives.Face face = selected.faces [i];
					if (face != null) {
						Debug.Log (i + " Cost: " + face.cost+" grad: "+face.potentialGrad+" vel: ["+selected.groupVelocity.x+", "+selected.groupVelocity.y+"]");
					}
				}
				Debug.Log ("-------------");
			}
		}

		public void addAgent(Vector2 pos){
			Debug.Log("Total agents: "+simManager.addAgent (pos, simObject));
			executeUpdate ();
		}
			
		private void displayGrid(GridGraphics graphics){
			if (graphics != null) {
				graphics.display ();
			}
		}

		private void hideGrid(GridGraphics graphics){
			if (graphics != null) {
				graphics.hide ();
			}
		}

		public void displaySim(){
			if (visilbe) {

				hideGrid (sharedGraphics);
				visilbe = false;
			} else {
				if (sharedGraphics == null) {
					Debug.Log ("Creating graphical grids");
					sharedGraphics = new GridGraphics (cellWidth/gridRatio, simManager.getGrid(), gridParent, (int)gridRatio);
				}
				displayGrid (sharedGraphics);
				visilbe = true;
			}
		}

		public void trigger(){
			simManager.trigger ();
		}
			
	}
}

