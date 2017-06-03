using UnityEngine;
using System.Collections;

namespace IFP.Camera
{
    // Modified MouseOrbitImproved from unity wiki
    // source: http://wiki.unity3d.com/index.php?title=MouseOrbitImproved

    //[AddComponentMenu("Camera-Control/Mouse Orbit with zoom")]
    public class MouseOrbitController : MonoBehaviour
    {
        public enum MouseButton { None, Left, Middle, Right }
        public MouseButton mouseButton = MouseButton.Right;
        public KeyCode comboKey = KeyCode.LeftAlt;

        public Transform target;

        public bool smoothLookAt = true;
        public float resetTime = 0.25f;
        public float rayMaxDistance = 1000;
        public float resetSnapAngle = 5;
        private float _resetTime0;
        private bool _reseting = false;

        public float distance = 5.0f;
        public float xSpeed = 120.0f;
        public float ySpeed = 120.0f;

        public float yMinLimit = -20f;
        public float yMaxLimit = 80f;

        public float distanceMin = .5f;
        public float distanceMax = 15f;

        private bool _orbiting = false;
        private Rigidbody _rigidbody;

        float x = 0.0f;
        float y = 0.0f;

        // Use this for initialization
        void Start()
        {
            Vector3 angles = transform.eulerAngles;
            x = angles.y;
            y = angles.x;

            _rigidbody = GetComponent<Rigidbody>();

            // Make the rigid body not change rotation
            if (_rigidbody != null) {
                _rigidbody.freezeRotation = true;
            }

            if (target) {
                distance = Vector3.Distance(target.position, transform.position);
            }
        }

        private void LateUpdate()
        {
            if (!target) {
                return;
            }

            _orbiting = CheckOribiting();
            _reseting = CheckReseting();

            distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * 5, distanceMin, distanceMax);

            if (_reseting) {
                UpdateResetingRotation();
                return;
            }           

            if (!_orbiting) {
                return;
            }

            UpdateOrbitalRotation();            
        }
        
        private bool CheckReseting()
        {
            bool resetTrigger = comboKey == KeyCode.None || Input.GetKey(comboKey);
            float angle = Vector3.Angle(transform.forward, target.position - transform.position);
            bool lookingAtTarget = Mathf.Abs(angle) < resetSnapAngle;
            bool reseting = !lookingAtTarget;
            if (reseting) {
                if (_resetTime0 == 0 && resetTrigger) {
                    _resetTime0 = Time.time;
                }
            } else {
                _resetTime0 = 0;
            }            
            return smoothLookAt && reseting && _resetTime0 != 0;
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
            transform.LookAt(target);
            Quaternion targetRotation = transform.rotation;                        
            transform.rotation = Quaternion.Lerp(currentRotation, targetRotation, scale);
        }

        private void UpdateOrbitalRotation()
        {         
            x += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

            y = ClampAngle(y, yMinLimit, yMaxLimit);

            Quaternion rotation = Quaternion.Euler(y, x, 0);

            RaycastHit hit;
            if (Physics.Linecast(target.position, transform.position, out hit)) {
                distance -= hit.distance;
            }
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
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