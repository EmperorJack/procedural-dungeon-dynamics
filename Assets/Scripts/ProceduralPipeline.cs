using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralPipeline : MonoBehaviour {

    public DungeonGeneration.DungeonGenerator dungeonGenerator;
    public DungeonGeneration.DungeonAssetPopulator dungeonAssetPopulator;

    private GameObject simpleLayoutParent;
    private DungeonGeneration.Cell[,] simpleLayout;

    private GameObject complexLayoutParent;

    public void Perform()
    {
        Reset();

        dungeonGenerator.Generate();

        simpleLayout = dungeonGenerator.GetSimpleLayout();

        dungeonAssetPopulator.Setup(dungeonGenerator.GetRooms(), dungeonGenerator.GetCorridors());

        DisplayComplexLayout();
    }

    public void Reset()
    {
        dungeonGenerator.Clear();
        DestroyImmediate(simpleLayoutParent);
        DestroyImmediate(complexLayoutParent);
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

        for (int i = 0; i < dungeonGenerator.gridSize; i++)
        {
            for (int j = 0; j < dungeonGenerator.gridSize; j++)
            {
                if (simpleLayout[i, j].GetType() == typeof(DungeonGeneration.FloorCell))
                {
                    GameObject instance = simpleLayout[i, j].Display();
                    instance.transform.SetParent(simpleLayoutParent.transform);
                    instance.transform.Translate((i) * dungeonGenerator.GetGridSpacing(), 0.0f, (j) * dungeonGenerator.GetGridSpacing());
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
}
