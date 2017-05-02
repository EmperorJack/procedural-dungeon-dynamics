using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Partition
{
	private DungeonGenerator generator;
    private int id;
    private Room room;

    // Worldspace fields
    public int x;
	public int y;
	public int width;
	public int height;

    // Tree fields
    public int depth;
	public Partition left;
	public Partition right;

    // Grid fields
    private Cell[,] grid;
    private GameObject gridParent;

    public Partition(DungeonGenerator generator, int x, int y, int width, int height, int depth)
	{
		this.generator = generator;
        this.id = generator.NextPartitionId();
        this.x = x;
		this.y = y;
		this.width = width;
		this.height = height;
        this.depth = depth;

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

    public void Print()
	{
		MonoBehaviour.print(x + ", " + y + " : " + width + ", " + height);

		if (left != null) left.Print();
		if (right != null) right.Print();
	}

	public void MakeRoom(List<Room> rooms)
	{
		// Intermediate node
		if (left != null && right != null)
		{
			left.MakeRoom(rooms);
			right.MakeRoom(rooms);
		}
		else // Leaf node
		{
			room = new Room(generator, this);
			rooms.Add(room);
		}
	}

    public void Display(GameObject dungeonParent)
    {
        Hide();

        Material material = new Material(Shader.Find("Diffuse"));
        material.color = new Color(Random.value, Random.value, Random.value);

        gridParent = new GameObject();
        gridParent.name = "Partition" + id;
        gridParent.transform.SetParent(dungeonParent.transform);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject instance = grid[i, j].Display();
                instance.transform.SetParent(gridParent.transform);
                instance.transform.Translate((x + i) * generator.GetGridSpacing(), (y + j) * generator.GetGridSpacing(), -2.0f * depth + 15.0f);
                instance.GetComponent<Renderer>().material = material;
            }
        }

        if (left != null) left.Display(dungeonParent);
        if (right != null) right.Display(dungeonParent);
    }

    public void Hide()
    {
        MonoBehaviour.DestroyImmediate(gridParent);

        if (left != null) left.Hide();
        if (right != null) right.Hide();
    }
}
