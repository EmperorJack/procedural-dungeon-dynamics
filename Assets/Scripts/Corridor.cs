using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corridor
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

	public Corridor(DungeonGenerator generator, Partition partitionA, Partition partitionB, bool horizontalCut)
	{ 
		this.generator = generator;
		this.id = generator.NextCorridorId();

		this.x = partitionA.x + partitionA.width / 2;
		this.y = partitionA.y + partitionA.height / 2;

		if (horizontalCut)
		{
			this.width = 1;
			this.height = (partitionB.y + partitionB.height / 2) - this.y;
		}
		else // Vertical cut
		{
			this.width = (partitionB.x + partitionB.width / 2) - this.x;
			this.height = 1;
		}

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
		gridParent.name = "Corridor" + id;
		gridParent.transform.SetParent(dungeonParent.transform);

		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				GameObject instance = grid[i, j].Display();
				instance.transform.SetParent(gridParent.transform);
				instance.transform.Translate ((x + i) * generator.GetGridSpacing(), (y + j) * generator.GetGridSpacing(), 1.0f);
				instance.GetComponent<Renderer>().material = material;
			}
		}
	}

	public void Hide()
	{
		MonoBehaviour.DestroyImmediate(gridParent);
	}
}

