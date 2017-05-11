using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Primitives
{
	public class Face
	{
		public float velocity;
		public float grad_Height;
		public float cost;


		public Cell cell;
		public Cell neighbour;
		public Vector2 neighbourIndex;

		public int index;

		public Face (Cell cell, Vector2 neighbourIndex, int index)
		{
			this.index = index;
			this.cell = cell;
			this.neighbourIndex = neighbourIndex;
		}
	}
}
