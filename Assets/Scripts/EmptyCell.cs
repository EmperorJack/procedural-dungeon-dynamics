using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyCell : Cell {

	public EmptyCell()
    { }

    public override GameObject Display()
    {
        return null;
    }
}
