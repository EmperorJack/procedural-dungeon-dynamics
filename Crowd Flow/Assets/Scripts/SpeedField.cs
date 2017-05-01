using System;
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

	float getTopSpeed (float f_max, float f_min, float s_max, float s_min, float height_grad)
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

	void assignSpeeds(List<Grid> groups){
		foreach (Grid group_grid in groups) {
			
		}
	}

}


