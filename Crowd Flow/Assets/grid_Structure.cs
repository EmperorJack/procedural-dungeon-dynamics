using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class grid_Structure : MonoBehaviour {

	public Vector3 start;
	public Vector3 end;

	// Use this for initialization
	void Start () {

		Gizmos.DrawLine (start, end);

	}
	
	// Update is called once per frame
	void Update () {
		Debug.DrawLine(Vector3.zero, new Vector3(1, 0, 0), Color.red);	
	}

	class Cell{
		double density;

		Cell(double density){
			this.density = density;
		}

		Cell(){ this.density = 0.0;}

	}
}
