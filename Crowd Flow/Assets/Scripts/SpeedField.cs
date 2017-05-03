using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedField
{
	Grid grid;

	public SpeedField (Grid grid)
	{
		this.grid = grid;

	}

	float getTerrSpeed (float f_max, float f_min, float s_max, float s_min, float height_grad)
	{
		if ((s_max - s_min) == 0) {
			return f_max;
		}

		return (f_max + ((height_grad - s_min) / (s_max - s_min)) * (f_min - f_max));
	}

	float getFlowSpeed(Face cell_face){
		SharedCell neighbour = (SharedCell)cell_face.neighbour;
		Vector2 offset = neighbour.position - cell_face.cell.position;
		float flow_speed = Vector2.Dot (neighbour.avg_Velocity, offset);
		return Mathf.Max (flow_speed, 0.01f);

	}

	public void assignSpeeds(List<GroupGrid> groups){
		foreach (Grid group_grid in groups) {
			GroupCell[,] grid = (GroupCell[,])group_grid.grid2;
			foreach (GroupCell cell in grid) {
				GroupFace[] faces = (GroupFace[])cell.faces;
				foreach (GroupFace face in faces) {
					float max_density = 5.0f;
					float min_density = 0.1f;

					float terr_speed = getTerrSpeed (0.5f,0.0f,0f,0f,0f);
					float flow_speed = getFlowSpeed (face);

					SharedCell neighbour = (SharedCell)face.neighbour;
					float density = neighbour.density;

					float final_speed;

					if (density >= max_density) {
						final_speed = flow_speed;
					} else if (density <= min_density) {
						final_speed = terr_speed;
					} else {
						final_speed = terr_speed + ((density-min_density)/(max_density-min_density)*(flow_speed-terr_speed));
					}

					Face shared_face = (Face)face;
					shared_face.velocity = final_speed;

				}
			}
		}
	}

}


