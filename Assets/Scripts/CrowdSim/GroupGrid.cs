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

		public List<SimObject> simObjects;

		public bool trigger = false;
		float cellWidth;

		public bool updateField = true;

		public GroupGrid (float cellWidth, int dim, SharedGrid sharedGrid, DungeonGeneration.Cell[,] dungeon, int gridRatio) : base (cellWidth, dim, dungeon, gridRatio)
		{
			dim = sharedGrid.grid.GetLength (0);
			Debug.Log ("HI: " + dim + " " + grid.GetLength (0) + " ");
			this.sharedGrid = sharedGrid;
			for (int i = 0; i < dim; i++) {
				for (int j = 0; j < dim; j++) {
					if (grid [i, j] != null) {
						grid [i, j].sharedCell = sharedGrid.grid [i, j];
					}
				}
			}

			this.cellWidth = cellWidth;
			simObjects = new List<SimObject> ();

			helper = new Helper<Cell> (grid, cellWidth, gridRatio);

		}

		private void assignPotentials ()
		{
			max = float.MinValue;
			SortedList<float, List<Cell>> candidates = new SortedList<float, List<Cell>> ();
			List<Cell> accepted = new List<Cell> ();
			for (int i = 0; i < dim; i++) {
				for (int j = 0; j < dim; j++) {
					if (grid [i, j] != null) {
						grid [i, j].reset ();
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

//				// only the upwind direction has associated velocity
//				Face[] upwinds = getUpwinds (minCandidate);
//				for (int i = 0; i < upwinds.GetLength (0); i++) {
//					if (upwinds != null) {
//						upwinds [i] = minCandidate.faces [upwinds [i].index];
//						if (upwinds [i] != null && upwinds [i].cell != null) {
//							upwinds [i].potentialGrad = upwinds [i].cell.potential - minCandidate.potential;
//						} else if (upwinds [i] != null && upwinds [i].cell.exists == false) {
//							upwinds [i].potentialGrad = float.MaxValue;
//						}
//					}
//				}
		

//				// normalize gradients and update velocties
//				Vector2 potGrad = new Vector2 (0, 0);
//				if (upwinds [0] != null) {
//					potGrad.x = upwinds [0].potentialGrad;
//				}
//
//				if (upwinds [1] != null) {
//					potGrad.y = upwinds [1].potentialGrad;
//				}
//				potGrad.Normalize ();
//
//				if (upwinds [0] != null) {
//					upwinds [0].potentialGrad = potGrad.x;
//					upwinds [0].groupVelocity = -upwinds [0].potentialGrad * minCandidate.sharedCell.faces [upwinds [0].index].velocity;
//				} else if (upwinds [0].cell.exists == false) {
//					upwinds [0].groupVelocity = 0f;
//				}
//
//				if (upwinds [1] != null) {
//					upwinds [1].potentialGrad = potGrad.y;
//					upwinds [1].groupVelocity = -upwinds [1].potentialGrad * minCandidate.sharedCell.faces [upwinds [1].index].velocity;
//				} else if (upwinds [1].cell.exists == false) {
//					upwinds [1].groupVelocity = 0f;
//				}
//				minCandidate.potGrad = potGrad;
//				minCandidate.groupVelocity = getCenterVelocity (minCandidate).normalized;
			}
		}

		private void setPotGrads ()
		{
			for (int i = 0; i < dim; i++) {
				for (int j = 0; j < dim; j++) {
					Cell cell = grid [i, j];
					if (cell != null) {
						Face[] faces = cell.faces;
						foreach (Face face in faces) {
							if (face != null) {
								if (face.cell != null && face.cell.exists) {
									face.potentialGrad = face.cell.potential - cell.potential;
								} else {
									face.potentialGrad = 0.0f;
								}
							}
						}

						if (faces [1].potentialGrad == 0 && faces [3].potentialGrad > 0) {
							faces [1].potentialGrad = faces [3].potentialGrad;
						}

						if (faces [3].potentialGrad == 0 && faces [1].potentialGrad > 0) {
							faces [3].potentialGrad = faces [1].potentialGrad;
						}

						if (faces [0].potentialGrad == 0 && faces [2].potentialGrad > 0) {
							faces [0].potentialGrad = faces [2].potentialGrad;
						}

						if (faces [2].potentialGrad == 0 && faces [0].potentialGrad > 0) {
							faces [2].potentialGrad = faces [0].potentialGrad;
						}

						cell.potGrad = new Vector2 (faces [1].potentialGrad - faces [3].potentialGrad, faces [0].potentialGrad - faces [2].potentialGrad);
						cell.potGrad = cell.potGrad.normalized;
						cell.groupVelocity = -cell.potGrad;
					}
				}
			}

			if (trigger) {
				for (int i = 0; i < dim; i++) {
					for (int j = 0; j < dim; j++) {
						Cell cell = grid [i, j];
						if (cell != null) {
							float totalXGrad = 0f;
							float totalYGrad = 0f;
							int existFaces = 0;
							foreach (Face face in cell.faces) {
								if (face.cell != null && face.cell.potGrad != null) {
									totalXGrad += face.cell.potGrad.x;
									totalYGrad += face.cell.potGrad.y;
									existFaces++;
								}
							}
							Vector2 newGrad = new Vector2 (totalXGrad / existFaces, totalYGrad / existFaces);
							cell.potGrad = newGrad.normalized;

							cell.groupVelocity = -cell.potGrad;
						}
					}
				}
			}
		}

		private void interpolateVelocities ()
		{

			foreach (SimObject simObject in simObjects) {
				int[] index = helper.getLeft (simObject.getPosition ());
				Cell leftCell = helper.accessGridCell (index);

				//simObject.velocity = leftCell.groupVelocity;

				// interpolate center of each surrounding cell

				float deltaX = simObject.getPosition ().x - leftCell.position.x;
				float deltaY = simObject.getPosition ().y - leftCell.position.y;

				// d ----- c
				// |       |
				// a ----- b

				Vector2 dVel = getCenterVelocity (grid [index [0], index [1] + 1]);
				Vector2 cVel = getCenterVelocity (grid [index [0] + 1, index [1] + 1]);
				Vector2 bVel = getCenterVelocity (grid [index [0] + 1, index [1]]);
				Vector2 aVel = getCenterVelocity (leftCell);

				Vector2 velocity = new Vector2 (0f, 0f);
				Vector2 cPos = grid [index [0] + 1, index [1] + 1].position;
				Vector2 lPos = leftCell.position;
				Vector2 pos = simObject.getPosition ();

				if (zeroVec (aVel) && zeroVec (dVel)) {
					aVel = bVel;
					dVel = cVel;
				} else if (zeroVec (cVel) && zeroVec (bVel)) {
					cVel = dVel;
					aVel = bVel;
				} 

				else if (zeroVec (aVel) && zeroVec (bVel)) {
					aVel = dVel;
					bVel = cVel;
				} else if (zeroVec (cVel) && zeroVec (dVel)) {
					cVel = bVel;
					dVel = aVel;
				} 
					
				else if (zeroVec (dVel)) {
					dVel = bVel;
				} else if (zeroVec (cVel)) {
					cVel = aVel;
				} else if (zeroVec (aVel)) {
					aVel = cVel;
				} else if (zeroVec (dVel)) {
					dVel = bVel;
				} 

				Vector2 dcx = Vector2.Lerp (dVel, cVel, deltaX / cellWidth);
				Vector2 abx = Vector2.Lerp (aVel, bVel, deltaX / cellWidth);
				Vector2 interp = Vector2.Lerp (dcx, abx, deltaY / cellWidth);


				simObject.velocity = 1.0f * interp;
			
				simObject.applyVelocity (simObject.velocity);
			}
		}

		private Vector2 interp2 (float x, float x1, float x2, Vector2 v1, Vector2 v2)
		{
			Vector2 interp = v1 * ((x2 - x) / (x2 - x1)) + v2 * ((x - x1) / (x2 - x1));
			return interp;
		}

		private bool zeroVec (Vector2 v)
		{
			return v.Equals (Vector2.zero);
		}

		private Vector2 getCenterVelocity (Cell cell)
		{
			if (cell != null) {
				return cell.groupVelocity;
			} else {
				return new Vector2 (0f, 0f);
			}
//			if (cell != null) {
//				Face[] faces = cell.faces;
//
//				Vector2 velocity = new Vector2 (0, 0);
//				if (faces [1] != null) {
//					velocity.x = faces [1].groupVelocity;
//				}
//
//				if (faces [3] != null) {
//					velocity.x = velocity.x - faces [3].groupVelocity;
//				}
//
//				if (faces [0] != null) {
//					velocity.y = faces [0].groupVelocity;
//				}
//
//				if (faces [2] != null) {
//					velocity.y = velocity.y - faces [2].groupVelocity;
//				}
//	
//				return velocity;
//			} else {
//				return new Vector2 (0f, 0f);
//			}
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
				if (neighbour != null && neighbour.isAccepted == false && neighbour.exists) {
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
				return 0f; 
			} else if (horUp == null) {
				return singleDif (vertUp);
			} else if (vertUp == null) {
				return singleDif (horUp);
			} else {
				return doubleDif (horUp, vertUp, cell);
			}
		}

		private Face[] getUpwinds (Cell cell)
		{
			Cell sharedCell = cell.sharedCell;
			Face horUp = upwindFace (sharedCell.faces [1], sharedCell.faces [3], cell.faces [1].cell, cell.faces [3].cell);
			Face vertUp = upwindFace (sharedCell.faces [0], sharedCell.faces [2], cell.faces [0].cell, cell.faces [2].cell);
			return new Face[]{ horUp, vertUp };
		}

		//Returns the shared upwind face
		private Face upwindFace (Face face1, Face face2, Cell neighbour1, Cell neighbour2)
		{
			
			if ((neighbour1 == null || neighbour1.potential == float.MaxValue || !neighbour1.exists) &&
			    (neighbour2 == null || neighbour2.potential == float.MaxValue || !neighbour2.exists)) {
				return null;
			} else if (neighbour1 == null || neighbour1.potential == float.MaxValue || !neighbour1.exists) {
				return face2;
			} else if (neighbour2 == null || neighbour2.potential == float.MaxValue || !neighbour2.exists) {
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

		public float doubleDif (Face face1, Face face2, Cell cell)
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
					return singleDif (face2);
				} else {
					return singleDif (face1);
				}
			}


				
			if (a == 0) {
				return float.MaxValue;
			}
				
			float doubleDif = Mathf.Max ((-b + Mathf.Sqrt (underRoot)) / (2 * a), (-b - Mathf.Sqrt (underRoot)) / (2 * a));

			foreach (Face face in cell.faces) {
				if (cell.isAccepted) {
					if (doubleDif <= cell.potential) {
						if (pot1 > pot2) {
							return singleDif (face2);
						} else {
							return singleDif (face1);
						}
					}
				}
			}
				
			

			return doubleDif;
		}

		public float getMax ()
		{
			return max;
		}

		public override void update ()
		{
			if (updateField) {
				assignPotentials ();
				setPotGrads ();
			}
			interpolateVelocities ();
		}
	}
}

