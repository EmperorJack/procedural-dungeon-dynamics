using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorchFlame : MonoBehaviour {

    public void Start()
    {
		Animator anim = GetComponent<Animator>();
		AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);
		anim.Play(state.fullPathHash, -1, Random.Range(0f, 1f));
    }

    public void Update()
    {
        Vector3 targetPostition = new Vector3(Camera.main.transform.position.x, this.transform.position.y, Camera.main.transform.position.z);
        this.transform.LookAt(targetPostition);
    }
}
