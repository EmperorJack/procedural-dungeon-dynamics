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
	private DensityGrid grid;

	// Use this for initialization
	void Start ()
	{
		isInitialized = true;
		grid = new DensityGrid (cell_width, dim);
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

	void clearGrid ()
	{
		for (int i = 0; i < dim; i++) {
			for (int j = 0; j < dim; j++) {
				sharedGrid [i, j].density = 0.0f;
			}
		}
	}

	bool gridMinMax (Vector2 pos)
	{
		bool contained = pos.x > -(dim / 2f) * cell_width && pos.x < (dim / 2f) * cell_width;
		return contained && pos.y > -(dim / 2f) * cell_width && pos.y < (dim / 2f) * cell_width;
	}

	// Update is called once per frame
	void Update ()
	{
		if (Input.GetMouseButton (0) && !isClicked) {
			Vector3 mPos = Input.mousePosition;
			mPos.z = 10;
			Vector3 pos = Camera.main.ScreenToWorldPoint (mPos);
			if (grid.contains(new Vector2 (pos.x, pos.z))) {
				GameObject agent = (GameObject)Instantiate (this.agent);
				print ("Instantiating Agent at x: " + pos.x + " y: " + agent.transform.localScale.y / 2 + " z: " + pos.z);

				// Instantiate(public game object)
				Rigidbody agentBody = agent.GetComponent<Rigidbody> ();
				agentBody.position = new Vector3 (pos.x, agent.transform.localScale.y / 2, pos.z);

				float x = agentBody.position.x;
				float y = agentBody.position.y;
											
				grid.agents.Add (agent);
			}
			isClicked = true;
		}

		if (Input.GetMouseButtonUp (0)) {
			isClicked = false;
		}

		grid.update ();

	}
		

	void OnDrawGizmosSelected ()
	{
		drawGrid ();

		if (isInitialized && agents.Count > 0) {
			
			//Start ();
			//drawGrid ();
			Rigidbody rb = agents [agents.Count - 1].GetComponent<Rigidbody> ();
			//Vector2 cell = getLeft (rb.position.x, rb.position.z);

			if (left_Pos.x < float.MaxValue) {

				foreach (GameObject agent in agents) {
					setColor (Color.blue);
					fillRect (agent.transform.position.x, agent.transform.position.z, 0.2f, 0.2f);
				}

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

		//print ("Creating cube at: " + x + " " + y);

		return cellObj;

	}

	void drawGrid ()
	{
		//clear ();
		float maxDensity = 5.0f;

		for (int i = 0; i < dim; i++) {
			for (int j = 0; j < dim; j++) {

				float density = grid.sharedCellDensity(i, j);

				density = Mathf.Min (1.0f, density / maxDensity);
				density = Mathf.Max (0f, density);

				Color d_Color = new Color (density, density, density);
				Vector2 position = grid.sharedCellPosition(i, j);
				//cellObjects [i, j].GetComponent<Renderer> ().material.color = d_Color;




				setColor (d_Color);
				fillRect (position.x, position.y, cell_width, cell_width);
				setColor (Color.red);
				drawRect (position.x, position.y, cell_width, cell_width);
			}
		}

		for (int i = 0; i < grid.agents.Count; i++) {
			Rigidbody rb = grid.agents[i].GetComponent<Rigidbody> ();
			float x = rb.position.x;
			float y = rb.position.z;

			setColor (Color.red);
			fillRect (new Vector2(x,y), 0.2f, 0.2f);

			if (i == grid.agents.Count - 1) {
				Vector2 cellPos = new Vector2 (Mathf.Floor (x / cell_width - cell_width / 2), Mathf.Floor (y / cell_width - cell_width / 2));
				cellPos = new Vector2 (cellPos.x * cell_width + cell_width / 2, cellPos.y * cell_width + cell_width / 2);

				setColor (Color.blue);
				fillRect (cellPos, 0.2f, 0.2f);
			}
		}
		foreach (GameObject agent in grid.agents) {
			
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


