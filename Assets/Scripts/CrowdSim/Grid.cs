using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Primitives;

namespace CrowdSim
{
	public abstract class Grid
	{
		protected float cell_width;
		protected int dim;

		protected Cell[,] grid;
		protected Dictionary<Vector2, Vector2> cellDic;


		public Grid (float cell_width, int dim)
		{
			this.cell_width = cell_width;
			this.dim = dim;

			populateGrid ();
		}

		public Cell getCell (Cell other_cell)
		{
			Vector2 index = other_cell.index;
			return grid [(int)index.x, (int)index.y];
		}

		void populateGrid ()
		{
			grid = new Cell[dim, dim];
			cellDic = new Dictionary<Vector2, Vector2> ();

			for (int i = 0; i < dim; i++) {
				for (int j = 0; j < dim; j++) {

					Vector2 pos = new Vector2 ((cell_width) * (i - dim / 2 + 0.5f), (cell_width) * (j - dim / 2 + 0.5f));
					instantiateCell (i, j, pos);
				}
			}
			
		}

		// Get the grid coordinate with it's center with
		// x and y coordinates less than the given x/y

		public Vector2 getLeft (float x, float y)
		{
			//		Vector2 cellPos = new Vector2 (Mathf.Floor (x / cell_width - cell_width / 2), Mathf.Floor (y / cell_width - cell_width / 2));
			//		cellPos = new Vector2 (cellPos.x * cell_width + cell_width / 2, cellPos.y * cell_width + cell_width / 2);
			//
			//		if (cellDic.ContainsKey (cellPos)) {
			//			return cellDic [cellPos];
			//		} else {
			//			return new Vector2 (0, 0);
			//		}

			x = x / cell_width;
			y = y / cell_width;

			float t_cell_width = 1.0f;

			Vector2 cellPos = new Vector2 (Mathf.Floor (x - t_cell_width / 2), Mathf.Floor (y - t_cell_width / 2));
			cellPos = new Vector2 (cellPos.x * cell_width + cell_width / 2, cellPos.y * cell_width + cell_width / 2);

			if (cellDic.ContainsKey (cellPos)) {
				return cellDic [cellPos];
			} else {
				return new Vector2 (0, 0);
			}
		}

		protected  abstract void instantiateCell (int i, int j, Vector2 pos);

		public abstract void update ();

		public bool contains (Vector2 pos)
		{
			bool contained = pos.x > -(dim / 2f) * cell_width && pos.x < (dim / 2f) * cell_width;
			return contained && pos.y > -(dim / 2f) * cell_width && pos.y < (dim / 2f) * cell_width;
		}

		public Cell getCell (Vector2 pos)
		{
			for (int i = 0; i < dim; i++) {
				for (int j = 0; j < dim; j++) {
					Cell cell = grid [i, j];
					Vector2 cellPos = cell.position;

					if (pos.x < cellPos.x + cell_width / 2 && pos.x > cellPos.x - cell_width / 2) {
						if (pos.y < cellPos.y + cell_width / 2 && pos.y > cellPos.y - cell_width / 2) {
							return cell;
						}
					}
				}
			}

			return null;
		}

		public Cell[,] grid2 {
			get {
				return grid;
			}
		}
	}

}
