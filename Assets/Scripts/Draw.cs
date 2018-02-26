using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using static PenData;

public class Draw : MonoBehaviour {
	// Use this for initialization
	public ButtonController b;
	public ViconClient vc;
	public float stroke_width = 0.0015f;
	public Transform mouse;
    public Transform PenTip;
    public int verticesPerPoint = 6;
    private MeshFilter mf;
    private Mesh strokes;
    /*private bool receivedFirstPoint = false;
    private Vector3 old_pt;*/
    private bool held = false;
    private bool down = false;
    private bool up = false;
    private bool strokeInitalized = false;
    void Awake () {
        mf = GetComponent<MeshFilter>();
        strokes = new Mesh();
    }
	
	// Update is called once per frame
    void Update() {
        if (b.Pressed()) {
            if(held==false) {
                down = true;
                up = false;
            }
            else {
                down = false;
            }
            held = true;
        }
        else {
            if(held==true) {
                up = true;
                down = false;
            }
            else {
                up = false;
            }
            held = false;
        }

        if(up) {
            Debug.Log("# of vertices: " + strokes.vertices.Length);
            Debug.Log("# of triangles indices: " + strokes.triangles.Length); 
        }
    }
	void LateUpdate () {
		if (held) {
            DrawStroke();
        }
	}

	private void DrawStroke() {
        Vector3[] vertices;
        Vector3[] normals;
        int[] triangles;
        if (strokeInitalized) {
            vertices = strokes.vertices;
            triangles = strokes.triangles;
            normals = strokes.normals;
            strokes.Clear();
        }
        else {
            vertices = new Vector3[0];
            normals = new Vector3[0];
            triangles = new int[0];
            strokeInitalized = true;
        }
        int oldVerticeLength = vertices.Length;
        Array.Resize(ref vertices, oldVerticeLength + verticesPerPoint);
        Array.Resize(ref normals, oldVerticeLength + verticesPerPoint);
        //Taking 1st column (column ranges from 0-3) of PenTip's rotation matrix, aka the X-axis of the rotation matrix, and make a vector out of the column
        /*Vector4 x = Matrix4x4.Rotate(PenTip.transform.rotation).GetColumn(0);
        Quaternion x_q = new Quaternion(x.x, x.y, x.z, x.w);
        Vector4 y = Matrix4x4.Rotate(PenTip.transform.rotation).GetColumn(1);
        Quaternion y_q = new Quaternion(y.x, y.y, y.z, y.w);*/
        Vector3 bn = Matrix4x4.Rotate(PenTip.transform.localRotation).GetColumn(0);
        Vector3 n = Matrix4x4.Rotate(PenTip.transform.localRotation).GetColumn(1);
        bn = bn.normalized;
        n = n.normalized;
        float r = stroke_width / 2.0f;

        for (int i = 0; i < verticesPerPoint; ++i) {
            vertices[oldVerticeLength + i] =
                PenTip.transform.localPosition +
                (float)Mathf.Cos(2 * Mathf.PI * (i) / verticesPerPoint) * r * bn +
                (float)Mathf.Sin(2 * Mathf.PI * (i) / verticesPerPoint) * r * n;
            normals[oldVerticeLength + i] = (vertices[oldVerticeLength + i] - PenTip.transform.position).normalized;
            /*vertices[oldVerticeLength + i].x = -vertices[oldVerticeLength + i].x;
            normals[oldVerticeLength + i].x = -normals[oldVerticeLength + i].x;*/
        }

        if (!down) {
            int oldTriangleLength = triangles.Length;
            Array.Resize(ref triangles, oldTriangleLength + verticesPerPoint * 6);
            for (int quad = 0; quad<verticesPerPoint; ++quad) {
                triangles[oldTriangleLength + quad * 6 + 0] = (oldVerticeLength - 6) + quad;
                triangles[oldTriangleLength + quad * 6 + 1] = (oldVerticeLength - 6) + (quad + 1) % verticesPerPoint;
                triangles[oldTriangleLength + quad * 6 + 2] = oldVerticeLength + quad;
                triangles[oldTriangleLength + quad * 6 + 3] = (oldVerticeLength - 6) + (quad + 1) % verticesPerPoint;
                triangles[oldTriangleLength + quad * 6 + 4] = oldVerticeLength + (quad + 1) % verticesPerPoint;
                triangles[oldTriangleLength + quad * 6 + 5] = oldVerticeLength + quad;
            }
        }
        strokes.vertices = vertices;
        strokes.normals = normals;
        strokes.triangles = triangles;
        mf.sharedMesh = strokes;

    }
}
