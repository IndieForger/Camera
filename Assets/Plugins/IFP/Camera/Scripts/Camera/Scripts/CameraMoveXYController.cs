using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMoveXYController : MonoBehaviour {

    public enum MouseButton { None, Left, Middle, Right }
    public MouseButton mouseButton = MouseButton.Left;

    public KeyCode comboKey = KeyCode.None;

    public float sensitivity = 10;
    private Vector2 _lastMousePositon;

    private void Update()
    {
        if (comboKey != KeyCode.None && !Input.GetKey(comboKey)) {
            return;
        }

        if ((mouseButton == MouseButton.Left && Input.GetMouseButtonDown(0)) ||
             (mouseButton == MouseButton.Right && Input.GetMouseButtonDown(1)) ||
             (mouseButton == MouseButton.Middle && Input.GetMouseButtonDown(2))
        ) {
            ResetPositionUpdate();
        }

        if ((mouseButton == MouseButton.Left && Input.GetMouseButton(0)) ||
            (mouseButton == MouseButton.Right && Input.GetMouseButton(1)) ||
            (mouseButton == MouseButton.Middle && Input.GetMouseButton(2))
        ) {
            UpdatePosition();
        }

    }

    private void ResetPositionUpdate()
    {
        _lastMousePositon = Input.mousePosition;
    }

    private void UpdatePosition()
    {        
        Vector2 currentMousePosition = Input.mousePosition;       
        Vector2 delta = (_lastMousePositon - currentMousePosition) * sensitivity;        
        //Vector3 position = transform.localPosition;
        Vector3 distance = new Vector3(delta.x, delta.y, 0);
        //Debug.Log(distance);
        transform.Translate(distance, Space.Self);
        //transform.localPosition = position + distance;
        _lastMousePositon = currentMousePosition;
    }
}
