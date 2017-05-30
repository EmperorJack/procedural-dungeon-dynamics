using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonGeneration
{

    public class DungeonAnchorGenerator : MonoBehaviour
    {

        // User set fields
        public GameObject anchorPrefab;

        // Anchor fields
        private int nextAnchorId;
        private List<Anchor> anchors;

        public void Generate(List<Room> rooms, float gridSpacing)
        {
            nextAnchorId = 0;
            anchors = new List<Anchor>();

            foreach (Room room in rooms)
            {
                if (room.width > 3 && room.height > 3)
                {
                    float x = (room.x + (room.width / 2.0f) - 0.5f) * gridSpacing;
                    float y = (room.y + (room.height / 2.0f) - 0.5f) * gridSpacing;
                    anchors.Add(new Anchor(nextAnchorId++, x, y, "Center"));
                }

                if (room.GetConnectedCorridors().Find(c => (c.x == room.x && c.y + c.height == room.y) ||
                                                           (c.y == room.y && c.x + c.width == room.x)) == null)
                    anchors.Add(new Anchor(nextAnchorId++, room.x, room.y, "Corner"));

                if (room.GetConnectedCorridors().Find(c => (c.x == room.x && c.y == room.y + room.height) ||
                                                           (c.y == room.y && c.x + c.width == room.x)) == null)
                    anchors.Add(new Anchor(nextAnchorId++, room.x + room.width - 1.0f, room.y, "Corner"));
                //anchors.Add(new Anchor(nextAnchorId++, room.x, room.y + room.height - 1.0f, "Corner"));
                //anchors.Add(new Anchor(nextAnchorId++, room.x + room.width - 1.0f, room.y + room.height - 1.0f, "Corner"));
            }
        }

        public void Display(GameObject parent)
        {
            foreach (Anchor anchor in anchors)
            {
                anchor.Display(parent, anchorPrefab);
            }
        }
    }

    public class Anchor
    {
        public int id;

        // Worldspace fields
        public float x;
        public float y;

        public string type;

        public Anchor(int id, float x, float y, string type)
        {
            this.id = id;
            this.x = x;
            this.y = y;
            this.type = type;
        }

        public void Display(GameObject parent, GameObject anchorPrefab)
        {
            GameObject instance = MonoBehaviour.Instantiate(anchorPrefab);
            instance.name = type + this.GetType().Name + id;
            instance.transform.SetParent(parent.transform);
            instance.transform.Translate(x, 0.5f, y);
        }
    }
}