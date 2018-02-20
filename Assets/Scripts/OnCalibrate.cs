using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using HoloToolkit.Unity.InputModule;

public class OnCalibrate : MonoBehaviour, ISpeechHandler {
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void RemovePen() {
		Destroy (gameObject);
	}

	public void OnSpeechKeywordRecognized(SpeechEventData eventData)
	{
		RemovePen ();
	}
}
