using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Sharing;
using System;

[StructLayout(LayoutKind.Sequential)]
public struct PenData {
    public float x, y, z;
    public float qx, qy, qz, qw;
    public int valid;
}


public class ViconClient : MonoBehaviour/*, ISpeechHandler */{

    public GameObject pen;
    [DllImport("Vicon_DLL.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int StartVicon();
    [DllImport("Vicon_DLL.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern PenData FetchNewViconFrame(string tracked_object);
    // Use this for initialization
    void Start() {
        int connection_result = 1;
        int attempts = 0; while (connection_result == 1 && attempts < 10) {
            connection_result = StartVicon();
            attempts++;
        }
        if (connection_result == 0) {
#if DEBUG
            Debug.Log("Successful connection");
#endif
        }
        else {
#if DEBUG
            Debug.Log("Unsuccessful connection");
#endif
            this.enabled = false;
        }
    }

    public PenData FetchFrame(string tracked_ojbect) {
        PenData point = FetchNewViconFrame(tracked_ojbect);
        int attempts = 0;
        while (point.valid != 2 && attempts < 20) {
            point = FetchNewViconFrame(tracked_ojbect);
            attempts++;
        }
#if Debug
        if (point.valid != 2) {
			Debug.LogFormat ("Failed to fetch a new frame! Result: {0}", point.valid);
		}
#endif

        return point;
    }
}
