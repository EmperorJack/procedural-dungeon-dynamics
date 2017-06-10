using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DungeonGeneration
{

    public class DungeonAnchorGenerator : MonoBehaviour
    {

        // User set fields
        public GameObject anchorPrefab;
        [Range(1, 10)] public int centerSpacing = 1;
        [Range(1, 10)] public int edgeSpacing = 1;

        // Anchor fields
        private int nextAnchorId;
        private List<Anchor> anchors;

        public void Generate(List<Room> rooms, float gridSpacing)
        {
            nextAnchorId = 0;
            anchors = new List<Anchor>();

            foreach (Room room in rooms)
            {
                // Compute the edge positions of the room
                List<Vector2> edgePositions = new List<Vector2>();

                // Ensure the torch spacing is not 0
                int spacing = Mathf.Max(edgeSpacing, 1);

                // Bottom
                for (int i = room.width - 1; i >= 0; i -= spacing)
                {
                    Vector2 pos = new Vector2(room.x + i, room.y);
                    if (!edgePositions.Contains(pos) && !IsDoorPosition(room, pos))
                    {
                        edgePositions.Add(pos);
                        MakeEdgeAnchor(room, pos, 0);
                    }
                }

                // Top
                for (int i = 0; i < room.width - 1; i += spacing)
                {
                    Vector2 pos = new Vector2(room.x + i, room.y + room.height - 1);
                    if (!edgePositions.Contains(pos) && !IsDoorPosition(room, pos))
                    {
                        MakeEdgeAnchor(room, pos, 180);
                        edgePositions.Add(pos);
                    }
                }

                // Left
                for (int j = room.height - 1; j >= 1; j -= spacing)
                {
                    Vector2 pos = new Vector2(room.x, room.y + j);
                    if (!edgePositions.Contains(pos) && !IsDoorPosition(room, pos))
                    {
                        MakeEdgeAnchor(room, pos, 90);
                        edgePositions.Add(pos);
                    }
                }

                // Right
                for (int j = 1; j < room.height; j += spacing)
                {
                    Vector2 pos = new Vector2(room.x + room.width - 1, room.y + j);
                    if (!edgePositions.Contains(pos) && !IsDoorPosition(room, pos))
                    {
                        MakeEdgeAnchor(room, pos, 270);
                        edgePositions.Add(pos);
                    }
                }

                if (room.width > 3 && room.height > 3)
                {
                    float x = (room.x + (room.width / 2.0f) - 0.5f) * gridSpacing;
                    float y = (room.y + (room.height / 2.0f) - 0.5f) * gridSpacing;
                    anchors.Add(new Anchor(nextAnchorId++, x, y, 0.0f, AnchorType.CENTER));
                }
            }
        }

        private bool IsDoorPosition(Room room, Vector2 pos)
        {
            return room.GetDoorPositions().Exists((Vector2 v) => v.x == pos.x && v.y == pos.y);
        }

        private bool IsCorner(Room room, Vector2 pos)
        {
            return (pos.x == room.x && pos.y == room.y) ||
                   (pos.x == room.x + room.width - 1 && pos.y == room.y) ||
                   (pos.x == room.x && pos.y == room.y + room.height - 1) ||
                   (pos.x == room.x + room.width - 1 && pos.y == room.y + room.height - 1);
        }

        private void MakeEdgeAnchor(Room room, Vector2 pos, float rotation)
        {
            anchors.Add(new Anchor(nextAnchorId++, pos.x, pos.y, rotation, IsCorner(room, pos) ? AnchorType.CORNER : AnchorType.EDGE));
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