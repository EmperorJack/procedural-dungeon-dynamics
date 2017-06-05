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

					// Populate any nested objects if they exist
					if (instance.GetComponent<NestedObject> () != null)
					{
						PopulateNested (instance, parent);
					}
                }
            }
        }

		private void PopulateNested(GameObject instance, GameObject parent)
		{
			// Get all the types of nested objects this instance has
			List<NestedObject> nestedObjects = new List<NestedObject>(instance.GetComponents<NestedObject>());

			// Get the nested anchor points
			List<Transform> anchors = new List<Transform>();

			foreach (Transform transform in instance.transform)
			{
				if (transform.name.Contains("anchor")) anchors.Add(transform);
			}

			// Apply random offset and rotation to the anchor points

			// Instantiate the correct nested object at each anchor point
			foreach (Transform transform in anchors)
			{
				NestedObject nestedObject = nestedObjects.Find(n => transform.name.Contains(n.name));

				if (nestedObject.spawnChance > UnityEngine.Random.value)
				{
					GameObject nestedInstance = MonoBehaviour.Instantiate (nestedObject.prefab);
					nestedInstance.transform.SetParent (parent.transform);
					nestedInstance.transform.Translate (transform.position.x, instance.transform.position.y + 0.01f, transform.position.z);
				}

				// Remove the nested anchor point
				DestroyImmediate(transform.gameObject);
			}
		}
    }
}
