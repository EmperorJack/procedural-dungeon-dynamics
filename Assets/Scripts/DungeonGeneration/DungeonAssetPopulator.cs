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
		public GameObject torchPrefab;
        [Range(1, 10)] public int torchSpacing;

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
            if (rooms == null || corridors == null) throw new Exception("Dungeon asset populator not setup!");

            PopulateRooms(parent);
            PopulateCorridors(parent);
        }

        private void PopulateRooms(GameObject parent)
        {
            foreach (Room room in rooms)
            {
                GameObject roomParent = new GameObject();
                roomParent.name = room.GetType().Name + room.id;
                roomParent.transform.SetParent(parent.transform);

                PopulateFloor(room, roomParent);
                PopulateWalls(room, roomParent);
				PopulateTorches(room, roomParent);
            }
        }

        private void PopulateCorridors(GameObject parent)
        {
            foreach (Corridor corridor in corridors)
            {
                GameObject corridorParent = new GameObject();
                corridorParent.name = corridor.GetType().Name + corridor.id;
                corridorParent.transform.SetParent(parent.transform);

                PopulateFloor(corridor, corridorParent);
                PopulateWalls(corridor, corridorParent);
                PopulateDoors(corridor, corridorParent);
				PopulateTorches(corridor, corridorParent);
            }
        }

        private void PopulateFloor(ConnectableGridArea area, GameObject parent)
        {
            if (floorPrefab == null) return;

            for (int i = 0; i < area.width; i++)
            {
                for (int j = 0; j < area.height; j++)
                {
					SpawnFloor(parent, new Vector3((area.x + i) * gridSpacing, 0, (area.y + j) * gridSpacing), 0);
                }
            }
        }

        private void PopulateWalls(ConnectableGridArea area, GameObject parent)
        {
            if (wallPrefab == null) return;

            if (area.GetType() != typeof(Corridor) || ((Corridor)area).horiztonal)
            {
                for (int i = 0; i < area.width; i++)
                {
                    // Bottom
                    if (area.GetConnectedCorridors().Find(c => c.x == area.x + i && c.y + c.height == area.y) == null)
                    {
						SpawnWall(parent, new Vector3((area.x + i) * gridSpacing, 0, (area.y) * gridSpacing), 0);
                    }

                    // Top
                    if (area.GetConnectedCorridors().Find(c => c.x == area.x + i && c.y == area.y + area.height) == null)
                    {
						SpawnWall(parent, new Vector3((area.x + i) * gridSpacing, 0, (area.y + area.height - 1) * gridSpacing), 180);
                    }
                }
            }

            if (area.GetType() != typeof(Corridor) || !((Corridor)area).horiztonal) // Vertical
            {
                for (int j = 0; j < area.height; j++)
                {
                    // Left
                    if (area.GetConnectedCorridors().Find(c => c.y == area.y + j && c.x + c.width == area.x) == null)
                    {
						SpawnWall(parent, new Vector3((area.x) * gridSpacing, 0, (area.y + j) * gridSpacing), 90);
                    }

                    // Right
                    if (area.GetConnectedCorridors().Find(c => c.y == area.y + j && c.x == area.x + area.width) == null)
                    {
						SpawnWall(parent, new Vector3((area.x + area.width - 1) * gridSpacing, 0, (area.y + j) * gridSpacing), 270);
                    }
                }
            }
        }

        private void PopulateDoors(Corridor corridor, GameObject parent)
        {
            if (doorPrefab == null) return;

            if (corridor.horiztonal)
            {
				SpawnDoor(parent, new Vector3((corridor.x) * gridSpacing, 0, (corridor.y) * gridSpacing), 90);
				SpawnDoor(parent, new Vector3((corridor.x + corridor.width - 1) * gridSpacing, 0, (corridor.y) * gridSpacing), 270);
            }
            else // Vertical
            {
				SpawnDoor(parent, new Vector3((corridor.x) * gridSpacing, 0, (corridor.y) * gridSpacing), 0);
				SpawnDoor(parent, new Vector3((corridor.x) * gridSpacing, 0, (corridor.y + corridor.height - 1) * gridSpacing), 180);
            }
        }

		private void PopulateTorches(ConnectableGridArea area, GameObject parent)
		{
            if (torchPrefab == null) return;

            // Ensure the torch spacing is not 0
            int spacing = Mathf.Max(torchSpacing, 1);

            // Bottom
            for (int i = area.width - 2; i >= 0; i -= spacing)
            {
                SpawnTorch(parent, new Vector3((area.x + i) * gridSpacing, 0, (area.y) * gridSpacing), 0);
            }

            // Top
            for (int i = 0; i < area.width - 1; i += spacing)
			{
                SpawnTorch(parent, new Vector3((area.x + i) * gridSpacing, 0, (area.y + area.height) * gridSpacing), 180);
            }

            // Left
            for (int j = area.height - 1; j >= 1; j -= spacing)
            {
                SpawnTorch(parent, new Vector3((area.x - 1) * gridSpacing, 0, (area.y + j) * gridSpacing), 90);
            }

            // Right
            for (int j = 1; j < area.height; j += spacing)
            {
                SpawnTorch(parent, new Vector3((area.x + area.width - 1) * gridSpacing, 0, (area.y + j) * gridSpacing), 270);
            }
        }

		private void SpawnFloor(GameObject parent, Vector3 position, int rotation)
		{
			SpawnAsset(parent, position, rotation, floorPrefab);
		}

		private void SpawnWall(GameObject parent, Vector3 position, int rotation)
		{
			SpawnAsset(parent, position, rotation, wallPrefab);
		}

		private void SpawnDoor(GameObject parent, Vector3 position, int rotation)
		{
			SpawnAsset(parent, position, rotation, doorPrefab);
		}

		private void SpawnTorch(GameObject parent, Vector3 position, int rotation)
		{
			SpawnAsset(parent, position, rotation, torchPrefab);
		}

		private void SpawnAsset(GameObject parent, Vector3 position, int rotation, GameObject asset)
		{
			GameObject instance = MonoBehaviour.Instantiate (asset);
			instance.transform.SetParent(parent.transform);
			instance.transform.Translate(position);
			instance.transform.Rotate (0, rotation, 0);
		}
    }
}