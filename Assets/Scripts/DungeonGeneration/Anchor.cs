using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonGeneration
{
    public class Anchor
    {
        public int id;

        // Worldspace fields
        public float x;
        public float y;
        public float rotation;

        public AnchorType type;

        public Anchor(int id, float x, float y, float rotation, AnchorType type)
        {
            this.id = id;
            this.x = x;
            this.y = y;
            this.rotation = rotation;
            this.type = type;
        }

        public void Display(GameObject parent, GameObject anchorPrefab)
        {
            GameObject instance = MonoBehaviour.Instantiate(anchorPrefab);
            instance.name = type.ToString().ToLower() + this.GetType().Name + id;
            instance.transform.SetParent(parent.transform);
            instance.transform.Translate(x, 0.5f, y);
            instance.transform.Rotate(0.0f, rotation, 0.0f);
        }
    }

    public enum AnchorType {
        CENTER,
        CORNER,
        EDGE
    }
}
