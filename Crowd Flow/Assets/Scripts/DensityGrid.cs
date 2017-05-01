using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

	public class DensityGrid
	{
		private SharedCell[,] sharedGrid;
		private Dictionary<Vector2, Vector2> cellDic;
		public List<GameObject> agents = new List<GameObject> ();

		private float cell_width;
		private int dim;

		public DensityGrid (float cell_width,int dim)
		{
			this.cell_width = cell_width;
			this.dim = dim;

			populateGrid ();
		}

		public void update(){
			clearGrid ();

			assignDensities ();
		}

		void clearGrid ()
		{
			for (int i = 0; i < dim; i++) {
				for (int j = 0; j < dim; j++) {
					sharedGrid [i, j].density = 0.0f;
				}
			}
		}

		public bool contains (Vector2 pos)
		{
			bool contained = pos.x > -(dim / 2f) * cell_width && pos.x < (dim / 2f) * cell_width;
			return contained && pos.y > -(dim / 2f) * cell_width && pos.y < (dim / 2f) * cell_width;
		}

	void populateGrid ()
	{
		sharedGrid = new SharedCell[dim, dim];
		cellDic = new Dictionary<Vector2, Vector2> ();

		for (int i = 0; i < dim; i++) {
			for (int j = 0; j < dim; j++) {

				Vector2 pos = new Vector2 ((cell_width) * (i - dim / 2 + 0.5f), (cell_width) * (j - dim / 2 + 0.5f));
				sharedGrid [i, j] = new SharedCell (pos);
				cellDic [new Vector2 ((pos.x), (pos.y))] = new Vector2 (i, j);
				//cellObjects [i, j] = createCellObj (pos.x, pos.y);
			}
			//d -= 0.1f;
		}

		for (int i = 0; i < dim; i++) {
			for (int j = 0; j < dim; j++) {
				SharedCell cell = sharedGrid [i, j];
				//add right face
				if (i < dim - 1) {
					cell.sharedCellFaces.Add (new SharedFace (sharedGrid [i + 1, j]));
				}
				//add bottom face
				if (j < dim - 1) {
					cell.sharedCellFaces.Add (new SharedFace (sharedGrid [i, j + 1]));
				}
				//add left face
				if (i > 0) {
					cell.sharedCellFaces.Add (new SharedFace (sharedGrid [i - 1, j]));
				}
				//add top face
				if (j > 0) {
					cell.sharedCellFaces.Add (new SharedFace (sharedGrid [i, j - 1]));
				}
			}
		}
	}

		void assignDensities ()
		{

			foreach (GameObject agent in agents) {
				Rigidbody agentBody = agent.GetComponent<Rigidbody> ();

				Vector2 cellIndex = getLeft (agentBody.position.x, agentBody.position.z);
				int x = (int)cellIndex.x;
				int y = (int)cellIndex.y;
				SharedCell cell = sharedGrid [x, y];

				float dif = cell_width;

				float deltaX = Mathf.Abs (agentBody.position.x - cell.position.x);
				float deltaY = Mathf.Abs (agentBody.position.z - cell.position.y);
				float densityExponent = 0.1f;
				float densityA = Mathf.Pow (Mathf.Min (dif - deltaX, dif - deltaY), densityExponent);
				float cell_og = cell.density;
				cell.density += densityA;
				//cell.avg_Velocity += densityA * agentVelocity;

				SharedCell cellB = sharedGrid [x + 1, y];
				float densityB = Mathf.Pow (Mathf.Min (deltaX, dif - deltaY), densityExponent);
				float cellB_og = cell.density;
				cellB.density += densityB;

				SharedCell cellC = sharedGrid [x + 1, y + 1];
				float densityC = Mathf.Pow (Mathf.Min (deltaX, deltaY), densityExponent);
				float cellC_og = cell.density;

				cellC.density += densityC;



				SharedCell cellD = sharedGrid [x, y + 1];
				float densityD = Mathf.Pow (Mathf.Min (dif - deltaX, deltaY), densityExponent);
				float cellD_og = cellD.density;

				cellD.density += densityD;
			}
		}

	// Get the grid coordinate with it's center with
	// x and y coordinates less than the given x/y

	Vector2 getLeft(float x,float y){
		Vector2 cellPos = new Vector2 (Mathf.Floor (x/cell_width- cell_width/2), Mathf.Floor (y/cell_width-cell_width/2));
		cellPos = new Vector2 (cellPos.x * cell_width + cell_width/2, cellPos.y * cell_width+cell_width/2);

		if (cellDic.ContainsKey (cellPos)) {
			return cellDic [cellPos];
		} else {
			return new Vector2 (0, 0);
		}
	}

	public float sharedCellDensity(int i, int j){
		return sharedGrid [i,j].density;
	}

	public Vector2 sharedCellPosition(int i,int j){
		return sharedGrid [i, j].position;
	}

	}

	class Cell
	{
		public Vector2 position;

		public Cell (Vector2 pos)
		{
			this.position = pos;
		}

	}

	// Shared Cell information, i.e. independant of groups
	class SharedCell : Cell
	{
		public float density, height, discomfort;
		public Vector2 avg_Velocity;

		public List<SharedFace> sharedCellFaces = new List<SharedFace> ();

		public SharedCell (Vector2 pos) : base (pos)
		{

		}
	}

	// Group cell information, i.e. for each goal, and hence it's potential
	class GroupCell : Cell
	{
		public float potential;

		public List<GroupFace> groupCellFaces = new List<GroupFace> ();

		public GroupCell (Vector2 pos) : base (pos)
		{

		}

	}

	class SharedFace
	{
		public float velocity;
		public float grad_Height;

		public Cell cell;

		public SharedFace (Cell cell)
		{
			this.cell = cell;
		}
	}

	class GroupFace : SharedFace
	{
		public float grad_Potential;

		public GroupFace (Cell cell) : base (cell)
		{
		}
	}


