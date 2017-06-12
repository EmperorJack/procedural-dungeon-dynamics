using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Primitives
{
	public class Cell : Utilities.GridCell
	{
		public bool exists;

		public Vector2 position, avgVelocity;
		public float density, discomfort;
		public Face[] faces;

		//Used only for group cells
		public float potential;
		public Vector2 potGrad;
		public float tempPotential;
		public bool isGoal;
		public bool isAccepted = false;
		public Cell sharedCell; // same sharedCell across all groups
		public Vector2 groupVelocity;

		public int[] index;
		public float maxDensity;

		public Cell(int[] index){
			this.index = index;
		}

		public void reset(){
			foreach (Face face in faces) {
				face.reset ();
			}
			avgVelocity = new Vector2 (0, 0);
			density = 0;
			discomfort = 0;
			potential = float.MaxValue;
			tempPotential = float.MaxValue;
			isAccepted = false;
		}

		public Vector2 getPosition(){
			return position;
		}

	}
}

