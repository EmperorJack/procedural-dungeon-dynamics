using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorCell : Cell
{

    private GameObject prefab;

    public FloorCell(Vector3 position, GameObject prefab) : base(position)
    {
        this.prefab = prefab;
    }

    public override GameObject Display()
    {
        Object instance = MonoBehaviour.Instantiate(prefab, position, Quaternion.Euler(90, 0, 0));
        return (GameObject) instance;
    }
}
