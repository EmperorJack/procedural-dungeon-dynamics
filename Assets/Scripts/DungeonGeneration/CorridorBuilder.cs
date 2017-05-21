using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace DungeonGeneration {

	public static class CorridorBuilder
	{

	    private struct PossibleOverlap
	    {
	        public Vector2 posA;
	        public Vector2 posB;

            public Room roomA;
            public Room roomB;

	        public PossibleOverlap(Vector2 posA, Vector2 posB, Room roomA, Room roomB)
	        {
	            this.posA = posA;
	            this.posB = posB;
                this.roomA = roomA;
                this.roomB = roomB;
	        }
	    }

	    public static Corridor CreateCorridor(DungeonGenerator generator, Partition partitionA, Partition partitionB, bool horizontalCut)
	    {
	        List<GridArea> areasA = new List<GridArea>();
	        partitionA.GetRooms(areasA);
            partitionA.GetCorridors(areasA);

	        List<GridArea> areasB = new List<GridArea>();
	        partitionB.GetRooms(areasB);
            partitionB.GetCorridors(areasB);

            List<Vector2> rangeA = new List<Vector2>();
	        List<Vector2> rangeB = new List<Vector2>();

            // Compute the possible placement points for both partitions
	        if (horizontalCut)
	        {
                int xStart;
                int xEnd;
	            foreach (GridArea area in areasA)
	            {
                    xStart = area.GetType() == typeof(Room) ? area.x : area.x + 1;
                    xEnd = area.GetType() == typeof(Room) ? area.x + area.width : area.x + area.width - 1;

                    for (int x = xStart; x < xEnd; x++)
	                {
	                    int xMatchIndex = rangeA.FindIndex(v => v.x == x);
	                    if (xMatchIndex != -1)
	                    {
	                        if (area.y + area.height > rangeA[xMatchIndex].y) rangeA[xMatchIndex] = new Vector2(x, area.y + area.height);
	                    }
	                    else
	                    {
	                        rangeA.Add(new Vector2(x, area.y + area.height));
	                    }
	                }
	            }
	            foreach (GridArea area in areasB)
	            {
                    xStart = area.GetType() == typeof(Room) ? area.x : area.x + 1;
                    xEnd = area.GetType() == typeof(Room) ? area.x + area.width : area.x + area.width - 1;

                    for (int x = xStart; x < xEnd; x++)
                    {
	                    int xMatchIndex = rangeB.FindIndex(v => v.x == x);
	                    if (xMatchIndex != -1)
	                    {
	                        if (area.y < rangeB[xMatchIndex].y) rangeB[xMatchIndex] = new Vector2(x, area.y);
	                    }
	                    else
	                    {
	                        rangeB.Add(new Vector2(x, area.y));
	                    }
	                }
	            }
	        }
	        else // Vertical cut
	        {
                int yStart;
                int yEnd;
                foreach (GridArea area in areasA)
	            {
                    yStart = area.GetType() == typeof(Room) ? area.y : area.y + 1;
                    yEnd = area.GetType() == typeof(Room) ? area.y + area.height : area.y + area.height - 1;

                    for (int y = yStart; y < yEnd; y++)
	                {
	                    int yMatchIndex = rangeA.FindIndex(v => v.y == y);
	                    if (yMatchIndex != -1)
	                    {
	                        if (area.x + area.width > rangeA[yMatchIndex].x) rangeA[yMatchIndex] = new Vector2(area.x + area.width, y);
	                    }
	                    else
	                    {
	                        rangeA.Add(new Vector2(area.x + area.width, y));
	                    }
	                }
	            }
	            foreach (GridArea area in areasB)
	            {
                    yStart = area.GetType() == typeof(Room) ? area.y : area.y + 1;
                    yEnd = area.GetType() == typeof(Room) ? area.y + area.height : area.y + area.height - 1;

                    for (int y = yStart; y < yEnd; y++)
	                {
	                    int yMatchIndex = rangeB.FindIndex(v => v.y == y);
	                    if (yMatchIndex != -1)
	                    {
	                        if (area.x < rangeB[yMatchIndex].x) rangeB[yMatchIndex] = new Vector2(area.x, y);
	                    }
	                    else
	                    {
	                        rangeB.Add(new Vector2(area.x, y));
	                    }
	                }
	            }
	        }

            List<Vector2> removeFromRangeA = new List<Vector2>();
            List<Vector2> removeFromRangeB = new List<Vector2>();

            // Compute and remove any possible placements that overlap with the room buffer
            if (horizontalCut)
            {
                foreach (Vector2 pos in rangeA)
                {
                    foreach (GridArea area in areasA.Where(a => a.GetType() == typeof(Room)))
                    {
                        if (area.y > pos.y &&
                            ((area.x - generator.roomBuffer <= pos.x && pos.x < area.x) ||
                            (area.x + area.width <= pos.x && pos.x < area.x + area.width + generator.roomBuffer)))
                        {
                            removeFromRangeA.Add(pos);
                        }
                    }
                }
                foreach (Vector2 pos in rangeB)
                {
                    foreach (GridArea area in areasB.Where(a => a.GetType() == typeof(Room)))
                    {
                        if (area.y + area.height < pos.y &&
                            ((area.x - generator.roomBuffer <= pos.x && pos.x < area.x) ||
                            (area.x + area.width <= pos.x && pos.x < area.x + area.width + generator.roomBuffer)))
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
                    foreach (GridArea area in areasA.Where(a => a.GetType() == typeof(Room)))
                    {
                        if (area.x > pos.x &&
                            ((area.y - generator.roomBuffer <= pos.y && pos.y < area.y) ||
                            (area.y + area.height <= pos.y && pos.y < area.y + area.height + generator.roomBuffer)))
                        {
                            removeFromRangeA.Add(pos);
                        }
                    }
                }
                foreach (Vector2 pos in rangeB)
                {
                    foreach (GridArea area in areasB.Where(a => a.GetType() == typeof(Room)))
                    {
                        if (area.x + area.width < pos.x &&
                            ((area.y - generator.roomBuffer <= pos.y && pos.y < area.y) ||
                            (area.y + area.height <= pos.y && pos.y < area.y + area.height + generator.roomBuffer)))
                        {
                            removeFromRangeB.Add(pos);
                        }
                    }
                }
            }

            rangeA.RemoveAll(v => removeFromRangeA.Contains(v));
            rangeB.RemoveAll(v => removeFromRangeB.Contains(v));

            List<PossibleOverlap> overlap = new List<PossibleOverlap>();

            // Compute the intersection points between the two partitions
            foreach (Vector2 posA in rangeA)
            {
                foreach (Vector2 posB in rangeB)
                {
					if ((horizontalCut && posA.x == posB.x) || (!horizontalCut && posA.y == posB.y)) overlap.Add(new PossibleOverlap(posA, posB, null, null));
                }
            }

            if (overlap.Count == 0)
            {
                throw new Exception("No possible was overlap found for corridor placement!");
            }

            // Choose a corridor placement
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

            return new Corridor(id, generator, xFinal, yFinal, widthFinal, heightFinal, !horizontalCut);
	    }
	}
}