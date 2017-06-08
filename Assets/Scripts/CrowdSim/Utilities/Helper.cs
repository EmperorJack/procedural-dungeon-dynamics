using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Primitives;
using CrowdSim;

namespace Utilities
{
	public class Helper<T>
	{
		private T[,] grid;
		private float cellWidth;
		private int ratio;

		public Helper(T[,] grid, float cellWidth, int ratio){
			this.grid = grid;
			this.cellWidth = cellWidth;
			this.ratio = ratio;
		}

		public  T getLeft(Cell cell){
			int[] cellIndex = getLeft (cell.position);
			return accessGridCell (cellIndex);
		}
			
		/// <summary>
		/// Get the cell with position left and below the given position.
		/// </summary>
		/// <returns>The left.</returns>
		/// <param name="position">Position.</param>  
		/// <param name="cellWidth">Cell width.</param>
		/// <param name="grid">Grid.</param>
		public  int[] getLeft(Vector2 position){
			//position = position * ratio;

			Vector2 newPos = position = position + new Vector2 ((cellWidth * (ratio-1)) / 2, (cellWidth * (ratio-1)) / 2);
				

			float x = newPos.x / cellWidth;
			float y = newPos.y / cellWidth;

			int i = ((int) (x + grid.GetLength(0))) - grid.GetLength(0);
			int j = ((int) (y + grid.GetLength(0))) - grid.GetLength(0);

			int cellRow = Mathf.Max (i, 0);
			int cellCol = Mathf.Max (j, 0);

//			cellWidth = cellWidth * ratio;
//			position = position + new Vector2 ((cellWidth * (ratio-1)) / 2, (cellWidth * (ratio-1)) / 2);
//
//			int cellRow = (int)Mathf.Floor (position.x);
//			int cellCol = (int)Mathf.Floor (position.y);
//
//			position = position - new Vector2 ((cellWidth * (ratio-1)) / 2, (cellWidth * (ratio-1)) / 2);
//			cellWidth /= ratio;
//			//position = position / ratio;
//
//			cellRow = Mathf.Max (cellRow, 0);
//			cellCol = Mathf.Max (cellCol, 0);

//			float cellX = position.x - position.x % (cellWidth / 2);
//			float cellY = position.y - position.y % (cellWidth / 2);
//
//			int cellRow = (int)(cellX / cellWidth);//- ratio - 1;
//			int cellCol = (int)(cellY / cellWidth);// - ratio - 1;

			return new int[]{ cellRow, cellCol };
		}

		public  T getCell(Vector2 position){
			return accessGridCell(getCellIndex(position));
		}

		public  int[] getCellIndex(Vector2 position){
			return new int[]{ (int)((position.x + cellWidth/2) / cellWidth), (int)((position.y + cellWidth/2) / cellWidth) };
		}

		public  T accessGridCell(int[] index){

			if (index [0] < 0 || index [1] < 0 || index [0] >= grid.GetLength(0) || index [1] >= grid.GetLength(0)) {
				return default(T);
			}
			return grid [index [0], index [1]];
		}
	}
}

