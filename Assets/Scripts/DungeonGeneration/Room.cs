using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonGeneration {

	public class Room : GridArea
	{

        public List<Corridor> connectedCorridors;

	    public Room(int id, DungeonGenerator generator, int x, int y, int width, int height) : base(id, generator, x, y, width, height)
	    {
            connectedCorridors = new List<Corridor>();
        }

        public override Color DisplayColor()
        {
            return new Color(1.0f, 0.0f, 0.0f);
        }

        public override int DisplayHeight()
        {
            return 0;
        }

        public void AddConnectedCorridor(Corridor corridor)
        {
            connectedCorridors.Add(corridor);
        }

        public override void Populate(DungeonAssetPopulator dungeonAssetPopulator, GameObject parent)
        {
            GameObject roomParent = new GameObject();
            roomParent.name = this.GetType().Name + id;
            roomParent.transform.SetParent(parent.transform);

            PopulateFloor(roomParent, dungeonAssetPopulator.floorPrefab);
            PopulateWalls(roomParent, dungeonAssetPopulator.wallPrefab);
        }

        private void PopulateFloor(GameObject parent, GameObject floorPrefab)
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

        private void PopulateWalls(GameObject parent, GameObject wallPrefab)
        {
            for (int i = 0; i < width; i++)
            {
                GameObject instance = MonoBehaviour.Instantiate(wallPrefab);
                instance.transform.SetParent(parent.transform);
                instance.transform.Translate((x + i) * generator.GetGridSpacing(), DisplayHeight(), (y) * generator.GetGridSpacing());

                instance = MonoBehaviour.Instantiate(wallPrefab);
                instance.transform.SetParent(parent.transform);
                instance.transform.Translate((x + i) * generator.GetGridSpacing(), DisplayHeight(), (y + height - 1) * generator.GetGridSpacing());
                instance.transform.Rotate(0, 180, 0);
            }

            for (int j = 0; j < height; j++)
            {
                GameObject instance = MonoBehaviour.Instantiate(wallPrefab);
                instance.transform.SetParent(parent.transform);
                instance.transform.Translate((x) * generator.GetGridSpacing(), DisplayHeight(), (y + j) * generator.GetGridSpacing());
                instance.transform.Rotate(0, 90, 0);

                instance = MonoBehaviour.Instantiate(wallPrefab);
                instance.transform.SetParent(parent.transform);
                instance.transform.Translate((x + width - 1) * generator.GetGridSpacing(), DisplayHeight(), (y + j) * generator.GetGridSpacing());
                instance.transform.Rotate(0, 270, 0);
            }
        }
    }
}