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
            anchors = new List<Anchor>();

            foreach (Room room in rooms)
            {
                if (room.width > 3 && room.height > 3)
                {
                    float x = (room.x + (room.width / 2.0f) - 0.5f) * gridSpacing;
                    float y = (room.y + (room.height / 2.0f) - 0.5f) * gridSpacing;
                    anchors.Add(new Anchor(nextAnchorId++, x, y));
                }
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

        public Anchor(int id, float x, float y)
        {
            this.id = id;
            this.x = x;
            this.y = y;
        }

        public void Display(GameObject parent, GameObject anchorPrefab)
        {
            GameObject instance = MonoBehaviour.Instantiate(anchorPrefab);
            instance.transform.SetParent(parent.transform);
            instance.transform.Translate(x, 0.5f, y);
        }
    }
}