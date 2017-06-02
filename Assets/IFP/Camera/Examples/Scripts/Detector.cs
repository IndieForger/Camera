using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IFP.Camera;

public class Detector : MonoBehaviour {

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
        Camera cam = Camera.main;
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


/*

    // Use this for initialization
    void Start()
    {
        this.ringGameObjects = new GameObject[points.Length];
        //this.connectingRings = new ProceduralRing[points.Length];
        for (int i = 0; i < points.Length; i++) {
            // Make a gameobject that we will put the ring on
            // And then put it as a child on the gameobject that has this Command and Control script
            this.ringGameObjects[i] = new GameObject();
            this.ringGameObjects[i].name = "Connecting ring #" + i;
            this.ringGameObjects[i].transform.parent = this.gameObject.transform;

            // We make a offset gameobject to counteract the default cubemesh pivot/origin being in the middle
            GameObject ringOffsetCubeMeshObject = new GameObject();
            ringOffsetCubeMeshObject.transform.parent = this.ringGameObjects[i].transform;

            // Offset the cube so that the pivot/origin is at the bottom in relation to the outer ring     gameobject.
            ringOffsetCubeMeshObject.transform.localPosition = new Vector3(0f, 1f, 0f);
            // Set the radius
            ringOffsetCubeMeshObject.transform.localScale = new Vector3(radius, 1f, radius);

            // Create the the Mesh and renderer to show the connecting ring
            MeshFilter ringMesh = ringOffsetCubeMeshObject.AddComponent<MeshFilter>();
            ringMesh.mesh = this.cubeMesh;

            MeshRenderer ringRenderer = ringOffsetCubeMeshObject.AddComponent<MeshRenderer>();
            ringRenderer.material = lineMat;

        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < points.Length; i++) {
            // Move the ring to the point
            this.ringGameObjects[i].transform.position = this.points[i].transform.position;

            this.ringGameObjects[i].transform.position = 0.5f * (this.points[i].transform.position + this.mainPoint.transform.position);
            var delta = this.points[i].transform.position - this.mainPoint.transform.position;
            this.ringGameObjects[i].transform.position += delta;

            // Match the scale to the distance
            float cubeDistance = Vector3.Distance(this.points[i].transform.position, this.mainPoint.transform.position);
            this.ringGameObjects[i].transform.localScale = new Vector3(this.ringGameObjects[i].transform.localScale.x, cubeDistance, this.ringGameObjects[i].transform.localScale.z);

            // Make the cube look at the main point.
            // Since the cube is pointing up(y) and the forward is z, we need to offset by 90 degrees.
            this.ringGameObjects[i].transform.LookAt(this.mainPoint.transform, Vector3.up);
            this.ringGameObjects[i].transform.rotation *= Quaternion.Euler(90, 0, 0);
        }
    }
*/