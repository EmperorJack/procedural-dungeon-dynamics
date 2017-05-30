using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralPipeline : MonoBehaviour {

    public DungeonGeneration.DungeonLayoutGenerator dungeonLayoutGenerator;
    public DungeonGeneration.DungeonAssetPopulator dungeonAssetPopulator;
    public DungeonGeneration.DungeonAnchorGenerator dungeonAnchorGenerator;

    private GameObject simpleLayoutParent;
    private DungeonGeneration.Cell[,] simpleLayout;

    private GameObject complexLayoutParent;

    private GameObject anchorsParent;

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

        DisplayComplexLayout();

        //DisplayAnchors();
    }

    public void Reset()
    {
        dungeonLayoutGenerator.Clear();
        DestroyImmediate(simpleLayoutParent);
        DestroyImmediate(complexLayoutParent);
        DestroyImmediate(anchorsParent);
        simpleLayout = null;
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

        complexLayoutParent.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
    }

    public void DisplayAnchors()
    {
        DestroyImmediate(anchorsParent);

        anchorsParent = new GameObject();
        anchorsParent.name = "AnchorsDisplay";

        dungeonAnchorGenerator.Display(anchorsParent);
    }
}
