using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonGeneration {
	public static class RoomBuilder
	{

	    public static Room CreateRoom(DungeonGenerator generator, Partition partition)
	    {
	        int xArea = (partition.width - generator.minimumRoomSize) / 2;
	        int yArea = (partition.height - generator.minimumRoomSize) / 2;

	        int x1 = partition.x + Random.Range(generator.roomBuffer, xArea);
	        int x2 = partition.x + Random.Range(xArea + generator.minimumRoomSize, partition.width - generator.roomBuffer);

	        int x = x1;
	        int width = x2 - x1;

	        int y1 = partition.y + Random.Range(generator.roomBuffer, yArea);
	        int y2 = partition.y + Random.Range(yArea + generator.minimumRoomSize, partition.height - generator.roomBuffer);

	        int y = y1;
	        int height = y2 - y1;

	        int id = generator.NextRoomId();

	        return new Room(id, generator, x, y, width, height);
	    }
	}
}