using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public int id;

    // Worldspace fields
    private int x;
    private int y;

    // Grid fields
    private int gridWidth;
    private int gridHeight;
    private FloorCell[,] grid;

    private GameObject gridParent;

    public Room(int id, int x, int y, int gridWidth, int gridHeight, float gridSpacing, GameObject cellPrefab)
    {
        this.id = id;
        this.x = x;
        this.y = y;
        this.gridWidth = gridWidth;
        this.gridHeight = gridHeight;

        GenerateCells(gridSpacing, cellPrefab);
    }

    private void GenerateCells(float gridSpacing, GameObject cellPrefab)
    {
        grid = new FloorCell[gridWidth, gridHeight];

        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                grid[i, j] = new FloorCell(new Vector3(x + i * gridSpacing, 0, y + j * gridSpacing), cellPrefab);
            }
        }
    }

    public void Display(GameObject dungeonParent)
    {
        Material material = new Material(Shader.Find("Diffuse"));
        material.color = new Color(Random.value, Random.value, Random.value);

        gridParent = new GameObject();
        gridParent.name = "Room" + id;

        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                GameObject instance = grid[i, j].Display();
                instance.transform.SetParent(gridParent.transform);
                instance.GetComponent<Renderer>().material = material;
            }
        }

        gridParent.transform.SetParent(dungeonParent.transform);
    }

}
