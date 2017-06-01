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

        public AnchorType type;

        public Anchor(int id, float x, float y, AnchorType type)
        {
            this.id = id;
            this.x = x;
            this.y = y;
            this.type = type;
        }

        public void Display(GameObject parent, GameObject anchorPrefab)
        {
            GameObject instance = MonoBehaviour.Instantiate(anchorPrefab);
            instance.name = type.ToString().ToLower() + this.GetType().Name + id;
            instance.transform.SetParent(parent.transform);
            instance.transform.Translate(x, 0.5f, y);
        }
    }

    public enum AnchorType {
        CENTER,
        CORNER
    }
}
