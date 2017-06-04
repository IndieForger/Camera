using UnityEngine;
using System.Collections;
namespace IFP.Camera
{
    public class CameraBaseController : MonoBehaviour
    {
        protected new UnityEngine.Camera camera { get { return CameraInputManager.Instance.camera; } }

        protected CameraInputManager manager { get { return CameraInputManager.Instance; } }

        protected Vector3 MousePointerDirection
        {
            get
            {
                var mousePos = Input.mousePosition;
                mousePos.z = 10; // select distance = 10 units from the camera
                var worldPos = camera.ScreenToWorldPoint(mousePos);
                return (worldPos - transform.position).normalized;
            }
        }
    }
}
