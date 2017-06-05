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
		public int dim;

		private Helper<Cell> helper;

		// 'constant' values
		float densityExp = 0.9f;

		public float minDensity = 0.1f;
		public float maxDensity = 1f;
		public float minVelocity = 0.1f;
		public float maxVelocity = 0.5f;
		public float distanceWeight = 1.0f;
		public float timeWeight = 1.0f;
		public float discomfortWeight = 1;

		private bool customDungeon = false;

		private DungeonGeneration.Cell[,] dungeon;

		public int realCells;

		private List<SimObject> simObjects = new List<SimObject> ();

		int ratio;

		public SharedGrid (float cellWidth, int dim, DungeonGeneration.Cell[,] dungeon, int ratio)
		{
			this.cellWidth = cellWidth;
			this.dim = dim;
			this.ratio = ratio;

			if (dungeon == null) {
				customDungeon = false;
			} else {
				customDungeon = true;
				this.dim = dungeon.GetLength(0)* ratio;
			}

			Debug.Log ("DIM: " + this.dim);

			grid = new Cell[this.dim,this.dim];
			helper = new Helper<Cell> (grid, cellWidth);
			this.dungeon = dungeon;

			initGrid ();
		}

		public void addAgent(SimObject simObject){
			simObjects.Add (simObject);
		}

		private void initGrid(){

			for (int i = 0; i < dim; i++) {
				for (int j = 0; j < dim; j++) {
					if (customDungeon == false || (isFloor(dungeon[i/ratio,j/ratio]))) {
						grid [i, j] = new Cell (new int[]{ i, j });
						grid [i, j].position = new Vector2 (i * cellWidth -0.25f * ratio * cellWidth, j * cellWidth - 0.25f * ratio * cellWidth);
						grid [i, j].exists = true;
						realCells++;
					} else if (customDungeon && isFloor(dungeon[i/ratio,j/ratio]) == false) {
						grid [i, j] = new Cell (new int[]{ i, j });
						grid [i, j].position = new Vector2 (i * cellWidth - 0.25f * ratio * cellWidth, j * cellWidth -0.25f *  ratio * cellWidth);
						grid [i, j].exists = false;
					}
				}
			}

			Debug.Log ("REAL CELLS: " + realCells);

			for (int i = 0; i < dim; i++) {
				for (int j = 0; j < dim; j++) {
					Face[] faces = new Face[4];

					faces [0] = new Face (0);
					if (j + 1 < dim && grid[i, j+1] != null) {
						faces [0].cell = grid [i, j + 1];
					}

					faces [1] = new Face (1);
					if (i + 1 < dim && grid[i+1, j] != null) {
						faces [1].cell = grid [i + 1, j];
					}
						
					faces [2] = new Face (2);
					if (j > 0 && grid[i, j-1] != null) {
						faces [2].cell = grid [i, j - 1];
					}
				

					faces [3] = new Face (3);
					if (i > 0 && grid[i-1, j] != null) {
						faces [3].cell = grid [i - 1, j];
					}



					if (grid [i, j] != null) {
						grid [i, j].faces = faces;
						grid [i, j].reset ();
					}
				}
			}
		}

		public bool isFloor(DungeonGeneration.Cell cell){
			return cell is DungeonGeneration.FloorCell;
		}

		private void resetGrid(){
			for (int i = 0; i < dim; i++) {
				for (int j = 0; j < dim; j++) {
					if (grid [i, j] != null) {
						grid [i, j].reset ();
					}
				}
			}
		}

		public void assignAgents(List<SimObject> simObjects){
			List<Cell> affectedCells = new List<Cell>(); // all cells who have an agent denstiy contribution

			foreach(SimObject simObject in simObjects){
				int[] index = helper.getCellIndex (simObject.getPosition());
				Cell leftCell = helper.accessGridCell(index);

				if (leftCell != null && leftCell.exists) {
					float deltaX = simObject.getPosition().x - leftCell.position.x;
					float deltaY = simObject.getPosition().y - leftCell.position.y;

					// D --- C
					// |     |
					// A --- B

					// add density contribution to neighbouring cell
					float leftDensity = Mathf.Pow (Mathf.Min (1 - deltaX, 1 - deltaY), densityExp);
					// add average velocity contribution
					leftCell.density += leftDensity;
					leftCell.avgVelocity += leftDensity * simObject.velocity; // cell A
					affectedCells.Add(leftCell);

					Cell bCell = helper.accessGridCell (new int[]{ index [0] + 1, index [1] });
					if (bCell != null) {
						float bDensity = Mathf.Pow (Mathf.Min (deltaX, 1 - deltaY), densityExp);
						bCell.density += bDensity;
						bCell.avgVelocity += bDensity * simObject.velocity;
						affectedCells.Add (bCell);
					}

					Cell cCell = helper.accessGridCell (new int[]{ index [0] + 1, index [1] +1 });
					if (cCell != null) {
						float cDensity = Mathf.Pow (Mathf.Min (deltaX,deltaY), densityExp);
						cCell.density += cDensity;
						cCell.avgVelocity += cDensity * simObject.velocity;
						affectedCells.Add (cCell);
					}

					Cell dCell = helper.accessGridCell (new int[]{ index [0], index [1] + 1 });
					if (dCell != null) {
						float dDensity = Mathf.Pow (Mathf.Min (1 - deltaX, deltaY), densityExp);
						dCell.density += dDensity;
						dCell.avgVelocity += dDensity * simObject.velocity;
						affectedCells.Add (dCell);
					}
				}
			}
			// calculate average velocity

			foreach (Cell cell in affectedCells) {
				cell.avgVelocity = cell.avgVelocity / cell.density;
			}
		}

		private void assignCosts(){
			for (int i = 0; i < dim; i++) {
				for (int j = 0; j < dim; j++) {
					if (grid [i, j] != null) {
						for (int f = 0; f < grid [i, j].faces.Length; f++) {
							Cell cell = grid [i, j];
							Face face = cell.faces [f];

							if (face.cell == null || face.cell.exists == false) {
								face.cost = float.MaxValue;
							} else {
								if (face.velocity == 0) {
									face.cost = float.MaxValue;
								} else {
									face.cost = (distanceWeight * face.velocity + timeWeight + discomfortWeight) / face.velocity;
								}
							}
						}
					}
				}
			}
		}

		private void assignSpeedField(){
			for (int i = 0; i < dim; i++) {
				for (int j = 0; j < dim; j++) {

					if (grid [i, j] != null) {
						for (int f = 0; f < grid [i, j].faces.Length; f++) {
							Face face = grid [i, j].faces [f];
							if (face.cell == null || face.cell.exists == false) {
								face.velocity = 0;
							} else {
								
								if (grid [i, j].density < minDensity) {
									face.velocity = topoSpeed (face);
								} else if (grid [i, j].density > maxDensity) {
									face.velocity = flowSpeed (grid[i,j],face, f);
								} else {
									if (face.cell.exists == false) {
										face.velocity = 0;
									} else {
										face.velocity = topoSpeed (face) + ((face.cell.density - minDensity) / (maxDensity - minDensity)) *
										(flowSpeed (grid[i,j],face, f) - topoSpeed (face));
									}
								}
							}
						}
					}
				}
			}
		}

		private float topoSpeed(Face face){
			return maxVelocity; // is more complicated when considering height variations
		}

		private float flowSpeed(Cell cell, Face face, int dir){
			Cell neighbour = face.cell;
			Vector2 offset = neighbour.position - cell.position;

			return Mathf.Max(Vector2.Dot(neighbour.avgVelocity, offset),0.01f);
		
		
		}

		public virtual void update(){
			resetGrid ();
			//assignAgents (simObjects);
			assignSpeedField ();
			assignCosts ();
		}
	}
}

