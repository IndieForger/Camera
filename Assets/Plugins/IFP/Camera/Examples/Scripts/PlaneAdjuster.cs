using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IFP.Camera;

namespace IFP.Camera.Examples
{
    public class PlaneAdjuster : MonoBehaviour
    {
        public CameraZoomController zoomCtrl;
        public float step = 0;

        public CameraZoomController.UpdateMethod ZoomMethod { get { return CameraZoomController.UpdateMethod.DistanceStepUpdate; } }

        private bool ShouldZoom { get { return ShouldListen && ShouldReact; } }
        private bool ShouldReact { get { return ShouldTrace || zoomCtrl.currentStep == step - 1; } }
        private bool ShouldTrace { get { return zoomCtrl.currentStep == step; } }
        private bool ShouldListen { get { return zoomCtrl.updateMethod == ZoomMethod; } }

        private Vector3 delta = Vector3.zero;
        private Vector3 origin = Vector3.zero;
        private Vector3 origin0 = Vector3.zero;

        void Start()
        {
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
            if (!ShouldTrace) return;
            GetPositionDelta();
        }

        void ZoomUpdated(float progress)
        {
            if (!ShouldZoom) return;
            float zoomDirection = zoomCtrl.currentStep > zoomCtrl.targetStep ? 1 : -1;
            Vector3 position = origin - delta * progress * zoomDirection;
            transform.position = new Vector3(position.x, transform.position.y, position.z);
        }

        void ZoomStarted()
        {
            if (!ShouldZoom) return;
            
            float zoomDirection = zoomCtrl.currentStep > zoomCtrl.targetStep ? 1 : -1;

            bool zoomingIn = zoomCtrl.currentStep == step && zoomDirection > 0;
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
            UnityEngine.Camera cam = UnityEngine.Camera.main;
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
}