using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonGeneration
{

    public abstract class ConnectableGridArea : GridArea
    {

        public List<Corridor> connectedCorridors;

        public ConnectableGridArea(int id, DungeonGenerator generator, int x, int y, int width, int height) : base(id, generator, x, y, width, height)
        {
            connectedCorridors = new List<Corridor>();
        }

        public void AddConnectedCorridor(Corridor corridor)
        {
            connectedCorridors.Add(corridor);
        }

        public void Populate(DungeonAssetPopulator dungeonAssetPopulator, GameObject parent)
        {
            GameObject roomParent = new GameObject();
            roomParent.name = this.GetType().Name + id;
            roomParent.transform.SetParent(parent.transform);

            PopulateFloor(roomParent, dungeonAssetPopulator.floorPrefab);
            PopulateWalls(roomParent, dungeonAssetPopulator.wallPrefab);
        }

        public void PopulateFloor(GameObject parent, GameObject floorPrefab)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    GameObject instance = MonoBehaviour.Instantiate(floorPrefab);
                    instance.transform.SetParent(parent.transform);
                    instance.transform.Translate((x + i) * generator.GetGridSpacing(), DisplayHeight(), (y + j) * generator.GetGridSpacing());
                }
            }
        }

        protected abstract void PopulateWalls(GameObject parent, GameObject wallPrefab);
    }
}