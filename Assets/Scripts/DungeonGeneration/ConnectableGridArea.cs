using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonGeneration
{

    public abstract class ConnectableGridArea : GridArea
    {

        protected List<Corridor> connectedCorridors;
        protected List<Vector2> doorPositions;

        public ConnectableGridArea(int id, DungeonLayoutGenerator generator, int x, int y, int width, int height) : base(id, generator, x, y, width, height)
        {
            connectedCorridors = new List<Corridor>();
            doorPositions = new List<Vector2>();
        }

        public List<Corridor> GetConnectedCorridors()
        {
            return connectedCorridors;
        }

        public void AddConnectedCorridor(Corridor corridor)
        {
            connectedCorridors.Add(corridor);
        }

        public List<Vector2> GetDoorPositions()
        {
            return doorPositions;
        }

        public void AddDoorPosition(Vector2 doorPosition)
        {
            doorPositions.Add(doorPosition);
        }
    }
}