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
			this.simManager = new SimManager(1,10);
			this.cellWidth = simManager.cellWidth;
			this.dim = simManager.dim;

			displayGrid (sharedGraphics);

		}
			
		private void displayGrid(GridGraphics graphics){
			if (graphics == null) {
				graphics = new GridGraphics (cellWidth, dim);
			} 
			graphics.display ();
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

