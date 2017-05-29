using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace DungeonGeneration
{

    public class DungeonAssetPopulator : MonoBehaviour
    {

        // User set fields
        public GameObject floorPrefab;
        public GameObject wallPrefab;
        public GameObject doorPrefab;

        // Dungeon fields
        private List<Room> rooms;
        private List<Corridor> corridors;
        private float gridSpacing;

        public void Setup(List<Room> rooms, List<Corridor> corridors, float gridSpacing)
        {
            this.rooms = rooms;
            this.corridors = corridors;
            this.gridSpacing = gridSpacing;
        }

        public void Populate(GameObject parent)
        {
            if (rooms == null || corridors == null) throw new Exception("Dungeon populator not setup!");

            PopulateRooms(parent);
            PopulateCorridors(parent);
        }

        private void PopulateRooms(GameObject parent)
        {
            foreach (Room room in rooms)
            {
                GameObject roomParent = new GameObject();
                roomParent.name = this.GetType().Name + room.id;
                roomParent.transform.SetParent(parent.transform);

                PopulateFloor(room, roomParent);
                PopulateWalls(room, roomParent);
            }
        }

        private void PopulateCorridors(GameObject parent)
        {
            foreach (Corridor corridor in corridors)
            {
                GameObject corridorParent = new GameObject();
                corridorParent.name = this.GetType().Name + corridor.id;
                corridorParent.transform.SetParent(parent.transform);

                PopulateFloor(corridor, corridorParent);
                PopulateWalls(corridor, corridorParent);
                PopulateDoors(corridor, corridorParent);
            }
        }

        private void PopulateFloor(ConnectableGridArea area, GameObject parent)
        {
            for (int i = 0; i < area.width; i++)
            {
                for (int j = 0; j < area.height; j++)
                {
                    GameObject instance = MonoBehaviour.Instantiate(floorPrefab);
                    instance.transform.SetParent(parent.transform);
                    instance.transform.Translate((area.x + i) * gridSpacing, 0, (area.y + j) * gridSpacing);
                }
            }
        }

        private void PopulateWalls(ConnectableGridArea area, GameObject parent)
        {
            if (area.GetType() != typeof(Corridor) || ((Corridor)area).horiztonal)
            {
                for (int i = 0; i < area.width; i++)
                {
                    if (area.GetConnectedCorridors().Find(c => c.x == area.x + i && c.y + c.height == area.y) == null)
                    {
                        GameObject instance = MonoBehaviour.Instantiate(wallPrefab);
                        instance.transform.SetParent(parent.transform);
                        instance.transform.Translate((area.x + i) * gridSpacing, 0, (area.y) * gridSpacing);
                    }

                    if (area.GetConnectedCorridors().Find(c => c.x == area.x + i && c.y == area.y + area.height) == null)
                    {
                        GameObject instance = MonoBehaviour.Instantiate(wallPrefab);
                        instance.transform.SetParent(parent.transform);
                        instance.transform.Translate((area.x + i) * gridSpacing, 0, (area.y + area.height - 1) * gridSpacing);
                        instance.transform.Rotate(0, 180, 0);
                    }
                }
            }

            if (area.GetType() != typeof(Corridor) || !((Corridor)area).horiztonal) // Vertical
            {
                for (int j = 0; j < area.height; j++)
                {
                    if (area.GetConnectedCorridors().Find(c => c.y == area.y + j && c.x + c.width == area.x) == null)
                    {
                        GameObject instance = MonoBehaviour.Instantiate(wallPrefab);
                        instance.transform.SetParent(parent.transform);
                        instance.transform.Translate((area.x) * gridSpacing, 0, (area.y + j) * gridSpacing);
                        instance.transform.Rotate(0, 90, 0);
                    }

                    if (area.GetConnectedCorridors().Find(c => c.y == area.y + j && c.x == area.x + area.width) == null)
                    {
                        GameObject instance = MonoBehaviour.Instantiate(wallPrefab);
                        instance.transform.SetParent(parent.transform);
                        instance.transform.Translate((area.x + area.width - 1) * gridSpacing, 0, (area.y + j) * gridSpacing);
                        instance.transform.Rotate(0, 270, 0);
                    }
                }
            }
        }

        private void PopulateDoors(Corridor corridor, GameObject parent)
        {
            if (corridor.horiztonal)
            {
                GameObject instance = MonoBehaviour.Instantiate(doorPrefab);
                instance.transform.SetParent(parent.transform);
                instance.transform.Translate((corridor.x) * gridSpacing, 0, (corridor.y) * gridSpacing);
                instance.transform.Rotate(0, 90, 0);

                instance = MonoBehaviour.Instantiate(doorPrefab);
                instance.transform.SetParent(parent.transform);
                instance.transform.Translate((corridor.x + corridor.width - 1) * gridSpacing, 0, (corridor.y) * gridSpacing);
                instance.transform.Rotate(0, 270, 0);
            }
            else // Vertical
            {
                GameObject instance = MonoBehaviour.Instantiate(doorPrefab);
                instance.transform.SetParent(parent.transform);
                instance.transform.Translate((corridor.x) * gridSpacing, 0, (corridor.y) * gridSpacing);
                instance.transform.Rotate(0, 0, 0);

                instance = MonoBehaviour.Instantiate(doorPrefab);
                instance.transform.SetParent(parent.transform);
                instance.transform.Translate((corridor.x) * gridSpacing, 0, (corridor.y + corridor.height - 1) * gridSpacing);
                instance.transform.Rotate(0, 180, 0);
            }
        }
    }
}