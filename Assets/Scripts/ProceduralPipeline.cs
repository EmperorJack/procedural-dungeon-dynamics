using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CrowdSim;

public class ProceduralPipeline : MonoBehaviour {

    public DungeonGeneration.DungeonGenerator dungeonGenerator;

    private GameObject layoutParent;
    private DungeonGeneration.Cell[,] simpleLayout;

	private SimAccess simAccess;

	public GameObject colliderQuad;

	public Vector2 pos;
	public float cellWidth;
	public int dim;

	public void createSim()
	{
		if (simAccess == null) {
			simAccess = new SimAccess ();
		}

		simAccess.init (cellWidth, dim);
		simAccess.displaySharedGrid ();

		Collider c = GetComponent<Collider> ();
		c.transform.position = pos;
		c.transform.localScale = new Vector3(cellWidth * dim, 0, cellWidth * dim);
		c.transform.position = c.transform.position + new Vector3 ((cellWidth * (dim-1)) / 2, 0, (cellWidth * (dim-1)) / 2);
	}

    public void Perform()
    {
        Reset();

        dungeonGenerator.Generate();
        simpleLayout = dungeonGenerator.GetSimpleLayout();
    }

    public void Reset()
    {
        dungeonGenerator.Clear();
        DestroyImmediate(layoutParent);
        simpleLayout = null;
    }

    public void DisplaySimpleLayout()
    {
        if (simpleLayout == null) return;

        DestroyImmediate(layoutParent);

        layoutParent = new GameObject();
        layoutParent.name = "SimpleLayout";

        Material material = new Material(Shader.Find("Diffuse"));
        material.color = new Color(200 / 255.0f, 125 / 255.0f, 30 / 255.0f);

        for (int i = 0; i < dungeonGenerator.gridSize; i++)
        {
            for (int j = 0; j < dungeonGenerator.gridSize; j++)
            {
                if (simpleLayout[i, j].GetType() == typeof(DungeonGeneration.FloorCell))
                {
                    GameObject instance = simpleLayout[i, j].Display();
                    instance.transform.SetParent(layoutParent.transform);
                    instance.transform.Translate((i) * dungeonGenerator.GetGridSpacing(), (j) * dungeonGenerator.GetGridSpacing(), 0.0f);
                    instance.GetComponent<Renderer>().material = material;
                }
            }
        }
    }

	public void OnMouseDown(){
		if (Input.GetMouseButtonDown (0)) {
			if (simAccess != null) {
				Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				RaycastHit hit;

				if (Physics.Raycast (ray, out hit)) {
					Vector3 hitPosition = hit.point;
					print (hitPosition.x + " " + hitPosition.y + " " + hitPosition.z);
					int[] selectedIndex = simAccess.selectCell (new Vector2 (hitPosition.x, hitPosition.z));
					if (selectedIndex != null) {
						print ("Selected cell: " + selectedIndex [0] + " " + selectedIndex [1]);
					} else {
						print ("Failed to select cell at: " + hitPosition.x + " " + hitPosition.z);
					}
				} else {
					print ("NOTHING");
				}
			}
		}
	}
}
