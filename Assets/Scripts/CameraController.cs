using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public float moveSpeed = 1.0f;
    public float turnSpeed = 10.0f;

	private bool animateCamera = false;

	public GameObject positionsParent;
	private List<Transform> positions;
	public List<float> keyFrames;

	public void Start() {
		positions = new List<Transform>();

		if (positionsParent != null) {
			foreach (Transform transform in positionsParent.transform) {
				positions.Add(transform);
			}
		}

		if (positions.Count > 0 && keyFrames.Count > 0) {
			animateCamera = true;
			transform.position = Vector3.zero;
			transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
		}
	}

    public void Update ()
    {
        float scaledMoveSpeed = moveSpeed / 10.0f;
        float scaledTurnSpeed = turnSpeed / 10.0f;

		if (!animateCamera) {
			if (Input.GetKey (KeyCode.W)) {
				transform.Translate (0.0f, 0.0f, scaledMoveSpeed);
			} else if (Input.GetKey (KeyCode.S)) {
				transform.Translate (0.0f, 0.0f, -scaledMoveSpeed);
			}

			if (Input.GetKey (KeyCode.A)) {
				transform.Rotate (0.0f, -scaledTurnSpeed, 0.0f);
			} else if (Input.GetKey (KeyCode.D)) {
				transform.Rotate (0.0f, scaledTurnSpeed, 0.0f);
			}

			if (Input.GetKey (KeyCode.E)) {
				transform.Translate (0.0f, -scaledMoveSpeed, 0.0f);
			} else if (Input.GetKey (KeyCode.Q)) {
				transform.Translate (0.0f, scaledMoveSpeed, 0.0f);
			}
		} else {
			float currentTime = Time.time;

			int fromIndex = keyFrames.Count - 1;
			int toIndex = keyFrames.Count - 1;

			for (int i = 0; i < keyFrames.Count; i++) {
				if (keyFrames[i] < currentTime) {
					fromIndex = i;
					toIndex = i + 1;
				}
			}

			if (toIndex >= keyFrames.Count)
				toIndex = fromIndex;

			Vector3 fromPos = positions [fromIndex].position;
			Vector3 toPos = positions [toIndex].position;

			Quaternion fromRot = positions [fromIndex].rotation;
			Quaternion toRot = positions [toIndex].rotation;

			float t = (currentTime - keyFrames[fromIndex]) / (keyFrames[toIndex] - keyFrames[fromIndex]);

			transform.position = Vector3.Lerp (fromPos, toPos, t);
			transform.rotation = Quaternion.Lerp (fromRot, toRot, t);
		}
    }
}
