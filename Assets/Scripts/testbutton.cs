using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class testbutton : MonoBehaviour {
    private ButtonController b;
    // Use this for initialization
    void Start () {
        b = GetComponent<ButtonController>();
	}
	
	// Update is called once per frame
	void Update () {
        if (b.Pressed()) {
            Debug.Log("clicked");
        }
    }
}
