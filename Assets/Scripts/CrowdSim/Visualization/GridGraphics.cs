using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Primitives;
using Utilities;

namespace Visualization
{
	public class GridGraphics
	{
		GridCell[,] cells;
		Cell[,] dataGrid;
		private float cellSize;
		private int dim;
		//private float width;

		Helper<GridCell> helper;

		private bool active = false;

		GameObject objectParent;

		public GridGraphics (float cellSize, Cell[,] dataGrid, GameObject objectParent)
		{
			//this.width = dim * cellSize;
			this.cellSize = cellSize;
			this.dim = dataGrid.GetLength(0);
			this.dataGrid = dataGrid;
			this.objectParent = objectParent;
			init ();
		}

		public void updatePotentialColors(float max){
			for (int i = 0; i < dim; i++) {
				for (int j = 0; j < dim; j++) {
					if (cells[i,j] != null && dataGrid[i,j] != null && dataGrid [i, j].isGoal == false) {
						float ratio = dataGrid [i, j].potential / max;
						Color col = new Color (ratio, 1-ratio, 0);
						cells [i, j].setColor (col);
					}
				}
			}
		}
			
		private void init ()
		{
			cells = new GridCell[dim, dim];

			helper = new Helper<GridCell> (cells, cellSize);
			for (int i = 0; i < dim; i++) {
				for (int j = 0; j < dim; j++) {
					if (dataGrid [i, j] != null && dataGrid[i,j].exists) {
						Vector2 cellPos = dataGrid [i, j].position;
						cells [i, j] = new GridCell (cellSize, Color.black, cellPos, objectParent);
						//cells [i, j].display ();
					}
				}
			}
		}

		public GridCell getDispCell (Vector2 pos)
		{
			int[] index = helper.getCellIndex (pos);
			return helper.accessGridCell (index);
		}

		public GridCell getDispCell(int[] index){
			return helper.accessGridCell (index);
		}

		public void display ()
		{
			if (active == false) {
				for (int i = 0; i < dim; i++) {
					for (int j = 0; j < dim; j++) {
						if (cells [i, j] != null) {
							cells [i, j].display ();
						}
					}
				}
			}
			active = true;
		}

		public void hide ()
		{
			if (active == true) {
				for (int i = 0; i < dim; i++) {
					for (int j = 0; j < dim; j++) {
						if (cells [i, j] != null) {
							cells [i, j].hide ();
						} 
					}
				}
			}
			active = false;
		}

		public bool isActive ()
		{
			return active;
		}
	}
}

