using System;
using UnityEngine;

namespace IFP.Camera
{
    public static class Utils
    {
        public static Quaternion ClampRotationAroundXAxis(Quaternion q, float minXAngle, float maxXAngle)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
            angleX = Mathf.Clamp(angleX, minXAngle, maxXAngle);
            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }
    }
}
