using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;


// Shared Cell information, i.e. independant of groups
namespace Primitives
{

	public class SharedCell : Cell
	{
		public float density, height, discomfort;
		public Vector2 avg_Velocity;


		public SharedCell (Vector2 pos, Vector2 index) : base (pos, index)
		{
		}

		public String print(){
			StringBuilder sb = new StringBuilder (faces.Length+"- length");
			for (int i = 0; i < 4; i++) {
				if (faces [i] != null) {
					sb.Append("  Face("+i+") =" + faces[i].cost );
				} else {
					sb.Append("  Face("+i+") = NAN");	 
				}
			}
			return sb.ToString();
		}
	}
}