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
		private float cellSize;
		private int dim;
		private float width;

		Helper<GridCell> helper;

		private bool active = false;

		public GridGraphics (float cellSize, int dim)
		{
			this.width = dim * cellSize;
			this.cellSize = cellSize;
			this.dim = dim;
		}


		private void intCells(Cell[,] dataGrid){
			cells = new GridCell[dim, dim];

			helper = new Helper<GridCell> (cells, cellSize);
			for (int i = 0; i < dim; i++) {
				for (int j = 0; j < dim; j++) {
					Vector2 cellPos = dataGrid [i, j].position;
					cells [i, j] = new GridCell (cellSize, Color.blue, cellPos);
					cells [i, j].display ();
				}
			}

			active = true;
		}

		public GridCell getDispCell(Vector3 pos){
			int[] index = helper.getCellIndex (new Vector2 (pos.x, pos.z));
			return helper.accessGridCell (index);
		}

		public void display(){
			if (active == false) {
				for (int i = 0; i < dim; i++) {
					for (int j = 0; j < dim; j++) {
						cells [i, j].display ();
					}
				}
			}
			active = true;
		}

		public void hide(){
			if (active = true) {
				for (int i = 0; i < dim; i++) {
					for (int j = 0; j < dim; j++) {
						cells [i, j].hide ();
					}
				}
			}
			active = false;
		}

		public bool isActive(){
			return active;
		}
	}
}

