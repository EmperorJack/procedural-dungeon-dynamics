using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonGeneration {

	public class Room : ConnectableGridArea
    {

	    public Room(int id, DungeonGenerator generator, int x, int y, int width, int height) : base(id, generator, x, y, width, height)
	    { }

        public override Color DisplayColor()
        {
            return new Color(1.0f, 0.0f, 0.0f);
        }

        public override int DisplayHeight()
        {
            return 0;
        }

        protected override void PopulateWalls(GameObject parent, GameObject wallPrefab)
        {
            for (int i = 0; i < width; i++)
            {
                if (connectedCorridors.Find(c => c.x == x + i && c.y + c.height == y) == null)
                {
                    GameObject instance = MonoBehaviour.Instantiate(wallPrefab);
                    instance.transform.SetParent(parent.transform);
                    instance.transform.Translate((x + i) * generator.GetGridSpacing(), DisplayHeight(), (y) * generator.GetGridSpacing());
                }

                if (connectedCorridors.Find(c => c.x == x + i && c.y == y + height) == null)
                {
                    GameObject instance = MonoBehaviour.Instantiate(wallPrefab);
                    instance.transform.SetParent(parent.transform);
                    instance.transform.Translate((x + i) * generator.GetGridSpacing(), DisplayHeight(), (y + height - 1) * generator.GetGridSpacing());
                    instance.transform.Rotate(0, 180, 0);
                }
            }
            
            for (int j = 0; j < height; j++)
            {
                if (connectedCorridors.Find(c => c.y == y + j && c.x + c.width == x) == null)
                {
                    GameObject instance = MonoBehaviour.Instantiate(wallPrefab);
                    instance.transform.SetParent(parent.transform);
                    instance.transform.Translate((x) * generator.GetGridSpacing(), DisplayHeight(), (y + j) * generator.GetGridSpacing());
                    instance.transform.Rotate(0, 90, 0);
                }

                if (connectedCorridors.Find(c => c.y == y + j && c.x == x + width) == null)
                {
                    GameObject instance = MonoBehaviour.Instantiate(wallPrefab);
                    instance.transform.SetParent(parent.transform);
                    instance.transform.Translate((x + width - 1) * generator.GetGridSpacing(), DisplayHeight(), (y + j) * generator.GetGridSpacing());
                    instance.transform.Rotate(0, 270, 0);
                }
            }
        }
    }
}