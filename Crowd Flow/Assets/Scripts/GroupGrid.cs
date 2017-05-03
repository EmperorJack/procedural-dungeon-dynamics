using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupGrid : Grid
{
	public float max_potential = float.MinValue;


	public GroupGrid (float cell_width, int dim, List<GameObject> agents ) : base (cell_width, dim)
	{

		for (int i = 0; i < dim; i++) {
			for (int j = 0; j < dim; j++) {
				GroupCell cell = (GroupCell)grid [i, j];
				foreach (GroupFace face in cell.faces) {
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
		List<GroupCell> candidate_cells = new List<GroupCell> ();

		for (int i = 0; i < dim; i++) {
			for (int j = 0; j < dim; j++) {
				GroupCell cell = (GroupCell)grid [i, j];
				if (cell.isGoal) {
					cell.potential = 0;
					known_cells.Add (cell);

					foreach (GroupFace face in cell.faces) {
						GroupCell candidate = (GroupCell)face.neighbour;
						candidate.temporary_potential = float.MaxValue;
						candidate_cells.Add (candidate);

					}
				} else {
					cell.potential = float.MaxValue;
				}
			}
		}

		if (candidate_cells.Count == 0) {
			return;
		}


		while (known_cells.Count != dim * dim) {
			GroupCell min_candidate = null;
			float min_pot = float.MaxValue;
			foreach (GroupCell candidate in candidate_cells) {
				if (candidate.temporary_potential == float.MaxValue) {
					candidate.temporary_potential = getCellPotential (candidate);
					Printer.message ("Potential: "+getCellPotential (candidate));
				}

				if (candidate.temporary_potential < min_pot) {
					min_pot = candidate.temporary_potential;
					min_candidate = candidate;
				}
			}

			min_candidate.potential = min_candidate.temporary_potential;
			max_potential = Mathf.Max (min_candidate.potential, max_potential);
			known_cells.Add (min_candidate);

			foreach (GroupFace face in min_candidate.faces) {
				if (face != null) {
					GroupCell neighbour_cell = (GroupCell)face.neighbour;
					neighbour_cell.temporary_potential = float.MaxValue;
					if (known_cells.Contains (neighbour_cell) == false) {
						candidate_cells.Add (neighbour_cell);
					}
				}
			}
		}
			
	}

	private enum Dir
	{
		Horizontal,
		Vertical}

	;

	float getCellPotential (GroupCell cell)
	{
		GroupFace least_x = null;

		float east_cost = adjCost (GroupCell.Dir.east, cell);
		float west_cost = adjCost (GroupCell.Dir.west, cell);

		if (east_cost <= west_cost) {
			least_x = (GroupFace)cell.getFace (GroupCell.Dir.east);
		} else {
			least_x = (GroupFace)cell.getFace (GroupCell.Dir.west);
		}

		GroupFace least_y = null;

		float north_cost = adjCost (GroupCell.Dir.north, cell);
		float south_cost = adjCost (GroupCell.Dir.south, cell);

		if (north_cost <= south_cost) {
			least_y = (GroupFace)cell.getFace (GroupCell.Dir.north);
		} else {
			least_y = (GroupFace)cell.getFace (GroupCell.Dir.south);
		}

		return doubleFiniteDif (cell, least_x, least_y);
	}

	private float adjCost (Cell.Dir dir, GroupCell cell)
	{
		if (cell.faces [(int)dir] == null) {
			return float.MaxValue;
		}

		GroupCell adj_cell = (GroupCell)cell.faces [(int)dir].neighbour;

		float cost = float.MaxValue;

		if (adj_cell != null) {
			GroupFace face = (GroupFace)cell.faces [(int)dir];
			cost = adj_cell.potential + face.cost;
		}

		return cost;
	}

	float singleFiniteDif (GroupCell cell, GroupFace f)
	{
		GroupCell neighbour = (GroupCell)f.neighbour;

		float cost = f.cost;
		float potential = cell.potential;

		return Mathf.Max (potential + cost, potential - cost);
	}

	float doubleFiniteDif (GroupCell cell, GroupFace face_one, GroupFace face_two)
	{

		GroupCell adj_one = null;
		float cost_one = 0;
		float potential_one = 0;
		if (face_one != null) {
			adj_one = (GroupCell)face_one.neighbour;
			cost_one = face_one.cost;
			potential_one = adj_one.potential;
		}


		GroupCell adj_two = null;
		float cost_two = 0;
		float potential_two = 0;
		if (face_two != null) {
			adj_two = (GroupCell)face_two.neighbour;
			cost_two = face_two.cost;
			potential_two = adj_two.potential;
		}

		if (adj_two == null) {
			return singleFiniteDif (adj_one, face_one);
		}

		if (adj_one == null) {
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


