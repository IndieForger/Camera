﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IFP.Camera
{
    public class CameraMoveToTarget : CameraBaseController
    {
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

        private Vector2 _mousePosition;
        private float _clickTime;
        private int _clicks = 0;
        private bool _inTransit = false;
        private Vector3 _targetPoint;
        private Vector3 _targetPosition;
        private Vector3 _startPosition;

        void Update()
        {
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

            if (_inTransit) {
                TrasitUpdate();
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
            if (shouldFindTarget && TraceTarget(out hit)) {
                Debug.Log(hit.collider.gameObject.name + " at: " + hit.point);
                StartTransit(hit.point);
            }
        }

        private void StartTransit(Vector3 point) {
            _targetPoint = point;
            _startPosition = Camera.transform.position;

            if (defaultDistance > 0) {
                _targetPosition = _targetPoint - Camera.transform.forward * defaultDistance;
                _inTransit = true;
                return;
            }
            
            Plane plane = new Plane(-transform.forward, point);            
            Ray ray = new Ray(Camera.transform.position, Camera.transform.forward);
            float rayDistance;
            if (plane.Raycast(ray, out rayDistance)) {                
                Vector3 hitPoint = ray.GetPoint(rayDistance);
                Vector3 delta = point - hitPoint;                
                _targetPosition = Camera.transform.position + delta;
                _inTransit = true;
            }            
        }

        private void TrasitUpdate()
        {
            Vector3 camPosition = Camera.transform.position;
            float totalDistance = Vector3.Distance(_startPosition, _targetPosition);
            float distance = Vector3.Distance(camPosition, _targetPosition);
            float speed = totalDistance / transitionTime;
            Vector3 direction = _targetPosition - _startPosition;
            Quaternion rotation = Camera.transform.rotation;           
            Camera.transform.LookAt(_targetPosition);
            Camera.transform.Translate(Vector3.forward * speed * Time.deltaTime);
            Camera.transform.rotation = rotation;
            
            camPosition = Camera.transform.position;
            bool shouldSnap = Vector3.Distance(camPosition, _targetPosition) < 0.1;
            bool movedTooFar = Vector3.Distance(camPosition, _targetPosition) > distance;
            if (shouldSnap || movedTooFar) {
                Camera.transform.position = _targetPosition;
                _inTransit = false;
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

        private bool TraceTarget(out RaycastHit hit)
        {
            Ray ray = new Ray(Camera.transform.position, MousePointerDirection);
            if (Physics.Raycast(ray, out hit, rayMaxDistance)) {                
                return true;
            }
            return false;
        }
    }
}