using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    private DungeonGenerator generator;
    public int id;

    // Worldspace fields
    private int x;
    private int y;
    private int width;
    private int height;

    // Grid fields
    private Cell[,] grid;
    private GameObject gridParent;

	public Room(DungeonGenerator generator, Partition partition)
    {
		this.generator = generator;
		this.id = generator.NextRoomId();

		this.width = 0;
		this.height = 0;

        int minimumSize = generator.minimumRoomSize;

        int xArea = (partition.width - minimumSize) / 2;
        int yArea = (partition.height - minimumSize) / 2;

		int x1 = partition.x + Random.Range (generator.roomBuffer, xArea);
		int x2 = partition.x + Random.Range (xArea + generator.minimumRoomSize, partition.width - generator.roomBuffer);

		this.x = x1;
		this.width = x2 - x1;

		int y1 = partition.y + Random.Range(generator.roomBuffer, yArea);
		int y2 = partition.y + Random.Range(yArea + generator.minimumRoomSize, partition.height - generator.roomBuffer);

		this.y = y1;
        this.height = y2 - y1;

        GenerateCells();
    }

    private void GenerateCells()
    {
        grid = new FloorCell[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
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

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
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
