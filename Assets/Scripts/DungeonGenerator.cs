using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour {

    // User set fields
    public int gridSize = 10;
    public int minimumRoomSize = 4;
    public int roomBuffer = 1;
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
	private int nextCorridorId;
    private List<Room> rooms;
	private List<Corridor> corridors;

    public void FixedUpdate()
    {
        if (Time.frameCount % 60 == 0)
        {
            Generate();
        }
    }

    public void Generate()
    {
        Clear();

        gridSpacing = 1; // worldSize / gridSize;

        dungeonParent = new GameObject();
        dungeonParent.name = "DungeonLayout";

        PerformBSP();

        DisplayRooms();
		DisplayCorridors();
        //DisplayPartitions();
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

	public void DisplayCorridors()
	{
		foreach (Corridor corridor in corridors) corridor.Display(dungeonParent);
	}

	public void HideCorridors()
	{
		foreach (Corridor corridor in corridors) corridor.Hide();
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
		nextCorridorId = 0;

        int worldSize = gridSize * cellSize;
        rooms = new List<Room>();
		corridors = new List<Corridor> ();

        root = new Partition(this, 0, 0, worldSize, worldSize, 0);

		root.MakeParition();

        root.MakeRoom(rooms);

		root.MakeCorridors(corridors);
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

	public int NextCorridorId()
	{
		return nextCorridorId++;
	}
}
