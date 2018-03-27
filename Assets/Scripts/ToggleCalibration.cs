using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UnityEngine;


/*[StructLayout(LayoutKind.Sequential)]
public struct PenData {
    public float x, y, z;
    public float qx, qy, qz, qw;
    public int valid;
}*/

public class ToggleCalibration : MonoBehaviour, ISpeechHandler {
    public bool calibrating = true;
    public GameObject cube;
    public GameObject vcToWorld;
    public GameObject cur_model;
    public GameObject sketch;
    public UIController uiController;
    public float translationSpeed;
    public GameObject speechRecognition;
    private enum Result { Success, Fail };
    private ViconClient vc;
    private PenData cd;

    void Awake() {
        vc = gameObject.GetComponent<ViconClient>();
    }

    private void Start() {

    }
    // Update is called once per frame
    void Update() {

        if (!calibrating) {
            bool y_up = Input.GetKey(KeyCode.PageUp);
            bool y_down = Input.GetKey(KeyCode.PageDown);
            bool x_right = Input.GetKey(KeyCode.RightArrow);
            bool x_left = Input.GetKey(KeyCode.LeftArrow);
            bool z_front = Input.GetKey(KeyCode.UpArrow);
            bool z_back = Input.GetKey(KeyCode.DownArrow);

            if (y_up)
                vcToWorld.transform.Translate(Vector3.up * translationSpeed * Time.deltaTime, Space.World);
            else if (y_down)
                vcToWorld.transform.Translate(Vector3.up * -translationSpeed * Time.deltaTime, Space.World);
            if (x_right)
                vcToWorld.transform.Translate(Camera.main.transform.right * translationSpeed * Time.deltaTime, Space.World);
            else if (x_left)
                vcToWorld.transform.Translate(Camera.main.transform.right * -translationSpeed * Time.deltaTime, Space.World);
            if (z_front)
                vcToWorld.transform.Translate(Camera.main.transform.forward * translationSpeed * Time.deltaTime, Space.World);
            else if (z_back)
                vcToWorld.transform.Translate(Camera.main.transform.forward * -translationSpeed * Time.deltaTime, Space.World);
            UnityEngine.XR.WSA.HolographicSettings.SetFocusPointForFrame(cube.transform.position - Camera.main.transform.position);
        }
    }
    public void OnSpeechKeywordRecognized(SpeechEventData eventData) {
        if (eventData.RecognizedText == "Calibrate") {
            if (calibrating) {
                if (!uiController.userNameSubmitted) {
                    uiController.enabled = true;
                    speechRecognition.SetActive(false);
                }
                cd = vc.FetchFrame("Cube");
                ComputeViconToHoloTransform();
                cube.SetActive(false);
                vcToWorld.SetActive(true);
                cur_model.SetActive(true);
                cur_model.transform.parent.position = cube.transform.position;
                sketch.SetActive(true);
                calibrating = false;
            }
            else {
                cube.SetActive(true);
                vcToWorld.SetActive(false);
                cur_model.SetActive(false);
                sketch.SetActive(false);
                calibrating = true;
            }
        }
    }

    private Result ComputeViconToHoloTransform() {
        Quaternion vicon_q = convertToLeftHandRotation(cd.qx, cd.qy, cd.qz, cd.qw);
        Vector3 vicon_pos = convertToLeftHandPosition(cd.x, cd.y, cd.z);
        Matrix4x4 vicon = Matrix4x4.Translate(vicon_pos) * Matrix4x4.Rotate(vicon_q);
        //Matrix4x4 world_q = Matrix4x4.Rotate(Quaternion.Inverse(cube.GetComponent<Calibration>().getInitialTransform().rotation) * cube.transform.rotation);
        //Matrix4x4 world_pos = Matrix4x4.Translate(cube.transform.position);
        Matrix4x4 world = cube.transform.localToWorldMatrix;
        Matrix4x4 viconToWorld = world * vicon.inverse;
        vcToWorld.transform.rotation = ExtractRotationFromMatrix(viconToWorld);
        vcToWorld.transform.position = ExtractTranslationFromMatrix(viconToWorld);
        return Result.Success;
    }

    private Quaternion ExtractRotationFromMatrix(Matrix4x4 mat) {
        Quaternion q = new Quaternion();
        q.w = Mathf.Sqrt(Mathf.Max(0, 1 + mat.m00 + mat.m11 + mat.m22)) / 2;
        q.x = Mathf.Sqrt(Mathf.Max(0, 1 + mat.m00 - mat.m11 - mat.m22)) / 2;
        q.y = Mathf.Sqrt(Mathf.Max(0, 1 - mat.m00 + mat.m11 - mat.m22)) / 2;
        q.z = Mathf.Sqrt(Mathf.Max(0, 1 - mat.m00 - mat.m11 + mat.m22)) / 2;
        q.x *= Mathf.Sign(q.x * (mat.m21 - mat.m12));
        q.y *= Mathf.Sign(q.y * (mat.m02 - mat.m20));
        q.z *= Mathf.Sign(q.z * (mat.m10 - mat.m01));
        return q;
    }

    private Vector3 ExtractTranslationFromMatrix(Matrix4x4 mat) {
        Vector3 translation = new Vector3(mat.m03, mat.m13, mat.m23);
        return translation;
    }

    private Vector3 convertToLeftHandPosition(float x, float y, float z) {
        return new Vector3(-x, y, z);
    }
    private Quaternion convertToLeftHandRotation(float qx, float qy, float qz, float qw) {
        return new Quaternion(-qx, qy, qz, -qw);
    }
}
