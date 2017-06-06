using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CrowdSim;

public class ProceduralPipeline : MonoBehaviour {

    // Dungeon components
    public DungeonGeneration.DungeonLayoutGenerator dungeonLayoutGenerator;
    public DungeonGeneration.DungeonAssetPopulator dungeonAssetPopulator;
    public DungeonGeneration.DungeonAnchorGenerator dungeonAnchorGenerator;
    public DungeonGeneration.DungeonObjectPlacer dungeonObjectPlacer;

    // Component reuslts
    private DungeonGeneration.Cell[,] simpleLayout;
    private List<DungeonGeneration.Anchor> anchors;

    // Parent transform objects
    private GameObject simpleLayoutParent;
    private GameObject complexLayoutParent;
    private GameObject anchorsParent;
    private GameObject objectsParent;

	private SimAccess simAccess;

	public GameObject colliderQuad;

	public int gridRatio = 1;
	public Vector2 pos;
	public float cellWidth;
	public int dim;
	GameObject simObject;

	public int displayField = 0;


	string action = "select";

	public void setAddAgent(GameObject simObject){
		action = "agent";
		this.simObject = simObject;
	}

	public void setAddGoal(){
		action = "goal";
	}

	public void setSelect(){
		action = "select";
	}

	public void createSim()
	{
		if (simAccess == null) {
			simAccess = new SimAccess ();
		}

		if (simpleLayout != null) {
			simAccess.init (cellWidth, dim, simpleLayout, gridRatio);
		} else {
			simAccess.init (cellWidth, dim, simpleLayout, gridRatio);
		}
			
		Collider c = GetComponent<Collider> ();
		c.transform.position = pos;
		c.transform.localScale = new Vector3(cellWidth * dim * gridRatio, 0, cellWidth * dim*gridRatio);
		c.transform.position = c.transform.position + new Vector3 ((cellWidth * gridRatio * (dim-1)) / 2, 0, (cellWidth *gridRatio* ( dim-1)) / 2);
	}

    public void Perform()
    {
        Reset();

        dungeonLayoutGenerator.Generate();

        simpleLayout = dungeonLayoutGenerator.GetSimpleLayout();

        dungeonAssetPopulator.Setup(
            dungeonLayoutGenerator.GetRooms(),
            dungeonLayoutGenerator.GetCorridors(),
            dungeonLayoutGenerator.GetGridSpacing()
        );

        dungeonAnchorGenerator.Generate(
            dungeonLayoutGenerator.GetRooms(),
            dungeonLayoutGenerator.GetGridSpacing()
        );

        anchors = dungeonAnchorGenerator.GetAnchors();

        dungeonObjectPlacer.Setup(anchors);

        DisplayComplexLayout();

        DisplayObjects();
    }

    public void Reset()
    {
        simpleLayout = null;
        anchors = null;

        DestroyImmediate(GameObject.Find("SimpleLayout"));
        DestroyImmediate(GameObject.Find("ComplexLayout"));
        DestroyImmediate(GameObject.Find("AnchorsDisplay"));
        DestroyImmediate(GameObject.Find("DungeonObjects"));
    }

	public void displaySim(){
		if (simAccess != null) {
			simAccess.displaySim ();
		}
	}

    public void DisplaySimpleLayout()
    {
        if (simpleLayout == null) return;

        DestroyImmediate(simpleLayoutParent);

        simpleLayoutParent = new GameObject();
        simpleLayoutParent.name = "SimpleLayout";

        Material material = new Material(Shader.Find("Diffuse"));
        material.color = new Color(200 / 255.0f, 125 / 255.0f, 30 / 255.0f);

        for (int i = 0; i < dungeonLayoutGenerator.gridSize; i++)
        {
            for (int j = 0; j < dungeonLayoutGenerator.gridSize; j++)
            {
                if (simpleLayout[i, j].GetType() == typeof(DungeonGeneration.FloorCell))
                {
                    GameObject instance = simpleLayout[i, j].Display();
                    instance.transform.SetParent(simpleLayoutParent.transform);
                    instance.transform.Translate((i) * dungeonLayoutGenerator.GetGridSpacing(), 0.0f, (j) * dungeonLayoutGenerator.GetGridSpacing());
                    instance.transform.Rotate(90, 0, 0);
                    instance.GetComponent<Renderer>().material = material;
                }
            }
        }
    }

	public void displayFields(){
		displayField++;
		if (displayField >= 3) {
			displayField = 0;
		}

		if (displayField == 1) {
			Debug.Log ("Displaying potential Gradients");
		}

		if (displayField == 2) {
			Debug.Log ("Displaying velocities");
		}
	}

	public void OnDrawGizmos(){
		if (displayField > 0) {
			if (simAccess != null) {
				Primitives.Cell[,] grid = simAccess.simManager.groupGrid.grid;
				for (int i = 0; i < grid.GetLength (0); i++) {
					for (int j = 0; j < grid.GetLength (0); j++) {
						if (grid [i, j].exists) {
							Vector2 norm = new Vector2 (0f, 0f);
							if (displayField == 1) {
								norm = 0.25f * grid [i, j].potGrad.normalized;
							} else if (displayField == 2) {
								norm = 0.25f * grid [i, j].groupVelocity.normalized;
							}
							Vector2 gPos = grid [i, j].position;
							Vector2 from = gPos - norm * 0.5f;
							Vector2 to = gPos + norm * 0.5f;
							Gizmos.color = Color.white;
							Gizmos.DrawLine (new Vector3 (from.x, 0.01f, from.y), new Vector3 (to.x, 0.01f, to.y));
							Gizmos.color = Color.blue;
							Gizmos.DrawCube (new Vector3 (to.x, 0f, to.y), new Vector3 (0.05f, 0.05f, 0.05f));
						}
					}
				}
			}
		}
	}



	void Update(){

		if (simAccess != null) {
			simAccess.update ();

			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast (ray, out hit)) {
				Vector3 hitPosition = hit.point;

				if (Input.GetMouseButtonDown (0)) {

					// set grid cell to a goal
					if (action.Equals ("goal")) {
						int[] selectedIndex = simAccess.addGoal (new Vector2 (hitPosition.x, hitPosition.z));
						if (selectedIndex != null) {
							print ("Selected cell: " + selectedIndex [0] + " " + selectedIndex [1]);
						} else {
							print ("Failed to select cell at: " + hitPosition.x + " " + hitPosition.z);
						}
					}

					if (action.Equals ("select")) {
						simAccess.selectCell (new Vector2 (hitPosition.x, hitPosition.z));
					}

					// add an agent
					if (action.Equals ("agent")) {
						simAccess.addAgent (new Vector2 (hitPosition.x, hitPosition.z), simObject);
					}
				} else if (Input.GetMouseButton (1)) {
					// add an agent
					if (action.Equals ("agent")) {
						simAccess.addAgent (new Vector2 (hitPosition.x, hitPosition.z), simObject);
					}
				}
			}
		}
	}

    public void DisplayComplexLayout()
    {
        if (simpleLayout == null) return;

        DestroyImmediate(complexLayoutParent);

        complexLayoutParent = new GameObject();
        complexLayoutParent.name = "ComplexLayout";

        dungeonAssetPopulator.Populate(complexLayoutParent);
    }

    public void DisplayAnchors()
    {
        DestroyImmediate(anchorsParent);

        anchorsParent = new GameObject();
        anchorsParent.name = "AnchorsDisplay";

        dungeonAnchorGenerator.Display(anchorsParent);
    }

    public void DisplayObjects()
    {
        if (anchors == null) return;

        DestroyImmediate(objectsParent);

        objectsParent = new GameObject();
        objectsParent.name = "DungeonObjects";

        dungeonObjectPlacer.Populate(objectsParent);
    }
}
