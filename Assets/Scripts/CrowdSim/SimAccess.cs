using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Visualization;
using DungeonGeneration;

namespace CrowdSim
{
	public class SimAccess
	{
		public SimManager simManager; // class which runs the simulation
		GridGraphics sharedGraphics;
		GameObject gridParent;

		float cellWidth;
		int dim;

		private bool visilbe = false;

		int frames = 1;
		public int frameLimit = 10;

		private float gridRatio;
			
		public void init(float cellWidth, int dim, float gridRatio){
			GameObject crowdSim = new GameObject ();
			crowdSim.name = "CrowdSim";
			crowdSim.transform.position = new Vector3 (0, 0, 0);

			gridParent = new GameObject ();
			gridParent.name = "gridParent";
			gridParent.transform.parent = crowdSim.transform;

			GameObject simObjects = new GameObject ();
			simObjects.name = "SimObjects";
			simObjects.transform.parent = crowdSim.transform;

			this.simManager = new SimManager(cellWidth/gridRatio,(int)(dim*gridRatio), simObjects, null, (int)gridRatio);
			this.cellWidth = simManager.cellWidth;
			this.dim = simManager.dim;
			this.gridRatio = gridRatio;

		}

		public void init(float cellWidth, int dim,DungeonGeneration.Cell[,] dungeon, float gridRatio){
			GameObject crowdSim = new GameObject ();
			crowdSim.name = "CrowdSim";
			crowdSim.transform.position = new Vector3 (0, 0, 0);

			gridParent = new GameObject ();
			gridParent.name = "gridParent";
			gridParent.transform.parent = crowdSim.transform;

			GameObject simObjects = new GameObject ();
			simObjects.name = "SimObjects";
			simObjects.transform.parent = crowdSim.transform;

			this.simManager = new SimManager(cellWidth/gridRatio,(int)(dim*gridRatio), simObjects, dungeon, (int)gridRatio);
			this.cellWidth = simManager.cellWidth;
			this.dim = simManager.dim;
			this.gridRatio = gridRatio;

		}

		public void update(){
			if (frames >= frameLimit) {
				executeUpdate ();
				frames = 0;
			} else {
				frames++;
			}
		}

		private void executeUpdate(){
			if (simManager != null) {
				simManager.update ();
				float max = simManager.getMax ();
				if (sharedGraphics != null) {
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

		public void addAgent(Vector2 pos, GameObject simObject){
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
					sharedGraphics = new GridGraphics (cellWidth, simManager.getGrid(), gridParent, (int)gridRatio);
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

