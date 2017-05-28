using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Visualization;

namespace CrowdSim
{
	public class SimAccess
	{
		private SimManager simManager; // class which runs the simulation
		GridGraphics sharedGraphics;

		float cellWidth;
		int dim;
			
		public void init(float cellWidth, int dim){
			GameObject crowdSim = new GameObject ();
			crowdSim.name = "CrowdSim";
			crowdSim.transform.position = new Vector3 (0, 0, 0);

			GameObject sharedGrid = new GameObject ();
			sharedGrid.name = "SharedGrid";
			sharedGrid.transform.parent = crowdSim.transform;

			GameObject simObjects = new GameObject ();
			simObjects.name = "SimObjects";
			simObjects.transform.parent = crowdSim.transform;

			this.simManager = new SimManager(cellWidth,dim, simObjects);
			this.cellWidth = simManager.cellWidth;
			this.dim = simManager.dim;

			sharedGraphics = new GridGraphics (cellWidth, dim, simManager.getGrid(), sharedGrid);
			displayGrid (sharedGraphics);
		}

		public int[] selectCell(Vector2 pos){
			int[] index = simManager.selectCell (pos);
			GridCell graphicCell = sharedGraphics.getDispCell (index);

			if (graphicCell != null) {
				if (graphicCell.getColor().Equals(Color.green)) {
					graphicCell.setColor (Color.black);
				} else {
					graphicCell.setColor (Color.green);
				}
				return index;
			} 
			return null;
		}

		public void addAgent(Vector2 pos){
			simManager.addAgent (pos);
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

		public void hideSharedGrid(){
			hideGrid (sharedGraphics);
		}

		public void displaySharedGrid(){
			displayGrid (sharedGraphics);
		}
			
	}
}

