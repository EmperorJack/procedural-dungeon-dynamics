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

		public SimAccess ()
		{
			init ();
		}

		public void init(){
			this.simManager = new SimManager(1,3);
			this.cellWidth = simManager.cellWidth;
			this.dim = simManager.dim;

			GameObject sharedGrid = new GameObject ();
			sharedGrid.name = "SharedGrid";
			sharedGrid.transform.position = new Vector3 (0, 0, 0);

			sharedGraphics = new GridGraphics (cellWidth, dim, simManager.getGrid(), sharedGrid);
			displayGrid (sharedGraphics);
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

