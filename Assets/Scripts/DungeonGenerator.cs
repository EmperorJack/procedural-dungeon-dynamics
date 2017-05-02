using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour {

    // User set fields
    public int gridSize = 10;
    public int minimumRoomSize = 4;
    public int roomBuffer = 1;
    public float minimumRoomToPartitionRatio = 0.5f;
    public GameObject cellPrefab;

    // Internal fields
    private float gridSpacing;
    private int cellSize = 1;

    // Representation fields
    private GameObject dungeonParent;

    // BSP fields
    private Partition root;
    private int nextPartitionId;
    private int nextRoomId;
    private List<Room> rooms;

    public void Generate()
    {
        Clear();

        gridSpacing = 1; // worldSize / gridSize;

        dungeonParent = new GameObject();
        dungeonParent.name = "DungeonLayout";

        PerformBSP();

        DisplayRooms();
    }

    public void DisplayRooms()
    {
        foreach (Room room in rooms) room.Display(dungeonParent);
    }

    public void HideRooms()
    {
        foreach (Room room in rooms) room.Hide();
    }

    public void DisplayPartitions()
    {
        if (root != null) root.Display(dungeonParent);
    }

    public void HidePartitions()
    {
        if (root != null) root.Hide();
    }

    public void Clear()
    {
        DestroyImmediate(dungeonParent);
        root = null;
        rooms = null;
    }

    private void PerformBSP()
    {
        nextPartitionId = 0;
        nextRoomId = 0;
        int worldSize = gridSize * cellSize;
        rooms = new List<Room>();

        root = new Partition(this, 0, 0, worldSize, worldSize, 0);

        MakeParition(root);

        root.MakeRoom(rooms);
    }

    private void MakeParition(Partition partition)
    {
        bool debug = false;

        int minimumSize = minimumRoomSize + roomBuffer * 2;
        if (debug) print("Minimum size: " + minimumSize + ", " + "PWidth: " + partition.width + ", " + "PHeight: " + partition.height);

        if (partition.width <= minimumSize * 2 || partition.height <= minimumSize * 2)
        {
            if (debug) print("No cut");
            return;
        }

        bool horizontalCut = true;
        if (Random.value > 0.5) horizontalCut = false;

        Partition partitionA;
        Partition partitionB;

        if (horizontalCut)
        {
            int yCut = Random.Range(minimumRoomSize + roomBuffer * 2, partition.height - minimumRoomSize - roomBuffer * 2 + 1);
            if (debug) print("Y cut: " + yCut);
            partitionA = new Partition(this, partition.x, partition.y, partition.width, yCut, partition.depth + 1);
            partitionB = new Partition(this, partition.x, partition.y + yCut, partition.width, partition.height - yCut, partition.depth + 1);
        }
        else // Vertical cut
        {
            int xCut = Random.Range(minimumRoomSize + roomBuffer * 2, partition.width - minimumRoomSize - roomBuffer * 2 + 1);
            if (debug) print("X cut: " + xCut);
            partitionA = new Partition(this, partition.x, partition.y, xCut, partition.height, partition.depth + 1);
            partitionB = new Partition(this, partition.x + xCut, partition.y, partition.width - xCut, partition.height, partition.depth + 1);
        }

        partition.left = partitionA;
        partition.right = partitionB;

        MakeParition(partitionA);
        MakeParition(partitionB);
    }

	public float GetGridSpacing() {
		return gridSpacing;
	}

    public int NextPartitionId()
    {
        return nextPartitionId++;
    }

    public int NextRoomId() {
		return nextRoomId++;
	}
}
