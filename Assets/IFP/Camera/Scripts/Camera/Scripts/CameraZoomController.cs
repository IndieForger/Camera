using UnityEngine;
using System.Collections;

// rotates camera with scroller
namespace IFP.Camera
{
    public class CameraZoomController : CameraBaseController
    {
        // triggered when started
        public delegate void ZoomStartedHandler();
        public event ZoomStartedHandler ZoomStarted;
        public delegate void ZoomUpdatedHandler(float progress);
        public event ZoomUpdatedHandler ZoomUpdated;
        public delegate void ZoomCompletedHandler();
        public event ZoomCompletedHandler ZoomCompleted;

        public enum UpdateMethod { DistanceFreeUpdate, DistanceClampedUpdate, DistanceStepUpdate, FOVStepUpdate }
        public UpdateMethod updateMethod = UpdateMethod.DistanceClampedUpdate;

        public enum ZoomMethod { CameraForward, ToMousePointer }
        public ZoomMethod zoomMethod = ZoomMethod.CameraForward;
        float rayMaxDistance = 500;

        public LayerMask layerMask;

        // common step properties
        public int defaultStep = 2;
        public float stepTime = 1;      // time between steps
        public float stepDampener = 1;
        

        // distance update properties
        public float[] distanceSteps = { 5, 10, 40, 60 };        
        public float distanceSnapDelta = 0.5f;
        public float distanceUpdateDelta = 50; // the zoom speed multiplier ( ignored for step based update )   
        public float minDistance = 50; // min limit value(distance between camera and focuspoint)
        public float maxDistance = 100; // max limit value(distance between camera and focuspoint)

        private int currentDistanceStep;
        private int targetDistanceStep;

        // fov update properties
        public float[] fovSteps = { 5, 20, 60, 80 };
        public float fovSnapDelta = 0.1f;

        private int currentFovStep;
        private int targetFovStep;

        // public API methods

        /// <summary>
        /// Returns step number for step based zoom or -1 for others
        /// </summary>
        public int currentStep
        {
            get
            {
                switch (updateMethod)
                {
                    case UpdateMethod.DistanceStepUpdate:
                        return currentDistanceStep;
                    case UpdateMethod.FOVStepUpdate:
                        return currentFovStep;
                }
                return -1;
            }
        }

        /// <summary>
        /// Returns step number for step based zoom or -1 for others
        /// </summary>
        public int targetStep
        {
            get
            {
                switch (updateMethod)
                {
                    case UpdateMethod.DistanceStepUpdate:
                        return targetDistanceStep;
                    case UpdateMethod.FOVStepUpdate:
                        return targetFovStep;
                }
                return -1;
            }
        }

        /// <summary>
        /// Returns controller zoom delta based on zoom type and current zoom
        /// </summary>
        public float zoomDelta
        {
            get
            {
                switch (updateMethod)
                {
                    case UpdateMethod.DistanceStepUpdate:
                        float minDistance = distanceSteps[0];
                        float maxDistance = distanceSteps[distanceSteps.Length - 1];
                        return (maxDistance - minDistance);
                    case UpdateMethod.FOVStepUpdate:
                        return targetFovStep;
                }
                return -1;
            }
        }

        Vector3 targetHitPoint
        {
            get
            {
                Vector3 direction = Vector3.zero;
                switch(zoomMethod) {
                case ZoomMethod.ToMousePointer:
                    direction = MousePointerDirection;
                    break;
                case ZoomMethod.CameraForward:
                    direction = camera.transform.forward;
                    break;
                default:
                    throw new System.Exception("Raycast method not assigned.");
                }
                
                Ray ray = new Ray(camera.transform.position, direction);             
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, rayMaxDistance, layerMask )) {
                    Debug.DrawLine(ray.origin, hit.point, Color.green);
                    return hit.point;
                } else {
                    Debug.DrawLine(ray.origin, Vector3.zero, Color.red);
                }
                return Vector3.zero;
            }
        }


        private float distanceToHitPoint
        {
            get
            {
                return Vector3.Distance(camera.transform.localPosition, targetHitPoint);
            }
        }

        // others
        private float cameraZoomDelta = 0;      // extracted from InputManager on update

        void Start()
        {
            SetupFOV();
            switch (updateMethod) {
                case UpdateMethod.DistanceStepUpdate:
                    InitDistanceStep();
                    break;
            }
        }

        void Update()
        {
            cameraZoomDelta = CameraInputManager.Instance.GetZoomInputDelta();

            switch (updateMethod)
            {
                case UpdateMethod.FOVStepUpdate:
                    UpdateFovStep();
                    break;
                case UpdateMethod.DistanceStepUpdate:
                    UpdateDistanceStep();
                    break;
                case UpdateMethod.DistanceClampedUpdate:
                    DistanceClampedUpdate();
                    break;
                case UpdateMethod.DistanceFreeUpdate:
                    DistanceFreeUpdate();
                    break;
            }
        }

        /// <summary>
        /// Triggered when zoom has started
        /// </summary>
        protected void TriggerZoomStarted()
        {
            if (ZoomStarted != null) {
                ZoomStarted();
            }
        }

        /// <summary>
        /// Triggered for every zoom update passing progress delta.
        /// TriggerZoomUpdated is called with different value for each zoom update type.
        /// </summary>
        /// <param name="progress">Progress delta</param>
        protected void TriggerZoomUpdated(float progress)
        {
            if (ZoomUpdated != null) {
                ZoomUpdated(progress);
            }
        }

        /// <summary>
        /// Trigger when zoom has completed
        /// </summary>
        protected void TriggerCompleted()
        {
            if (ZoomCompleted != null) {
                ZoomCompleted();
            }
        }

        void SetupFOV()
        {
            if (defaultStep > fovSteps.Length - 1 || defaultStep < 0)
            {
                Debug.LogWarning("[IFC.RTSCamera.CameraZoomController()] defaultFovLevel auto set to 0");
                defaultStep = 0;
            }
            currentFovStep = defaultStep;
            targetFovStep = currentFovStep;
            camera.fieldOfView = fovSteps[defaultStep];
        }

        void InitDistanceStep()
        {
            targetDistanceStep = defaultStep;
            float closestDistance = distanceToHitPoint;
            int step = currentDistanceStep  = 0;
            foreach (float distance in distanceSteps) {
                if (Mathf.Abs(distanceToHitPoint - distance) < closestDistance) {
                    closestDistance = distance;
                    currentDistanceStep = step;
                }
                step++;
            }
        }

        void UpdateDistanceStep()
        {
            float zoomDelta = CameraInputManager.Instance.GetZoomInputDelta();
            Vector3 targetHitPoint = this.targetHitPoint;
            int currentStep = currentDistanceStep;
            int targetStep = targetDistanceStep;
            int zoomDirection = 0;  // 1 -> zooming in, -1 -> zooming out

            if (currentStep == targetStep) 
            {
                if (zoomDelta == 0) {
                    return;
                }
                zoomDirection = zoomDelta > 0 ? 1 : -1;
                targetStep = Mathf.Clamp(targetStep - zoomDirection, 0, distanceSteps.Length - 1);
                this.targetDistanceStep = targetStep;
                TriggerZoomStarted();
                return;
            }

            float distanceDelta = Mathf.Abs(distanceSteps[currentStep] - distanceSteps[targetStep]) / stepTime * Time.deltaTime;
            zoomDirection = currentStep > targetStep ? 1 : -1;
            
            if (zoomDirection > 0 && this.distanceToHitPoint - distanceDelta < distanceSteps[targetStep]) {
                distanceDelta = this.distanceToHitPoint - distanceSteps[targetStep];
            }

            if (zoomDirection < 0 && this.distanceToHitPoint + distanceDelta > distanceSteps[targetStep]) {
                distanceDelta = distanceSteps[targetStep] - this.distanceToHitPoint;
            }
        
            camera.transform.localPosition = Vector3.MoveTowards(camera.transform.localPosition, targetHitPoint, distanceDelta * zoomDirection);

            float distDiff = Mathf.Abs(this.distanceToHitPoint - distanceSteps[targetStep]);
            float distStep = Mathf.Abs(distanceSteps[targetStep] - distanceSteps[currentStep]);
            float progress = Mathf.Clamp(1 - (distDiff / distStep), 0, 1);
            TriggerZoomUpdated(progress);            

            if (Mathf.Abs(distanceToHitPoint - distanceSteps[targetDistanceStep]) <= distanceSnapDelta) {
                currentDistanceStep = targetDistanceStep;
                TriggerCompleted();
            }
        }

        void DistanceFreeUpdate()
        {
            Ray ray = new Ray(camera.transform.position, camera.transform.forward);
            Debug.DrawLine(ray.origin, ray.direction + camera.transform.forward * 100, Color.red);

            if (cameraZoomDelta == 0) return;
            TriggerZoomStarted();
            float distance = 100;
            Vector3 screenPoint = new Vector3(Screen.width / 2, Screen.height / 2, distance);
            Vector3 targetPoint = camera.ScreenToWorldPoint(screenPoint);
            Vector3 heading = cameraZoomDelta > 0
                ? targetPoint - camera.transform.position
                : camera.transform.position - targetPoint;
            float distanceDelta = Time.deltaTime * this.distanceUpdateDelta;
            if (manager.GetSpecial1())
            {
                distanceDelta *= 10;
            }
            camera.transform.position = Vector3.MoveTowards(camera.transform.position, camera.transform.position + heading, distanceDelta);
            TriggerZoomUpdated(distanceDelta);
            TriggerCompleted();
        }

        void DistanceClampedUpdate()
        {
            Vector3 targetHitPoint = this.targetHitPoint;
            if (cameraZoomDelta == 0) return;
            TriggerZoomStarted();
            float distance = Vector3.Distance(camera.transform.localPosition, targetHitPoint);
            if ((distance > minDistance && cameraZoomDelta > 0) || (distance < maxDistance && cameraZoomDelta < 0)) {
                float distanceDelta = cameraZoomDelta * Time.deltaTime * this.distanceUpdateDelta;
                Vector3 localPosition = Vector3.MoveTowards(camera.transform.localPosition, targetHitPoint, distanceDelta);
                camera.transform.localPosition = ClampDistance(localPosition, targetHitPoint, minDistance, maxDistance);
            }

            float distDiff = Mathf.Abs(this.distanceToHitPoint - minDistance);
            float distStep = Mathf.Abs(maxDistance - minDistance);
            float progress = Mathf.Clamp(1 - (distDiff / distStep), 0, 1);
            TriggerZoomUpdated(progress);

            TriggerCompleted();
        }

        Vector3 ClampDistance(Vector3 localPosition, Vector3 targetPosition, float minDistance, float maxDistance)
        {
            float distance = Vector3.Distance(localPosition, targetPosition);
            Vector3 heading = localPosition - targetPosition;            
            Vector3 direction = heading.normalized;

            if (distance <= minDistance)
            {
                return targetPosition + direction * minDistance;
            }

            if (distance >= maxDistance)
            {
                return targetPosition + direction * maxDistance;
            }

            return localPosition;
        }

        Vector3 fovUpdateStartForwardDirection;        
        Vector3 fovUpdateStartForwardHitpoint;
        Vector3 fovUpdateStartTargetDirection;
        Vector3 fovUpdateStartTargetHitpoint;
        bool fovUpdateContinueLookAt = false;
        float fovUpdateStartTime;

        void UpdateFovStep()
        {
            bool recordStartHitpoint = false;
            Vector3 camPosition = camera.transform.position;

            if (currentFovStep == targetFovStep)
            {
                if (cameraZoomDelta < 0)
                {
                    targetFovStep++;
                }

                if (cameraZoomDelta > 0)
                {
                    targetFovStep--;
                    fovUpdateContinueLookAt = targetFovStep <= currentFovStep; // only if zooming in
                }

                if (cameraZoomDelta != 0) {
                    targetFovStep = Mathf.Clamp(targetFovStep, 0, fovSteps.Length - 1);
                    TriggerZoomStarted();

                    recordStartHitpoint = true;
                    fovUpdateStartTime = Time.time;
                   
                    fovUpdateStartForwardDirection = camera.transform.forward;
                    fovUpdateStartTargetDirection = MousePointerDirection;
                }        
            }

            // forward ray
            Ray forwardRay = new Ray(camPosition, camera.transform.forward);
            RaycastHit forwardHit;
            Vector3 forwardHitPoint = Vector3.zero;            
            if (Physics.Raycast(forwardRay, out forwardHit, rayMaxDistance, layerMask)) {
                forwardHitPoint = forwardHit.point;

                if (recordStartHitpoint) {
                    fovUpdateStartForwardHitpoint = forwardHitPoint;
                }
            }

            if (camera.fieldOfView != fovSteps[targetFovStep])
            {
                float fovUpdateDelta = Mathf.Abs(fovSteps[currentFovStep] - fovSteps[targetFovStep]) / stepTime * Time.deltaTime;
                camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, fovSteps[targetFovStep], fovUpdateDelta);

                float fovMin = fovSteps[targetFovStep] >= fovSteps[currentFovStep] ? fovSteps[currentFovStep] : fovSteps[targetFovStep];
                float fovMax = fovSteps[targetFovStep] >= fovSteps[currentFovStep] ? fovSteps[targetFovStep] : fovSteps[currentFovStep];
                camera.fieldOfView = Mathf.Clamp(camera.fieldOfView, fovMin, fovMax);

                float fovDiff = Mathf.Abs(camera.fieldOfView - fovSteps[targetStep]);
                float fovStep = Mathf.Abs(fovSteps[targetStep] - fovSteps[currentStep]);
                float progress = Mathf.Clamp(1 - (fovDiff / fovStep), 0, 1);

                // target ray
                if (zoomMethod == ZoomMethod.ToMousePointer) {
                    Ray targetRay = new Ray(camera.transform.position, fovUpdateStartTargetDirection);
                    RaycastHit targetHit;
                    if (Physics.Raycast(targetRay, out targetHit, rayMaxDistance, layerMask)) {
                        if (recordStartHitpoint) { 
                            fovUpdateStartTargetHitpoint = targetHit.point;
                        }
                    }

                }
                
                TriggerZoomUpdated(progress);
            }
                    
            if (Mathf.Abs(camera.fieldOfView - fovSteps[targetFovStep]) <= fovSnapDelta)
            {
                currentFovStep = targetFovStep;
                if (ZoomCompleted != null) {
                    ZoomCompleted();
                }
            }

            Color forwardLineColor = zoomMethod == ZoomMethod.ToMousePointer && fovUpdateContinueLookAt ? Color.yellow : Color.green;
            Debug.DrawLine(camPosition, forwardHit.point, forwardLineColor);

            if (fovUpdateContinueLookAt) {                
                Debug.DrawLine(camPosition, fovUpdateStartForwardHitpoint, Color.white);
                Debug.DrawLine(camPosition, fovUpdateStartTargetHitpoint, Color.green);

                float deltaTime = (Time.time - fovUpdateStartTime) / (stepTime * stepDampener);
                fovUpdateContinueLookAt = deltaTime < 1;
                camera.transform.LookAt(Vector3.Lerp(fovUpdateStartForwardHitpoint, fovUpdateStartTargetHitpoint, deltaTime));                
            }
        }
    }
}
