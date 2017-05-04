using UnityEngine;
using System.Collections;
namespace IFC.Camera
{
    public class CameraBaseController : MonoBehaviour
    {
        protected new UnityEngine.Camera camera { get { return CameraInputManager.Instance.camera; } }
        protected CameraInputManager manager { get { return CameraInputManager.Instance; } }
    }
}
