using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Visualization;

namespace CrowdSim
{
	public class SimManager
	{
		SharedGrid sharedGrid;
		public float cellWidth;
		public int dim;

		public SimManager (float cellWidth, int dim)
		{
			this.cellWidth = cellWidth;
			this.dim = dim;
			sharedGrid = new SharedGrid(1,10);	
		}


	}
}

