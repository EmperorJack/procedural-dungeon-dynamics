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

			}

			//Debug.Log ("DONE");
		}

		private void removeCell(SortedList<float, List<Cell>> candidates, Cell cell, float oldKey){
			if (candidates.ContainsKey (oldKey)) {
				List<Cell> bucket = candidates [oldKey];
				bucket.Remove (cell);
				if (bucket.Count == 0) {
					candidates.Remove (oldKey);
				}
			}
		}

		private void addNeighbours(SortedList<float, List<Cell>> candidates, Cell cell){
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

			Face horUp = upwindFace (sharedCell.faces [1],
				sharedCell.faces [3], 
				cell.faces [1].cell, 
				cell.faces [3].cell);
			Face vertUp = upwindFace (sharedCell.faces [0], sharedCell.faces [2], cell.faces [0].cell, cell.faces [2].cell);

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

