using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Primitives
{
	public class GroupFace : Face
	{
		public float grad_Potential;

		public GroupFace (Cell cell, Vector2 neighbourIndex, int index) : base (cell, neighbourIndex, index)
		{
		}
	}
}

