using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonGeneration {

	public class Room : ConnectableGridArea
    {

	    public Room(int id, DungeonGenerator generator, int x, int y, int width, int height) : base(id, generator, x, y, width, height)
	    { }

        public override Color DisplayColor()
        {
            return new Color(1.0f, 0.0f, 0.0f);
        }

        public override int DisplayHeight()
        {
            return 0;
        }
    }
}