using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CrowdSim;

public class colliderScript : MonoBehaviour {

	private SimManager manager;
	private int groupId;

	public void Update(){

	}

	public void OnCollisionEnter(Collision collision){
		manager.removeAgent (collision.gameObject, groupId);
	}

	public void setManager(SimManager manager){
		this.manager = manager;
	}

	public void setGroupId(int groupId){
		this.groupId = groupId;
	}
}
