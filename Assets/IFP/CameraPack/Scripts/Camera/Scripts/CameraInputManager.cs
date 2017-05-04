using UnityEngine;
using IFC.Helpers;

namespace IFC.Camera
{

    public class CameraInputManager : MonoSingleton<CameraInputManager>
    {
        new public UnityEngine.Camera camera;

        public KeyCode zoomIn = KeyCode.Plus;
        public KeyCode zoomOut = KeyCode.Minus;
        public KeyCode moveLeft = KeyCode.A;
        public KeyCode moveRight = KeyCode.D;
        public KeyCode moveBack = KeyCode.S;
        public KeyCode moveForward = KeyCode.W;
        public KeyCode centerOnTarget = KeyCode.C;
        public KeyCode special1 = KeyCode.LeftShift;

        void Awake()
        {
            if (camera == null) {
                camera = GetComponent<UnityEngine.Camera>();
            }
            if (camera == null) {
                camera = UnityEngine.Camera.main;
            }
        }

        public bool GetSpecial1()
        {
            return (Input.GetKey(special1));
        }

        public float GetZoomInputDelta()
        {
            float value = 0;

            if (Input.GetKey(zoomOut)) {
                value = -0.3f;
            } else if (Input.GetKey(zoomIn)) {
                value = 0.3f;
            }

            if (Input.GetAxis("Mouse ScrollWheel") < 0) {
                value = -1;
            } else if (Input.GetAxis("Mouse ScrollWheel") > 0) {
                value = 1;
            }

            return value;
        }
    }
}