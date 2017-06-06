using UnityEngine;
using System.Collections;
namespace IFP.Camera
{
    public class CameraBaseController : MonoBehaviour
    {

        private UnityEngine.Camera _camera;
        protected UnityEngine.Camera Camera { get { return _camera; } }

        protected void Awake()
        {
            _camera = GetComponent<UnityEngine.Camera>();
        } 

        protected Vector3 MousePointerDirection
        {
            get
            {
                var mousePos = Input.mousePosition;
                mousePos.z = 10; // select distance = 10 units from the camera
                var worldPos = Camera.ScreenToWorldPoint(mousePos);
                return (worldPos - transform.position).normalized;
            }
        }
    }
}
