﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonGeneration {
	public class Room : GridArea
	{

	    public Room(int id, DungeonGenerator generator, int x, int y, int width, int height) : base(id, generator, x, y, width, height)
	    { }

	}
}