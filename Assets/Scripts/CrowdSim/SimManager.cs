using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Visualization;
using Primitives;

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
			sharedGrid = new SharedGrid(cellWidth,dim);	
		}

		public Cell[,] getGrid(){
			return sharedGrid.grid;
		}

	}
}

