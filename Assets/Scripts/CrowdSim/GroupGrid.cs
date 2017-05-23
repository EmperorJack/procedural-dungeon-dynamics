using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Primitives;

namespace CrowdSim
{
	public class GroupGrid : Grid
	{
		public float max_potential = float.MinValue;

		private SharedGrid shared_grid;
		private List<GameObject> agents;

		public GroupGrid (float cell_width, int dim, List<GameObject> agents, SharedGrid shared_grid) : base (cell_width, dim)
		{
			this.shared_grid = shared_grid;
			this.agents = agents;

			for (int i = 0; i < dim; i++) {
				for (int j = 0; j < dim; j++) {
					GroupCell cell = (GroupCell)grid [i, j];
					foreach (Face face in cell.faces) {
						if (face != null) {
							face.neighbour = grid [(int)face.neighbourIndex.x, (int)face.neighbourIndex.y];
						}
					}
				}
			}
		}

		public override void update ()
		{
			max_potential = float.MinValue;
			fastMarch ();
			//testMach();
			computeVelocity();
			foreach (GameObject agent in agents) {
				Rigidbody rb = agent.GetComponent<Rigidbody> ();
				Vector2 interpVel = interpolateVelocity (new Vector2 (rb.position.x, rb.position.z));
				//Printer.message ("INTERP: " + interpVel.x + " " + interpVel.y);

				// move agent
				rb.position = new Vector3(rb.position.x + interpVel.x, 0, rb.position.z + interpVel.y);
			}
		}

		public void testMach ()
		{

			max_potential = 10;
			for (int i = 0; i < dim; i++) {
				for (int j = 0; j < dim; j++) {
					GroupCell cell = (GroupCell)grid [i, j];
					cell.potential = 9f;

				}
			}
		}

		protected override void instantiateCell (int i, int j, Vector2 pos)
		{
			GroupCell cell = new GroupCell (pos, new Vector2 (i, j));
			grid [i, j] = cell;
			cellDic [new Vector2 ((pos.x), (pos.y))] = new Vector2 (i, j);

			if (i < dim - 1) {
				cell.setFace (GroupCell.Dir.east, new GroupFace (cell, new Vector2 (i + 1, j), (int)GroupCell.Dir.east));
			} 
			//add bottom face
			if (j < dim - 1) {
				cell.setFace (GroupCell.Dir.south, new GroupFace (cell, new Vector2 (i, j + 1), (int)GroupCell.Dir.south));
			} 
			//add left face
			if (i > 0) {
				cell.setFace (GroupCell.Dir.west, new GroupFace (cell, new Vector2 (i - 1, j), (int)GroupCell.Dir.west));
			} 
			//add top face
			if (j > 0) {
				cell.setFace (GroupCell.Dir.north, new GroupFace (cell, new Vector2 (i, j - 1), (int)GroupCell.Dir.north));
			}
		}

		private void fastMarch ()
		{
			List<GroupCell> known_cells = new List<GroupCell> ();
			List<GroupCell> candidates = new List<GroupCell> ();
		
			for (int i = 0; i < dim; i++) {
				for (int j = 0; j < dim; j++) {
					GroupCell cell = (GroupCell)grid [i, j];
					if (cell.isGoal) {
						cell.potential = 0f;
						cell.temporary_potential = 0f;
						known_cells.Add (cell);
					} else {
						cell.temporary_potential = float.MaxValue;
						cell.potential = float.MaxValue;
					}
				}
			}
		
			if (known_cells.Count > 0) {
		
				foreach (GroupCell knownCell in known_cells) {
					// add neighbours to candidates
					for (int f_index = 0; f_index < knownCell.faces.Length; f_index++) {
						GroupFace face = (GroupFace)knownCell.faces [f_index];
						if (face != null && face.neighbour != null) {
							GroupCell candidate = (GroupCell)face.neighbour;
							float aprox_potential = getCellPotential (candidate);
							candidate.temporary_potential = aprox_potential;
							candidates.Add (candidate);
						}
					}
				}
		
				while (known_cells.Count < dim * dim) {
		
					float min_potential = float.MaxValue;
					GroupCell min_candidate = null;
		
					foreach (GroupCell candidate in candidates) {
						if (candidate.temporary_potential <= min_potential) {
							//Printer.message ("TEMPPOT(" + candidate.index.x + ", " + candidate.index.y + ") = " + candidate.temporary_potential);
							min_potential = candidate.temporary_potential;
							min_candidate = candidate;
						}
					}
		
					// add candidate with min potential
					min_candidate.potential = min_candidate.temporary_potential;
					candidates.Remove (min_candidate);

					//Printer.message ("POT("+min_candidate.index.x+", "+min_candidate.index.y+") = "+min_candidate.potential);
					max_potential = Mathf.Max (max_potential, min_candidate.potential);
					known_cells.Add (min_candidate);
		
		
					// add candidates neighbours
					foreach (GroupFace face in min_candidate.faces) {
						if (face != null && face.neighbour != null) {
							GroupCell candidate = (GroupCell)face.neighbour;
							if (known_cells.Contains (candidate) == false) {
								//Printer.message ("ATTEMPTING: " + candidate.index.x + ", " + candidate.index.y);
								float aprox_potential = getCellPotential (candidate);
								// TODO: THIS IS ALWAYS FLOAT.MAX
								//Printer.message ("APROX("+candidate.index.x+", "+candidate.index.y+") = "+aprox_potential);
								candidate.temporary_potential = aprox_potential;
								if (candidates.Contains (candidate) == false) {
									candidates.Add (candidate);
								}
							}
						}
					}
				}

				// iterate through all to compute gradient 
				for (int i = 0; i < dim; i++) {
					for (int j = 0; j < dim; j++) {
						GroupCell cell = (GroupCell)grid [i, j];

						foreach (GroupFace face in cell.faces)
						{
							float grad = 0f;
							if (face != null && face.neighbour != null) {
								GroupCell neighbour = (GroupCell)face.neighbour;
								grad = neighbour.potential - cell.potential;
								face.grad_Potential = grad;
							}

						}
						normaliseGrads (cell);
					}
				}

			}
		}

		void normaliseGrads(GroupCell cell){
			GroupFace eastFace = (GroupFace)cell.faces [(int)GroupCell.Dir.east];
			GroupFace westFace = (GroupFace)cell.faces [(int)GroupCell.Dir.west];
			GroupFace northFace = (GroupFace)cell.faces [(int)GroupCell.Dir.north];
			GroupFace southFace = (GroupFace)cell.faces [(int)GroupCell.Dir.south];

			float xGrad = 0f;
			if (eastFace != null && westFace != null) {
				xGrad = eastFace.grad_Potential - westFace.grad_Potential;
			}

			float yGrad = 0f;
			if (northFace != null && southFace != null) {
				yGrad = northFace.grad_Potential - southFace.grad_Potential;
			}

			Vector2 newGrads = new Vector2 (xGrad, yGrad);
			newGrads.Normalize();

			float newXGrad = newGrads.x;
			float newYGrad = newGrads.y;

			float xRatio = 1f;
			float yRatio = 1f;

			if (xGrad != 0) {
				xRatio = newXGrad / xGrad;
			}

			if (yGrad != 0) {
				yRatio = newYGrad / yGrad;
			}

			if (eastFace != null) {
				eastFace.grad_Potential = eastFace.grad_Potential * xRatio;
			}
			if (westFace != null) {
				westFace.grad_Potential = westFace.grad_Potential * xRatio;
			}
			if (northFace != null) {
				northFace.grad_Potential = northFace.grad_Potential * yRatio;
			}
			if (southFace != null) {
				southFace.grad_Potential = southFace.grad_Potential * yRatio;
			}
		}

		float lerp(float t, float a, float b){
			return (1.0f - t) * a + t * b;
		}

		Vector2 getCenterVelocity(GroupCell cell){
			float northVel = 0;

			if (cell.faces [(int)GroupCell.Dir.north] != null) {
				northVel = cell.faces [(int)GroupCell.Dir.north].velocity;
			}

			float southVel = 0;

			if (cell.faces [(int)GroupCell.Dir.south] != null) {
				southVel = cell.faces [(int)GroupCell.Dir.south].velocity;
			}

			float eastVel = 0;

			if (cell.faces [(int)GroupCell.Dir.east] != null) {
				eastVel = cell.faces [(int)GroupCell.Dir.east].velocity;
			}

			float westVel = 0;

			if (cell.faces [(int)GroupCell.Dir.west] != null) {
				westVel = cell.faces [(int)GroupCell.Dir.west].velocity;
			}
				
			return new Vector2 (lerp (0.5f, southVel, northVel), lerp (0.5f, eastVel, westVel));
			
		}
		 
		Vector2 interpolateVelocity(Vector2 pos){
			// get cell left and down of pos
			
			// Velocity a,b,c,d = getCenterVelocity(a,b,c,d)
			Vector2 leftPos = getLeft(pos.x, pos.y);
			GroupCell leftCell = (GroupCell)grid2 [(int)leftPos.x, (int)leftPos.y];

			Vector2 c = new Vector2 (0, 0);//getCenterVelocity (leftCell);
			Vector2 d = new Vector2 (0, 0);
			Vector2 b = new Vector2 (0, 0);
			Vector2 a = new Vector2 (0, 0);

			if (leftCell != null) {
				c = getCenterVelocity (leftCell);
				d = getCenterVelocity ((GroupCell)grid2 [(int)leftCell.index.x + 1, (int)leftCell.index.y]);
				b = getCenterVelocity ((GroupCell)grid2 [(int)leftCell.index.x + 1, (int)leftCell.index.y + 1]);
				a = getCenterVelocity((GroupCell)grid2[(int)leftCell.index.x, (int)leftCell.index.y + 1]);
			}

			// A -- B
			// |    |
			// C -- D
			
			float t = (pos.x - leftCell.position.x) / cell_width;
			float cdX = (1f - t) * c.x + d.x * t;
			float abX = (1f - t) * a.x + b.x * t;
			float cdZ = (1f - t) * c.y + d.y * t;
			float abZ = (1f - t) * a.y + b.y * t;
			
			t = (pos.y - leftCell.position.y) / cell_width;
			float terpX = (1 - t) * cdX + t * abX;
			float terpY = (1 - t) * cdZ + t * abZ;
			return new Vector2(terpX, terpY);
		}

		void computeVelocity ()
		{
			foreach (GroupCell cell in grid) {
				normaliseGrads (cell);
				SharedCell sharedCell = (SharedCell)shared_grid.grid2[(int)cell.index.x, (int)cell.index.y];
				for (int i = 0; i < cell.faces.Length; i++) {
					Face sharedFace = sharedCell.faces [i];
					if (sharedFace != null) {
						float sharedVelocity = sharedFace.velocity;
						GroupFace groupFace = (GroupFace)cell.faces [i];
						groupFace.velocity = -sharedVelocity * groupFace.grad_Potential;
					} 
				}
				float[] dirVels = getDirVels (sharedCell);
				sharedCell.avg_Velocity = new Vector2 (dirVels [1] - dirVels [3], dirVels [0] - dirVels [2]);
			}
		}

		float[] getDirVels(Cell cell){
			float northVel = 0;

			if (cell.faces [(int)GroupCell.Dir.north] != null) {
				northVel = cell.faces [(int)GroupCell.Dir.north].velocity;
			}

			float southVel = 0;

			if (cell.faces [(int)GroupCell.Dir.south] != null) {
				southVel = cell.faces [(int)GroupCell.Dir.south].velocity;
			}

			float eastVel = 0;

			if (cell.faces [(int)GroupCell.Dir.east] != null) {
				eastVel = cell.faces [(int)GroupCell.Dir.east].velocity;
			}

			float westVel = 0;

			if (cell.faces [(int)GroupCell.Dir.west] != null) {
				westVel = cell.faces [(int)GroupCell.Dir.west].velocity;
			}

			return new float[]{ northVel, eastVel, southVel, westVel };
		}

		// Returns the upwind directions from this cell

		GroupFace[] getUpwind (GroupCell cell)
		{
			GroupFace min_x = null;
			GroupFace min_y = null;

			float east_cost = adjCost (Cell.Dir.east, cell);
			float west_cost = adjCost (Cell.Dir.west, cell);
			float north_cost = adjCost (Cell.Dir.north, cell);
			float south_cost = adjCost (Cell.Dir.south, cell);



			if (east_cost < west_cost) {
				min_x = (GroupFace)cell.faces [(int)Cell.Dir.east];
			} else if (west_cost < east_cost) {
				min_x = (GroupFace)cell.faces [(int)Cell.Dir.west];
			} else if (west_cost < float.MaxValue) {
				min_x = (GroupFace)cell.faces [(int)Cell.Dir.west];
			}

			if (north_cost < south_cost) {
				min_y = (GroupFace)cell.faces [(int)Cell.Dir.north];
			} else if (south_cost < north_cost) {
				min_y = (GroupFace)cell.faces [(int)Cell.Dir.south];
			} else if (south_cost < float.MaxValue) {
				min_y = (GroupFace)cell.faces [(int)Cell.Dir.south];
			}

			return new GroupFace[]{ min_x, min_y };
		}

		float getCellPotential (GroupCell cell)
		{
			GroupFace[] upwinds = getUpwind (cell);
			GroupFace min_x = upwinds [0];
			GroupFace min_y = upwinds [1];

			if (cell.index.x == 4 && cell.index.y == 4) {
				if (min_y == null) {
					//Printer.message ("min_y: null");
				} else {
					GroupCell neighbour = (GroupCell)min_y.neighbour;
					//Printer.message (neighbour.index.x +", "+neighbour.index.y+"POT: "+neighbour.potential);				
				}
			}

			//Printer.message (east_cost + " " + west_cost + " " + north_cost + " " + south_cost);

			if (min_x == null && min_y != null) {
				return singleFiniteDif (cell, min_y);
			}

			if (min_y == null && min_x != null) {
				return singleFiniteDif (cell, min_x);
			}

			// TODO this condition is almost always false

			if (min_x != null && min_y != null) {
				//Printer.message ("DF: " + doubleFiniteDif (cell, (GroupCell)min_x.neighbour, (GroupCell)min_y.neighbour, (Face)min_x, (Face)min_y));
				return doubleFiniteDif (cell, (GroupCell)min_x.neighbour, (GroupCell)min_y.neighbour, min_x, min_y);
			} else {
				//Printer.message ("MAX VALUE");
				return float.MaxValue;
			}
		}

		private float adjCost (Cell.Dir dir, Cell cell)
		{
			float cost = float.MaxValue;

			if (cell.faces [(int)dir] == null) {
				return cost;
			}

			GroupCell adj_cell = (GroupCell)cell.faces [(int)dir].neighbour;

			cell = shared_grid.grid2 [(int)cell.index.x, (int)cell.index.y];
	

			if (adj_cell != null) {
				
				Face face = (Face)cell.faces [(int)dir];

				if (cell.index.x == 6 && cell.index.y == 5) {
					//Printer.message (face.cost + " + " + adj_cell.potential + ": " + dir);
				}

				cost = adj_cell.potential + face.cost;
			}
			
			return cost;
		}

		float singleFiniteDif (GroupCell cell, Face f)
		{
			GroupCell neighbour = (GroupCell)f.neighbour;
			SharedCell sharedCell = (SharedCell)shared_grid.grid2 [(int)cell.index.x, (int)cell.index.y];

			Face sharedFace = sharedCell.faces [f.index];
			float cost = sharedFace.cost;
			float potential = neighbour.potential;

			return Mathf.Max (potential + cost, potential - cost);
		}

		float doubleFiniteDif (GroupCell cell, GroupCell adj_one, GroupCell adj_two, Face face_one, Face face_two)
		{
			SharedCell shared_one = (SharedCell)shared_grid.getCell (adj_one); // get the corresponding cell from the shared grid
			SharedCell shared_two = (SharedCell)shared_grid.getCell (adj_two);

			GroupFace orignalFace_one = (GroupFace)face_one;
			GroupFace orignalFace_two = (GroupFace)face_two;

			face_one = (Face)shared_one.faces [face_one.index]; // get the corresponding faces from the shared cell
			face_two = (Face)shared_two.faces [face_two.index];

			//Printer.message (face_one.cost + " : " + face_two.cost);

			float cost_one = 0;
			float potential_one = 0;
			if (face_one != null) {
				//Printer.message ("F!: not null");
				cost_one = face_one.cost;
				potential_one = adj_one.potential;
			} else {
				//Printer.message ("F1: null");
			}


			float cost_two = 0;
			float potential_two = 0;
			if (face_two != null) {
				//Printer.message ("F2: not null");
				cost_two = face_two.cost;
				potential_two = adj_two.potential;
			} else {
				//Printer.message ("F2: null");
			}

			// TODO: Sometimes a is 0	
			float a = (cost_one * cost_one) + (cost_two * cost_two);
			float b = -2 * ((cost_one * cost_one * potential_two) + (cost_two * cost_two * potential_one));
			float c = (cost_one * cost_one * potential_two * potential_two) + (cost_two * cost_two * potential_one * potential_one) - 1;

			// TODO: potential_one and Potential_two seem to be zero
			float under_root = (b * b) - (4 * a * c);
			//Printer.message ("(" + b + " * " + b + ") - ( 4" + " * " + a + " * " + c + " ) = " + under_root);

			if (under_root < 0) {
				if (potential_one < potential_two) {
					return singleFiniteDif (cell, orignalFace_one);
				} else {
					return singleFiniteDif (cell, orignalFace_two);
				}
			}

			float arg_one = (-b + Mathf.Sqrt (under_root)) / (2 * a);
			float arg_two = (-b - Mathf.Sqrt (under_root)) / (2 * a);

			return Mathf.Max (arg_one, arg_two);
		}
	}
}

