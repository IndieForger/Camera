using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDragController : MonoBehaviour {

    public enum Axis { None, MouseX, MouseY }
    public Axis axisX = Axis.MouseX;
    public Axis axisY = Axis.MouseY;
    public Axis axisZ = Axis.None;

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
        Vector2 delta = (_lastMousePositon - currentMousePosition);

        float dx = axisX == Axis.MouseX ? delta.x : axisX == Axis.MouseY ? delta.y : 0;
        float dy = axisY == Axis.MouseX ? delta.x : axisY == Axis.MouseY ? delta.y : 0;
        float dz = axisZ == Axis.MouseX ? delta.x : axisZ == Axis.MouseY ? delta.y : 0;

        Vector3 distance = new Vector3(dx, dy, dz) * sensitivity * Time.deltaTime;

        Debug.Log(distance);
        transform.Translate(distance, Space.Self);
        //transform.localPosition = position + distance;
        _lastMousePositon = currentMousePosition;
    }
}
