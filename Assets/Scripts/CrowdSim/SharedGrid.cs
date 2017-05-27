using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Primitives;
using Utilities;

namespace CrowdSim
{
	public class SharedGrid
	{
		public Cell[,] grid;
		float cellWidth;
		int dim;

		private Helper<Cell> helper;

		// 'constant' values
		float densityExp = 0.1f;

		public SharedGrid (float cellWidth, int dim)
		{
			grid = new Cell[dim, dim];
			helper = new Helper<Cell> (grid, cellWidth);

			this.cellWidth = cellWidth;
			this.dim = dim;

			for (int i = 0; i < dim; i++) {
				for (int j = 0; j < dim; j++) {
					grid [i, j] = new Cell ();
					grid [i, j].position = new Vector2 (i * cellWidth, j * cellWidth);
				}
			}
		}

		List<Cell> affectedCells = new List<Cell>(); // all cells who have an agent denstiy contribution

		public void assignAgents(List<Agent> agents){
			foreach(Agent agent in agents){
				int[] index = helper.getCellIndex (agent.position);
				Cell leftCell = helper.accessGridCell(index);

				if (leftCell != null && leftCell.exists) {
					float deltaX = agent.position.x - leftCell.position.x;
					float deltaY = agent.position.y - leftCell.position.y;

					// D --- C
					// |     |
					// A --- B

					// add density contribution to neighbouring cell
					float leftDensity = Mathf.Pow (Mathf.Min (1 - deltaX, 1 - deltaY), densityExp);
					// add average velocity contribution
					leftCell.density += leftDensity;
					leftCell.avgVelocity += leftDensity * agent.velocity; // cell A
					affectedCells.Add(leftCell);

					Cell bCell = helper.accessGridCell (new int[]{ index [0] + 1, index [1] });
					if (bCell != null) {
						float bDensity = Mathf.Pow (Mathf.Min (deltaX, 1 - deltaY), densityExp);
						bCell.density += bDensity;
						bCell.avgVelocity += bDensity * agent.velocity;
						affectedCells.Add (bCell);
					}

					Cell cCell = helper.accessGridCell (new int[]{ index [0] + 1, index [1] +1 });
					if (cCell != null) {
						float cDensity = Mathf.Pow (Mathf.Min (deltaX,deltaY), densityExp);
						cCell.density += cDensity;
						cCell.avgVelocity += cDensity * agent.velocity;
						affectedCells.Add (cCell);
					}

					Cell dCell = helper.accessGridCell (new int[]{ index [0], index [1] + 1 });
					if (dCell != null) {
						float dDensity = Mathf.Pow (Mathf.Min (1 - deltaX, deltaY), densityExp);
						dCell.density += dDensity;
						dCell.avgVelocity += dDensity * agent.velocity;
						affectedCells.Add (dCell);
					}
				}
			}
			// calculate average velocity

			foreach (Cell cell in affectedCells) {
				cell.avgVelocity = cell.avgVelocity / cell.density;
			}
		}

		public void update(){

		}
	}
}

