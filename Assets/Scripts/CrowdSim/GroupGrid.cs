using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Primitives;
using Utilities;

namespace CrowdSim
{
	public class GroupGrid : SharedGrid
	{
		private SharedGrid sharedGrid;
		private float max = float.MinValue;

		private Helper<Cell> helper;


		public GroupGrid (float cellWidth, int dim, SharedGrid sharedGrid, DungeonGeneration.Cell[,] dungeon) : base (cellWidth, dim, dungeon)
		{
			this.sharedGrid = sharedGrid;
			for (int i = 0; i < dim; i++) {
				for (int j = 0; j < dim; j++) {
					if (grid [i, j] != null) {
						grid [i, j].sharedCell = sharedGrid.grid [i, j];
					}
				}
			}

			helper = new Helper<Cell> (grid, cellWidth);

		}

		private void assignPotentials ()
		{
			max = float.MinValue;
			SortedList<float, List<Cell>> candidates = new SortedList<float, List<Cell>> ();
			List<Cell> accepted = new List<Cell> ();
			for (int i = 0; i < dim; i++) {
				for (int j = 0; j < dim; j++) {
					if (grid [i, j] != null) {
						if (grid [i, j].isGoal) {
							grid [i, j].potential = 0;
							grid [i, j].isAccepted = true;
							accepted.Add (grid [i, j]);
							addNeighbours (candidates, grid [i, j]);
						} else {
							grid [i, j].potential = float.MaxValue;
							grid [i, j].isAccepted = false;
						}
					}
				}
			}

			foreach (Cell cell in accepted) {
				addNeighbours (candidates, cell);
			}

			if (accepted.Count == 0) {
				return; // no goals
			}

			while (accepted.Count < realCells) { // total number of cells that are connected

				Cell minCandidate = null;

				foreach (KeyValuePair<float, List<Cell>> pair in candidates) {
					minCandidate = pair.Value [0];
					break;
				}

				if (minCandidate == null) {
					return;
				}

				removeCell (candidates, minCandidate, minCandidate.potential);
				minCandidate.isAccepted = true;

				max = Mathf.Max (max, minCandidate.potential);

				addNeighbours (candidates, minCandidate);
				accepted.Add (minCandidate);

				//calculate gradients

				// only the upwind direction has associated velocity
				Face[] upwinds = getUpwinds (minCandidate);
				for (int i = 0; i < upwinds.GetLength (0); i++) {
					if (upwinds [i].cell != null) {
						upwinds [i].potentialGrad = upwinds [i].cell.potential - minCandidate.potential;
					}
				}
		
				// normalize gradients and update velocties
				Vector2 potGrad = new Vector2 (upwinds [0].potentialGrad, upwinds [1].potentialGrad);
				upwinds [0].potentialGrad = potGrad.x;
				upwinds [1].potentialGrad = potGrad.y;

				upwinds [0].groupVelocity = -upwinds [0].potentialGrad * minCandidate.sharedCell.faces [upwinds [0].index].velocity;
				upwinds [1].groupVelocity = -upwinds [1].potentialGrad * minCandidate.sharedCell.faces [upwinds [1].index].velocity;
			}

			//Debug.Log ("DONE");
		}

		private void interpolateVelocities (List<SimObject> simObjects)
		{
			foreach (SimObject simObject in simObjects) {
				int[] index = helper.getCellIndex (simObject.position);
				Cell leftCell = helper.accessGridCell (index);

				// interpolate center of each surrounding cell

				if (index [0] + 1 < dim && index [1] + 1 < dim && grid [index [0] + 1, index [1] + 1] != null) {
					// simple case for grid
					// a ----- b
					// |       |
					// d ----- d

					Vector2 aVel = getCenterVelocity(grid[index[0], index[1] + 1]);
						
				}

				// interpolate from neighbouring centers
			}
		}

		private Vector2 getCenterVelocity(Cell cell){
			Face[] faces = cell.faces;
			float northVel = faces[0].groupVelocity;
			if (northVel == 0 && faces[0].cell != null) {
				northVel = faces [0].cell.faces [2].groupVelocity;
			} 

			float southVel = faces[2].groupVelocity;
			if (southVel == 0 && faces[2].cell != null) {
				southVel = faces [2].cell.faces [0].groupVelocity;
			} 

			float eastVel = faces[1].groupVelocity;
			if (eastVel == 0 && faces[1].cell != null) {
				eastVel = faces [1].cell.faces [3].groupVelocity;
			} 

			float westVel = faces[3].groupVelocity;
			if (westVel == 0 && faces[3].cell != null) {
				westVel = faces [3].cell.faces [1].groupVelocity;
			} 

			Vector2 velocity = new Vector2 ();
			velocity.x = eastVel - westVel;
			velocity.y = northVel - southVel;

			return velocity;
		}

		private void removeCell (SortedList<float, List<Cell>> candidates, Cell cell, float oldKey)
		{
			if (candidates.ContainsKey (oldKey)) {
				List<Cell> bucket = candidates [oldKey];
				bucket.Remove (cell);
				if (bucket.Count == 0) {
					candidates.Remove (oldKey);
				}
			}
		}

		private void addNeighbours (SortedList<float, List<Cell>> candidates, Cell cell)
		{
			foreach (Face face in cell.faces) {
				Cell neighbour = face.cell;
				if (neighbour != null && neighbour.isAccepted == false) {
					float tempPotential = calculatePotential (neighbour);

					if (tempPotential < neighbour.potential) {

						removeCell (candidates, neighbour, neighbour.potential);

						if (candidates.ContainsKey (tempPotential)) {
							candidates [tempPotential].Add (neighbour);
						} else {
							List<Cell> bucket = new List<Cell> ();
							bucket.Add (neighbour);
							candidates.Add (tempPotential, bucket);
						}
						neighbour.potential = tempPotential;
					}
				}
			}
		}


		private float calculatePotential (Cell cell)
		{
			Cell sharedCell = cell.sharedCell;

			if (sharedCell == null) {
				return float.MaxValue; // not ideal, shouldn't be happening
			}

			Face[] upwinds = getUpwinds (cell);
			Face horUp = upwinds [0];
			Face vertUp = upwinds [1];

			if (horUp == null && vertUp == null) {
				return float.MaxValue; 
			} else if (horUp == null) {
				return singleDif (vertUp);
			} else if (vertUp == null) {
				return singleDif (horUp);
			} else {
				return doubleDif (horUp, vertUp);
			}
		}

		private Face[] getUpwinds (Cell cell)
		{
			Cell sharedCell = cell.sharedCell;
			Face horUp = upwindFace (sharedCell.faces [1], sharedCell.faces [3], cell.faces [1].cell, cell.faces [3].cell);
			Face vertUp = upwindFace (sharedCell.faces [0], sharedCell.faces [2], cell.faces [0].cell, cell.faces [2].cell);
			return new Face[]{ horUp, vertUp };
		}

		private Face upwindFace (Face face1, Face face2, Cell neighbour1, Cell neighbour2)
		{
			
			if ((neighbour1 == null || neighbour1.potential == float.MaxValue) &&
			    (neighbour2 == null || neighbour2.potential == float.MaxValue)) {
				return null;
			} else if (neighbour1 == null) {
				return face2;
			} else if (neighbour2 == null) {
				return face1;
			} else {
				float totalCost1 = neighbour1.potential + face1.cost;
				float totalCost2 = neighbour2.potential + face2.cost;

				if (totalCost1 < totalCost2) {
					return face1;
				} else {
					return face2;
				}
			}
		}

		public float singleDif (Face face)
		{
			float potential = grid [face.cell.index [0], face.cell.index [1]].potential;
			return Mathf.Max (face.cost + potential, face.cost - potential);
		}

		public float doubleDif (Face face1, Face face2)
		{	
			float pot1 = grid [face1.cell.index [0], face1.cell.index [1]].potential;

			float pot2 = grid [face2.cell.index [0], face2.cell.index [1]].potential;
		
			float a = face1.cost * face1.cost + face2.cost * face2.cost;
			float b = -2 * (pot1 * face2.cost * face2.cost + pot2 * face1.cost * face1.cost);
			float c = face1.cost * face1.cost * pot2 * pot2 + face2.cost * face2.cost * pot1 * pot1 -
			          face1.cost * face1.cost * face2.cost * face2.cost;
			
			float underRoot = b * b - 4 * a * c;
			if (underRoot < 0) {
				if (pot1 > pot2) {
					return singleDif (face1);
				} else {
					return singleDif (face2);
				}
			}

			if (a == 0) {
				return float.MaxValue;
			}
				
			return Mathf.Max ((-b + Mathf.Sqrt (underRoot)) / (2 * a), (-b - Mathf.Sqrt (underRoot)) / (2 * a)); 
		}

		public float getMax ()
		{
			return max;
		}

		public override void update ()
		{
			assignPotentials ();
		}
	}
}

