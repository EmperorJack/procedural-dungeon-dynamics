using System;
using UnityEngine;

namespace Primitives
{
	public class Cell
	{
		public Vector2 position;
		public Vector2 index;

		public bool obstruction;

		public enum Dir : int
		{
			east,
			south,
			west,
			north}

		;

		public Face[] faces = new Face[4];

		public void setFace (Dir dir, Face face)
		{
			faces [(int)dir] = face;
		}

		public Face getFace (Dir dir)
		{
			return faces [(int)dir];
		}

		public Cell (Vector2 pos, Vector2 index)
		{
			this.position = pos;
			this.index = index;
		}

	}
}

