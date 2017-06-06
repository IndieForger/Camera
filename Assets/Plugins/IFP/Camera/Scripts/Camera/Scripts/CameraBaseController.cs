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

        private static float _pointerTraceTime = 0;
        private static RaycastHit _tpointerTraceHit;
        private static Ray _pointerTraceRay;
        private static bool _pointerTraceFound;

        protected bool PointerTrace(out RaycastHit hit, float rayMaxDistance = 1000)
        {
            Ray ray;
            return PointerTrace(out hit, out ray, rayMaxDistance);
        }

        protected bool PointerTrace(out RaycastHit hit, out Ray ray, float rayMaxDistance = 1000)
        {
            if (_pointerTraceTime == Time.time) {
                hit = _tpointerTraceHit;
                ray = _pointerTraceRay;
                return _pointerTraceFound;
            }
            _pointerTraceFound = false;
            _pointerTraceRay = ray = new Ray(Camera.transform.position, MousePointerDirection);
            if (Physics.Raycast(_pointerTraceRay, out hit, rayMaxDistance)) {
                _pointerTraceFound = true;
                return _pointerTraceFound;
            }
            return _pointerTraceFound;
        }
    }     
}
