using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonGeneration
{

    public abstract class ConnectableGridArea : GridArea
    {

        protected List<Corridor> connectedCorridors;

        public ConnectableGridArea(int id, DungeonGenerator generator, int x, int y, int width, int height) : base(id, generator, x, y, width, height)
        {
            connectedCorridors = new List<Corridor>();
        }

        public List<Corridor> GetConnectedCorridors()
        {
            return connectedCorridors;
        }

        public void AddConnectedCorridor(Corridor corridor)
        {
            connectedCorridors.Add(corridor);
        }
    }
}