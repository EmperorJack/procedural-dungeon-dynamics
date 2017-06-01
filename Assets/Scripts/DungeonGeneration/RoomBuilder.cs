using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonGeneration {

	public static class RoomBuilder
	{

	    public static Room CreateRoom(DungeonLayoutGenerator generator, Partition partition)
	    {
	        int xArea = (partition.width - generator.minimumRoomSize) / 2;
	        int yArea = (partition.height - generator.minimumRoomSize) / 2;

            int x;
            int y;
            int width;
            int height;

            float widthHeightRatio;
            int attemptsRemaining = 10;

            // Attempt to fit the room bounds within the width height ratio constraints
            do
            {
                int x1 = partition.x + Random.Range(generator.roomBuffer, xArea);
                int x2 = partition.x + Random.Range(xArea + generator.minimumRoomSize, partition.width - generator.roomBuffer);

                x = x1;
                width = x2 - x1;

                int y1 = partition.y + Random.Range(generator.roomBuffer, yArea);
                int y2 = partition.y + Random.Range(yArea + generator.minimumRoomSize, partition.height - generator.roomBuffer);

                y = y1;
                height = y2 - y1;

                widthHeightRatio = width / (float) height;

                attemptsRemaining--;
            }
            while ((widthHeightRatio > generator.maxRoomWidthHeightRatio ||
                   widthHeightRatio < generator.minRoomWidthHeightRatio) &&
                   attemptsRemaining > 0);

            int id = generator.NextRoomId();

	        return new Room(id, generator, x, y, width, height);
	    }
	}
}