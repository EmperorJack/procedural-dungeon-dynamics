using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupGrid : Grid
{
	public float max_potential = float.MinValue;

	private SharedGrid shared_grid;

	public GroupGrid (float cell_width, int dim, List<GameObject> agents, SharedGrid shared_grid ) : base (cell_width, dim)
	{
		this.shared_grid = shared_grid;

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
	}

	public void testMach(){

		max_potential = 10;
		for (int i = 0; i < dim; i++) {
			for (int j = 0; j < dim; j++) {
				GroupCell cell = (GroupCell)grid[i,j];
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

		float min_potential = float.MaxValue;
		GroupCell min_candidate = null;

		for (int i = 0; i < dim; i++) {
			for (int j = 0; j < dim; j++) {
				GroupCell cell = (GroupCell)grid [i, j];
				if (cell.isGoal) {
					cell.potential = 0f;
					cell.temporary_potential = 0f;
					known_cells.Add (cell);

					// add neighbours to candidates
					for(int f_index = 0; f_index < cell.faces.Length; f_index++){
						GroupFace face = (GroupFace)cell.faces [f_index];
						if (face != null && face.neighbour != null) {
							GroupCell candidate = (GroupCell)face.neighbour;
							float aprox_potential = getCellPotential (candidate);
							candidate.temporary_potential = getCellPotential (candidate);

							if (candidate.temporary_potential < min_potential) {
								min_potential = candidate.temporary_potential;
								min_candidate = candidate;
							}
						}
					}
					// add candidates, approximate potentials
				} else {
					cell.temporary_potential = float.MaxValue;
					cell.potential = float.MaxValue;
				}
			}
		}

		// TODO: THIS IS DIFFERENT EACH ITERATION
		// 3 ON FIRST AND THEN MAX ON OTHERS
		Printer.message ("min: "+min_potential);

		while (known_cells.Count < dim * dim) {
			// add candidate with min potential
			min_candidate.potential = min_candidate.temporary_potential;
			//Printer.message ("POT: "+min_candidate.potential);
			max_potential = Mathf.Max (max_potential, min_candidate.potential);
			known_cells.Add(min_candidate);

			// add candidates neighbours
			foreach (GroupFace face in min_candidate.faces) {
				if (face != null && face.neighbour != null) {
					GroupCell candidate = (GroupCell)face.neighbour;
					float aprox_potential = getCellPotential (candidate);
					// TODO: THIS IS ALWAYS FLOAT.MAX
					//Printer.message ("APROX: " + aprox_potential);
					candidate.temporary_potential = aprox_potential;

					if (candidate.temporary_potential <= min_potential) {
						min_potential = candidate.temporary_potential;
						min_candidate = candidate;
					}
				}
			}
		}

		max_potential = 5;
	}

	float getCellPotential(GroupCell cell)
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

		//Printer.message (east_cost + " " + west_cost + " " + north_cost + " " + south_cost);

//		if (min_x == null) {
//			Printer.message ("X: null");
//		} else {
//			Printer.message ("X: not null");
//		}
//
//		if (min_y == null) {
//			Printer.message ("Y: null");
//		} else {
//			Printer.message ("Y: not null");
//		}
//
//		if (min_x == null && min_y != null) {
//			return singleFiniteDif (cell, min_y);
//		}
//
//		if (min_y == null && min_x != null) {
//			return singleFiniteDif (cell, min_x);
//		}

		// TODO this condition is almost always false

		if (min_x != null && min_y != null) {
			//Printer.message ("DF: " + doubleFiniteDif (cell, (GroupCell)min_x.neighbour, (GroupCell)min_y.neighbour, (Face)min_x, (Face)min_y));
			return doubleFiniteDif (cell, (GroupCell)min_x.neighbour, (GroupCell)min_y.neighbour, (Face)min_x, (Face)min_y);
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
			cost = adj_cell.potential + face.cost;
		}
			
		return cost;
	}

	float singleFiniteDif (GroupCell cell, Face f)
	{

		float cost = f.cost;
		float potential = cell.potential;

		return Mathf.Max (potential + cost, potential - cost);
	}

	float doubleFiniteDif (GroupCell cell, GroupCell adj_one, GroupCell adj_two, Face face_one, Face face_two)
	{
		SharedCell shared_one = (SharedCell)shared_grid.getCell (adj_one); // get the corresponding cell from the shared grid
		SharedCell shared_two = (SharedCell)shared_grid.getCell (adj_two);

		face_one = shared_one.faces[face_one.index]; // get the corresponding faces from the shared cell
		face_two = shared_two.faces[face_two.index];

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
		float c = (cost_one * cost_one * potential_two * potential_two) + (cost_two * cost_two * potential_one * potential_one);

		// TODO: potential_one and Potential_two seem to be zero
		float under_root = (b * b) - (4 * a * c);
		//Printer.message ("(" + b + " * " + b + ") - ( 4" + " * " + a + " * " + c + " ) = " + under_root);

		if (under_root <= 0) {
			if (potential_one < potential_two) {
				return singleFiniteDif (cell, face_one);
			} else {
				return singleFiniteDif (cell, face_two);
			}
		}

		float arg_one = (-b + Mathf.Sqrt (under_root)) / (2 * a);
		float arg_two = (-b - Mathf.Sqrt (under_root)) / (2 * a);

		return Mathf.Max (arg_one, arg_two);
	}
}


