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

        public void Setup(List<Room> rooms, List<Corridor> corridors)
        {
            this.rooms = rooms;
            this.corridors = corridors;
        }

        public void Populate(GameObject parent)
        {
            if (rooms == null || corridors == null) throw new Exception("Dungeon populator not setup!");

            PopulateRooms(parent);
            PopulateCorridors(parent);
        }

        private void PopulateRooms(GameObject parent)
        {
            foreach (Room room in rooms) room.Populate(this, parent);
        }

        private void PopulateCorridors(GameObject parent)
        {
            foreach (Corridor corridor in corridors) corridor.Populate(this, parent);
        }
    }
}