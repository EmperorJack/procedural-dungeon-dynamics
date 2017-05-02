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
    private int nextRoomId;
    private List<Room> rooms;

    public void Generate()
    {
        Clear();

        gridSpacing = 1; // worldSize / gridSize;

        dungeonParent = new GameObject();
        dungeonParent.name = "DungeonLayout";

        PerformBSP();

        Display();
    }

    public void Display()
    {
        foreach (Room room in rooms) room.Display(dungeonParent);
    }

    public void Hide()
    {
        foreach (Room room in rooms) room.Hide();
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
            partitionA = new Partition(this, partition.x, partition.y, partition.width, yCut);
            partitionB = new Partition(this, partition.x, partition.y + yCut, partition.width, partition.height - yCut);
        }
        else // Vertical cut
        {
            int xCut = Random.Range(minimumRoomSize + roomBuffer * 2, partition.width - minimumRoomSize - roomBuffer * 2 + 1);
            if (debug) print("X cut: " + xCut);
            partitionA = new Partition(this, partition.x, partition.y, xCut, partition.height);
            partitionB = new Partition(this, partition.x + xCut, partition.y, partition.width - xCut, partition.height);
        }

        partition.left = partitionA;
        partition.right = partitionB;

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
