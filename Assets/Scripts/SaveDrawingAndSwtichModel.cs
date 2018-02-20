using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.UI;
using HoloToolkit.Sharing;

public class SaveDrawingAndSwtichModel : MonoBehaviour, ISpeechHandler {

	public Text endText;
	public GameObject sketch;
	public void OnSpeechKeywordRecognized(SpeechEventData eventData)
	{
		if (eventData.RecognizedText == "Next") {
			SaveDrawing ();
			SwitchDisplayModel ();
		}
	}

	private void SaveDrawing() {
		/*TODO*/
		//Save the sketch into a object file somehow?
		Transform sketchTransform = sketch.transform;
		foreach (Transform child in sketchTransform) {
            if(child.name!="PenTip") {
                Destroy(child.gameObject);
            }
		}
	}

	private void SwitchDisplayModel () {
		foreach (Transform child in transform) {
			Debug.Log (child.gameObject.name);
		}
		string activeModelName = "";
		foreach (Transform child in transform) {
			if (child.gameObject.activeSelf) {
				activeModelName = child.gameObject.name;
				Debug.Log ("Destroying: "+child.gameObject.name);
				Destroy (child.gameObject);
				break;
			}
		}

		if (transform.childCount > 1) {
			foreach (Transform child in transform) {
				if (child.gameObject.name != activeModelName) {
					Debug.Log ("Activating: "+child.gameObject.name);
					child.gameObject.SetActive (true);
					break;
				}
			}
		}
		else {
			endText.text = "This is The End.\nThank You for Your Participation :)";
		}
	}
}
