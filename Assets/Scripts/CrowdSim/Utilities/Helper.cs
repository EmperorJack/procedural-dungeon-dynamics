﻿using System.Collections;
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

		public Helper(T[,] grid, float cellWidth){
			this.grid = grid;
			this.cellWidth = cellWidth;
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
			float cellX = position.x - position.x % (cellWidth / 2);
			float cellY = position.y - position.y % (cellWidth / 2);

			int cellRow = (int)(cellX / cellWidth);
			int cellCol = (int)(cellY / cellWidth);

			return new int[]{ cellRow, cellCol };
		}

		public  T getCell(Vector2 position){
			return accessGridCell(getCellIndex(position));
		}

		public  int[] getCellIndex(Vector2 position){
			return new int[]{ (int)((position.x + cellWidth/2) / cellWidth), (int)((position.y + cellWidth/2) / cellWidth) };
		}

		public  T accessGridCell(int[] index){

			if (index [0] < 0 || index [1] < 0 || index [0] >= grid.Length || index [1] >= grid.Length) {
				return default(T);
			}
			return grid [index [0], index [1]];
		}
	}
}

