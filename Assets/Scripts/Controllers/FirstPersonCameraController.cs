using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonCameraController : MonoBehaviour
{
    public float sensitivity = 0.2f;

    private float xRotation = 0f;
    private Transform playerBody;
    private Rigidbody playerRb;

    // stored mouse input deltas to use
    private float mouseX;
    private float mouseY;

    // smoothing variables
    private float smoothMouseX;
    private float smoothMouseY;
    private float smoothMouseXVelocity;
    private float smoothMouseYVelocity;
    public float smoothTime = 0.2f;

    void Start()
    {
        playerBody = transform.parent;
        playerRb = playerBody.GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OnLook(InputValue value)
    {
        Vector2 lookInput = value.Get<Vector2>();
        // store the mouse input to use below
        mouseX = lookInput.x * sensitivity;
        mouseY = lookInput.y * sensitivity;
    }

    // run camera and player rotation after game logic (Update) but before rendering
    void LateUpdate()
    {
        // Apply smoothing to mouse input using SmoothDamp
        smoothMouseX = Mathf.SmoothDamp(smoothMouseX, mouseX, ref smoothMouseXVelocity, smoothTime);
        smoothMouseY = Mathf.SmoothDamp(smoothMouseY, mouseY, ref smoothMouseYVelocity, smoothTime);

        // Vertical rotation (pitch)
        xRotation -= smoothMouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Horizontal rotation (yaw)
        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * smoothMouseX);
        }

        // Clear mouse deltas after applying
        mouseX = 0;
        mouseY = 0;
    }
}