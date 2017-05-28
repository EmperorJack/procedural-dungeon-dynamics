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
		SharedGrid sharedGrid;
		public float cellWidth;
		public int dim;
		Helper<Cell> helper;

		public SimManager (float cellWidth, int dim)
		{
			this.cellWidth = cellWidth;
			this.dim = dim;
			sharedGrid = new SharedGrid(cellWidth,dim);	
			helper = new Helper<Cell>(sharedGrid.grid, cellWidth);
		}

		public int[] selectCell(Vector2 pos){

			Cell cell = helper.getCell (pos);
			int[] index = helper.getCellIndex(pos);
			if (cell != null) {
				if (cell.isGoal) {
					cell.isGoal = false;
				} else {
					cell.isGoal = true;
				}
			} 
			return index;
		}

		public Cell[,] getGrid(){
			return sharedGrid.grid;
		}

	}
}

