using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour {

    public int gridSize = 10;
    public GameObject cellPrefab;

    private Cell[,] grid;
    private float gridSpacing;

    private GameObject dungeonLayout;
    
    public void Generate()
    {
        Clear();

        gridSpacing = 1; // worldSize / gridSize;

        grid = new Cell[gridSize, gridSize];
        dungeonLayout = new GameObject();
        dungeonLayout.name = "DungeonLayout";

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                if (Random.value > 0.5)
                {
                    grid[i, j] = new FloorCell(new Vector3(i * gridSpacing, 0, j * gridSpacing), cellPrefab);
                } else {
                    grid[i, j] = new EmptyCell(new Vector3(i * gridSpacing, 0, j * gridSpacing));
                }
                
            }
        }

        Display();
    }

    public void Display()
    {
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                GameObject instance = grid[i, j].Display();
                if (instance != null)
                {
                    instance.transform.SetParent(dungeonLayout.transform);
                }
            }
        }
    }

    public void Clear()
    {
        DestroyImmediate(dungeonLayout);
    }
}
