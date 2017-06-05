﻿using System.Collections;
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

	public Vector2 pos;
	public float cellWidth;
	public int dim;

	 bool addAgent = true;
	 bool addGoal = false;

	string action = "select";

	public void setAddAgent(){
		action = "agent";
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
			simAccess.init (cellWidth, dim, simpleLayout);
		} else {
			simAccess.init (cellWidth, dim, simpleLayout);
		}

		Collider c = GetComponent<Collider> ();
		c.transform.position = pos;
		c.transform.localScale = new Vector3(cellWidth * dim, 0, cellWidth * dim);
		c.transform.position = c.transform.position + new Vector3 ((cellWidth * (dim-1)) / 2, 0, (cellWidth * (dim-1)) / 2);
	}

    public void Perform()
    {
        Reset();

<<<<<<< HEAD
        dungeonGenerator.Generate();
        simpleLayout = dungeonGenerator.GetSimpleLayout();
=======
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
>>>>>>> master
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

<<<<<<< HEAD
	void Update(){

		if (simAccess != null) {
			simAccess.update ();

			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast (ray, out hit)) {
				Vector3 hitPosition = hit.point;

				if (Input.GetMouseButtonDown (0)) {

					// set grid cell to a goal
					if (action.Equals("goal")) {
						int[] selectedIndex = simAccess.addGoal (new Vector2 (hitPosition.x, hitPosition.z));
						if (selectedIndex != null) {
							print ("Selected cell: " + selectedIndex [0] + " " + selectedIndex [1]);
						} else {
							print ("Failed to select cell at: " + hitPosition.x + " " + hitPosition.z);
						}
					}

					// add an agent
						if (action.Equals("agent")) {
						simAccess.addAgent (new Vector2 (hitPosition.x, hitPosition.z));
					}

					if (action.Equals ("select")) {
						simAccess.selectCell (new Vector2 (hitPosition.x, hitPosition.z));
					}
				}
			}
		}
	}
=======
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
>>>>>>> master
}
