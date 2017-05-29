using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonGeneration {

	public class Corridor : ConnectableGridArea
    {

        public bool horiztonal;

        public Corridor(int id, DungeonGenerator generator, int x, int y, int width, int height, bool horiztonal) : base(id, generator, x, y, width, height)
	    {
            this.horiztonal = horiztonal;
        }

        public override Color DisplayColor()
        {
            return new Color(0.0f, 0.0f, 1.0f);
        }

        public override int DisplayHeight()
        {
            return 0;
        }
    }
}