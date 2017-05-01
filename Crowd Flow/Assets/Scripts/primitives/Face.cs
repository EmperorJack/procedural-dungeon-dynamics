using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Face
{
	public float velocity;
	public float grad_Height;

	public Cell cell;
	public Cell neighbour;
	public Vector2 neighbourIndex;

	public Face (Cell cell, Vector2 neighbourIndex)
	{
		this.cell = cell;
		this.neighbourIndex = neighbourIndex;
	}
}
