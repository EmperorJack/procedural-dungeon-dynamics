using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Partition
{
	DungeonGenerator generator;

	// Worldspace fields
	public int x;
	public int y;
	public int width;
	public int height;

	// Children fields
	public Partition left;
	public Partition right;

	// Room fields
	public Room room;

	public Partition(DungeonGenerator generator, int x, int y, int width, int height)
	{
		this.generator = generator;
		this.x = x;
		this.y = y;
		this.width = width;
		this.height = height;
	}

	public void Print()
	{
		MonoBehaviour.print(x + ", " + y + " : " + width + ", " + height);

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
			room = new Room(generator, this);
			rooms.Add(room);
		}
	}
}
