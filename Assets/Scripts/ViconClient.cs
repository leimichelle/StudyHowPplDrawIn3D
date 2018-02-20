using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Sharing;
using System;

[StructLayout(LayoutKind.Sequential)]
public struct PenData
{
    public float x, y, z;
    public float qx, qy, qz, qw;
	public int valid;
}
	

public class ViconClient : MonoBehaviour, ISpeechHandler {

	public GameObject pen;
    [DllImport("Vicon_DLL.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int StartVicon();
	public GameObject drawn_objects;
	public GameObject first_model;
	private enum Result {Success, Fail};
	private Matrix4x4 ViconToWorld;
    PenData pd;
    public struct CalibrationPoint 
    {
        public Vector3 translation_in_Unity;
        public Vector3 translation; //The difference between the mocap translation and the virtual object's translation
        public Quaternion q;
    }
    CalibrationPoint[] calibrationpts;
    int c; // the index of the current calibration point

    [DllImport("Vicon_DLL.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern PenData FetchNewViconFrame();
    // Use this for initialization
    void Start () {
        int connection_result = 1;
		int attempts = 0;
		pd = new PenData ();
        while (connection_result == 1 && attempts<10) {
			connection_result = StartVicon ();
			attempts++;
		}
		if(connection_result==0) {
			Debug.Log ("Successful connection");
            calibrationpts = new CalibrationPoint[4];
            calibrationpts[0].translation_in_Unity = pen.transform.position;
            calibrationpts[1].translation_in_Unity = new Vector3(0.0f, 0.0f, -0.5f);
            calibrationpts[2].translation_in_Unity = new Vector3(0.5f, 0.0f, 0.0f);
            calibrationpts[3].translation_in_Unity = new Vector3(-0.5f, 0.0f, 0.0f);
            c = 0; 
        }
		else {
			Debug.Log ("Unsuccessful connection");
			this.enabled = false;
		}
    }

    private Quaternion[] AverageQuaternion(Quaternion[] qs) {
        Debug.Log("New call");
        int size = qs.Length;
        if (size <= 1) {
            return qs;
        }
        Quaternion[] new_qs;
        if ( (size&1)==1 ) {
            new_qs = new Quaternion[size / 2 + 1];
        }
        else {
            new_qs = new Quaternion[size / 2];
        }
        int i;
        for (i=0; i<size-1; i+=2) {
            new_qs[i / 2] = Quaternion.Slerp(qs[i], qs[i + 1], 0.5f);
            Debug.Log(new_qs[i / 2]);
        }
        if ((size & 1) == 1) {
            new_qs[i / 2] = qs[i];
            Debug.Log(new_qs[i / 2]);
        }
        return AverageQuaternion(new_qs);
    }

    // Update is called once per frame
    private Result ComputeViconToHoloTransform () {
        Debug.Log("in ComputeViconToHoloTransform");
        int num_calibration_pts = calibrationpts.Length;
        float f_num_cali = (float)num_calibration_pts;
        Vector3 ave_translation = new Vector3(0.0f, 0.0f, 0.0f);
        Quaternion[] qs = new Quaternion[num_calibration_pts];
        for (int i = 0; i < num_calibration_pts; i++) {
            qs[i] = calibrationpts[i].q;
        }
        Quaternion[] ave_q = AverageQuaternion(qs);
        Matrix4x4 viconToWorld_rot = Matrix4x4.Rotate(ave_q[0]);
        if (ave_q.Length == 1) {
            for (int i=0; i<num_calibration_pts; i++) {
                Matrix4x4 tmpViconToWorld = Matrix4x4.Translate(calibrationpts[i].translation) * viconToWorld_rot;
                tmpViconToWorld = Matrix4x4.Translate(calibrationpts[i].translation_in_Unity) * tmpViconToWorld.inverse;
                ave_translation = ave_translation + ExtractTranslationFromMatrix(tmpViconToWorld);
            }
            ave_translation /= f_num_cali;
            drawn_objects.transform.position = ave_translation;
            drawn_objects.transform.rotation = Quaternion.Inverse(ave_q[0]);
        }
        else {
            Debug.Log("Quaternions averaging algorithm is wrong");
        }
        Debug.Log("drawn_objects: ");
        Debug.Log(drawn_objects.transform.localToWorldMatrix);
        return Result.Success;
    }

	private Vector3 ExtractTranslationFromMatrix(Matrix4x4 mat) {
		Vector3 translation = new Vector3 (mat.m03, mat.m13, mat.m23);
		return translation;
	}

	private Quaternion ExtractRotationFromMatrix(Matrix4x4 mat) {
		Quaternion q = new Quaternion ();
		q.w = Mathf.Sqrt (Mathf.Max (0, 1 + mat.m00 + mat.m11 + mat.m22)) / 2;
		q.x = Mathf.Sqrt (Mathf.Max (0, 1 + mat.m00 - mat.m11 - mat.m22)) / 2;
		q.y = Mathf.Sqrt (Mathf.Max (0, 1 - mat.m00 + mat.m11 - mat.m22)) / 2;
		q.z = Mathf.Sqrt (Mathf.Max (0, 1 - mat.m00 - mat.m11 + mat.m22)) / 2;
		q.x *= Mathf.Sign (q.x * (mat.m21 - mat.m12));
		q.y *= Mathf.Sign (q.y * (mat.m02 - mat.m20));
		q.z *= Mathf.Sign (q.z * (mat.m10 - mat.m01));
		return q;
	}

    public Vector3 convertToLeftHandPosition(float x, float y, float z)
    {
        return new Vector3(-x, y, z);
    }
    public Quaternion convertToLeftHandRotation(float qx, float qy, float qz, float qw)
    {
        return new Quaternion(-qx, qy, qz, -qw);
    }

    private Result SaveCalibrationPoint() {
        Debug.Log("in SaveCalibrationPoint");
        pd = FetchNewViconFrame();
        int attempts = 0;
        while (pd.valid != 2 && attempts < 20) {
            pd = FetchNewViconFrame();
            attempts++;
        }
        if (pd.valid != 2) {
            Debug.LogFormat("Failed to fetch a new frame! Result: {0}", pd.valid);
            return Result.Fail;
        }
        else {
            Vector3 translation = convertToLeftHandPosition(pd.x, pd.y, pd.z);
            calibrationpts[c].translation = translation;
            calibrationpts[c].q = convertToLeftHandRotation(pd.qx, pd.qy, pd.qz, pd.qw);
            if(c+1 < calibrationpts.Length) {
                pen.transform.position = calibrationpts[c + 1].translation_in_Unity;
            }
            return Result.Success;
        }
    }

    public void OnSpeechKeywordRecognized(SpeechEventData eventData)
	{
		if (eventData.RecognizedText == "Calibrate") {
            Debug.Log("c: " + c);
            Result result;
            if (c < calibrationpts.Length) {
                result = SaveCalibrationPoint();
            }

            if (c == calibrationpts.Length-1){
                result = ComputeViconToHoloTransform();
                if (result == Result.Success)
                {
                    Destroy(pen);
                    drawn_objects.SetActive(true);
                    first_model.SetActive(true);
                }
            }
            else {
                c++;
            }
		}
	}

	public PenData FetchFrame() {
		PenData point = FetchNewViconFrame();
		int attempts = 0;
		while (point.valid!=2 && attempts < 20) {
			point = FetchNewViconFrame ();
			attempts++;
		}
		if (pd.valid != 2) {
			Debug.LogFormat ("Failed to fetch a new frame! Result: {0}", pd.valid);
		}

		return point;
	}
}
