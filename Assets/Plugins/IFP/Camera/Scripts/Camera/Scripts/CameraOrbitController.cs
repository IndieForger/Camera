using UnityEngine;
using System.Collections;

namespace IFP.Camera
{
    // Modified MouseOrbitImproved from unity wiki
    // source: http://wiki.unity3d.com/index.php?title=MouseOrbitImproved

    //[AddComponentMenu("Camera/CameraOrbitController")]
    public class CameraOrbitController : MonoBehaviour
    {
        public enum MouseButton { None, Left, Middle, Right }
        public MouseButton mouseButton = MouseButton.Right;
        public KeyCode comboKey = KeyCode.LeftAlt;

        public enum UpdateMethod { None, FreeUpdate, ClampedUpdate, Cinematic }
        public UpdateMethod updateMethod = UpdateMethod.ClampedUpdate;

        public Transform target;
        public bool horizontal = true;
        public bool vertical = true;
        

        public float resetTime = 0.25f;
        public float lookSnapAngle = 0.2f;

        private float _resetTime0;
        private bool _reseting = false;
        private float _distance;

        public float sensitivity = 1;
        public Vector2 autoOrbitSpeed = Vector3.zero;

        public float yMinLimit = -20f;
        public float yMaxLimit = 80f;

        private bool _orbiting = false;
        private Rigidbody _rigidbody;

        float x = 0.0f;
        float y = 0.0f;

        public bool IsCinematic { get { return updateMethod == UpdateMethod.Cinematic; } }

        // Use this for initialization
        void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();

            // Make the rigid body not change rotation
            if (_rigidbody != null) {
                _rigidbody.freezeRotation = true;
            }
        }

        private void LateUpdate()
        {
            if (!target) {
                return;
            }

            _distance = Vector3.Distance(target.position, transform.position);

            _orbiting = CheckOribiting() || IsCinematic;
            _reseting = CheckReseting();            

            if (_reseting) {
                InitOrbitalRotation();
                UpdateResetingRotation();                
                return;
            }
           
            if (!_orbiting) {
                return;
            }

            switch(updateMethod) {
            case UpdateMethod.ClampedUpdate:
                ClampedUpdateOrbitalRotation();
                break;
            case UpdateMethod.Cinematic:
            case UpdateMethod.FreeUpdate:
                FreeUpdateOrbitalRotation();
                break;                           
            }
            
           
        }
        
        private bool CheckReseting()
        {
            bool resetTrigger = comboKey == KeyCode.None || Input.GetKey(comboKey);
            float angle = Vector3.Angle(transform.forward, target.position - transform.position);
            bool lookingAtTarget = Mathf.Abs(angle) < lookSnapAngle;
            bool expired = false;
            bool reseting = !lookingAtTarget;
            if (reseting) {
                if (_resetTime0 == 0 && resetTrigger) {
                    _resetTime0 = Time.time;
                }
                expired = Time.time - _resetTime0 > resetTime;               
            } else {
                _resetTime0 = 0;
            }            
            return reseting && !expired;
        }
        
        private bool CheckOribiting() 
        {
            var orbiting = false;
            if ((mouseButton == MouseButton.Left && Input.GetMouseButton(0)) ||
                (mouseButton == MouseButton.Right && Input.GetMouseButton(1)) ||
                (mouseButton == MouseButton.Middle && Input.GetMouseButton(2))
                ) {
                if (comboKey == KeyCode.None || Input.GetKey(comboKey)) {
                    orbiting = true;
                }
            }
            return orbiting;
        }

        private void UpdateResetingRotation()
        {
            float scale = (Time.time - _resetTime0) / resetTime;
            Quaternion currentRotation = transform.rotation;
            float angle = Vector3.Angle(transform.forward, target.position - transform.position);
            bool isLookingAtTarget = Mathf.Abs(angle) > 0 && Mathf.Abs(angle) < lookSnapAngle;
            transform.LookAt(target);
            if (!isLookingAtTarget) {
                Quaternion targetRotation = transform.rotation;
                transform.rotation = Quaternion.Lerp(currentRotation, targetRotation, scale);
            }
        }

        private void InitOrbitalRotation()
        {
            Vector3 angles = transform.eulerAngles;
            x = angles.y;
            y = angles.x;
        }

        private void FreeUpdateOrbitalRotation()
        {
            float distance = Vector3.Distance(target.position, transform.position);

            float angleY = horizontal ? Input.GetAxis("Mouse X") * sensitivity : 0;
            float angleX = vertical ? Input.GetAxis("Mouse Y") * sensitivity : 0;

            if (IsCinematic) {
                angleY = autoOrbitSpeed.x;
                angleX = autoOrbitSpeed.y;
            }

            transform.Rotate(Vector3.up, angleY, Space.World);            
            transform.Rotate(Vector3.left, angleX, Space.Self);

            transform.position = target.position - transform.forward * distance;
        }

        private void ClampedUpdateOrbitalRotation()
        {
            x += horizontal ? Input.GetAxis("Mouse X") * sensitivity : 0;
            if (vertical) { 
                y -= Input.GetAxis("Mouse Y") * sensitivity;
                y = ClampAngle(y, yMinLimit, yMaxLimit);
            }

            Quaternion rotation = Quaternion.Euler(y, x, 0);

            RaycastHit hit;
            if (Physics.Linecast(target.position, transform.position, out hit)) {
                _distance -= hit.distance;
            }
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -_distance);
            Vector3 position = rotation * negDistance + target.position;

            transform.rotation = rotation;
            transform.position = position;
        }

        public static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360F)
                angle += 360F;
            if (angle > 360F)
                angle -= 360F;
            return Mathf.Clamp(angle, min, max);
        }
    }
}