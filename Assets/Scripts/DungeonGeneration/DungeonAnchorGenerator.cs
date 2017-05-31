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
                    anchors.Add(new Anchor(nextAnchorId++, x, y, AnchorType.CENTER));
                }

                // Bottom left corner
                if (!room.GetDoorPositions().Exists(v => v.x == room.x && v.y == room.y))
                    anchors.Add(new Anchor(nextAnchorId++, room.x, room.y, AnchorType.CORNER));

                // Bottom right corner
                if (!room.GetDoorPositions().Exists(v => v.x == room.x + room.width - 1 && v.y == room.y))
                    anchors.Add(new Anchor(nextAnchorId++, room.x + room.width - 1.0f, room.y, AnchorType.CORNER));

                // Top left corner
                if (!room.GetDoorPositions().Exists(v => v.x == room.x && v.y == room.y + room.height - 1))
                    anchors.Add(new Anchor(nextAnchorId++, room.x, room.y + room.height - 1.0f, AnchorType.CORNER));

                // Top right corner
                if (!room.GetDoorPositions().Exists(v => v.x == room.x + room.width - 1 && v.y == room.y + room.height - 1))
                    anchors.Add(new Anchor(nextAnchorId++, room.x + room.width - 1.0f, room.y + room.height - 1.0f, AnchorType.CORNER));
            }
        }

        public List<Anchor> GetAnchors()
        {
            return anchors;
        }

        public void Display(GameObject parent)
        {
            foreach (Anchor anchor in anchors)
            {
                anchor.Display(parent, anchorPrefab);
            }
        }
    }
}