using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IFP.Camera;

public class PlaneAdjuster : MonoBehaviour {

    public CameraZoomController zoomCtrl;
    public float step = 0;

    // todo: should be readonly
    private CameraZoomController.UpdateMethod method = CameraZoomController.UpdateMethod.DistanceStepUpdate;

    private bool shouldZoom { get { return shouldListen && shouldReact; } }
    private bool shouldReact { get { return shouldTrace || zoomCtrl.currentStep == step - 1; } }
    private bool shouldTrace { get { return zoomCtrl.currentStep == step; } }
    private bool shouldListen { get { return zoomCtrl.updateMethod == method; } }

    private Vector3 delta = Vector3.zero;
    private Vector3 origin = Vector3.zero;
    private Vector3 origin0 = Vector3.zero;

    void Start () {
        if (zoomCtrl == null) {
            Debug.LogError("Zoom controller is not attached.");
        }

        origin0 = origin = transform.position;

        zoomCtrl.ZoomStarted += ZoomStarted;
        zoomCtrl.ZoomUpdated += ZoomUpdated;
        zoomCtrl.ZoomCompleted += ZoomCompleted;
    }

    private void Update()
    {
        if (!shouldTrace) return;
        GetPositionDelta();
    }

    void ZoomUpdated(float progress)
    {
        if (!shouldZoom) return;
        float zoomDirection = zoomCtrl.currentStep > zoomCtrl.targetStep ? 1 : -1;
        transform.position = origin - delta * progress * zoomDirection;
    }

    void ZoomStarted()
    {
        if (!shouldZoom) return;

        float zoomDirection = zoomCtrl.currentStep > zoomCtrl.targetStep ? 1 : -1;

        bool isCurrent = zoomCtrl.currentStep == step;
        bool zoomingIn = isCurrent && zoomDirection > 0;
        bool zoomingOut = zoomDirection < 0;

        origin = transform.position;

        if (zoomingIn) {
            delta = GetPositionDelta() - origin;
        }

        if (zoomingOut) {
            delta = origin0 - origin;
        }
    }

    void ZoomCompleted()
    {
        if (zoomCtrl.currentStep != step) {
            return;
        }
    }
    
    private Vector3 GetPositionDelta()
    {
        Camera cam = Camera.main;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(transform.position, transform.up);
        float rayDistance;
        
        if (plane.Raycast(ray, out rayDistance)) {
            Vector3 targetPosition = ray.GetPoint(rayDistance);
            Debug.DrawLine(cam.transform.position, targetPosition, Color.white);
            return targetPosition;
        }

        Debug.LogWarning("Couldn't find target position.", this.gameObject);
        return Vector3.zero;
    }
}