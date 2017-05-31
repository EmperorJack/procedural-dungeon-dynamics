using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public float moveSpeed = 1.0f;
    public float turnSpeed = 10.0f;

    public void Start ()
    {
		
	}

    public void Update ()
    {
        float scaledMoveSpeed = moveSpeed / 10.0f;
        float scaledTurnSpeed = turnSpeed / 10.0f;

        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(0.0f, 0.0f, scaledMoveSpeed);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(0.0f, 0.0f, -scaledMoveSpeed);
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(0.0f, -scaledTurnSpeed, 0.0f);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(0.0f, scaledTurnSpeed, 0.0f);
        }
    }
}
