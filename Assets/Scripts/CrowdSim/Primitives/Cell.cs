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
		public float tempPotential;
		public bool isGoal;
		public bool isAccepted;
		public Cell sharedCell; // same sharedCell across all groups

		public int[] index;

		public Cell(int[] index){
			this.index = index;
		}

		public void reset(){
			foreach (Face face in faces) {
				face.reset ();
			}
			avgVelocity = new Vector2 (0, 0);
			density = 0;
			potential = float.MaxValue;
			tempPotential = float.MaxValue;
			isAccepted = false;
		}

	}
}

