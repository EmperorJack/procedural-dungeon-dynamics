using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Primitives;
using CrowdSim;

namespace Utilities
{
	public class Helper
	{
		private Cell[,] grid;
		private float cellWidth;

		public Helper(Cell[,] grid, float cellWidth){
			this.grid = grid;
			this.cellWidth = cellWidth;
		}

		public  Cell getLeft(Cell cell){
			int[] cellIndex = getLeft (cell.position);
			return accessCellGrid (cellIndex);
		}

		/// <summary>
		/// Get the cell with position left and below the given position.
		/// </summary>
		/// <returns>The left.</returns>
		/// <param name="position">Position.</param>
		/// <param name="cellWidth">Cell width.</param>
		/// <param name="grid">Grid.</param>
		public  int[] getLeft(Vector2 position){
			float cellX = position.x - position.x % (cellWidth / 2);
			float cellY = position.y - position.y % (cellWidth / 2);

			int cellRow = (int)(cellX / cellWidth);
			int cellCol = (int)(cellY / cellWidth);

			return new int[]{ cellRow, cellCol };
		}

		public  Cell getCell(Vector2 position){
			return accessCellGrid(getCellIndex(position));
		}

		public  int[] getCellIndex(Vector2 position){
			return new int[]{ (int)(position.x / cellWidth), (int)(position.y / cellWidth) };
		}

		public  Cell accessCellGrid(int[] index){
			if (index [0] < 0 || index [1] < 0 || index [0] >= grid.Length || index [1] >= grid.Length) {
				return null;
			}
			return grid [index [0], index [1]];
		}
	}
}

