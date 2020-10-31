using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{

    public GameObject target;
    public Vector3 targetSize;

    [Range(0, 1)] public float height;
    [Range(0, 2)] public float objectDistance;
    [Range(0, 1)] public float speed;
    private float theta;

    private Vector3 targetCenter;

    Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();

        cam.transform.position = target.transform.position;

        
        
        
    }

    // Update is called once per frame
    void Update()
    {

        targetSize = target.GetComponent<MeshRenderer>().bounds.size;
        targetCenter = new Vector3(targetSize.x / 2, -targetSize.y / 2, targetSize.z / 2);
        transform.position = targetCenter;
        transform.rotation = Quaternion.identity;
        transform.Translate(objectDistance * targetSize / 2);
        transform.Translate(Vector3.up * height);

        transform.RotateAround(targetCenter, Vector3.up, theta);

        transform.LookAt(targetCenter);

        if (theta > 360)
        {
            theta -= 360;
        }

        if (!GameObject.Find("Fabric").GetComponent<MeshGenerator>().paused)
        {
            theta += speed;
        }
    }

    public void viewUpdate(float value)
    {
        speed = 0;
        theta = value;
    }

    public void speedUpdate(float value)
    {
        speed = value;
    }
}
