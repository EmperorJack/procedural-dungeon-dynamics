using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Group cell information, i.e. for each goal, and hence it's potential

namespace Primitives
{

	public class GroupCell : Cell
	{
		public float potential;
		public float temporary_potential;

		public Boolean isCandidate = false;
		public Boolean isGoal = false;
		public Boolean isKnown = false;
		public Boolean isUnknown = false;

		public Vector2 avgVelocity;

		public Vector2 gradPotential;

		public GroupCell (Vector2 pos, Vector2 index) : base (pos, index)
		{

		}

		public String print(){
			String text = "GroupCell: " + index.x + " " + index.y;
			text = text + "\nPot: " + potential + " TempPot: " + temporary_potential;
			return text;
		}

	}
}
