using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Partition : GridArea
{
    
    // Tree fields
    public int depth;
    public bool horizontalCut;
    public Partition left;
	public Partition right;
    private Room room;

    public Partition(DungeonGenerator generator, int x, int y, int width, int height, int depth) : base(generator.NextPartitionId(), generator, x, y, width, height)
    { }

    public void Print()
	{
		MonoBehaviour.print(x + ", " + y + " : " + width + ", " + height);

		if (left != null) left.Print();
		if (right != null) right.Print();
	}

	public void MakeParition()
	{
		int minimumSize = generator.minimumRoomSize + generator.roomBuffer * 2;

        // Check if this partition is big enough to cut
        if (this.width <= minimumSize * 2 || this.height <= minimumSize * 2) return;

        horizontalCut = true;
		if (Random.value > 0.5) horizontalCut = false;

		Partition partitionA;
		Partition partitionB;

		if (horizontalCut)
		{
			int yCut = Random.Range(generator.minimumRoomSize + generator.roomBuffer * 2, this.height - generator.minimumRoomSize - generator.roomBuffer * 2 + 1);
			partitionA = new Partition(generator, this.x, this.y, this.width, yCut, this.depth + 1);
			partitionB = new Partition(generator, this.x, this.y + yCut, this.width, this.height - yCut, this.depth + 1);
		}
		else // Vertical cut
		{
			int xCut = Random.Range(generator.minimumRoomSize + generator.roomBuffer * 2, this.width - generator.minimumRoomSize - generator.roomBuffer * 2 + 1);
			partitionA = new Partition(generator, this.x, this.y, xCut, this.height, this.depth + 1);
			partitionB = new Partition(generator, this.x + xCut, this.y, this.width - xCut, this.height, this.depth + 1);
		}

		this.left = partitionA; // Also top
		this.right = partitionB; // Also bottom

		left.MakeParition();
		right.MakeParition();
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
			room = RoomBuilder.CreateRoom(generator, this);
			rooms.Add(room);
		}
	}

	public void MakeCorridors(List<Corridor> corridors)
	{
		// Intermediate node
		if (left != null && right != null)
		{
			left.MakeCorridors(corridors);
			right.MakeCorridors(corridors);

			// Connect left and right partitions by corridor
			Corridor corridor = CorridorBuilder.CreateCorridor(generator, left, right, horizontalCut);
			corridors.Add(corridor);
		}
		else // Leaf node
		{
			return;
		}
	}

    public void GetRooms(List<Room> rooms)
    {
        // Intermediate node
        if (left != null && right != null)
        {
            left.GetRooms(rooms);
            right.GetRooms(rooms);
        }
        else // Leaf node
        {
            rooms.Add(room);
        }
    }

    public new void Display(GameObject dungeonParent)
    {
        base.Display(dungeonParent);

        if (left != null) left.Display(dungeonParent);
        if (right != null) right.Display(dungeonParent);
    }

    public new void Hide()
    {
        base.Hide();

        if (left != null) left.Hide();
        if (right != null) right.Hide();
    }
}
