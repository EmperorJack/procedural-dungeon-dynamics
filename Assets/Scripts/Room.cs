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
	private Cell[,] grid;

    private GameObject gridParent;

	private DungeonGenerator generator;

	public Room(DungeonGenerator generator, Partition partition)
    {
		this.generator = generator;
		this.id = generator.NextRoomId();

		this.gridWidth = 0;
		this.gridHeight = 0;

        int minimumSize = generator.minimumRoomSize;

        int xArea = (partition.width - minimumSize) / 2;
        int yArea = (partition.height - minimumSize) / 2;

        int x1 = partition.x + generator.roomBuffer + Random.Range (0, xArea);
		int x2 = partition.x + generator.roomBuffer + Random.Range (xArea + generator.minimumRoomSize, partition.width);

		this.x = x1;
		this.gridWidth = x2 - x1;

        int y1 = partition.y + generator.roomBuffer + Random.Range(0, yArea);
        int y2 = partition.y + generator.roomBuffer + Random.Range(yArea + generator.minimumRoomSize, partition.height);

		this.y = y1;
        this.gridHeight = y2 - y1;

        if (this.gridHeight == 1)
        {
            MonoBehaviour.print("Py: " + partition.y + ", " + "Pheight: " + partition.height);
            MonoBehaviour.print("Ry: " + this.y + ", " + "Rheight: " + this.gridHeight);
        }

        GenerateCells();
    }

    private void GenerateCells()
    {
        grid = new FloorCell[gridWidth, gridHeight];

        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                grid[i, j] = new FloorCell(generator.cellPrefab);
            }
        }
    }

    public void Display(GameObject dungeonParent)
    {
        Hide();

        Material material = new Material(Shader.Find("Diffuse"));
        material.color = new Color(Random.value, Random.value, Random.value);

        gridParent = new GameObject();
        gridParent.name = "Room" + id;
        gridParent.transform.SetParent(dungeonParent.transform);

        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                GameObject instance = grid[i, j].Display();
                instance.transform.SetParent(gridParent.transform);
				instance.transform.Translate ((x + i) * generator.GetGridSpacing(), (y + j) * generator.GetGridSpacing(), 0.0f);
                instance.GetComponent<Renderer>().material = material;
            }
        }
    }

    public void Hide()
    {
        MonoBehaviour.DestroyImmediate(gridParent);
    }

}
