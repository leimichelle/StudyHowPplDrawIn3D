using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FetchPenVcFrame : MonoBehaviour {
    public ViconClient vc;
    public float jittery_tolerance = 0.004f;
    private PenData pd;
    private bool initialized = false;

    // Update is called once per frame
    void Update() {
        /*if (!initialized) {
            pd = vc.FetchFrame("Pen_experimental");
            initialized = true;
        }
        else {
            PenData cur_pd = vc.FetchFrame("Pen_experimental");
            JitterRemoval(cur_pd);
        }*/
        pd = vc.FetchFrame("Pen_experimental");
        if (pd.valid == 2) {
            transform.localPosition = convertToLeftHandPosition(pd.x, pd.y, pd.z);
            transform.localRotation = convertToLeftHandRotation(pd.qx, pd.qy, pd.qz, pd.qw);
        }
    }

    private void JitterRemoval(PenData cur_pd) {
        Vector3 new_pos = new Vector3(cur_pd.x, cur_pd.y, cur_pd.z);
        Vector3 old_pos = new Vector3(pd.x, pd.y, pd.z);
        Vector3 diff = new_pos - old_pos;
        float minimum = Mathf.Min(jittery_tolerance, diff.magnitude);
        Vector3 delta_pos = minimum * diff.normalized;
        pd.x += delta_pos.x;
        pd.y += delta_pos.y;
        pd.z += delta_pos.z;
        pd.qx = cur_pd.qx;
        pd.qy = cur_pd.qy;
        pd.qz = cur_pd.qz;
        pd.qw = cur_pd.qw;
        pd.valid = cur_pd.valid;
        return;
    }
    private Vector3 convertToLeftHandPosition(float x, float y, float z) {
        return new Vector3(-x, y, z);
    }
    private Quaternion convertToLeftHandRotation(float qx, float qy, float qz, float qw) {
        return new Quaternion(-qx, qy, qz, -qw);
    }
}
