using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonGeneration {

	public class Corridor : GridArea
	{

	    public Corridor(int id, DungeonGenerator generator, int x, int y, int width, int height) : base(id, generator, x, y, width, height)
	    { }

        public override Color DisplayColor()
        {
            return new Color(0.0f, 0.0f, Random.value + 0.5f);
        }

        public override int DisplayHeight()
        {
            return 0;
        }
    }
}