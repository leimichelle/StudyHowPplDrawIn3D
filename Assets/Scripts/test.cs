using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class test : MonoBehaviour {
	public ButtonController b;
	private float x;
	// Use this for initialization
	void Start () {
		x = 1.0f;
	}
	
	// Update is called once per frame
	void Update () {
		if (b.Pressed()) {
			Debug.Log ("Detected mouse pressed!");
			GameObject pointObj = GameObject.CreatePrimitive (PrimitiveType.Sphere);
			pointObj.transform.position = new Vector3 (x, x, x);
			x += 1.0f;
			pointObj.GetComponent <Renderer> ().material.SetColor ("_Color", Color.black);
			pointObj.transform.SetParent (gameObject.transform);
		}
	}
}
