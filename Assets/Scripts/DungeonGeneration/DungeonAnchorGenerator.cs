using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DungeonGeneration
{

    public class DungeonAnchorGenerator : PipelineComponent
    {

        // User set fields
        public GameObject anchorPrefab;
        [Range(1, 10)] public int centerSpacing = 3;
        [Range(1, 10)] public int edgeSpacing = 2;
        [Range(1, 10)] public int edgeBuffer = 1;

        // Anchor fields
        private int nextAnchorId;
        private List<Anchor> anchors;

        public override void ChangeValue(string targetField, float value)
        {
            if (targetField.Equals("centerSpacing")) centerSpacing = (int) value;
            else if (targetField.Equals("edgeSpacing")) edgeSpacing = (int) value;
            else if (targetField.Equals("edgeBuffer")) edgeBuffer = (int) value;
        }

        public void Generate(List<Room> rooms, float gridSpacing)
        {
            nextAnchorId = 0;
            anchors = new List<Anchor>();

            foreach (Room room in rooms)
            {
                GenerateCornerAnchors(room, gridSpacing);
                GenerateEdgeAnchors(room, gridSpacing);
                GenerateCenterAnchors(room, gridSpacing);
            }
        }

        private void GenerateCornerAnchors(Room room, float gridSpacing)
        {
            Vector2 pos;

            // Bottom left
            pos = new Vector2(room.x, room.y) * gridSpacing;
            if (!IsDoorPosition(room, pos))
            {
                MakeAnchor(pos, 45, AnchorType.CORNER);
            }

            // Bottom right
            pos = new Vector2(room.x + room.width - 1, room.y) * gridSpacing;
            if (!IsDoorPosition(room, pos))
            {
                MakeAnchor(pos, 315, AnchorType.CORNER);
            }

            // Top right
            pos = new Vector2(room.x + room.width - 1, room.y + room.height - 1) * gridSpacing;
            if (!IsDoorPosition(room, pos))
            {
                MakeAnchor(pos, 210, AnchorType.CORNER);
            }

            // Top left
            pos = new Vector2(room.x, room.y + room.height - 1) * gridSpacing;
            if (!IsDoorPosition(room, pos))
            {
                MakeAnchor(pos, 135, AnchorType.CORNER);
            }
        }

        private void GenerateEdgeAnchors(Room room, float gridSpacing)
        {
            // Ensure the torch spacing is not 0
            int spacing = Mathf.Max(edgeSpacing, 1);

            // Bottom
            for (int i = room.width - 2; i > 0; i -= spacing)
            {
                Vector2 pos = new Vector2(room.x + i, room.y) * gridSpacing;
                if (!IsDoorPosition(room, pos))
                {
                    MakeAnchor(pos, 0, AnchorType.EDGE);
                }
            }

            // Top
            for (int i = 1; i < room.width - 1; i += spacing)
            {
                Vector2 pos = new Vector2(room.x + i, room.y + room.height - 1) * gridSpacing;
                if (!IsDoorPosition(room, pos))
                {
                    MakeAnchor(pos, 180, AnchorType.EDGE);
                }
            }

            // Left
            for (int j = room.height - 2; j > 0; j -= spacing)
            {
                Vector2 pos = new Vector2(room.x, room.y + j) * gridSpacing;
                if (!IsDoorPosition(room, pos))
                {
                    MakeAnchor(pos, 90, AnchorType.EDGE);
                }
            }

            // Right
            for (int j = 1; j < room.height - 1; j += spacing)
            {
                Vector2 pos = new Vector2(room.x + room.width - 1, room.y + j) * gridSpacing;
                if (!IsDoorPosition(room, pos))
                {
                    MakeAnchor(pos, 270, AnchorType.EDGE);
                }
            }
        }

        private void GenerateCenterAnchors(Room room, float gridSpacing)
        {
            int xRange = room.width - edgeBuffer * 2;
            int yRange = room.height - edgeBuffer * 2;

            float xOffset = ((xRange - 1) % centerSpacing) / 2.0f;
            float yOffset = ((yRange - 1) % centerSpacing) / 2.0f;

            for (int i = edgeBuffer; i < room.width - edgeBuffer; i += centerSpacing)
            {
                for (int j = edgeBuffer; j < room.height - edgeBuffer; j += centerSpacing)
                {
                    Vector2 pos = new Vector2(room.x + i + xOffset, room.y + j + yOffset) * gridSpacing;
                    MakeAnchor(pos, 0, AnchorType.CENTER);
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

        private void MakeAnchor(Vector2 pos, float rotation, AnchorType type)
        {
            anchors.Add(new Anchor(nextAnchorId++, pos.x, pos.y, rotation, type));
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