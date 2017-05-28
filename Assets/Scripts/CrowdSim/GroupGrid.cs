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

		public GroupGrid (float cellWidth, int dim, SharedGrid sharedGrid) : base (cellWidth, dim)
		{
			this.sharedGrid = sharedGrid;
		}

		private void assignPotentials ()
		{
			max = float.MinValue;
			SortedList<float, List<Cell>> candidates = new SortedList<float, List<Cell>> ();
			int accepted = 0;
			for (int i = 0; i < dim; i++) {
				for (int j = 0; j < dim; j++) {
					if (grid [i, j].isGoal) {
						grid [i, j].potential = 0;
						addNeighbours (candidates, grid [i, j]);
						accepted++;
					} else {
						grid [i, j].potential = float.MaxValue;
					}
				}
			}
				
			if (accepted == 0) {
				return; // no goals
			}

			while (accepted < dim * dim) { // total number of cells that are connected
				Cell minCandidate = null;
			
				foreach (float key in candidates.Keys) {
					minCandidate = candidates [key][0];
					break;
				}

				removeCell (candidates, minCandidate, minCandidate.potential);

				//Debug.Log (minCandidate.index [0] + " " + minCandidate.index [1] + " " + minCandidate.potential);

				max = Mathf.Max (max, minCandidate.potential);

				addNeighbours (candidates, minCandidate);
				accepted++;
			}
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
				if (neighbour != null && neighbour.isGoal == false) {
					float tempPotential = calculatePotential (neighbour);
					if (tempPotential <= neighbour.potential) {
					
						removeCell (candidates, neighbour, neighbour.potential);

						if (candidates.ContainsKey (tempPotential)) {
							candidates [tempPotential].Add (neighbour);
						} else {
							List<Cell> bucket = new List<Cell> ();
							bucket.Add (neighbour);
							candidates.Add (tempPotential, bucket);
						}
						cell.potential = tempPotential;
					}
				}
			}
		}

		private float calculatePotential (Cell cell)
		{
			Cell sharedCell = sharedGrid.grid [cell.index [0], cell.index [1]];
			Face horUp = upwindFace (sharedCell.faces [1], sharedCell.faces [3], cell.faces[1].cell, cell.faces[3].cell);
			Face vertUp = upwindFace (sharedCell.faces [0], sharedCell.faces [3], cell.faces[0].cell, cell.faces[3].cell);

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

		private Face upwindFace (Face face1, Face face2, Cell cell1, Cell cell2)
		{
			Cell neighbour1 = cell1;
			Cell neighbour2 = cell2;

			if (neighbour1 == null && neighbour2 == null) {
				return null;
			} else if (neighbour1 == null) {
				return face2;
			} else if (neighbour2 == null) {
				return face1;
			} else {

				//TODO: THE POTENTIALS ARE ALWAYS NULL
				Debug.Log (neighbour1.potential + " + " + face1.cost);
				Debug.Log (neighbour2.potential + " + " + face2.cost);

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
			Debug.Log (potential + " " + face.cost);
			return Mathf.Max (face.cost + potential, face.cost - potential);
		}

		public float doubleDif (Face face1, Face face2)
		{
			float pot1 = grid [face1.cell.index [0], face1.cell.index [1]].potential;
			float pot2 = grid [face2.cell.index [0], face2.cell.index [1]].potential;

			float a = face1.cost * face1.cost + face2.cost * face2.cost;
			float b = -2 * (pot1 * face2.cost * face2.cost - pot2 * face1.cost * face1.cost);
			float c = face1.cost * face1.cost * pot2 * pot2 + face2.cost * face2.cost * pot1 * pot1 -
			          face1.cost * face1.cost * face2.cost * face2.cost;

			Debug.Log (a + " " + b + " " + c);
			Debug.Log (face1.cost + " " + face2.cost + " " + pot1 + " " + pot2);


			float underRoot = b * b - 4 * a * c;
			if (underRoot < 0) {
				return float.MaxValue;
			}

			if (a == 0) {
				return float.MaxValue;
			}
				
			return Mathf.Max ((-b + 2 * Mathf.Sqrt (underRoot)) / 2 * a, (-b - 2 * Mathf.Sqrt (underRoot)) / 2 * a); 
		}

		public float getMax(){
			return max;
		}
			
		public override void update(){
			 assignPotentials();
		}
	}
}

