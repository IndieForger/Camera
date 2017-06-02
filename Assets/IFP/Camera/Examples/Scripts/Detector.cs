using System.Collections.Generic;
using UnityEngine;

namespace IFP.Camera.Examples
{
    public class Detector : MonoBehaviour
    {

        public CameraZoomController zoomCtrl;
        public float step = 0;
        public float snapDistance = 6;
        public List<Transform> objects = new List<Transform>();

        public Material lineMat;
        public float radius = 0.05f;
        public Mesh cubeMesh;
        private GameObject pointerPivot;

        private void Start()
        {
            InitPointer();
        }

        private void Update()
        {
            Vector3 point;
            if (FindHitPoint(out point)) {
                Transform transform = FindClosest(point);
                UpdatePointer(point, transform.position);
            }
        }

        private void InitPointer()
        {
            // create pointer pivot
            pointerPivot = new GameObject();
            pointerPivot.name = "Pointer #" + step;
            pointerPivot.transform.parent = this.gameObject.transform;
            pointerPivot.transform.localPosition = Vector3.zero;

            // create pointer mesh
            GameObject pointerMesh = new GameObject();
            pointerMesh.name = "Pointer mesh";
            pointerMesh.transform.parent = pointerPivot.transform;

            // adjust to pivot and orient
            pointerMesh.transform.localPosition = new Vector3(0f, 0.001f, 0.5f);
            pointerMesh.transform.localScale = new Vector3(1, 1, 1);
            pointerMesh.transform.rotation = Quaternion.Euler(90, 0, 0);

            // add mesh and apply material
            MeshFilter ringMesh = pointerMesh.AddComponent<MeshFilter>();
            ringMesh.mesh = this.cubeMesh;
            MeshRenderer ringRenderer = pointerMesh.AddComponent<MeshRenderer>();
            ringRenderer.material = lineMat;
        }

        private void UpdatePointer(Vector3 origin, Vector3 targetPoint)
        {
            pointerPivot.transform.position = origin;
            var delta = origin - targetPoint;
            var distance = Vector3.Distance(origin, targetPoint);
            var shouldSnap = distance < snapDistance;
            pointerPivot.SetActive(shouldSnap);
            if (!shouldSnap) return;
            pointerPivot.transform.localScale = new Vector3(radius, radius, distance);
            pointerPivot.transform.LookAt(targetPoint, Vector3.up);
        }

        private Transform FindClosest(Vector3 point)
        {
            Transform retTrans = null;
            float retDistance = -1;

            foreach (Transform objTrans in objects) {
                float distance = Vector3.Distance(point, objTrans.position);

                if (retTrans == null) {
                    retTrans = objTrans;
                    retDistance = distance;
                    continue;
                }

                if (distance < retDistance) {
                    retDistance = distance;
                    retTrans = objTrans;
                }
            }
            return retTrans;
        }

        private bool FindHitPoint(out Vector3 point)
        {
            UnityEngine.Camera cam = UnityEngine.Camera.main;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(transform.position, transform.up);
            float rayDistance;

            point = Vector3.zero;
            if (plane.Raycast(ray, out rayDistance)) {
                point = ray.GetPoint(rayDistance);
                Debug.DrawLine(cam.transform.position, point, Color.white);
                return true;
            }

            return false;
        }
    }
}