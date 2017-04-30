using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyCell : Cell {

    public EmptyCell(Vector3 position) : base(position)
    {}

    public override GameObject Display()
    {
        return null;
    }
}
