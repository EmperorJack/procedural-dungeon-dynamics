using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Corridor
{
	private DungeonGenerator generator;
	public int id;

    // Worldspace fields
    public int x;
    public int y;
    public int width;
    public int height;

	// Grid fields
	private Cell[,] grid;
	private GameObject gridParent;

	public Corridor(DungeonGenerator generator, Partition partitionA, Partition partitionB, bool horizontalCut)
	{ 
		this.generator = generator;
		this.id = generator.NextCorridorId();

        List<Room> roomsA = new List<Room>();
        partitionA.GetRooms(roomsA);

        List<Room> roomsB = new List<Room>();
        partitionB.GetRooms(roomsB);

        List<Vector2> rangeA = new List<Vector2>();
        List<Vector2> rangeB = new List<Vector2>();
        List<PossibleOverlap> overlap = new List<PossibleOverlap>();

        if (horizontalCut) {
            foreach (Room room in roomsA)
            {
                for (int x = room.x; x < room.x + room.width; x++)
                {
                    int xMatchIndex = rangeA.FindIndex(v => v.x == x);
                    if (xMatchIndex != -1)
                    {
                        if (room.y + room.height > rangeA[xMatchIndex].y) rangeA[xMatchIndex] = new Vector2(x, room.y + room.height);
                    }
                    else
                    {
                        rangeA.Add(new Vector2(x, room.y + room.height));
                    }
                }
            }
            foreach (Room room in roomsB)
            {
                for (int x = room.x; x < room.x + room.width; x++)
                {
                    int xMatchIndex = rangeB.FindIndex(v => v.x == x);
                    if (xMatchIndex != -1)
                    {
                        if (room.y < rangeB[xMatchIndex].y) rangeB[xMatchIndex] = new Vector2(x, room.y);
                    }
                    else
                    {
                        rangeB.Add(new Vector2(x, room.y));
                    }
                }
            }

            foreach (Vector2 posA in rangeA)
            {
                foreach(Vector2 posB in rangeB)
                {
                    if (posA.x == posB.x) overlap.Add(new PossibleOverlap(posA, posB));
                }
            }

            PossibleOverlap chosenOverlap = overlap[Random.Range(0, overlap.Count)];

            this.x = (int) chosenOverlap.posA.x;
            this.y = (int) chosenOverlap.posA.y;

            this.width = 1;
            this.height = (int) (chosenOverlap.posB.y - chosenOverlap.posA.y);
        }
        else // Vertical cut
        {
            foreach (Room room in roomsA)
            {
                for (int y = room.y; y < room.y + room.height; y++)
                {
                    int yMatchIndex = rangeA.FindIndex(v => v.y == y);
                    if (yMatchIndex != -1)
                    {
                        if (room.x + room.width > rangeA[yMatchIndex].x) rangeA[yMatchIndex] = new Vector2(room.x + room.width, y);
                    }
                    else
                    {
                        rangeA.Add(new Vector2(room.x + room.width, y));
                    }
                }
            }
            foreach (Room room in roomsB)
            {
                for (int y = room.y; y < room.y + room.height; y++)
                {
                    int yMatchIndex = rangeB.FindIndex(v => v.y == y);
                    if (yMatchIndex != -1)
                    {
                        if (room.x < rangeB[yMatchIndex].x) rangeB[yMatchIndex] = new Vector2(room.x, y);
                    }
                    else
                    {
                        rangeB.Add(new Vector2(room.x, y));
                    }
                }
            }

            foreach (Vector2 posA in rangeA)
            {
                foreach (Vector2 posB in rangeB)
                {
                    if (posA.y == posB.y) overlap.Add(new PossibleOverlap(posA, posB));
                }
            }

            PossibleOverlap chosenOverlap = overlap[Random.Range(0, overlap.Count)];

            this.x = (int)chosenOverlap.posA.x;
            this.y = (int)chosenOverlap.posA.y;

            this.width = (int)(chosenOverlap.posB.x - chosenOverlap.posA.x);
            this.height = 1;
        }

        /**
        Debug.Log("x: " + x + " ey: " + rangeB[xMatchIndex].y + " ty: " + room.y);
        Debug.Log("ny: " + rangeB[xMatchIndex].y);
        */

        GenerateCells();
	}

    public struct PossibleOverlap
    {
        public Vector2 posA;
        public Vector2 posB;

        public PossibleOverlap(Vector2 posA, Vector2 posB)
        {
            this.posA = posA;
            this.posB = posB;
        }
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

