using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CrowdSim;

public class ProceduralPipeline : MonoBehaviour
{
	// Seed
	public bool specifySeed = false;
	public int seed = 0;

	public GameObject breakableWall = null;
	private GameObject wallInstance;

	// Dungeon components
	public DungeonGeneration.DungeonLayoutGenerator dungeonLayoutGenerator;
	public DungeonGeneration.DungeonAssetPopulator dungeonAssetPopulator;
	public DungeonGeneration.DungeonAnchorGenerator dungeonAnchorGenerator;
	public DungeonGeneration.DungeonObjectPlacer dungeonObjectPlacer;

	// Component reuslts
	private DungeonGeneration.Cell[,] simpleLayout;
	private List<DungeonGeneration.Anchor> anchors;
    private List<GameObject> populatedObjects;

    // Parent transform objects
    private GameObject simpleLayoutParent;
	private GameObject complexLayoutParent;
	private GameObject anchorsParent;
	private GameObject objectsParent;

	public GameObject colliderQuad;

	public CrowdSim.SimAccess simAccess;

	public void setAddAgent ()
	{
		simAccess.setAction ("agent");
	}

	public void setAddGoal ()
	{
		simAccess.setAction ("goal");
	}

	public void setSelect ()
	{
		simAccess.setAction ("select");
	}
		
	public void increaseAvoidance(){
		if (simAccess == null) {
			return;
		}
		simAccess.increaseAvoidance ();
	}

	public void decreaseAvoidance(){
		if (simAccess == null) {
			return;
		}

		simAccess.decreaseAvoidance ();
	}

	public void createSim ()
	{
		if (simAccess == null) {
			//simAccess = new SimAccess ();
		} else {
			Debug.Log ("Initializing Simulation");
			simAccess.init (simpleLayout);
			simAccess.addDungeonObjects (populatedObjects);
		}
	}

	public void Update()
	{
		if (Input.GetKeyDown (KeyCode.Space))
			Perform();
		if (Input.GetKeyDown (KeyCode.U))
			setAddAgent();
		if (Input.GetKeyDown (KeyCode.I))
			setAddGoal();
		if (Input.GetKeyDown (KeyCode.O))
			addGroup();
		if (Input.GetKeyDown (KeyCode.P))
			swapGroups();
		if (Input.GetKeyDown (KeyCode.M))
			increaseAvoidance ();
		if (Input.GetKeyDown (KeyCode.N))
			decreaseAvoidance ();
		if (Input.GetKeyDown (KeyCode.Escape))
			Application.Quit();
		if (Input.GetKeyDown (KeyCode.Z))
			togglePause ();
	}

	public void Perform ()
	{
		// Set the user set seed
		if (!specifySeed) seed = (int) (Random.value * 10000);

		Random.InitState (seed);

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
        populatedObjects = dungeonObjectPlacer.GetPopulatedObjects();

        createSim();
	}

	public void swapGroups(){
		if (simAccess != null) {
			simAccess.swapGroups ();
		}
	}

	public void addGroup(){
		if (simAccess != null) {
			simAccess.addGroup ();
		}
	}

	public void togglePause(){
		if (simAccess != null) {
			simAccess.togglePause ();
		}
	}

	public void Reset ()
	{
		simpleLayout = null;
		anchors = null;

		DestroyImmediate (GameObject.Find ("SimpleLayout"));
		DestroyImmediate (GameObject.Find ("ComplexLayout"));
		DestroyImmediate (GameObject.Find ("AnchorsDisplay"));
		DestroyImmediate (GameObject.Find ("DungeonObjects"));

		if (simAccess != null) {
			simAccess.reset ();
		}

		if (wallInstance != null) {
			DestroyImmediate (wallInstance);
		}
	}
		
	public void displaySim ()
	{
		Debug.Log ("Displaying Sim");

		if (simAccess != null) {
			simAccess.displaySim ();
		}
	}

	public void DisplaySimpleLayout ()
	{
		if (simpleLayout == null)
			return;

		DestroyImmediate (simpleLayoutParent);

		simpleLayoutParent = new GameObject ();
		simpleLayoutParent.name = "SimpleLayout";

		Material material = new Material (Shader.Find ("Diffuse"));
		material.color = new Color (200 / 255.0f, 125 / 255.0f, 30 / 255.0f);

		for (int i = 0; i < dungeonLayoutGenerator.gridSize; i++) {
			for (int j = 0; j < dungeonLayoutGenerator.gridSize; j++) {
				if (simpleLayout [i, j].GetType () == typeof(DungeonGeneration.FloorCell)) {
					GameObject instance = simpleLayout [i, j].Display ();
					instance.transform.SetParent (simpleLayoutParent.transform);
					instance.transform.Translate ((i) * dungeonLayoutGenerator.GetGridSpacing (), 0.0f, (j) * dungeonLayoutGenerator.GetGridSpacing ());
					instance.transform.Rotate (90, 0, 0);
					instance.GetComponent<Renderer> ().material = material;
				}
			}
		}
	}

	public void displayFields ()
	{
		simAccess.setDisplayFields ();
	}

	public void trigger ()
	{
		simAccess.trigger ();
	}

	public void DisplayComplexLayout ()
	{
		if (simpleLayout == null)
			return;

		DestroyImmediate (complexLayoutParent);

		complexLayoutParent = new GameObject ();
		complexLayoutParent.name = "ComplexLayout";

		dungeonAssetPopulator.Populate (complexLayoutParent);
	}

	public void DisplayAnchors ()
	{
		DestroyImmediate (anchorsParent);

		anchorsParent = new GameObject ();
		anchorsParent.name = "AnchorsDisplay";

		dungeonAnchorGenerator.Display (anchorsParent);
	}

	public void DisplayObjects ()
	{
		if (anchors == null)
			return;

		DestroyImmediate (objectsParent);

		objectsParent = new GameObject ();
		objectsParent.name = "DungeonObjects";

		dungeonObjectPlacer.Populate (objectsParent);
	}
}
