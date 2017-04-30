using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour {

    // User set fields
    public int gridSize = 10;
    public GameObject cellPrefab;

    // Internal fields
    private Cell[,] grid;
    private float gridSpacing;
    private int cellSize = 1;

    // Representation fields
    private GameObject dungeonParent;

    // BSP fields
    private PartitionCell root;
    private int minimumCellSize = 2;
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

        root = new PartitionCell(this, 0, 0, worldSize, worldSize);

        MakeParition(root);

        //root.Print();

        rooms = new List<Room>();
        root.MakeRoom(rooms);

        //print(rooms.Count);
    }

    private void MakeParition(PartitionCell cell)
    {
        if (cell.width <= minimumCellSize || cell.height <= minimumCellSize) return;

        bool horizontalCut = true;
        if (Random.value > 0.5) horizontalCut = false;

        PartitionCell partitionA;
        PartitionCell partitionB;

        if (horizontalCut)
        {
            partitionA = new PartitionCell(this, cell.x, cell.y, cell.width, cell.height / 2);
            partitionB = new PartitionCell(this, cell.x, cell.y + cell.height / 2, cell.width, cell.height / 2);
        }
        else // Vertical cut
        {
            partitionA = new PartitionCell(this, cell.x, cell.y, cell.width / 2, cell.height);
            partitionB = new PartitionCell(this, cell.x + cell.width / 2, 0, cell.width / 2, cell.height);
        }

        cell.left = partitionA;
        cell.right = partitionB;

        MakeParition(partitionA);
        MakeParition(partitionB);
    }

    private class PartitionCell
    {
        DungeonGenerator generator;

        // Worldspace fields
        public int x;
        public int y;
        public int width;
        public int height;

        // Children fields
        public PartitionCell left;
        public PartitionCell right;

        // Room fields
        public Room room;

        public PartitionCell(DungeonGenerator generator, int x, int y, int width, int height)
        {
            this.generator = generator;
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public void Print()
        {
            print(x + ", " + y + " : " + width + ", " + height);

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
                room = new Room(generator.nextRoomId++, x, y, width, height, generator.gridSpacing, generator.cellPrefab);
                rooms.Add(room);
            }
        }
    }
}
