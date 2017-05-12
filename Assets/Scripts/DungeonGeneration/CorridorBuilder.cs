using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace DungeonGeneration {

	public static class CorridorBuilder
	{

	    private struct PossibleOverlap
	    {
	        public Vector2 posA;
	        public Vector2 posB;

	        public PossibleOverlap(Vector2 posA, Vector2 posB)
	        {
	            this.posA = posA;
	            this.posB = posB;
	        }
	    }

	    public static Corridor CreateCorridor(DungeonGenerator generator, Partition partitionA, Partition partitionB, bool horizontalCut)
	    {
	        List<Room> roomsA = new List<Room>();
	        partitionA.GetRooms(roomsA);

	        List<Room> roomsB = new List<Room>();
	        partitionB.GetRooms(roomsB);

	        List<Vector2> rangeA = new List<Vector2>();
	        List<Vector2> rangeB = new List<Vector2>();

	        if (horizontalCut)
	        {
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
	        }

            List<Vector2> removeFromRangeA = new List<Vector2>();
            List<Vector2> removeFromRangeB = new List<Vector2>();

            if (horizontalCut)
            {
                foreach (Vector2 pos in rangeA)
                {
                    foreach (Room room in roomsA)
                    {
                        if (room.y > pos.y &&
                            ((room.x - generator.roomBuffer <= pos.x && pos.x < room.x) ||
                            (room.x + room.width <= pos.x && pos.x < room.x + room.width + generator.roomBuffer)))
                        {
                            removeFromRangeA.Add(pos);
                        }
                    }
                }
                foreach (Vector2 pos in rangeB)
                {
                    foreach (Room room in roomsB)
                    {
                        if (room.y + room.height < pos.y &&
                            ((room.x - generator.roomBuffer <= pos.x && pos.x < room.x) ||
                            (room.x + room.width <= pos.x && pos.x < room.x + room.width + generator.roomBuffer)))
                        {
                            removeFromRangeB.Add(pos);
                        }
                    }
                }
            }
            else // Vertical cut
            {
                foreach (Vector2 pos in rangeA)
                {
                    foreach (Room room in roomsA)
                    {
                        if (room.x > pos.x &&
                            ((room.y - generator.roomBuffer <= pos.y && pos.y < room.y) ||
                            (room.y + room.height <= pos.y && pos.y < room.y + room.height + generator.roomBuffer)))
                        {
                            removeFromRangeA.Add(pos);
                        }
                    }
                }
                foreach (Vector2 pos in rangeB)
                {
                    foreach (Room room in roomsB)
                    {
                        if (room.x + room.width < pos.x &&
                            ((room.y - generator.roomBuffer <= pos.y && pos.y < room.y) ||
                            (room.y + room.height <= pos.y && pos.y < room.y + room.height + generator.roomBuffer)))
                        {
                            removeFromRangeB.Add(pos);
                        }
                    }
                }
            }

            rangeA.RemoveAll(v => removeFromRangeA.Contains(v));
            rangeB.RemoveAll(v => removeFromRangeB.Contains(v));

            List<PossibleOverlap> overlap = new List<PossibleOverlap>();

            foreach (Vector2 posA in rangeA)
            {
                foreach (Vector2 posB in rangeB)
                {
					if ((horizontalCut && posA.x == posB.x) || (!horizontalCut && posA.y == posB.y)) overlap.Add(new PossibleOverlap(posA, posB));
                }
            }

            if (overlap.Count == 0)
            {
                throw new Exception("No possible was overlap found for corridor placement!");
            }

            PossibleOverlap chosenOverlap = overlap[UnityEngine.Random.Range(0, overlap.Count)];

            int xFinal = (int)chosenOverlap.posA.x;
            int yFinal = (int)chosenOverlap.posA.y;
            int widthFinal;
            int heightFinal;

            if (horizontalCut)
            {
                widthFinal = 1;
                heightFinal = (int)(chosenOverlap.posB.y - chosenOverlap.posA.y);
            }
            else // Vertical cut
            {
                widthFinal = (int)(chosenOverlap.posB.x - chosenOverlap.posA.x);
                heightFinal = 1;
            }

            int id = generator.NextCorridorId();

            return new Corridor(id, generator, xFinal, yFinal, widthFinal, heightFinal);
	    }
	}
}