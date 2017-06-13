using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NestedObject : MonoBehaviour {

	public string name = "";
	public List<GameObject> prefabs;
    [Range(0.0f, 1.0f)] public float spawnChance = 0.5f;
	public Vector3 translationOffset = new Vector3(0.0f, 0.0f, 0.0f);
	public Vector3 rotationOffset = new Vector3(0.0f, 0.0f, 0.0f);

	public GameObject GetPrefab()
	{
		return prefabs[UnityEngine.Random.Range(0, prefabs.Count)];
	}

}
