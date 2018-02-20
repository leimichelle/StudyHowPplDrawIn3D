using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowPenTip : MonoBehaviour {
    public ViconClient vc;
    private float nextFetch = 0.0f;
    private float fetchRate = 0.001f;

	// Update is called once per frame
	void Update () {
        PenData pd = vc.FetchFrame();
        if (pd.valid == 2) {
            transform.localPosition = vc.convertToLeftHandPosition(pd.x, pd.y, pd.z);
            transform.localRotation = vc.convertToLeftHandRotation(pd.qx, pd.qy, pd.qz, pd.qw);
        }
    }
}
