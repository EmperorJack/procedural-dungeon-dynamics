using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Primitives
{
	public class Cell 
	{
		public bool exists;

		public Vector2 position, avgVelocity;
		public float density;
		public Face[] faces;

		//Used only for group cells
		public float potential;
		public bool isGoal;

	}
}

