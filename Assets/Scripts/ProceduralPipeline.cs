using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralPipeline : MonoBehaviour {

    public DungeonGeneration.DungeonGenerator dungeonGenerator;

    private GameObject layoutParent;
    private DungeonGeneration.Cell[,] simpleLayout;

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
}
