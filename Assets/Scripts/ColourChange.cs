using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using HoloToolkit.Unity.InputModule;

public class ColourChange : MonoBehaviour, ISpeechHandler {
	private TextMesh textMesh;
	// Use this for initialization
	void Awake () {
		textMesh = GetComponent<TextMesh> ();
	}
	
	// Update is called once per frame
	public void ChangeToBlue() {
		textMesh.color = Color.blue;
	}

	public void OnSpeechKeywordRecognized(SpeechEventData eventData)
	{
		ChangeToBlue();
	}
}
