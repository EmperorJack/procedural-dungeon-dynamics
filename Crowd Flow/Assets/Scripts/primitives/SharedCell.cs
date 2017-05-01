using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Shared Cell information, i.e. independant of groups
public class SharedCell : Cell
{
	public float density, height, discomfort;
	public Vector2 avg_Velocity;

	public List<Face> sharedCellFaces = new List<Face> ();

	public SharedCell (Vector2 pos) : base (pos)
	{
	}
}