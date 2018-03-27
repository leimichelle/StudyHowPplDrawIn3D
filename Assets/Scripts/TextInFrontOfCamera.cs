using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextInFrontOfCamera : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        gameObject.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 1.0f;
        gameObject.transform.rotation = Camera.main.transform.rotation;
	}
}
