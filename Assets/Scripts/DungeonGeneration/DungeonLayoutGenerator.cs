using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonGeneration {

	public class DungeonLayoutGenerator : MonoBehaviour {

	    // User set fields
	    public int gridSize = 10;

        [Range(1, 20)] public int minimumRoomSize = 4;
        [Range(1, 10)] public int roomBuffer = 1;
        public float minRoomWidthHeightRatio = 1.0f;
        public float maxRoomWidthHeightRatio = 1.0f;
        public GameObject simpleLayoutPrefab;
        public int maxDepth = 5;
        public int addLoopsFromLevel = 0;
        public int addLoopsToLevel = 0;

        [Range(0.0f, 1.0f)] public float loopSpawnChance = 0.5f;

        public bool allowLoopsBetweenTwoRooms = false;

        // Internal fields
        private float gridSpacing = 1;
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
        private int treeDepth;

	    public void Generate()
	    {
            Clear();

            nextPartitionId = 0;
	        nextRoomId = 0;
			nextCorridorId = 0;

	        int worldSize = gridSize * cellSize;
	        rooms = new List<Room>();
			corridors = new List<Corridor>();

	        root = new Partition(this, 0, 0, worldSize, worldSize, 0);

			treeDepth = root.MakeParition();

	        root.MakeRoom(rooms);

			root.MakeCorridors(corridors);
	    }

        public Cell[,] GetSimpleLayout()
        {
            Cell[,] layout = new Cell[gridSize, gridSize];
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    layout[i, j] = new EmptyCell();
                }
            }

            foreach (Room room in rooms)
            {
                AddAreaToLayout(room, layout);
            }

            foreach (Corridor corridor in corridors)
            {
                AddAreaToLayout(corridor, layout);
            }

            return layout;
        }

        private void AddAreaToLayout(GridArea area, Cell[,] layout)
        {
            for (int i = area.x; i < area.x + area.width; i++)
            {
                for (int j = area.y; j < area.y + area.height; j++)
                {
                    layout[i, j] = new FloorCell(simpleLayoutPrefab);
                }
            }
        }

        public float GetGridSpacing()
        {
            return gridSpacing;
        }

        public int GetTreeDepth()
        {
            return treeDepth;
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

        public void Display()
        {
            dungeonParent = new GameObject();
            dungeonParent.name = "DungeonLayout";

            DisplayRooms();
            DisplayCorridors();
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

        public List<Room> GetRooms()
        {
            return rooms;
        }

        public List<Corridor> GetCorridors()
        {
            return corridors;
        }
    }
}