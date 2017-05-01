using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Group cell information, i.e. for each goal, and hence it's potential

public class GroupCell : SharedCell
{
	public float potential;

	public List<GroupFace> groupCellFaces = new List<GroupFace> ();

	public GroupCell (Vector2 pos) : base (pos)
	{

	}

}
