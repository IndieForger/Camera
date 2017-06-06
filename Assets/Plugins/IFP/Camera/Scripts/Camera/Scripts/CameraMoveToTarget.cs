using UnityEngine;

namespace IFP.Camera
{
    [RequireComponent(typeof(CameraTracerController))]
    public class CameraMoveToTarget : CameraTracerController
    {         
        private bool _inTransit = false;
        private Vector3 _targetPoint;
        private Vector3 _targetPosition;
        private Vector3 _startPosition;

        protected override void Update()
        {
            base.Update();

            if (!_inTransit && IsHit) {
                Debug.Log(HitCollider.gameObject.name + " at: " + HitPoint);
                StartTransit(HitPoint);
            }

            if (_inTransit) {
                TrasitUpdate();
            }                
        }     

        private void StartTransit(Vector3 point) {
            _targetPoint = point;
            _startPosition = Camera.transform.position;

            if (defaultDistance > 0) {
                _targetPosition = _targetPoint - Camera.transform.forward * defaultDistance;
                _inTransit = true;
                return;
            }
            
            Plane plane = new Plane(-transform.forward, point);            
            Ray ray = new Ray(Camera.transform.position, Camera.transform.forward);
            float rayDistance;
            if (plane.Raycast(ray, out rayDistance)) {                
                Vector3 hitPoint = ray.GetPoint(rayDistance);
                Vector3 delta = point - hitPoint;                
                _targetPosition = Camera.transform.position + delta;
                _inTransit = true;
            }            
        }

        private void TrasitUpdate()
        {
            Vector3 camPosition = Camera.transform.position;
            float totalDistance = Vector3.Distance(_startPosition, _targetPosition);
            float distance = Vector3.Distance(camPosition, _targetPosition);
            float speed = totalDistance / transitionTime;
            Vector3 direction = _targetPosition - _startPosition;
            Quaternion rotation = Camera.transform.rotation;           
            Camera.transform.LookAt(_targetPosition);
            Camera.transform.Translate(Vector3.forward * speed * Time.deltaTime);
            Camera.transform.rotation = rotation;
            
            camPosition = Camera.transform.position;
            bool shouldSnap = Vector3.Distance(camPosition, _targetPosition) < 0.1;
            bool movedTooFar = Vector3.Distance(camPosition, _targetPosition) > distance;
            if (shouldSnap || movedTooFar) {
                Camera.transform.position = _targetPosition;
                _inTransit = false;
            }
        }       
    }
}