using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform camTransform;

    void Start()
    {
        // Automatically find the main camera
        camTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        // Rotate to look at the camera
        transform.LookAt(transform.position + camTransform.rotation * Vector3.forward,
                         camTransform.rotation * Vector3.up);
    }
}