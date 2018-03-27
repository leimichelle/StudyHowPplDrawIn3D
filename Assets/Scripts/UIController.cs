using HoloToolkit.UI.Keyboard;
using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;
using UnityEngine.UI;

public class UIController : MonoBehaviour {
    public SaveDrawingAndSwtichModel sdasm;
    public GameObject speechRecognition;
    public bool userNameSubmitted = false;
    private TextMesh userName;


    // Use this for initialization
    void Awake() {
        userName = gameObject.GetComponentInChildren<TextMesh>();
        Keyboard.Instance.InputField.onEndEdit.AddListener(delegate { saveUserName(Keyboard.Instance.InputField); });
    }
    void OnEnable() {
        Keyboard.Instance.PresentKeyboard();
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            userName.gameObject.SetActive(false);
            Keyboard.Instance.Enter();
            userNameSubmitted = true;
            return;
        }
        gameObject.transform.position = Camera.main.transform.position + Camera.main.transform.forward.normalized * 1.0f;
        gameObject.transform.rotation = Camera.main.transform.rotation;
        userName.text = "You Name:\n" + Keyboard.Instance.InputField.text;
    }

    void saveUserName(InputField input) {
        sdasm.userName = input.text;
        speechRecognition.SetActive(true);
    }
}
