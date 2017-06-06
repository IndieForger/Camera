using UnityEngine;
using System.Collections;

namespace IFP.Camera
{
    public class CameraMoveXZController : CameraBaseController
    {
        public enum MoveType { None, Keyboard, ScreenBorder, Auto }
        public MoveType moveType = MoveType.Auto;
        public float internalBorder = 10;   // distance to the edge to start scrolling
        public float activeBorder = 20;    // active screen edge scrolling area
        public bool useActiveBorder = true;
        public float keyPanSpeed = 15;
        public float edgePanSpeed = 15;
        public float speedMultiplayer = 10;  // press and hold functional manager.special1

        void Update()
        {
            Vector3 moveAxis = Vector3.zero;
            switch (moveType) {
            case MoveType.Keyboard:
                if (GetKeyMoveAxis(out moveAxis)) {
                    UpdateCameraPosition(moveAxis, keyPanSpeed);
                }
                break;
            case MoveType.ScreenBorder:
                if (GetEdgeMoveAxis(out moveAxis)) {
                    UpdateCameraPosition(moveAxis, edgePanSpeed);
                }
                break;
            case MoveType.Auto:
                if (GetEdgeMoveAxis(out moveAxis)) {
                    UpdateCameraPosition(moveAxis, edgePanSpeed);
                } else if (GetKeyMoveAxis(out moveAxis)) {
                    UpdateCameraPosition(moveAxis, keyPanSpeed);
                }
                break;
            }

        }

        void UpdateCameraPosition(Vector3 moveAxis, float panSpeed)
        {
            float multiplayer = Input.GetKey(KeyCode.LeftShift) ? speedMultiplayer : 1;
            float yLock = Camera.transform.position.y;
            Camera.transform.Translate(moveAxis * Time.deltaTime * panSpeed * multiplayer);
            Vector3 position = Camera.transform.position;
            Camera.transform.position = new Vector3(position.x, yLock, position.z);
        }

        private bool GetEdgeMoveAxis(out Vector3 moveAxis)
        {
            moveAxis = Vector3.zero;
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2)) return false;

            if (Input.mousePosition.y > Screen.height - internalBorder && IsInActiveArea()) {
                moveAxis.z = 1;
            } else if (Input.mousePosition.y < internalBorder && IsInActiveArea()) {
                moveAxis.z = -1;
            }

            if (Input.mousePosition.x > Screen.width - internalBorder && IsInActiveArea()) {
                moveAxis.x = 1;
            } else if (Input.mousePosition.x < internalBorder && IsInActiveArea()) {
                moveAxis.x = -1;
            }

            return moveAxis != Vector3.zero;
        }
        private bool IsInActiveArea()
        {
            if (!useActiveBorder) return true;

            if ((Input.mousePosition.x > internalBorder - activeBorder) &&
                (Input.mousePosition.x < Screen.width - internalBorder + activeBorder) &&
                (Input.mousePosition.y > internalBorder - activeBorder) &&
                (Input.mousePosition.y < Screen.height - internalBorder + activeBorder)) {

                return true;
            }
            return false;
        }
        private bool GetKeyMoveAxis(out Vector3 moveAxis)
        {
            moveAxis = Vector3.zero;

            if (Input.GetKey(KeyCode.W)) {
                moveAxis.z = 1;
            } else if (Input.GetKey(KeyCode.S)) {
                moveAxis.z = -1;
            }

            if (Input.GetKey(KeyCode.A)) {
                moveAxis.x = -1;
            } else if (Input.GetKey(KeyCode.D)) {
                moveAxis.x = 1;
            }

            return moveAxis != Vector3.zero;
        }

    }
}
