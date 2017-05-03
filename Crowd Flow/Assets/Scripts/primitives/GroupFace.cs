using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupFace : Face
{
	public float grad_Potential;

	public GroupFace (Cell cell, Vector2 neighbourIndex) : base (cell, neighbourIndex)
	{
	}
}

