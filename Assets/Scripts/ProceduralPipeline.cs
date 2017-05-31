using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        DestroyImmediate(simpleLayoutParent);
        DestroyImmediate(complexLayoutParent);
        DestroyImmediate(anchorsParent);
        DestroyImmediate(objectsParent);
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
