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

		public bool solid = false;

		GameObject objectParent;

		public void reset(){
			for (int i = 0; i < dim; i++) {
				for (int j = 0; j < dim; j++) {
					cells [i, j].reset();
				}
			}
		}

		private int ratio;
		public GridGraphics (float cellSize, Cell[,] dataGrid, GameObject objectParent, int ratio)
		{
			//this.width = dim * cellSize;
			this.cellSize = cellSize;
			this.dim = dataGrid.GetLength(0);
			this.dataGrid = dataGrid;
			this.objectParent = objectParent;
			init ();
			this.ratio = ratio;
		}

		public void updatePotentialColors(float max){
			for (int i = 0; i < dim; i++) {
				for (int j = 0; j < dim; j++) {
					if (solid == false) {
						if (cells [i, j] != null && dataGrid [i, j] != null && dataGrid [i, j].isGoal == false) {
							float ratio = dataGrid [i, j].potential / max;
							Color col = new Color (ratio, 1 - ratio, 0);
							cells [i, j].setColor (col);
						}
					} else {
						if (cells [i, j] != null) {

//							if (dataGrid [i, j].density <= 1.0f) {
//								cells [i, j].setColor (Color.green);
//							} else if (dataGrid [i, j].density >= 2.0f) {
//								cells [i, j].setColor (Color.red);
//							} else {
//								cells [i, j].setColor (Color.yellow);
//							}

							if (dataGrid [i, j].isGoal) {
								cells [i, j].setColor (Color.green);
							} else {
								cells [i, j].setColor (Color.red);
							}
						}
					}
				}
			}
		}
			
		private void init ()
		{
			cells = new GridCell[dim, dim];

			helper = new Helper<GridCell> (cells, cellSize, ratio);
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

