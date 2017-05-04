using UnityEngine;
using System.Collections;

// rotates camera with scroller
namespace IFC.Camera
{
    public class CameraZoomController : CameraBaseController
    {
        public enum ZoomType { DistanceFreeUpdate, DistanceClampedUpdate, DistanceStepUpdate, FOVStepUpdate }
        public ZoomType zoomType = ZoomType.DistanceClampedUpdate;

        // distance update properties
        public string terrainLayerName = "Terrain";
        public float[] distanceSteps = { 5, 10, 40, 60 };
        public float distanceTolerance = 0.5f;
        public float distanceUpdateSpeed = 50; // the zoom speed multiplier   
        public float distanceMininum = 5; // min limit value(distance between camera and focuspoint)
        public float distanceMaximum = 20; // max limit value(distance between camera and focuspoint)

        private int currentDistanceStep;
        private int targetDistanceStep;

        // fov update properties
        public float[] fovSteps = { 5, 20, 60, 80 };
        public int defaultFovStep = 2;
        public float fovTolerance = 0.1f;
        public float fovUpdateSpeed = 20;

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
                switch (zoomType)
                {
                    case ZoomType.DistanceStepUpdate:
                        return currentDistanceStep;
                    case ZoomType.FOVStepUpdate:
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
                switch (zoomType)
                {
                    case ZoomType.DistanceStepUpdate:
                        return targetDistanceStep;
                    case ZoomType.FOVStepUpdate:
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
                switch (zoomType)
                {
                    case ZoomType.DistanceStepUpdate:
                        float minDistance = distanceSteps[0];
                        float maxDistance = distanceSteps[distanceSteps.Length - 1];
                        return (maxDistance - minDistance);
                    case ZoomType.FOVStepUpdate:
                        return targetFovStep;
                }
                return -1;
            }
        }

        Vector3 targetHitPoint
        {
            get
            {
                Ray ray = new Ray(camera.transform.position, camera.transform.forward);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100, 1 << LayerMask.NameToLayer(terrainLayerName)))
                {
                    Debug.DrawLine(ray.origin, hit.point, Color.green);
                    return hit.point;
                }
                else
                {
                    Debug.DrawLine(ray.origin, ray.direction + camera.transform.forward * 100, Color.red);
                }
                return Vector3.zero;
            }
        }


        private float cameraDistance
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
        }

        void Update()
        {
            cameraZoomDelta = CameraInputManager.Instance.GetZoomInputDelta();
            switch (zoomType)
            {
                case ZoomType.FOVStepUpdate:
                    UpdateFovStep();
                    break;
                case ZoomType.DistanceStepUpdate:
                    UpdateDistanceStep();
                    break;
                case ZoomType.DistanceClampedUpdate:
                    DistanceClampedUpdate();
                    break;
                case ZoomType.DistanceFreeUpdate:
                    DistanceFreeUpdate();
                    break;
            }
        }

        void SetupDistance()
        {

        }

        void SetupFOV()
        {
            if (defaultFovStep > fovSteps.Length - 1 || defaultFovStep < 0)
            {
                Debug.LogWarning("[IFC.RTSCamera.CameraZoomController()] defaultFovLevel auto set to 0");
                defaultFovStep = 0;
            }
            currentFovStep = defaultFovStep;
            targetFovStep = currentFovStep;
            camera.fieldOfView = fovSteps[defaultFovStep];
        }



        void UpdateDistanceStep()
        {
            float zoomDelta = CameraInputManager.Instance.GetZoomInputDelta();
            Vector3 targetHitPoint = this.targetHitPoint;

            if (currentDistanceStep == targetDistanceStep)
            {
                if (zoomDelta < 0)
                {
                    targetDistanceStep++;
                }

                if (zoomDelta > 0)
                {
                    targetDistanceStep--;
                }

                targetDistanceStep = Mathf.Clamp(targetDistanceStep, 0, distanceSteps.Length - 1);
            }

            if (cameraDistance != distanceSteps[targetDistanceStep])
            {
                camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, fovSteps[targetFovStep], fovUpdateSpeed * Time.deltaTime);
                int zoomDirection = currentDistanceStep > targetDistanceStep ? 1 : -1;

                // zooomDirection correction 
                if (currentDistanceStep > targetDistanceStep && cameraDistance < distanceSteps[targetDistanceStep])
                {
                    zoomDirection = -1; // reverse zoom direction
                }
                if (currentDistanceStep < targetDistanceStep && cameraDistance > distanceSteps[targetDistanceStep])
                {
                    zoomDirection = 1; // reverse zoom direction
                }

                float distanceDelta = Time.deltaTime * distanceUpdateSpeed * zoomDirection;
                Vector3 localPosition = Vector3.MoveTowards(camera.transform.localPosition, targetHitPoint, distanceDelta);
                float minDistance = distanceSteps[targetDistanceStep] >= distanceSteps[currentDistanceStep] ?
                    distanceSteps[currentDistanceStep] : distanceSteps[targetDistanceStep];
                float maxDistance = distanceSteps[targetDistanceStep] >= distanceSteps[currentDistanceStep] ?
                    distanceSteps[targetDistanceStep] : distanceSteps[currentDistanceStep];
                camera.transform.localPosition = ClampVector3(localPosition, targetHitPoint, minDistance, maxDistance);
            }

            if (Mathf.Abs(cameraDistance - distanceSteps[targetDistanceStep]) <= distanceTolerance)
            {
                currentDistanceStep = targetDistanceStep;
            }

        }

        void DistanceFreeUpdate()
        {
            Ray ray = new Ray(camera.transform.position, camera.transform.forward);
            Debug.DrawLine(ray.origin, ray.direction + camera.transform.forward * 100, Color.red);

            if (cameraZoomDelta == 0) return;
            float distance = 100;
            Vector3 screenPoint = new Vector3(Screen.width / 2, Screen.height / 2, distance);
            Vector3 targetPoint = camera.ScreenToWorldPoint(screenPoint);
            Vector3 heading = cameraZoomDelta > 0
                ? targetPoint - camera.transform.position
                : camera.transform.position - targetPoint;
            //Vector3 direction = heading / distance;
            float distanceDelta = Time.deltaTime * distanceUpdateSpeed;
            if (manager.GetSpecial1())
            {
                distanceDelta *= 10;
            }
            camera.transform.position = Vector3.MoveTowards(camera.transform.position, camera.transform.position + heading, distanceDelta);
        }

        void DistanceClampedUpdate()
        {
            Vector3 targetHitPoint = this.targetHitPoint;
            if (cameraZoomDelta == 0) return;

            float distance = Vector3.Distance(camera.transform.localPosition, targetHitPoint);
            if ((distance > distanceMininum && cameraZoomDelta > 0) || (distance < distanceMaximum && cameraZoomDelta < 0))
            {
                float distanceDelta = cameraZoomDelta * Time.deltaTime * distanceUpdateSpeed;
                Vector3 localPosition = Vector3.MoveTowards(camera.transform.localPosition, targetHitPoint, distanceDelta);
                camera.transform.localPosition = ClampVector3(localPosition, targetHitPoint, distanceMininum, distanceMaximum);
            }
        }

        Vector3 ClampVector3(Vector3 localPosition, Vector3 targetPosition, float minDistance, float maxDistance)
        {
            float distance = Vector3.Distance(localPosition, targetPosition);
            Vector3 heading = localPosition - targetPosition;
            Vector3 direction = heading / distance;

            if (distance < minDistance)
            {
                return targetPosition + direction * minDistance;
            }

            if (distance > maxDistance)
            {
                return targetPosition + direction * maxDistance;
            }
            return localPosition;
        }

        void UpdateFovStep()
        {
            if (currentFovStep == targetFovStep)
            {
                if (cameraZoomDelta < 0)
                {
                    targetFovStep++;
                }

                if (cameraZoomDelta > 0)
                {
                    targetFovStep--;
                }

                targetFovStep = Mathf.Clamp(targetFovStep, 0, fovSteps.Length - 1);
            }


            if (camera.fieldOfView != fovSteps[targetFovStep])
            {
                camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, fovSteps[targetFovStep], fovUpdateSpeed * Time.deltaTime);

                float fovMin = fovSteps[targetFovStep] >= fovSteps[currentFovStep] ? fovSteps[currentFovStep] : fovSteps[targetFovStep];
                float fovMax = fovSteps[targetFovStep] >= fovSteps[currentFovStep] ? fovSteps[targetFovStep] : fovSteps[currentFovStep];
                camera.fieldOfView = Mathf.Clamp(camera.fieldOfView, fovMin, fovMax);
            }

            if (Mathf.Abs(camera.fieldOfView - fovSteps[targetFovStep]) <= fovTolerance)
            {
                currentFovStep = targetFovStep;
            }
        }
    }
}
