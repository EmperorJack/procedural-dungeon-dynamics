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
	// position to sharedGrid index

	private Vector2 left_Pos = new Vector2 (float.MaxValue, float.MaxValue);

	public static bool isClicked = false;

	List<GameObject> agents = new List<GameObject> ();

	private bool isInitialized = false;

	public GameObject board;
	private GroupGrid grid;
	private SharedGrid shared_grid;
	private SpeedField speed_field;

	public bool show_Potential = false;
	public bool show_Speed = false;

	// Use this for initialization
	void Start ()
	{
		isInitialized = true;
		shared_grid = new SharedGrid (cell_width, dim, agents);
		speed_field = new SpeedField (shared_grid);
		grid = new GroupGrid (cell_width, dim, agents, shared_grid);
	}

	// Update is called once per frame
	void Update ()
	{
		Vector3 mPos = Input.mousePosition;
		mPos.z = 10;
		Vector3 pos = Camera.main.ScreenToWorldPoint (mPos);
		if (isClicked == false) {
			if (Input.GetMouseButton (0)) {
				if (grid.contains (new Vector2 (pos.x, pos.z))) {
					GameObject agent = (GameObject)Instantiate (this.agent);
					//print ("Instantiating Agent at x: " + pos.x + " y: " + agent.transform.localScale.y / 2 + " z: " + pos.z);

					// Instantiate(public game object)
					Rigidbody agentBody = agent.GetComponent<Rigidbody> ();
					agent.transform.localScale = new Vector3 (0, 0, 0);
					agentBody.position = new Vector3 (pos.x, agent.transform.localScale.y / 2, pos.z);


					float x = agentBody.position.x;
					float y = agentBody.position.y;
										
					agents.Add (agent);
				}
				isClicked = true;

			}
		}

		if (Input.GetMouseButtonUp (0)) {
			isClicked = false;
		}

		if (Input.GetMouseButton (1)) {
			GroupCell cell = (GroupCell)grid.getCell (new Vector2 (pos.x, pos.z));
			cell.isGoal = true;
		}

		if (agents.Count > 0) {
			speed_field.assignSpeeds ();
			shared_grid.update ();
			grid.update ();
		}

	}


	void OnDrawGizmosSelected ()
	{
		drawGrid ();

		if (isInitialized && agents.Count > 0) {
			Rigidbody rb = agents [agents.Count - 1].GetComponent<Rigidbody> ();

			if (left_Pos.x < float.MaxValue) {

				foreach (GameObject agent in agents) {
					setColor (Color.blue);
					fillRect (agent.transform.position.x, agent.transform.position.z, 0.2f, 0.2f);
				}
					
			}
		}
	}

	////////////////////////////
	// Methods for visualization
	/// ////////////////////////

	void drawGrid ()
	{
		//clear ();
		float maxDensity = 5.0f;

		for (int i = 0; i < dim; i++) {
			for (int j = 0; j < dim; j++) {


				GroupCell groupCell = (GroupCell)grid.grid2 [i, j];
				SharedCell sharedCell = (SharedCell)shared_grid.grid2 [i, j];

				float density = sharedCell.density;
					

				float color_var = density;
				density = Mathf.Min (1.0f, density / maxDensity);
				color_var = Mathf.Max (0f, density);

				if (show_Potential) {
					float potential = groupCell.potential;
					color_var = 1f - potential / grid.max_potential;
				} 

				Color color = new Color (color_var, color_var, color_var);

				Vector2 position = groupCell.position;
				//cellObjects [i, j].GetComponent<Renderer> ().material.color = d_Color;

				if (groupCell.isGoal) {
					setColor (Color.green);
				} else {
					setColor (color);
				}
				fillRect (position.x, position.y, cell_width, cell_width);

				if (show_Speed) {
					for (int face_index = 0; face_index < sharedCell.faces.Length; face_index++) {
						Face e_face = sharedCell.getFace (SharedCell.Dir.east);
						Face s_face = sharedCell.getFace (SharedCell.Dir.south);
						Face w_face = sharedCell.getFace (SharedCell.Dir.west);
						Face n_face = sharedCell.getFace (SharedCell.Dir.north);

						float small_width = cell_width / 4;

						if (e_face != null) {
							setColor (new Color (speed_field.max_speed / e_face.velocity, speed_field.max_speed / e_face.velocity, speed_field.max_speed / e_face.velocity));
							fillRect (position.x + cell_width / 2 - small_width / 2, position.y, small_width, small_width);
						}

						if (s_face != null) {
							setColor (new Color (speed_field.max_speed / s_face.velocity, speed_field.max_speed / s_face.velocity, speed_field.max_speed / s_face.velocity));
							fillRect (position.x, position.y + cell_width/2 - small_width/2, small_width, small_width);
						}

						if (w_face != null) {
							setColor (new Color (speed_field.max_speed / w_face.velocity, speed_field.max_speed / w_face.velocity, speed_field.max_speed / w_face.velocity));
							fillRect (position.x - cell_width / 2 + small_width / 2, position.y, small_width, small_width);
						}

						if (n_face != null) {
							setColor (new Color (speed_field.max_speed / n_face.velocity, speed_field.max_speed / n_face.velocity, speed_field.max_speed / n_face.velocity));
							fillRect (position.x , position.y - cell_width/2 + small_width/2, small_width, small_width);
						}

					}
				}

			}
		}

		for (int i = 0; i < agents.Count; i++) {
			Rigidbody rb = agents [i].GetComponent<Rigidbody> ();
			float x = rb.position.x;
			float y = rb.position.z;

			setColor (Color.red);
			fillRect (new Vector2 (x, y), 0.2f, 0.2f);

			if (i == agents.Count - 1) {
				Vector2 cellPos = new Vector2 (Mathf.Floor (x / cell_width - cell_width / 2), Mathf.Floor (y / cell_width - cell_width / 2));
				cellPos = new Vector2 (cellPos.x * cell_width + cell_width / 2, cellPos.y * cell_width + cell_width / 2);

				setColor (Color.blue);
				fillRect (cellPos, 0.2f, 0.2f);
			}
		}
		foreach (GameObject agent in agents) {
			
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


