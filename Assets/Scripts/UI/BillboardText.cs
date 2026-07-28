using UnityEngine;

public class BillboardText : MonoBehaviour
{
    private Transform cam;

    void Start()
    {
        // Get the main camera's transform
        cam = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }

    void LateUpdate()
    {
        // Make the object always look at the camera's position
        transform.LookAt(transform.position + cam.rotation * Vector3.forward, cam.rotation * Vector3.up);
    }
}