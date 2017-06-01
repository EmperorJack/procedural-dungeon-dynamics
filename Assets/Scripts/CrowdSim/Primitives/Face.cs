using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Primitives
{
	public class Face
	{
		public int index;

		public float cost, velocity;
		public Cell cell;

		//used only for Group faces
		public float groupVelocity, potentialGrad;

		public Face(int index){
			this.index = index;
		}

		public void reset(){
			cost = float.MaxValue;
			velocity = 0;
			groupVelocity = 0;
			potentialGrad = float.MaxValue;
		}
	}
}

