using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class logic : MonoBehaviour
{

	public int dim;
	public float cell_width;

	public float left;
	public float top;

	public GameObject agent;
	public float left2;

	private SharedCell[,] sharedGrid;
	private Dictionary<Vector2, Vector2> cellDic;
	// position to sharedGrid index

	private Vector2 left_Pos = new Vector2 (float.MaxValue, float.MaxValue);

	public static bool isClicked = false;

	List<GameObject> agents = new List<GameObject> ();

	private bool isInitialized = false;



	// Use this for initialization
	void Start ()
	{
		isInitialized = true;
		populateGrid ();
	}

	void populateGrid ()
	{
		sharedGrid = new SharedCell[dim, dim];
		cellDic = new Dictionary<Vector2, Vector2> ();

		for (int i = 0; i < dim; i++) {
			for (int j = 0; j < dim; j++) {
				
				Vector2 pos = new Vector2 ((cell_width)*(i - dim/2+0.5f), (cell_width)*(j - dim/2+0.5f));
				sharedGrid [i, j] = new SharedCell (pos);
				cellDic [new Vector2 ((pos.x),(pos.y))] = new Vector2 (i, j);

				if (pos.x == 0) {
					print ("x = 0 at: " + i + " " + j);
				}

				if (pos.y == 0) {
					print ("y = 0 at: " + i + " " + j);
				}

				print (pos.x + " " + pos.y);

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

	// Update is called once per frame
	void Update ()
	{

		if (Input.GetMouseButton (0) && !isClicked) {
			Vector3 mPos = Input.mousePosition;
			mPos.z = 10;
			Vector3 pos = Camera.main.ScreenToWorldPoint (mPos);
			print ("Instantiating Agent at x: " + pos.x + " y: " + pos.y + " z: " + pos.z);
			GameObject agent = (GameObject)Instantiate (Resources.Load ("Agent"));
			Rigidbody agentBody = agent.GetComponent<Rigidbody> ();
			agentBody.position = new Vector3 (pos.x, agent.transform.localScale.y / 4, pos.z);
			agents.Add (agent);

			assignDensities ();

			isClicked = true;

		}

		if (Input.GetMouseButtonUp (0)) {
			isClicked = false;
		}

		if (Input.GetMouseButton (1)) {

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

			float deltaX = agentBody.position.x - cell.position.x;
			float deltaY = agentBody.position.y - cell.position.y;
			float densityExponent = 0.1f;
			float densityA = Mathf.Pow (Mathf.Min (1 - deltaX, 1 - deltaY), densityExponent);
			cell.density += densityA;
			//cell.avg_Velocity += densityA * agentVelocity;

			SharedCell cellB = sharedGrid [x + 1, y];
			float densityB = Mathf.Pow (Mathf.Min (deltaX, 1 - deltaY), densityExponent);
			cellB.density += densityB;

			SharedCell cellC = sharedGrid [x + 1, y + 1];
			float densityC = Mathf.Pow (Mathf.Min (deltaX, deltaY), densityExponent);
			cellC.density += densityC;

			SharedCell cellD = sharedGrid [x, y + 1];
			float densityD = Mathf.Pow (Mathf.Min (1 - deltaX, deltaY), densityExponent);
			cellD.density += densityD;

		}
	}

	void OnDrawGizmosSelected ()
	{
		drawGrid ();

		if (isInitialized && agents.Count>0) {
			
			//Start ();
			drawGrid ();
			Rigidbody rb = agents[agents.Count-1].GetComponent<Rigidbody> ();
			Vector2 cell = getLeft (rb.position.x, rb.position.z);

			if (left_Pos.x < float.MaxValue) {

//				print ("DRAWING");
//
//				setColor (Color.red);
//				fillRect (sharedGrid [(int)cell.x, (int)cell.y].position, cell_width, cell_width);
//
//				setColor (Color.blue);
//				fillRect (left_Pos.x, left_Pos.y, 0.1f, 0.1f);
			}
		}
	}

	// Get the grid coordinate with it's center with
	// x and y coordinates less than the given x/y
	Vector2 getLeft (float x, float y)
	{

		// to make all positions positive
		x += cell_width * (dim / 2);
		y += cell_width * (dim / 2);

		print ("Mouse: " + x + " " + y);

		float x_Rem = x - x % (cell_width / 2.0f);
		float y_Rem = y - y % (cell_width / 2.0f);

		print ("Before: " + x_Rem + " " + y_Rem);

		if (x_Rem % cell_width == 0) {
			x_Rem -= cell_width / 2.0f;
		}

		if (y_Rem % cell_width == 0) {
			y_Rem -= cell_width / 2.0f;
		}
			
		x_Rem -= cell_width * (dim / 2);
	    y_Rem -= cell_width * (dim / 2);

		print (x_Rem + " " + y_Rem);
			
		// edge case where there are no cells left
		x_Rem = Mathf.Max (x_Rem, -(dim / 2) * cell_width + cell_width / 2);
		y_Rem = Mathf.Max (y_Rem, -(dim / 2) * cell_width + cell_width / 2);

		left_Pos = new Vector2 (x_Rem,y_Rem);

		Vector2 index = new Vector2 ((x_Rem),(y_Rem));
		if (cellDic.ContainsKey (index)) {
			return cellDic [index];
		} else {
			print ("Could not find index: " + index);
			return new Vector2(0,0);
		}
	}

	////////////////////////////
	// Classes for Data Storage
	/// ////////////////////////

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

	////////////////////////////
	// Methods for visualization
	/// ////////////////////////

	void drawGrid ()
	{
		float maxDensity = 5.0f;

		for (int i = 0; i < dim; i++) {
			for (int j = 0; j < dim; j++) {

				float density = sharedGrid [i, j].density;
				density = density / maxDensity;

				Color d_Color = new Color (density, density, density);
				Vector2 position = sharedGrid [i, j].position;

				setColor (d_Color);
				fillRect (position.x, position.y, cell_width, cell_width);
				setColor (Color.red);
				drawRect (position.x, position.y, cell_width, cell_width);
			}
		}
	}


	void drawRect (float x, float y, float width, float height)
	{
		Vector2 pos = new Vector2 (x, y);
		drawRect (pos, width, height);

	}

	void fillRect (float x, float y, float width, float height)
	{
		Vector2 pos = new Vector2 (x, y);
		fillRect (pos, width, height);
	}

	void drawRect (Vector2 pos, float width, float height)
	{

		Vector3 top_l = new Vector3 (pos.x - width / 2, 0, pos.y - width / 2);
		Vector3 top_r = new Vector3 (pos.x + width / 2, 0, pos.y - width / 2);
		Vector3 bot_r = new Vector3 (pos.x + width / 2, 0, pos.y + height / 2);
		Vector3 bot_l = new Vector3 (pos.x - width / 2, 0, pos.y + height / 2);

		Gizmos.DrawLine (top_l, top_r);
		Gizmos.DrawLine (top_r, bot_r);
		Gizmos.DrawLine (bot_r, bot_l);
		Gizmos.DrawLine (bot_l, top_l);
	}

	void fillRect (Vector2 pos, float width, float height)
	{
		Vector3 pos_3 = new Vector3 (pos.x, 0, pos.y);
		Gizmos.DrawCube (pos_3, new Vector3 (width, 0.0f, height));
	}

	void setColor (Color color)
	{
		Gizmos.color = color;
	}
		
}


