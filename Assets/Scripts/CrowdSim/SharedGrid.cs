using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Primitives;
using Utilities;

namespace CrowdSim
{
	public class SharedGrid
	{
		Cell[,] grid;
		float cellWidth;
		int gridWidth;

		private Helper helper;

		// 'constant' values
		float densityExp = 0.1f;

		public SharedGrid (float cellWidth, int gridWidth)
		{
			grid = new Cell[gridWidth, gridWidth];
			helper = new Helper (grid, cellWidth);
		}

		public void assignAgents(List<Agent> agents){
			for(int i = 0; i < agents.Count; i++){
				Agent agent = agents [i];
				int[] index = helper.getCellIndex (agent.position);
				Cell leftCell = helper.accessCellGrid(index);

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

					Cell bCell = helper.accessCellGrid (new int[]{ index [0] + 1, index [1] });
					if (bCell != null) {
						float bDensity = Mathf.Pow (Mathf.Min (deltaX, 1 - deltaY), densityExp);
						bCell.density += bDensity;
						bCell.avgVelocity += bDensity * agent.velocity;
					}

					Cell cCell = helper.accessCellGrid (new int[]{ index [0] + 1, index [1] +1 });
					if (cCell != null) {
						float cDensity = Mathf.Pow (Mathf.Min (deltaX,deltaY), densityExp);
						cCell.density += cDensity;
						cCell.avgVelocity += cDensity * agent.velocity;
					}

					Cell dCell = helper.accessCellGrid (new int[]{ index [0], index [1] + 1 });
					if (dCell != null) {
						float dDensity = Mathf.Pow (Mathf.Min (1 - deltaX, deltaY), densityExp);
						dCell.density += dDensity;
						dCell.avgVelocity += dDensity * agent.velocity;
					}
				}

				// calculate average velocity
			}
		}

		public void update(){

		}
	}
}

