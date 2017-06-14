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
		private List<Cell> goals;

		public bool trigger = false;
		public bool paused = false;
		float cellWidth;

		public bool updateField = true;

		int counter = 0;

		private GameObject groupParent;

		public int groupId = -1;

		public Color color;

		public GroupGrid (GameObject groupParent, Color color, float cellWidth, int dim, SharedGrid sharedGrid, DungeonGeneration.Cell[,] dungeon, int gridRatio) : base (cellWidth, dim, dungeon, gridRatio)
		{
			this.color = color;
			goals = new List<Cell> ();
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
			this.groupParent = groupParent;

			this.cellWidth = cellWidth;
			simObjects = new List<SimObject> ();

			helper = new Helper<Cell> (grid, cellWidth, gridRatio);

		}

		public void addGoal(Cell cell){
			if (!goals.Contains(cell)) {
				goals.Add (cell);
			}
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

		public new void  addAgent(SimObject simObject){
			simObjects.Add (simObject);
			simObject.sceneObject.transform.parent = groupParent.transform;
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

		private void assignPotentials ()
		{
			if (goals.Count == 0) {
				return; 
			}
			max = float.MinValue;
			SortedList<float, List<Cell>> candidates = new SortedList<float, List<Cell>> ();
			int accepted = 0;

			foreach (Cell goal in goals) {
				goal.isAccepted = true;
				goal.potential = 0.0f;
				//addNeighbours (candidates, goal);
				accepted++;
			}

			foreach (Cell goal in goals) {
				addNeighbours (candidates, goal);
			}
				
			while (accepted < realCells) { // total number of cells that are connected

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
				accepted++;
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
									face.potentialGrad = 0;
								}
							}
						}


//						if (faces [1].potentialGrad == 0 && faces [3].potentialGrad > 0) {
//							faces [3].potentialGrad = 0;
//						}
//
//						if (faces [3].potentialGrad == 0 && faces [1].potentialGrad > 0) {
//							faces [1].potentialGrad = 0;
//						}
//
//						if (faces [0].potentialGrad == 0 && faces [2].potentialGrad > 0) {
//							faces [2].potentialGrad = 0;
//						}
//
//						if (faces [2].potentialGrad == 0 && faces [0].potentialGrad > 0) {
//							faces [0].potentialGrad = 0;
//						}
							
						normaliseGrads (cell);
						calculateGroupVelocity (cell);
					}
				}
			}

//			if (trigger) {
//				for (int i = 0; i < dim; i++) {
//					for (int j = 0; j < dim; j++) {
//						Cell cell = grid [i, j];
//						if (cell != null) {
//
//							Vector2 totalVel = Vector2.zero;
//							Vector2 totalGrad = Vector2.zero;
//							int existFaces = 0;
//
//							foreach (Face face in cell.faces) {
//								if (face.cell != null && face.cell.potGrad != null) {
//									totalGrad += face.cell.potGrad;
//									totalVel += face.cell.groupVelocity;
//
//									existFaces++;
//								}
//							}
//							Vector2 newVel = totalVel / existFaces;
//							Vector2 newGrad = totalGrad / existFaces;
//							cell.potGrad = newGrad;
//							cell.groupVelocity = totalVel;
//																			
//						}
//					}
//				}
//			}
		}

		private void calculateGroupVelocity (Cell cell)
		{
			if (cell.potential.Equals (Vector2.zero)) {
				cell.groupVelocity = Vector2.zero;
			} else {
				Face[] faces = cell.faces;
				int[] index = cell.index;
				Face[] sharedFaces = sharedGrid.grid [index [0], index [1]].faces;


				if (cell.isGoal == false) {
					faces [0].groupVelocity = -faces [0].potentialGrad * sharedFaces [0].velocity;
					faces [1].groupVelocity = -faces [1].potentialGrad * sharedFaces [1].velocity;
					faces [2].groupVelocity = -faces [2].potentialGrad * sharedFaces [2].velocity;
					faces [3].groupVelocity = -faces [3].potentialGrad * sharedFaces [3].velocity;
				}
				cell.groupVelocity = new Vector2 (faces [1].groupVelocity - faces [3].groupVelocity, faces [0].groupVelocity - faces [2].groupVelocity);
			}
		}

		private void normaliseGrads (Cell cell)
		{

			Face north, east, south, west;
			north = cell.faces [0];
			east = cell.faces [1];
			south = cell.faces [2];
			west = cell.faces [3];
			float xGrad, yGrad;
			xGrad = east.potentialGrad - west.potentialGrad;
			yGrad = north.potentialGrad - south.potentialGrad;

			Vector2 newGrads = new Vector2 (xGrad, yGrad);
			newGrads.Normalize ();

			float xMul = 1.0f;
			float yMul = 1.0f;

			if (xGrad != 0.0f) {
				xMul = newGrads.x / xGrad;
			}

			if (yGrad != 0.0f) {
				yMul = newGrads.y / yGrad;
			}

			north.potentialGrad *= yMul;
			south.potentialGrad *= yMul;
			east.potentialGrad *= xMul;
			west.potentialGrad *= xMul;

			cell.potGrad = new Vector2 (east.potentialGrad - west.potentialGrad, north.potentialGrad - south.potentialGrad);

		}

		private void interpolateVelocities ()
		{

			foreach (SimObject simObject in simObjects) {
				int[] index = helper.getLeft (simObject.getPosition ());
				Cell leftCell = helper.accessGridCell (index);

				if (leftCell == null) {
					return;
				}
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

				float u = deltaX / cellWidth;
				float v = deltaY / cellWidth;

				Vector2 interp2V1 = Vector2.zero;
				Vector2 interp2V2 = Vector2.zero;
				bool doInterp2 = false;

				Vector2 total = Vector2.zero;

				if (zeroVec (aVel) && zeroVec (dVel)) {
					interp2V1 = bVel;
					interp2V2 = cVel;
					u = deltaY / cellWidth;
					doInterp2 = true;
				} else if (zeroVec (cVel) && zeroVec (bVel)) {
					interp2V1 = aVel;
					interp2V2 = dVel;
					u = deltaY / cellWidth;
					doInterp2 = true;
				} else if (zeroVec (aVel) && zeroVec (bVel)) {
					interp2V1 = dVel;
					interp2V1 = cVel;
					u = deltaX / cellWidth;
					doInterp2 = true;
				} else if (zeroVec (cVel) && zeroVec (dVel)) {

					interp2V1 = aVel;
					interp2V2 = bVel;
					u = deltaX / cellWidth;
					doInterp2 = true;
				}
//
				else if (zeroVec (cVel)) {
					Vector2 interpX = interp2 (aVel, bVel, u); 
					total = interpX;//interp2 (interpX, dVel, v);
				} else if (zeroVec (dVel)) {
					Vector2 interpX = interp2 (aVel, bVel, u);
					total = interpX;//interp2 (interpX, cVel, v);
				} else if (zeroVec (aVel)) {
					Vector2 interpX = interp2 (cVel, dVel, u);
					total = interpX;//interp2 (interpX, bVel, v);
				} else if (zeroVec (bVel)) {
					Vector2 interpX = interp2 (cVel, dVel, u);
					total = interpX;//interp2 (interpX, aVel, v);
				}

				if (total != Vector2.zero) {
					simObject.velocity = total;
				} else 

				if (doInterp2) {
					simObject.velocity = interp2 (interp2V1, interp2V2, u);
				
				} else {
					// 4 point interpolation
					Vector2 dcx = Vector2.Lerp (dVel, cVel, deltaX / cellWidth);
					Vector2 abx = Vector2.Lerp (aVel, bVel, deltaX / cellWidth);
					Vector2 interp = Vector2.Lerp (dcx, abx, deltaY / cellWidth);

					simObject.velocity = 1.0f * interp;
				}

				simObject.applyVelocity (simObject.velocity);
			}
		}

		public Vector2 interp2 (Vector2 v1, Vector2 v2, float u)
		{
			Vector2 interp = Vector2.Lerp (v1, v2, u);
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
			Face horUp = upwindFace (cell.faces [1], cell.faces [3], cell.faces [1].cell, cell.faces [3].cell);
			Face vertUp = upwindFace (cell.faces [0], cell.faces [2], cell.faces [0].cell, cell.faces [2].cell);
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


		public override void update (float time)
		{

			if (updateField) {
				assignPotentials ();
				setPotGrads ();
			}

			if (paused == false) {
				interpolateVelocities ();
			}

			counter++;
		}
	}
}

