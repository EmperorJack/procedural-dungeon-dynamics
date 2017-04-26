using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class logic : MonoBehaviour
{

	public int dim;
	public float cell_width;
	public float width;

	public float left;
	public float top;

	public GameObject agent;
	public float left2;
	public GameObject cellObj;

	private GameObject[,] cellObjects;
	private SharedCell[,] sharedGrid;
	private Dictionary<Vector2, Vector2> cellDic;
	// position to sharedGrid index

	private Vector2 left_Pos = new Vector2 (float.MaxValue, float.MaxValue);

	public static bool isClicked = false;

	List<GameObject> agents = new List<GameObject> ();

	private bool isInitialized = false;

	public GameObject board;

	// Use this for initialization
	void Start ()
	{
		isInitialized = true;
		populateGrid ();
	}

	void populateGrid ()
	{
		print ("Populating Grid");
		sharedGrid = new SharedCell[dim, dim];
		cellObjects = new GameObject[dim, dim];
		cellDic = new Dictionary<Vector2, Vector2> ();

		for (int i = 0; i < dim; i++) {
			for (int j = 0; j < dim; j++) {
				
				Vector2 pos = new Vector2 ((cell_width) * (i - dim / 2 + 0.5f), (cell_width) * (j - dim / 2 + 0.5f));
				sharedGrid [i, j] = new SharedCell (pos);
				cellDic [new Vector2 ((pos.x), (pos.y))] = new Vector2 (i, j);
				cellObjects [i, j] = createCellObj (pos.x, pos.y);
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

	void clearGrid ()
	{
		for (int i = 0; i < dim; i++) {
			for (int j = 0; j < dim; j++) {
				sharedGrid [i, j].density = 0.0f;
			}
		}
	}

	// Update is called once per frame
	void Update ()
	{

		//drawGrid ();

		if (Input.GetMouseButton (0) && !isClicked) {
			Vector3 mPos = Input.mousePosition;
			mPos.z = 10;
			Vector3 pos = Camera.main.ScreenToWorldPoint (mPos);
			print ("Instantiating Agent at x: " + pos.x + " y: " + pos.y + " z: " + pos.z);
			GameObject agent = (GameObject)Instantiate (this.agent);
			// Instantiate(public game object)
			Rigidbody agentBody = agent.GetComponent<Rigidbody> ();
			agentBody.position = new Vector3 (pos.x, agent.transform.localScale.y / 2, pos.z);
			agents.Add (agent);
			isClicked = true;
		}

		if (Input.GetMouseButtonUp (0)) {
			isClicked = false;
		}

		clearGrid ();
		assignDensities ();

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
			float deltaY = Mathf.Abs (agentBody.position.y - cell.position.y);
			float densityExponent = 0.1f;
			float densityA = Mathf.Pow (Mathf.Min (dif - deltaX, dif - deltaY), densityExponent);
			float cell_og = cell.density;
			if (float.IsNaN (cell.density + densityA)) {
				print ("A: "+dif + " " + deltaX + " " + deltaY + " " + densityExponent);
				print (cell.density + " " + densityA);
			} else {
				cell.density += densityA;

			}
			//cell.avg_Velocity += densityA * agentVelocity;

			SharedCell cellB = sharedGrid [x + 1, y];
			float densityB = Mathf.Pow (Mathf.Min (deltaX, dif - deltaY), densityExponent);
			float cellB_og = cell.density;
			if (float.IsNaN (cellB.density + densityB)) {
				print ("B: "+dif + " " + deltaX + " " + deltaY + " " + densityExponent);

				print (cellB.density + " " + densityB);
			} else {
				cellB.density += densityB;
			}

			SharedCell cellC = sharedGrid [x + 1, y + 1];
			float densityC = Mathf.Pow (Mathf.Min (deltaX, deltaY), densityExponent);
			float cellC_og = cell.density;
			if (float.IsNaN (cellC.density + densityC)) {
				print ("C: "+dif + " " + deltaX + " " + deltaY + " " + densityExponent);

				print (cellC.density + " " + densityC);
			} else {
				cellC.density += densityC;

			}

			SharedCell cellD = sharedGrid [x, y + 1];
			float densityD = Mathf.Pow (Mathf.Min (dif - deltaX, deltaY), densityExponent);
			float cellD_og = cellD.density;
			if (float.IsNaN (cellD.density + densityD)) {
				print ("D: "+dif + " " + deltaX + " " + deltaY + " " + densityExponent);

				print (cellD.density + " " + densityD);
			} else {
				cellD.density += densityD;
			}
		}
	}

	void OnDrawGizmosSelected ()
	{
		drawGrid ();

//		if (isInitialized && agents.Count>0) {
//			
//			//Start ();
//			//drawGrid ();
//			Rigidbody rb = agents[agents.Count-1].GetComponent<Rigidbody> ();
//			Vector2 cell = getLeft (rb.position.x, rb.position.z);
//
//			if (left_Pos.x < float.MaxValue) {
//
////				print ("DRAWING");
////
////				setColor (Color.red);
////				fillRect (sharedGrid [(int)cell.x, (int)cell.y].position, cell_width, cell_width);
////
////				setColor (Color.blue);
////				fillRect (left_Pos.x, left_Pos.y, 0.1f, 0.1f);
//			}
//		}
	}

	// Get the grid coordinate with it's center with
	// x and y coordinates less than the given x/y
	Vector2 getLeft (float x, float y)
	{

		// to make all positions positive
		x += cell_width * (dim / 2);
		y += cell_width * (dim / 2);

		float x_Rem = x - x % (cell_width / 2.0f);
		float y_Rem = y - y % (cell_width / 2.0f);

		if (x_Rem % cell_width == 0) {
			x_Rem -= cell_width / 2.0f;
		}

		if (y_Rem % cell_width == 0) {
			y_Rem -= cell_width / 2.0f;
		}
			
		x_Rem -= cell_width * (dim / 2);
		y_Rem -= cell_width * (dim / 2);
					
		// edge case where there are no cells left
		x_Rem = Mathf.Max (x_Rem, -(dim / 2) * cell_width + cell_width / 2);
		y_Rem = Mathf.Max (y_Rem, -(dim / 2) * cell_width + cell_width / 2);

		left_Pos = new Vector2 (x_Rem, y_Rem);

		Vector2 index = new Vector2 ((x_Rem), (y_Rem));
		if (cellDic.ContainsKey (index)) {
			return cellDic [index];
		} else {
			return new Vector2 (0, 0);
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

	GameObject createCellObj (float x, float y)
	{
		GameObject cellObj = Instantiate (this.cellObj);
		cellObj.transform.position = new Vector3 (x, 0.0f, y);
		cellObj.transform.localScale = new Vector3 (cell_width, 0.001f, cell_width);
		cellObj.SetActive (true);
		cellObj.GetComponent<Renderer> ().material.color = new Color (0f, 0f, 0f);

		print ("Creating cube at: " + x + " " + y);

		return cellObj;

	}

	void drawGrid ()
	{
		//clear ();
		float maxDensity = 5.0f;

		for (int i = 0; i < dim; i++) {
			for (int j = 0; j < dim; j++) {

				float density = sharedGrid [i, j].density;

				density = Mathf.Min (1.0f, density / maxDensity);
				density = Mathf.Max (0f, density);

				Color d_Color = new Color (density, density, density);
				Vector2 position = sharedGrid [i, j].position;
				cellObjects [i, j].GetComponent<Renderer> ().material.color = d_Color;


				//setColor (d_Color);
				//fillRect (position.x, position.y, cell_width, cell_width);
				//setColor (Color.red);
				//drawRect (position.x, position.y, cell_width, cell_width);
			}
		}
		//redraw ();
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


