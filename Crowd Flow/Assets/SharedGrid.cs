using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SharedGrid : Grid
{
	List<GameObject> agents; // all agents in the environment

	public SharedGrid (float cell_width, int dim, List<GameObject> agents) : base(cell_width,dim)
	{
		this.agents = agents;
	}

	protected override void instantiateCell(int i, int j, Vector2 pos){
		SharedCell cell = new SharedCell (pos);
		grid [i, j] = cell;
		cellDic [new Vector2 ((pos.x), (pos.y))] = new Vector2 (i, j);

		if (i < dim - 1) {
			cell.sharedCellFaces.Add (new Face (cell, new Vector2(i+1,j)));
		}
		//add bottom face
		if (j < dim - 1) {
			cell.sharedCellFaces.Add (new Face (cell, new Vector2(i,j+1)));
		}
		//add left face
		if (i > 0) {
			cell.sharedCellFaces.Add (new Face (cell, new Vector2(i-1,j)));
		}
		//add top face
		if (j > 0) {
			cell.sharedCellFaces.Add (new Face (cell, new Vector2(i,j-1)));
		}	
	}

	void assignDensities ()
	{

		foreach (GameObject agent in agents) {
			Rigidbody agentBody = agent.GetComponent<Rigidbody> ();

			Vector2 cellIndex = getLeft (agentBody.position.x, agentBody.position.z);
			int x = (int)cellIndex.x;
			int y = (int)cellIndex.y;
			SharedCell cell = grid [x, y] as SharedCell;

			float dif = cell_width;

			float deltaX = Mathf.Abs (agentBody.position.x - cell.position.x);
			float deltaY = Mathf.Abs (agentBody.position.z - cell.position.y);
			float densityExponent = 0.1f;
			float densityA = Mathf.Pow (Mathf.Min (dif - deltaX, dif - deltaY), densityExponent);
			float cell_og = cell.density;
			cell.density += densityA;
			//cell.avg_Velocity += densityA * agentVelocity;

			SharedCell cellB = grid [x + 1, y] as SharedCell;
			float densityB = Mathf.Pow (Mathf.Min (deltaX, dif - deltaY), densityExponent);
			float cellB_og = cell.density;
			cellB.density += densityB;

			SharedCell cellC = grid [x + 1, y + 1] as SharedCell;
			float densityC = Mathf.Pow (Mathf.Min (deltaX, deltaY), densityExponent);
			float cellC_og = cell.density;
			cellC.density += densityC;

			SharedCell cellD = grid [x, y + 1] as SharedCell;
			float densityD = Mathf.Pow (Mathf.Min (dif - deltaX, deltaY), densityExponent);
			float cellD_og = cellD.density;
			cellD.density += densityD;
		}
	}

	// Get the grid coordinate with it's center with
	// x and y coordinates less than the given x/y

	Vector2 getLeft (float x, float y)
	{
		Vector2 cellPos = new Vector2 (Mathf.Floor (x / cell_width - cell_width / 2), Mathf.Floor (y / cell_width - cell_width / 2));
		cellPos = new Vector2 (cellPos.x * cell_width + cell_width / 2, cellPos.y * cell_width + cell_width / 2);

		if (cellDic.ContainsKey (cellPos)) {
			return cellDic [cellPos];
		} else {
			return new Vector2 (0, 0);
		}
	}

	public float density (int i, int j)
	{
		SharedCell cell = grid [i, j] as SharedCell;
		return cell.density;
	}

	public Vector2 position (int i, int j)
	{
		return grid[i, j].position;
	}

	private void clear(){
		for (int i = 0; i < dim; i++) {
			for (int j = 0; j < dim; j++) {
				SharedCell cell = grid [i, j] as SharedCell;
				cell.density = 0.0f;
			}
		}
	}

	public override void update(){
		clear ();

		assignDensities();
	}
}


