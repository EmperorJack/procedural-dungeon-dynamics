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
	}

	protected override void instantiateCell (int i, int j, Vector2 pos)
	{
		GroupCell cell = new GroupCell (pos, new Vector2 (i, j));
		grid [i, j] = cell;
		cellDic [new Vector2 ((pos.x), (pos.y))] = new Vector2 (i, j);

		if (i < dim - 1) {
			cell.setFace (GroupCell.Dir.east, new GroupFace (cell, new Vector2 (i + 1, j)));
		} 
		//add bottom face
		if (j < dim - 1) {
			cell.setFace (GroupCell.Dir.south, new GroupFace (cell, new Vector2 (i, j + 1)));
		} 
		//add left face
		if (i > 0) {
			cell.setFace (GroupCell.Dir.west, new GroupFace (cell, new Vector2 (i - 1, j)));
		} 
		//add top face
		if (j > 0) {
			cell.setFace (GroupCell.Dir.north, new GroupFace (cell, new Vector2 (i, j - 1)));
		}
	}
		
	private void fastMarch ()
	{
		List<GroupCell> known_cells = new List<GroupCell> ();

		for (int i = 0; i < dim; i++) {
			for (int j = 0; j < dim; j++) {
				GroupCell cell = (GroupCell)grid [i, j];
				if (cell.isGoal) {
					cell.potential = 0f;
					known_cells.Add (cell);
				} else {
					cell.potential = float.MaxValue;
					cell.temporary_potential = float.MaxValue;
				}
			}
		}

		int count = 0;

		while (known_cells.Count != dim * dim) {
			GroupCell min_candidate = null;
			float min_pot = float.MaxValue;

			List<GroupCell> candidate_cells = new List<GroupCell> ();

			foreach (GroupCell known in known_cells) {
				foreach (Face face in known.faces) {
					if (face != null && face.neighbour!=null) {
						GroupCell candidate = (GroupCell)face.neighbour;
						if (candidate_cells.Contains (candidate) == false && known_cells.Contains(candidate) == false) {
							candidate.temporary_potential = getCellPotential(candidate);
							if (candidate.index.x == 7 && candidate.index.y == 6) {
								Printer.message (candidate.temporary_potential+"");
							}
							candidate_cells.Add (candidate);
							if (candidate.temporary_potential < min_pot) {
								min_pot = candidate.temporary_potential;
								min_candidate = candidate;
							}
						}
					}
				}
			}
				
			//Printer.message ("C: " + count+" "+known_cells.Count);
			count = count + 1;

			min_candidate.potential = min_candidate.temporary_potential;
			max_potential = Mathf.Max (min_candidate.potential, max_potential);
			known_cells.Add (min_candidate);
		}
			
	}
		
	float getCellPotential (GroupCell cell)
	{
		GroupCell least_x = null;
		Face least_x_shared = null;

		SharedCell shared_cell = (SharedCell)shared_grid.grid2 [(int)cell.index.x, (int)cell.index.y];

		float east_cost = adjCost (GroupCell.Dir.east, cell);
		float west_cost = adjCost (GroupCell.Dir.west, cell);

		if (cell.index.x == 7 && cell.index.y == 6) {
			Printer.message (east_cost + " " + west_cost);
		}
		//Printer.message ("HOR"+east_cost + " " + west_cost+" "+cell.index.x+" "+cell.index.y);

		if (east_cost < west_cost) {
			least_x = (GroupCell)cell.getFace (GroupCell.Dir.east).neighbour;
			least_x_shared = shared_cell.getFace (GroupCell.Dir.east);
		} else if (west_cost < east_cost) {
			least_x = (GroupCell)cell.getFace (GroupCell.Dir.west).neighbour;
			least_x_shared = shared_cell.getFace (GroupCell.Dir.west);
		} 

		GroupCell least_y = null;
		Face least_y_shared = null;

		float north_cost = adjCost (GroupCell.Dir.north, cell);
		float south_cost = adjCost (GroupCell.Dir.south, cell);

		//Printer.message ("VER"+north_cost + " " + south_cost);

		if (north_cost < south_cost) {
			least_y = (GroupCell)cell.getFace (GroupCell.Dir.north).neighbour;
			least_y_shared = shared_cell.getFace (GroupCell.Dir.north);

		} else if(south_cost < north_cost) {
			least_y = (GroupCell)cell.getFace (GroupCell.Dir.south).neighbour;
			least_y_shared = shared_cell.getFace (GroupCell.Dir.south);

		}
			
		return doubleFiniteDif (cell, least_x, least_y, least_x_shared,least_y_shared);
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

		float cost_one = 0;
		float potential_one = 0;
		if (face_one != null) {
			cost_one = face_one.cost;
			potential_one = adj_one.potential;
		}


		float cost_two = 0;
		float potential_two = 0;
		if (face_two != null) {
			cost_two = face_two.cost;
			potential_two = adj_two.potential;
		}
			
		if (adj_two == null || potential_two == float.MaxValue) {
			return singleFiniteDif (adj_one, face_one);
		}

		if (adj_one == null || potential_one == float.MaxValue) {
			return singleFiniteDif (adj_two, face_two);
		}
			
		float a = (cost_one * cost_one) + (cost_two * cost_two);
		float b = -2 * ((cost_one * cost_one * potential_two) + (cost_two * cost_two * potential_one));
		float c = (cost_one * cost_one * potential_two * potential_two) + (cost_two * cost_two * potential_one * potential_one);

		float under_root = (b * b) - (4 * a * c);


		float arg_one = (-b + Mathf.Sqrt (under_root)) / (2 * a);
		float arg_two = (-b - Mathf.Sqrt (under_root)) / (2 * a);

		return Mathf.Max (arg_one, arg_two);


	}
}


