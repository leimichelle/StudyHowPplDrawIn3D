using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Calibration : MonoBehaviour, IInputClickHandler {
    public float translationSpeed = 0.1f;
    public float rotationSpeed = 0.1f;
    public PlaySpaceManager psm;
    public GameObject speechRecognition;
    public TextMesh dbtext;
    private Transform initialTransform;
    private enum Mode { AlignLookAt, ContinueScanning, Placing, SetRotation, Tuning };
    private Mode mode = Mode.AlignLookAt;
    private TapToPlace ttp;
    //private bool y_up, y_down, x_right, x_left, z_front, z_back, y_pos_rot, y_neg_rot, x_pos_rot, x_neg_rot, z_pos_rot, z_neg_rot;
    private Vector3 aligned_forward;
    // Use this for initialization
    void Start() {
        initialTransform = transform;
        foreach (Renderer r in gameObject.GetComponentsInChildren<Renderer>())
            r.enabled = false;
        ttp = gameObject.GetComponent<TapToPlace>();
        //y_up = y_down = x_right = x_left = z_front = z_back = y_pos_rot = y_neg_rot = x_pos_rot = x_neg_rot = z_pos_rot = z_neg_rot = false;
        InputManager.Instance.PushFallbackInputHandler(gameObject);
    }

    // Update is called once per frame
    void Update() {
        switch (mode) {
            case Mode.AlignLookAt:
                if (psm.meshesProcessed) {
                    List<GameObject> vertical = new List<GameObject>();
                    List<GameObject> horizontal = new List<GameObject>();
                    horizontal = SurfaceMeshesToPlanes.Instance.GetActivePlanes(PlaneTypes.Table);
                    vertical = SurfaceMeshesToPlanes.Instance.GetActivePlanes(PlaneTypes.Wall);
                    if (vertical.Count > 0) {
                        PlacementSurfaces surfaceType = PlacementSurfaces.Vertical;
                        vertical.Sort((lhs, rhs) => {
                            Vector3 headPosition = Camera.main.transform.position;
                            Collider rightCollider = rhs.GetComponent<Collider>();
                            Collider leftCollider = lhs.GetComponent<Collider>();

                            // This plane is big enough, now we will evaluate how far the plane is from the user's head.  
                            // Since planes can be quite large, we should find the closest point on the plane's bounds to the 
                            // user's head, rather than just taking the plane's center position.
                            Vector3 rightSpot = rightCollider.ClosestPointOnBounds(headPosition);
                            Vector3 leftSpot = leftCollider.ClosestPointOnBounds(headPosition);

                            return Vector3.Distance(leftSpot, headPosition).CompareTo(Vector3.Distance(rightSpot, headPosition));
                        });
                        int index = -1;
                        Collider collider = gameObject.GetComponent<Collider>();
                        index = FindNearestPlane(vertical, collider.bounds.size);
                        Quaternion rotation = Quaternion.identity;
                        if (index >= 0) {
                            GameObject surface = vertical[index];
                            SurfacePlane plane = surface.GetComponent<SurfacePlane>();
#if DEBUG
                            Debug.Log("Wall's surface normal with respect to starting camera direction: " + plane.SurfaceNormal);
#endif 
                            aligned_forward = surface.transform.forward;
                            if (!SpatialMappingManager.Instance.IsObserverRunning()) {
                                // If running, Stop the observer by calling
                                // StopObserver() on the SpatialMappingManager.Instance.
                                SpatialMappingManager.Instance.StartObserver();
                            }
                            GameObject SpatialProcessing = GameObject.Find("SpatialProcessing");
                            SpatialProcessing.SetActive(false);
                            dbtext.text = "Tap anywhere to finish scanning";
                            mode = Mode.ContinueScanning;
                        }
                    }
                }
                break;
            case Mode.ContinueScanning:
                //Now it is waiting for a Tap event to happen. A state change will occur in the OnInputClicked function if mode equals to Mode.ContinueScanning
                break;
            case Mode.Placing:
                if (!ttp.IsBeingPlaced) {
                    WorldAnchorManager.Instance.RemoveAnchor(gameObject);
                    speechRecognition.SetActive(true);
                    //transform.rotation = Quaternion.LookRotation(aligned_forward, Vector3.up);
                    mode = Mode.SetRotation;
                }
                break;
            case Mode.SetRotation:
                Quaternion tmp = Quaternion.LookRotation(aligned_forward, Vector3.up);
                gameObject.transform.rotation = Quaternion.Euler(ttp.initial_rotation) * Quaternion.LookRotation(aligned_forward, Vector3.up);
                mode = Mode.Tuning;
                break;
            case Mode.Tuning:
                Tune();
                break;
        }
        UnityEngine.XR.WSA.HolographicSettings.SetFocusPointForFrame(transform.position - Camera.main.transform.position);
    }
    public virtual void OnInputClicked(InputClickedEventData eventData) {
        if (mode == Mode.ContinueScanning) {
            StopRenderingSpatialMesh();
            foreach (Renderer r in gameObject.GetComponentsInChildren<Renderer>())
                r.enabled = true;
            dbtext.text = "";
            ttp.enabled = true;
            mode = Mode.Placing;
        }
    }

    private Vector3 AdjustPositionWithSpatialMap(Vector3 position, Vector3 surfaceNormal) {
        Vector3 newPosition = position;
        RaycastHit hitInfo;
        float distance = 0.5f;

        // Check to see if there is a SpatialMapping mesh occluding the object at its current position.
        if (Physics.Raycast(position, surfaceNormal, out hitInfo, distance, SpatialMappingManager.Instance.LayerMask)) {
            // If the object is occluded, reset its position.
            newPosition = hitInfo.point;
        }

        return newPosition;
    }

    private int FindNearestPlane(List<GameObject> surfaces, Vector3 minSize) {
        if (surfaces.Count < 1) {
            return -1;
        }
        RaycastHit hitInfo;
        float distance = 2.0f;
        if (!Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hitInfo, distance, SpatialMappingManager.Instance.LayerMask)) {
            return -1;
        }
        int planeIndex = 0;
        float min_angle = Vector3.Angle(hitInfo.normal, surfaces[0].GetComponent<SurfacePlane>().SurfaceNormal);
        for (int i = 0; i < surfaces.Count; i++) {

            Collider collider = surfaces[i].GetComponent<Collider>();
            if (collider.bounds.size.x < minSize.x || collider.bounds.size.y < minSize.y) {
                // This plane is too small to fit our vertical object.
                continue;
            }

            float angle = Vector3.Angle(hitInfo.normal, surfaces[i].GetComponent<SurfacePlane>().SurfaceNormal);
            if (angle < min_angle) {
                min_angle = angle;
                planeIndex = i;
            }
        }

        if (min_angle >= 10.0f) {
            return -1;
        }
        else {
            return planeIndex;
        }
    }

    private void StopRenderingSpatialMesh() {
        SpatialMappingManager.Instance.DrawVisualMeshes = false;
        if (SpatialMappingManager.Instance.IsObserverRunning()) {
            // If running, Stop the observer by calling
            // StopObserver() on the SpatialMappingManager.Instance.
            SpatialMappingManager.Instance.StopObserver();
        }
        var customMesh = SpatialUnderstanding.Instance.GetComponent<SpatialUnderstandingCustomMesh>();
        customMesh.DrawProcessedMesh = false;
        SpatialUnderstanding.Instance.RequestFinishScan();
        /*GameObject spUnderstanding = GameObject.Find("SpatialUnderstanding");
        spUnderstanding.SetActive(false);*/
    }

    void Tune() {
        bool cur_y_up = Input.GetKey(KeyCode.PageUp);
        bool cur_y_down = Input.GetKey(KeyCode.PageDown);
        bool cur_x_right = Input.GetKey(KeyCode.RightArrow);
        bool cur_x_left = Input.GetKey(KeyCode.LeftArrow);
        bool cur_z_front = Input.GetKey(KeyCode.UpArrow);
        bool cur_z_back = Input.GetKey(KeyCode.DownArrow);

        bool cur_y_pos_rot = Input.GetKey(KeyCode.Keypad4);
        bool cur_y_neg_rot = Input.GetKey(KeyCode.Keypad1);
        bool cur_x_pos_rot = Input.GetKey(KeyCode.Keypad6);
        bool cur_x_neg_rot = Input.GetKey(KeyCode.Keypad3);
        bool cur_z_pos_rot = Input.GetKey(KeyCode.Keypad5);
        bool cur_z_neg_rot = Input.GetKey(KeyCode.Keypad2);

        if (cur_y_up) {
            transform.Translate(Vector3.up * translationSpeed * Time.deltaTime);
        }
        else if (cur_y_down) {
            transform.Translate(Vector3.up * -translationSpeed * Time.deltaTime);
        }

        if (cur_x_right) {
            transform.Translate(Vector3.forward * -translationSpeed * Time.deltaTime);
        }
        else if (cur_x_left) {
            transform.Translate(Vector3.forward * translationSpeed * Time.deltaTime);
        }

        if (cur_z_front) {
            transform.Translate(Vector3.right * translationSpeed * Time.deltaTime);
        }
        else if (cur_z_back) {
            transform.Translate(Vector3.right * -translationSpeed * Time.deltaTime);
        }

        if (cur_y_pos_rot) {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
        else if (cur_y_neg_rot) {
            transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
        }

        if (cur_x_pos_rot) {
            transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime);
        }
        else if (cur_x_neg_rot) {
            transform.Rotate(Vector3.right, -rotationSpeed * Time.deltaTime);
        }

        if (cur_z_pos_rot) {
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }
        else if (cur_z_neg_rot) {
            transform.Rotate(Vector3.forward, -rotationSpeed * Time.deltaTime);
        }
    }

    public Transform getInitialTransform() {
        return initialTransform;
    }
}
