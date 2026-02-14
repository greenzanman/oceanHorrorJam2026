using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonCameraController : MonoBehaviour
{
    public float sensitivity = 0.1f;

    private float xRotation = 0f;
    private Transform playerBody;
    private Rigidbody playerRb;

    // stored mouse input deltas to use
    private float mouseX; 
    private float mouseY;

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

    // run camera rotation after game logic (Update) but before rendering
    void LateUpdate()
    {
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    // run player body rotation in sync with physics updates
    void FixedUpdate()
    {
        if (playerRb != null)
        {
            // Rotate the Rigidbody safely
            Quaternion deltaRotation = Quaternion.Euler(Vector3.up * mouseX);
            playerRb.MoveRotation(playerRb.rotation * deltaRotation);
            
            // clear horiz spin
            mouseX = 0; 
        }
    }
}