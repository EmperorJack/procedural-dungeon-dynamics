using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace DungeonGeneration {

	public static class CorridorBuilder
	{

        private struct PossiblePlacement
        {
            public int x;
            public int y;
            public ConnectableGridArea area;

            public PossiblePlacement(int x, int y, ConnectableGridArea area)
            {
                this.x = x;
                this.y = y;
                this.area = area;
            }
        }

        private struct PossibleOverlap
	    {
	        public Vector2 posA;
	        public Vector2 posB;
            public ConnectableGridArea areaA;
            public ConnectableGridArea areaB;

	        public PossibleOverlap(Vector2 posA, Vector2 posB, ConnectableGridArea areaA, ConnectableGridArea areaB)
	        {
	            this.posA = posA;
	            this.posB = posB;
                this.areaA = areaA;
                this.areaB = areaB;
	        }
	    }

	    public static Corridor CreateCorridor(DungeonLayoutGenerator generator, Partition partitionA, Partition partitionB, bool horizontalCut)
	    {
	        List<ConnectableGridArea> areasA = new List<ConnectableGridArea>();
	        partitionA.GetRooms(areasA);
            partitionA.GetCorridors(areasA);

	        List<ConnectableGridArea> areasB = new List<ConnectableGridArea>();
	        partitionB.GetRooms(areasB);
            partitionB.GetCorridors(areasB);

            List<PossiblePlacement> rangeA = new List<PossiblePlacement>();
	        List<PossiblePlacement> rangeB = new List<PossiblePlacement>();

            // Compute the possible placement points for both partitions
	        if (horizontalCut)
	        {
                int xStart;
                int xEnd;
	            foreach (ConnectableGridArea area in areasA)
	            {
                    xStart = area.GetType() == typeof(Room) ? area.x : area.x + 1;
                    xEnd = area.GetType() == typeof(Room) ? area.x + area.width : area.x + area.width - 1;

                    for (int x = xStart; x < xEnd; x++)
	                {
	                    int xMatchIndex = rangeA.FindIndex(v => v.x == x);
	                    if (xMatchIndex != -1)
	                    {
	                        if (area.y + area.height > rangeA[xMatchIndex].y) rangeA[xMatchIndex] = new PossiblePlacement(x, area.y + area.height, area);
	                    }
	                    else
	                    {
	                        rangeA.Add(new PossiblePlacement(x, area.y + area.height, area));
	                    }
	                }
	            }
	            foreach (ConnectableGridArea area in areasB)
	            {
                    xStart = area.GetType() == typeof(Room) ? area.x : area.x + 1;
                    xEnd = area.GetType() == typeof(Room) ? area.x + area.width : area.x + area.width - 1;

                    for (int x = xStart; x < xEnd; x++)
                    {
	                    int xMatchIndex = rangeB.FindIndex(v => v.x == x);
	                    if (xMatchIndex != -1)
	                    {
	                        if (area.y < rangeB[xMatchIndex].y) rangeB[xMatchIndex] = new PossiblePlacement(x, area.y, area);
	                    }
	                    else
	                    {
	                        rangeB.Add(new PossiblePlacement(x, area.y, area));
	                    }
	                }
	            }
	        }
	        else // Vertical cut
	        {
                int yStart;
                int yEnd;
                foreach (ConnectableGridArea area in areasA)
	            {
                    yStart = area.GetType() == typeof(Room) ? area.y : area.y + 1;
                    yEnd = area.GetType() == typeof(Room) ? area.y + area.height : area.y + area.height - 1;

                    for (int y = yStart; y < yEnd; y++)
	                {
	                    int yMatchIndex = rangeA.FindIndex(v => v.y == y);
	                    if (yMatchIndex != -1)
	                    {
	                        if (area.x + area.width > rangeA[yMatchIndex].x) rangeA[yMatchIndex] = new PossiblePlacement(area.x + area.width, y, area);
	                    }
	                    else
	                    {
	                        rangeA.Add(new PossiblePlacement(area.x + area.width, y, area));
	                    }
	                }
	            }
	            foreach (ConnectableGridArea area in areasB)
	            {
                    yStart = area.GetType() == typeof(Room) ? area.y : area.y + 1;
                    yEnd = area.GetType() == typeof(Room) ? area.y + area.height : area.y + area.height - 1;

                    for (int y = yStart; y < yEnd; y++)
	                {
	                    int yMatchIndex = rangeB.FindIndex(v => v.y == y);
	                    if (yMatchIndex != -1)
	                    {
	                        if (area.x < rangeB[yMatchIndex].x) rangeB[yMatchIndex] = new PossiblePlacement(area.x, y, area);
	                    }
	                    else
	                    {
	                        rangeB.Add(new PossiblePlacement(area.x, y, area));
	                    }
	                }
	            }
	        }

            List<PossiblePlacement> removeFromRangeA = new List<PossiblePlacement>();
            List<PossiblePlacement> removeFromRangeB = new List<PossiblePlacement>();

            // Compute and remove any possible placements that overlap with the room buffer
            if (horizontalCut)
            {
                foreach (PossiblePlacement pos in rangeA)
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
                foreach (PossiblePlacement pos in rangeB)
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
                foreach (PossiblePlacement pos in rangeA)
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
                foreach (PossiblePlacement pos in rangeB)
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
            foreach (PossiblePlacement posA in rangeA)
            {
                foreach (PossiblePlacement posB in rangeB)
                {
					if ((horizontalCut && posA.x == posB.x) || (!horizontalCut && posA.y == posB.y))
                        overlap.Add(new PossibleOverlap(new Vector3(posA.x, posA.y), new Vector3(posB.x, posB.y), posA.area, posB.area));
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

            Corridor corridor = new Corridor(id, generator, xFinal, yFinal, widthFinal, heightFinal, !horizontalCut);

            // Setup connection relationships between areas
            chosenOverlap.areaA.AddConnectedCorridor(corridor);
            chosenOverlap.areaB.AddConnectedCorridor(corridor);
            chosenOverlap.areaA.AddDoorPosition(new Vector2(chosenOverlap.posA.x - (horizontalCut ? 0.0f : 1.0f), chosenOverlap.posA.y - (horizontalCut ? 1.0f : 0.0f)));
            chosenOverlap.areaB.AddDoorPosition(new Vector2(chosenOverlap.posB.x, chosenOverlap.posB.y));

            return corridor;
        }
	}
}