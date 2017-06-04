using UnityEngine;
using System.Collections;

namespace IFP.Camera
{
    public class CameraLookAroundController : CameraBaseController
    {

        public enum MouseButton { None, Left, Middle, Right }
        public MouseButton mouseButton = MouseButton.Right;
        public Vector2 sensitivity = new Vector2(4, 2);
        
        public bool invertX = false;
        public bool invertY = false;

        private void Update()
        {            
            if (IsLookingAround(mouseButton)) {
                LookAround();
            }
        }

        void LookAround()
        {
            Vector2 inversion = new Vector2(invertX ? -1 : 1, invertY ? 1 : -1);
            transform.Rotate(Vector3.right, Input.GetAxis("Mouse Y") * sensitivity.y * inversion.y, Space.Self);
            transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * sensitivity.x * inversion.x, Space.World);
        }

        bool IsLookingAround(MouseButton mouseButton)
        {
            switch (mouseButton) {
            case MouseButton.Left: return Input.GetMouseButton(0);
            case MouseButton.Right: return Input.GetMouseButton(1);
            case MouseButton.Middle: return Input.GetMouseButton(2);
            }
            return false;
        }
    }
}
