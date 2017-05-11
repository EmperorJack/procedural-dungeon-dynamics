using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonGeneration {
	public class FloorCell : Cell
	{

	    private GameObject prefab;

	    public FloorCell(GameObject prefab)
	    {
	        this.prefab = prefab;
	    }

	    public override GameObject Display()
	    {
			Object instance = MonoBehaviour.Instantiate(prefab, Vector3.zero, Quaternion.Euler(90, 0, 0));
	        return (GameObject) instance;
	    }
	}
}