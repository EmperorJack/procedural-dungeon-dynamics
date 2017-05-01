using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour {

    // User set fields
    public int gridSize = 10;
    public int minimumSizeToPartition = 4;
	public float minimumRoomToPartitionRatio = 0.5f;
    public GameObject cellPrefab;

    // Internal fields
    private Cell[,] grid;
    private float gridSpacing;
    private int cellSize = 1;

    // Representation fields
    private GameObject dungeonParent;

    // BSP fields
    private Partition root;
    private int nextRoomId;
    private List<Room> rooms;

    public void Generate()
    {
        Clear();

        gridSpacing = 1; // worldSize / gridSize;

        grid = new Cell[gridSize, gridSize];

        dungeonParent = new GameObject();
        dungeonParent.name = "DungeonLayout";

        PerformBSP();

        /**
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
        */

        Display();
    }

    private void Display()
    {
        /**
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
        */

		print (rooms.Count);

        foreach (Room room in rooms)
        {
            room.Display(dungeonParent);
        }
    }

    public void Clear()
    {
        DestroyImmediate(dungeonParent);
    }

    private void PerformBSP()
    {
        nextRoomId = 0;
        int worldSize = gridSize * cellSize;

        root = new Partition(this, 0, 0, worldSize, worldSize);

        MakeParition(root);

        //root.Print();

        rooms = new List<Room>();
        root.MakeRoom(rooms);

        //print(rooms.Count);
    }

    private void MakeParition(Partition cell)
    {
        if (cell.width <= minimumSizeToPartition || cell.height <= minimumSizeToPartition) return;

        bool horizontalCut = true;
        if (Random.value > 0.5) horizontalCut = false;

        Partition partitionA;
        Partition partitionB;

        if (horizontalCut)
        {
            int yCut = Random.Range(minimumSizeToPartition, cell.height);
            if (yCut <= minimumSizeToPartition || cell.height - yCut <= minimumSizeToPartition) return;
            partitionA = new Partition(this, cell.x, cell.y, cell.width, yCut);
            partitionB = new Partition(this, cell.x, cell.y + yCut, cell.width, cell.height - yCut);
        }
        else // Vertical cut
        {
            int xCut = Random.Range(minimumSizeToPartition, cell.width);
            if (xCut <= minimumSizeToPartition || cell.width - xCut <= minimumSizeToPartition) return;
            partitionA = new Partition(this, cell.x, cell.y, xCut, cell.height);
            partitionB = new Partition(this, cell.x + xCut, cell.y, cell.width - xCut, cell.height);
        }

        cell.left = partitionA;
        cell.right = partitionB;

        MakeParition(partitionA);
        MakeParition(partitionB);
    }

	public float GetGridSpacing() {
		return gridSpacing;
	}

	public int NextRoomId() {
		return nextRoomId++;
	}
}
