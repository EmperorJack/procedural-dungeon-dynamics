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

		while (this.gridWidth / (float) partition.width < generator.minimumRoomToPartitionRatio) {
			int x1 = Random.Range (partition.x + 1, partition.x + partition.width - 1);
			int x2 = Random.Range (partition.x + 1, partition.x + partition.width - 1);

			if (x1 < x2) {
				this.x = x1;
				this.gridWidth = x2 - x1 + 1;
			} else {
				this.x = x2;
				this.gridWidth = x1 - x2 + 1;
			}
		}

		while (this.gridHeight / (float) partition.height < generator.minimumRoomToPartitionRatio) {
			int y1 = Random.Range (partition.y + 1, partition.y + partition.height - 1);
			int y2 = Random.Range (partition.y + 1, partition.y + partition.height - 1);

			if (y1 < y2) {
				this.y = y1;
				this.gridHeight = y2 - y1 + 1;
			} else {
				this.y = y2;
				this.gridHeight = y1 - y2 + 1;
			}
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
				instance.transform.Translate ((x + i) * generator.GetGridSpacing(), (y + j) * generator.GetGridSpacing(), 0.0f);
                instance.GetComponent<Renderer>().material = material;
            }
        }

        gridParent.transform.SetParent(dungeonParent.transform);
    }

}
