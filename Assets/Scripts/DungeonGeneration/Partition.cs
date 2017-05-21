using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonGeneration {

	public class Partition : GridArea
	{
	    
	    // Tree fields
	    public int depth;
	    public bool horizontalCut;
	    public Partition left;
		public Partition right;
	    private Room room;
        private Corridor corridor;

        public Partition(DungeonGenerator generator, int x, int y, int width, int height, int depth) : base(generator.NextPartitionId(), generator, x, y, width, height)
	    {
            this.depth = depth;
        }

	    public void Print()
		{
			MonoBehaviour.print(x + ", " + y + " : " + width + ", " + height);

			if (left != null) left.Print();
			if (right != null) right.Print();
		}

		public void MakeParition()
		{
			int minimumSize = generator.minimumRoomSize + generator.roomBuffer * 2;

            float widthHeightRatio = width / (float) height;

            // Check if this partition is big enough to cut
            if (width <= minimumSize * 2 && height <= minimumSize * 2) return;

            if (widthHeightRatio > generator.maxRoomWidthHeightRatio)
            {
                horizontalCut = false;
            }
            else if (widthHeightRatio < generator.minRoomWidthHeightRatio)
            {
                horizontalCut = true;
            }
            else horizontalCut = UnityEngine.Random.value > 0.5f;

			Partition partitionA;
			Partition partitionB;

			if (horizontalCut)
			{
                if (height <= minimumSize * 2) return;

                int yCut = UnityEngine.Random.Range(generator.minimumRoomSize + generator.roomBuffer * 2, height - generator.minimumRoomSize - generator.roomBuffer * 2 + 1);
				partitionA = new Partition(generator, x, y, width, yCut, depth + 1);
				partitionB = new Partition(generator, x, y + yCut, width, height - yCut, depth + 1);
			}
			else // Vertical cut
			{
                if (width <= minimumSize * 2) return;

                int xCut = UnityEngine.Random.Range(generator.minimumRoomSize + generator.roomBuffer * 2, width - generator.minimumRoomSize - generator.roomBuffer * 2 + 1);
				partitionA = new Partition(generator, x, y, xCut, height, depth + 1);
				partitionB = new Partition(generator, x + xCut, y, width - xCut, height, depth + 1);
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
				corridor = CorridorBuilder.CreateCorridor(generator, left, right, horizontalCut);
                corridors.Add(corridor);
			}
			else // Leaf node
			{
				return;
			}
		}

	    public void GetRooms(List<ConnectableGridArea> areas)
	    {
	        // Intermediate node
	        if (left != null && right != null)
	        {
	            left.GetRooms(areas);
	            right.GetRooms(areas);
	        }
	        else // Leaf node
	        {
                areas.Add(room);
	        }
	    }

        public void GetCorridors(List<ConnectableGridArea> areas)
        {
            // Intermediate node
            if (left != null && right != null)
            {
                left.GetCorridors(areas);
                right.GetCorridors(areas);
            }
            else // Leaf node
            {
                return;
            }

            areas.Add(corridor);
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

        public override Color DisplayColor()
        {
            return new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
        }

        public override int DisplayHeight()
        {
            return 20 - depth;
        }
    }
}
