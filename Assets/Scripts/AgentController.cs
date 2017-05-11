using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AgentController : MonoBehaviour {

	public float speed;
	public Text countText;
	public Text winText;

	private Rigidbody rb;
	private int count;

	void Start()
	{
		rb = GetComponent<Rigidbody>();
	}

	void Updat()
	{

	}

	void FixedUpdate()
	{
//		float moveHorizontal = Input.GetAxis("Horizontal");
//		float moveVertical = Input.GetAxis("Vertical");
//
//		Vector3 movement = new Vector3 (moveHorizontal, 0, moveVertical);
//		if (Input.GetMouseButton(0)) {
//
//			Vector3 mPos = Input.mousePosition;
//			mPos.z = 10;
//			Vector3 pos = Camera.main.ScreenToWorldPoint (mPos);
//
//			//print (pos.x + " " + pos.y + " " + pos.z);
//			rb.position = new Vector3 (pos.x, rb.position.y, pos.z);
//		}
//
//		//rb.AddForce(movement * speed);
	}
		
}