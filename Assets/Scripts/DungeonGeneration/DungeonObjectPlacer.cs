using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace DungeonGeneration
{

    public class DungeonObjectPlacer : MonoBehaviour
    {

        // User set fields
        public GameObject cornerObjectPrefab;
        public GameObject centerObjectPrefab;

        // Anchor fields
        private List<Anchor> anchors;

        public void Setup(List<Anchor> anchors)
        {
            this.anchors = anchors;
        }

        public void Populate(GameObject parent)
        {
            if (anchors == null) throw new Exception("Dungeon object placer not setup!");

            foreach (Anchor anchor in anchors)
            {
                GameObject selectedPrefab = null;

                if (anchor.type == AnchorType.CENTER && centerObjectPrefab != null)
                {
                    selectedPrefab = centerObjectPrefab;
                }
                else if (anchor.type == AnchorType.CORNER && centerObjectPrefab != null)
                {
                    selectedPrefab = cornerObjectPrefab;
                }

                if (selectedPrefab != null)
                {
                    GameObject instance = MonoBehaviour.Instantiate(selectedPrefab);
                    instance.transform.SetParent(parent.transform);
                    instance.transform.Translate(anchor.x, 0.1f, anchor.y);
                }
            }
        }
    }
}
