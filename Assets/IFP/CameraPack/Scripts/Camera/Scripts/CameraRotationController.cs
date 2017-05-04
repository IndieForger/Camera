using UnityEngine;
using System.Collections;

// rotates camera with mouse right click
namespace IFC.Camera
{
    public class CameraRotationController : CameraBaseController
    {
        public float rotateSpeed = 10; // mouse down rotation speed about x and y axes
        public bool restrictedPointer = false; // rotation only works if mouse is inside of the screen
        public Vector3 minAngle = new Vector3(30, 0, 0);
        public Vector3 maxAngle = new Vector3(70, 360, 0);

        Vector3 lastMousePosition;

        void Update()
        {
            if (Input.GetMouseButton(1)) {

                Vector3 mouseDelta;

                if (restrictedPointer) {
                    if (lastMousePosition.x >= 0 &&
                        lastMousePosition.y >= 0 &&
                        lastMousePosition.x <= Screen.width &&
                        lastMousePosition.y <= Screen.height) { 

                        mouseDelta = Input.mousePosition - lastMousePosition;
                    } else {
                        mouseDelta = Vector3.zero;
                    }
                } else {
                    mouseDelta = Input.mousePosition - lastMousePosition;
                 }
              
                var rotation = Vector3.up * Time.deltaTime * rotateSpeed * mouseDelta.x;
                rotation += Vector3.left * Time.deltaTime * rotateSpeed * mouseDelta.y;
                camera.transform.Rotate(rotation, Space.Self);

                // Clamp angles
                rotation = camera.transform.rotation.eulerAngles;
                rotation.x = Mathf.Clamp(rotation.x, minAngle.x, maxAngle.x);
                rotation.y = Mathf.Clamp(rotation.y, minAngle.y, maxAngle.y);
                rotation.z = Mathf.Clamp(rotation.z, minAngle.z, maxAngle.z);
                transform.rotation = Quaternion.Euler(rotation);
            }

            lastMousePosition = Input.mousePosition;
        }
    }
}
