using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonCameraController : MonoBehaviour
{
    public float sensitivity = 0.2f;
    public float smoothTime = 0.1f;

    private Transform playerBody;
    private Rigidbody playerRb;

    private float targetXRotation = 0f;
    private float targetYRotation = 0f;

    private float currentXRotation = 0f;
    private float currentYRotation = 0f;

    private float xRotationVelocity;
    private float yRotationVelocity;

    void Start()
    {
        playerBody = transform.parent;
        playerRb = playerBody.GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;

        // Initialize to current body rotation to prevent snapping on start
        targetYRotation = playerBody.eulerAngles.y;
        currentYRotation = targetYRotation;
    }

    public void OnLook(InputValue value)
    {
        Vector2 lookInput = value.Get<Vector2>();
        
        // Accumulate raw input directly into the target angles
        targetYRotation += lookInput.x * sensitivity;
        targetXRotation -= lookInput.y * sensitivity;
        
        // Prevent looking too far up or down
        targetXRotation = Mathf.Clamp(targetXRotation, -90f, 90f);
    }

    void Update()
    {
        if (Time.timeScale == 0) return;

        // Smoothly interpolate the current angles toward the target angles
        currentXRotation = Mathf.SmoothDamp(currentXRotation, targetXRotation, ref xRotationVelocity, smoothTime);
        currentYRotation = Mathf.SmoothDamp(currentYRotation, targetYRotation, ref yRotationVelocity, smoothTime);

        // Apply pitch (up/down) locally to the camera
        transform.localRotation = Quaternion.Euler(currentXRotation, 0f, 0f);
    }

    void FixedUpdate()
    {
        // Apply yaw (left/right) to the physics body using MoveRotation
        if (playerRb != null)
        {
            Quaternion targetBodyRotation = Quaternion.Euler(0f, currentYRotation, 0f);
            playerRb.MoveRotation(targetBodyRotation);
        }
    }
}