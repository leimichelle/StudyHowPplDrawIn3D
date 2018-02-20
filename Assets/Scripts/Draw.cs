using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using static PenData;

public class Draw : MonoBehaviour {
	// Use this for initialization
	public ButtonController b;
	public ViconClient vc;
	public Vector3 stroke_width;
	public Material stroke_material;
	public Transform mouse;
    public Transform PenTip;
	void Start () {
	}
	
	// Update is called once per frame
	void LateUpdate () {
		if (b.Pressed()) {
			DrawStroke ();
		}
	}

	private void DrawStroke() {
        GameObject pointObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pointObj.transform.position = PenTip.localPosition;
        pointObj.transform.rotation = PenTip.localRotation;
        /*pointObj.transform.position = new Vector3 (0.0f, 0.0f, z);
        z += 0.01f;*/
        Debug.Log("sphere in Vicon space:");
        Debug.Log(pointObj.transform.localToWorldMatrix);
        pointObj.transform.localScale = stroke_width;
        pointObj.GetComponent<Renderer>().material = stroke_material;
        Debug.Log("gameobjects");
        Debug.Log(transform.localToWorldMatrix);
        pointObj.transform.SetParent(gameObject.transform, false);
        Debug.Log("after set parent");
        Debug.Log(pointObj.transform.localToWorldMatrix);
    }
}
