using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Primitives;
using Utilities;

namespace CrowdSim
{
	public class SharedGrid : MonoBehaviour
	{
		public Cell[,] grid;
		float cellWidth;
		public int dim;

		private Helper<Cell> helper;

		// 'constant' values
		float densityExp = 0.5f;
		// 0 (spread out) -> 10 (form lines)
		public float maxCalcDensity = 0f;
		public float minDensity = 2.0f;
		public float maxDensity = 5.0f;
		public float maxVelocity = 1.0f;
		public float distanceWeight = 0.5f;
		public float timeWeight = 0.5f;
		public float discomfortWeight = 1.0f;

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
				this.dim = dungeon.GetLength (0) * ratio;
			}

			Debug.Log ("DIM: " + this.dim);

			grid = new Cell[this.dim, this.dim];
			helper = new Helper<Cell> (grid, cellWidth, ratio);
			this.dungeon = dungeon;

			initGrid ();

		}

		public void addAgent (SimObject simObject)
		{
			simObjects.Add (simObject);
		}

		public void removeAgent(SimObject simObject){
			simObjects.Remove (simObject);
		}

		private void initGrid ()
		{

			for (int i = 0; i < dim; i++) {
				for (int j = 0; j < dim; j++) {
					if (customDungeon == false || (isFloor (dungeon [i / ratio, j / ratio]))) {
						grid [i, j] = new Cell (new int[]{ i, j });
						grid [i, j].position = new Vector2 (i * cellWidth - (ratio - 1) * cellWidth * 0.5f, j * cellWidth - (ratio - 1) * cellWidth * 0.5f);
						grid [i, j].exists = true;
						realCells++;
					} else if (customDungeon && isFloor (dungeon [i / ratio, j / ratio]) == false) {
						grid [i, j] = new Cell (new int[]{ i, j });
						grid [i, j].position = new Vector2 (i * cellWidth - (ratio - 1) * cellWidth * 0.5f, j * cellWidth - (ratio - 1) * cellWidth * 0.5f);
						grid [i, j].exists = false;
					}
				}
			}

			Debug.Log ("REAL CELLS: " + realCells);

			for (int i = 0; i < dim; i++) {
				for (int j = 0; j < dim; j++) {
					Face[] faces = new Face[4];

					faces [0] = new Face (0);
					if (j + 1 < dim && grid [i, j + 1] != null) {
						faces [0].cell = grid [i, j + 1];
					}

					faces [1] = new Face (1);
					if (i + 1 < dim && grid [i + 1, j] != null) {
						faces [1].cell = grid [i + 1, j];
					}
						
					faces [2] = new Face (2);
					if (j > 0 && grid [i, j - 1] != null) {
						faces [2].cell = grid [i, j - 1];
					}
				

					faces [3] = new Face (3);
					if (i > 0 && grid [i - 1, j] != null) {
						faces [3].cell = grid [i - 1, j];
					}



					if (grid [i, j] != null) {
						grid [i, j].faces = faces;
						grid [i, j].reset ();
					}
				}
			}
		}

		public bool isFloor (DungeonGeneration.Cell cell)
		{
			return cell is DungeonGeneration.FloorCell;
		}

		private void resetGrid ()
		{
			for (int i = 0; i < dim; i++) {
				for (int j = 0; j < dim; j++) {
					if (grid [i, j] != null) {
						grid [i, j].reset ();
					}
				}
			}
		}

		public void assignAgents (List<SimObject> simObjects, float time)
		{

			foreach (SimObject simObject in simObjects) {
				
				calculateDensity(simObject, simObject.getPosition(), true);

//				Vector2 newPos = simObject.getPosition () + time * simObject.velocity;
//				Cell newCell = helper.getCell (newPos);
//				newCell.discomfort += 0.5f;
//				float[] densityCont = calculateDensity(simObject, newPos, false);
//				int[] leftIndex = helper.getLeft (newPos);
//				int[] newIndex = helper.getCellIndex (newPos);
//				if (newIndex [0] == leftIndex [0] && newIndex [1] == leftIndex [1]) {
//					newCell.discomfort += densityCont [0];
//				} else if (newIndex [0] == leftIndex [0] + 1 && newIndex [1] == leftIndex [1]) {
//					newCell.discomfort += densityCont [1];
//				} else if (newIndex [0] == leftIndex [0] + 1 && newIndex [1] == leftIndex [1] + 1) {
//					newCell.discomfort += densityCont [2];
//				} else if (newIndex [0] == leftIndex [0] && newIndex [1] == leftIndex [1] + 1) {
//					newCell.discomfort += densityCont [3];
//				}

			}
			// calculate average velocity

			for (int i = 0; i < dim; i++) {
				for (int j = 0; j < dim; j++) {
					Cell cell = grid [i, j];
					if (cell.density > 0.0f) {
						cell.avgVelocity = cell.avgVelocity / cell.density;
					}
				}
			}
		}

		public void splatStaticObject(SimObject simObject, Cell cell){
			Rigidbody rb = simObject.getBody ();
			Vector3 cellPos = new Vector3 (cell.position.x, 0.0f, cell.position.y);
			Vector2 rbCenter = simObject.getPosition ();

			float dist = Mathf.Sqrt(Vector2.SqrMagnitude(cell.position - rbCenter));
			Vector2 closestPoint = new Vector2 (rb.ClosestPointOnBounds (cellPos).x, rb.ClosestPointOnBounds (cellPos).z);
			float closestDist = Mathf.Sqrt (Vector2.SqrMagnitude (cell.position - closestPoint));
			cell.discomfort += simObject.densityWeight * (1.0f - (closestDist/ dist));
		}

		public float[] calculateDensity (SimObject simObject, Vector2 pos, bool assign)
		{
			int[] index = helper.getLeft (simObject.getPosition ());
			Cell leftCell = helper.accessGridCell (index);
			int[] selectedIndex = helper.getCellIndex (pos);

			float[] densityCont = new float[4];

			if (simObject.moveable == false) {
				Cell center = helper.accessGridCell (index);
				splatStaticObject (simObject, center);
				foreach (Face face in center.faces) {
					if (face.cell != null && face.cell.exists) {
						splatStaticObject (simObject, face.cell);
					}
				}


				// get central grid cell
					// get surronding grid cells
					// add dWeight * (1 - sqrDistance(center)/(cellCenter - boxCenter));
			} else {

				if (leftCell != null) {
					float deltaX = (pos.x - leftCell.position.x) / cellWidth;
					float deltaY = (pos.y - leftCell.position.y) / cellWidth;

					// D --- C
					// |     |
					// A --- B


					// add density contribution to neighbouring cell
					float leftDensity = Mathf.Pow (Mathf.Min (1 - deltaX, 1 - deltaY), densityExp) * simObject.densityWeight;
					// add average velocity contribution
					if (assign) {
						leftCell.density += leftDensity;
						if (simObject.moveable) {
							leftCell.avgVelocity += leftDensity * simObject.velocity; // cell A
						}
					} else {
						densityCont [0] = leftDensity;
					}

					Cell bCell = helper.accessGridCell (new int[]{ index [0] + 1, index [1] });
					if (bCell != null && bCell.exists) {
						float bDensity = Mathf.Pow (Mathf.Min (deltaX, 1 - deltaY), densityExp) * simObject.densityWeight;
						if (assign) {
							bCell.density += bDensity;
							if (simObject.moveable) {
								bCell.avgVelocity += bDensity * simObject.velocity;
							}
						} else {
							densityCont [1] = bDensity;
						}
					}

					Cell cCell = helper.accessGridCell (new int[]{ index [0] + 1, index [1] + 1 });
					if (cCell != null && cCell.exists) {
						float cDensity = Mathf.Pow (Mathf.Min (deltaX, deltaY), densityExp) * simObject.densityWeight;
						if (assign) {
							cCell.density += cDensity;
							if (simObject.moveable) {
								cCell.avgVelocity += cDensity * simObject.velocity;
							}
						} else {
							densityCont [2] = cDensity;
						}
					}

					Cell dCell = helper.accessGridCell (new int[]{ index [0], index [1] + 1 });
					if (dCell != null && dCell.exists) {
						float dDensity = Mathf.Pow (Mathf.Min (1 - deltaX, deltaY), densityExp) * simObject.densityWeight;
						if (assign) {
							dCell.density += dDensity;
							if (simObject.moveable) {
								dCell.avgVelocity += dDensity * simObject.velocity;
							}
						} else {
							densityCont [3] = dDensity;
						}
					}

				}
			}
			return densityCont;

		}

		private void assignCosts ()
		{
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
									face.cost = (distanceWeight * face.velocity + timeWeight + discomfortWeight * face.cell.discomfort) / face.velocity;
								}
							}
						}
					}
				}
			}
		}

		private void assignSpeedField ()
		{
			for (int i = 0; i < dim; i++) {
				for (int j = 0; j < dim; j++) {

					if (grid [i, j] != null) {
						for (int f = 0; f < grid [i, j].faces.Length; f++) {
							Face face = grid [i, j].faces [f];
							if (face.cell == null || face.cell.exists == false) {
								face.velocity = 0;
							} else {

								float fS = flowSpeed (grid [i, j], face, f);
								float fT = topoSpeed (face);

								if (face.cell.density < minDensity) {
									face.velocity = fT;
								} else if (face.cell.density > maxDensity) {
									face.velocity = fS;
								} else {

									if (face.cell.exists == false) {
										face.velocity = 0;
									} else {
										float deltaP = maxDensity - minDensity;
										if (deltaP == 0) {
											face.velocity = fT;
										} else {
											face.velocity = fT + ((face.cell.density - minDensity) / (deltaP)) * (fT - fS);
										}
									}
								}
							}

						}
					}
				}
			}
		}

		private float topoSpeed (Face face)
		{
			return maxVelocity; // is more complicated when considering height variations
		}

		private float flowSpeed (Cell cell, Face face, int dir)
		{
			Cell neighbour = face.cell;

			Vector2 offset = neighbour.position - cell.position;
		
			return Mathf.Max (Vector2.Dot (neighbour.avgVelocity, offset), 0.01f);
		}

		public virtual void update (float time)
		{
			//resetGrid ();
			assignAgents (simObjects, time);
			assignSpeedField ();
			assignCosts ();
		}
	}
}

