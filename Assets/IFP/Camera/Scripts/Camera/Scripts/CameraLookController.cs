using UnityEngine;

namespace IFP.Camera
{    
    public class CameraLookController : CameraBaseController
    {
        public enum MouseButton { None, Left, Middle, Right }
        public MouseButton mouseButton = MouseButton.Right;
        public KeyCode comboKey = KeyCode.None;        
        public Vector2 sensitivity = new Vector2(4, 2);

        public bool clampXAngle = false;
        public float minXAngle = -90F;
        public float maxXAngle = 90F;

        public bool invertMouseX = false;
        public bool invertMouseY = false;

        private void Update()
        {            
            if (IsLookingAround(mouseButton)) {
                LookAround();
            }
        }

        void LookAround()
        {
            Vector2 inversion = new Vector2(invertMouseX ? -1 : 1, invertMouseY ? 1 : -1);
            float angleX = Input.GetAxis("Mouse Y") * sensitivity.y * inversion.y;            
            
            if (clampXAngle) {
                Quaternion newRotation = transform.localRotation;
                newRotation *= Quaternion.Euler(angleX, 0f, 0f);
                newRotation = ClampRotationAroundXAxis(newRotation);
                transform.localRotation = newRotation;
            } else {               
                transform.Rotate(Vector3.right, angleX, Space.Self);
            }
            
            float angleY = Input.GetAxis("Mouse X") * sensitivity.x * inversion.x;
            transform.Rotate(Vector3.up, angleY, Space.World);
        }

        bool IsLookingAround(MouseButton mouseButton)
        {
            if (comboKey == KeyCode.None || Input.GetKey(comboKey)) {
                switch (mouseButton) {
                case MouseButton.Left: return Input.GetMouseButton(0);
                case MouseButton.Right: return Input.GetMouseButton(1);
                case MouseButton.Middle: return Input.GetMouseButton(2);
                }
            }            
            return false;
        }

        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
            angleX = Mathf.Clamp(angleX, minXAngle, maxXAngle);
            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }
    }
}
