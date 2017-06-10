using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Primitives;
using CrowdSim;

namespace Utilities
{
	public class Helper<T>
		where T : GridCell
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

			Vector2 newPos = position;//+ new Vector2 ((cellWidth * (ratio-1)) / 2, (cellWidth * (ratio-1)) / 2);
				

			float x = newPos.x + (cellWidth * (ratio - 1)) / 2;
			float y = newPos.y + (cellWidth * (ratio - 1)) / 2;

			x *= ratio;
			y *= ratio;

			int i = (int)Mathf.Floor (x);
			int j = (int)Mathf.Floor (y);

			int cellRow = Mathf.Max (i, 0);
			int cellCol = Mathf.Max (j, 0);

			return new int[]{ cellRow, cellCol };
		}

		public  T getCell(Vector2 position){
			return accessGridCell(getCellIndex(position));
		}

		public  int[] getCellIndex(Vector2 pos){

			int[] leftIndex = getLeft (pos);
			GridCell leftCell = accessGridCell (leftIndex);
			if (leftCell == null) {
				return new int[]{ -1, -1 };
			} else {
				float deltaX = pos.x - leftCell.getPosition().x;
				float deltaY = pos.y - leftCell.getPosition().y;

				if (deltaX >= cellWidth / 2) {
					leftIndex [0]++;
				}

				if (deltaY >= cellWidth / 2) {
					leftIndex [1]++;
				}
				return leftIndex;
			}
		}

		public  T accessGridCell(int[] index){

			if (index [0] < 0 || index [1] < 0 || index [0] >= grid.GetLength(0) || index [1] >= grid.GetLength(0)) {
				return default(T);
			}
			return grid [index [0], index [1]];
		}
	}
}

