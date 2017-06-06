using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IFP.Camera
{
    public class CameraTracerController : CameraBaseController
    {
        // settings 

        public enum TriggerType { None, Manual, OnClick, OnDoubleClick }
        public TriggerType triggerType = TriggerType.OnDoubleClick;

        public enum MouseButton { None, Left, Middle, Right }
        public MouseButton mouseButton = MouseButton.Left;

        public KeyCode comboKey = KeyCode.None;

        public float doubleClickTime = 0.7f; // seconds
        public float nonDragDistance = 4; // pixels
        public float rayMaxDistance = 1000; // meters
        public float transitionTime = 1; // seconds

        public float defaultDistance = 0; // use current distance

        // API 

        public bool IsHit { get { return _tracing; } }        
        public RaycastHit Hit { get { return _traceHit; } }
        public Collider HitCollider { get { return _traceHit.collider; } }
        public Vector3 HitPoint { get { return _traceHit.point; } }

        // private 

        private Vector2 _mousePosition;
        private float _clickTime;
        private int _clicks = 0;
        private bool _tracing = false;
        private RaycastHit _traceHit;        

        protected virtual void Update()
        {
            _tracing = false;

            if (comboKey != KeyCode.None && !Input.GetKey(comboKey)) {
                return;
            }

            if ((Input.GetMouseButtonDown(0) && mouseButton == MouseButton.Left) ||
                (Input.GetMouseButtonDown(1) && mouseButton == MouseButton.Right) ||
                (Input.GetMouseButtonDown(2) && mouseButton == MouseButton.Middle)) {
                OnMouseButtonDown();
            }

            if ((Input.GetMouseButtonUp(0) && mouseButton == MouseButton.Left) ||
                (Input.GetMouseButtonUp(1) && mouseButton == MouseButton.Right) ||
                (Input.GetMouseButtonUp(2) && mouseButton == MouseButton.Middle)) {
                OnMouseButtonUp();
            }
        }

        private void OnMouseButtonDown()
        {
            _mousePosition = Input.mousePosition;
        }

        private void OnMouseButtonUp()
        {
            bool shouldFindTarget = false;
            UpdateClickCount();

            switch (triggerType) {
            case TriggerType.OnClick:
                shouldFindTarget = _clicks == 1;
                break;
            case TriggerType.OnDoubleClick:
                shouldFindTarget = _clicks == 2;
                break;
            }

            RaycastHit hit;
            if (shouldFindTarget && PointerTrace(out hit)) {            
                _tracing = true;
                _traceHit = hit;
            }
        }

        private void UpdateClickCount()
        {
            if (Vector2.Distance(_mousePosition, Input.mousePosition) > nonDragDistance) {
                _clicks = 0;
                return;
            }

            _mousePosition = Input.mousePosition;
            if (_clicks == 1 && _clickTime + doubleClickTime >= Time.time) {
                _clicks++;
            } else {
                _clicks = 1;
            }

            _clickTime = Time.time;
        }
    }
}