using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace DungeonGeneration
{

    public class DungeonObjectPlacer : MonoBehaviour
    {

        // User set fields
        public List<GameObject> edgeObjectPrefabs;
        public Vector3 edgeTranslationOffset = new Vector3(0.0f, 0.0f, 0.0f);
        public Vector3 edgeRotationOffset = new Vector3(0.0f, 0.0f, 0.0f);
        [Range(0.0f, 1.0f)] public float edgeSpawnChance = 0.6f;

        public List<GameObject> cornerObjectPrefabs;
        public Vector3 cornerTranslationOffset = new Vector3(0.0f, 0.0f, 0.0f);
        public Vector3 cornerRotationOffset = new Vector3(0.0f, 0.0f, 0.0f);
        [Range(0.0f, 1.0f)] public float cornerSpawnChance = 1.0f;

        public List<GameObject> centerObjectPrefabs;
        public Vector3 centerTranslationOffset = new Vector3(0.0f, 0.0f, 0.0f);
        public Vector3 centerRotationOffset = new Vector3(0.0f, 0.0f, 0.0f);
        [Range(0.0f, 1.0f)] public float centerSpawnChance = 0.4f;

        // Anchor fields
        private List<Anchor> anchors;

        // Object fields
        private List<GameObject> populatedObjects;

        public void Setup(List<Anchor> anchors)
        {
            this.anchors = anchors;
            this.populatedObjects = new List<GameObject>();
        }

        public void Populate(GameObject parent)
        {
            if (anchors == null) throw new Exception("Dungeon object placer not setup!");

            foreach (Anchor anchor in anchors)
            {
                GameObject selectedPrefab = null;

                if (anchor.type == AnchorType.CENTER && centerObjectPrefabs.Count > 0)
                {
                    selectedPrefab = centerObjectPrefabs[UnityEngine.Random.Range(0, centerObjectPrefabs.Count)];
                }
                else if (anchor.type == AnchorType.EDGE && edgeObjectPrefabs.Count > 0)
                {
                    selectedPrefab = edgeObjectPrefabs[UnityEngine.Random.Range(0, edgeObjectPrefabs.Count)];
                }
                else if (anchor.type == AnchorType.CORNER && centerObjectPrefabs.Count > 0)
                {
                    selectedPrefab = cornerObjectPrefabs[UnityEngine.Random.Range(0, cornerObjectPrefabs.Count)];
                }

                if (selectedPrefab != null && GetSpawnChance(anchor) > UnityEngine.Random.value)
                {
                    GameObject instance = MonoBehaviour.Instantiate(selectedPrefab);
                    instance.transform.SetParent(parent.transform);
                    instance.transform.Translate(anchor.x, 0.0f, anchor.y);
                    instance.transform.Rotate(0.0f, anchor.rotation, 0.0f);

                    // Apply random offset and rotation to the anchor point
                    instance.transform.Translate(GetTranslationOffset(anchor) * UnityEngine.Random.Range(-1.0f, 1.0f));
                    instance.transform.Rotate(GetRotationOffset(anchor) * UnityEngine.Random.Range(-1.0f, 1.0f));

                    populatedObjects.Add(instance);

                    // Populate any nested objects if they exist
                    if (instance.GetComponent<NestedObject> () != null)
					{
						PopulateNested (instance, parent, selectedPrefab);
					}
                }
            }
        }

        private Vector3 GetTranslationOffset(Anchor anchor)
        {
            if (anchor.type == AnchorType.CENTER)
            {
                return centerTranslationOffset;
            }
            else if (anchor.type == AnchorType.EDGE)
            {
                return edgeTranslationOffset;
            }
            else if (anchor.type == AnchorType.CORNER)
            {
                return cornerTranslationOffset;
            }

            return Vector3.zero;
        }

        private Vector3 GetRotationOffset(Anchor anchor)
        {
            if (anchor.type == AnchorType.CENTER)
            {
                return centerRotationOffset;
            }
            else if (anchor.type == AnchorType.EDGE)
            {
                return edgeRotationOffset;
            }
            else if (anchor.type == AnchorType.CORNER)
            {
                return cornerRotationOffset;
            }

            return Vector3.zero;
        }

        private float GetSpawnChance(Anchor anchor)
        {
            if (anchor.type == AnchorType.CENTER)
            {
                return centerSpawnChance;
            }
            else if (anchor.type == AnchorType.EDGE)
            {
                return edgeSpawnChance;
            }
            else if (anchor.type == AnchorType.CORNER)
            {
                return cornerSpawnChance;
            }

            return 1.0f;
        }

		private void PopulateNested(GameObject instance, GameObject parent, GameObject originalPrefab)
		{
			// Get all the types of nested objects this instance has
			List<NestedObject> nestedObjects = new List<NestedObject>(instance.GetComponents<NestedObject>());

			Debug.Log (nestedObjects.Count);

			// Get the nested anchor points
			List<Transform> anchors = new List<Transform>();

			// Check the prefab does not have another layer before we hit the nested anchor objects
			if (instance.transform.childCount == 1 && instance.transform.FindChild(originalPrefab.name) != null)
			{
				instance = instance.transform.FindChild(originalPrefab.name).gameObject;
			}

			foreach (Transform transform in instance.transform)
			{
				if (transform.name.Contains("anchor")) anchors.Add(transform);
			}

			Debug.Log ("NUM ANCHORS: " + anchors.Count);
				
			// Instantiate the correct nested object at each anchor point
			foreach (Transform transform in anchors)
			{
				NestedObject nestedObject = nestedObjects.Find(n => transform.name.Contains(n.name));

				if (nestedObject != null)
				{
					// Apply random offset and rotation to the anchor point
					transform.Translate (nestedObject.translationOffset * UnityEngine.Random.Range (-1.0f, 1.0f));
					transform.Rotate (nestedObject.rotationOffset * UnityEngine.Random.Range (-1.0f, 1.0f));

					if (nestedObject.spawnChance > UnityEngine.Random.value) {
						GameObject nestedInstance = MonoBehaviour.Instantiate (nestedObject.prefab);
						nestedInstance.transform.SetParent (parent.transform);
						nestedInstance.transform.Translate (transform.position.x, instance.transform.position.y, transform.position.z);
						nestedInstance.transform.Rotate (transform.rotation.eulerAngles);
						//nestedInstance.transform.RotateAround(transform.position, Vector3.up, transform.rotation.eulerAngles.y);

						populatedObjects.Add (nestedInstance);
					}
				}

				// Remove the nested anchor point
				DestroyImmediate(transform.gameObject);
			}
		}

        public List<GameObject> GetPopulatedObjects()
        {
            return populatedObjects;
        }
    }
}
