using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Cell {

    protected Vector3 position;

    public Cell(Vector3 position)
    {
        this.position = position;
    }

    public abstract GameObject Display();
}
