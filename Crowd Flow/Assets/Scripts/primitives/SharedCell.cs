using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Shared Cell information, i.e. independant of groups
public class SharedCell : Cell
{
	public float density, height, discomfort;
	public Vector2 avg_Velocity;


	public SharedCell (Vector2 pos, Vector2 index) : base (pos, index)
	{
	}
}