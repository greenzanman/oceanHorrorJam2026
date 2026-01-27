using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
/**
 * Stroke movement.
 * Press a button to apply a burst of force in the desired direction.
 * This force will only be applied once per button press, with a cooldown before the next stroke.
 * Direction is determined by the camera orientation and input axes.
 */
public class StrokePlayerController : MonoBehaviour
{
    
    public float strokeForce = 2.5f;
    public float strokeCooldownSeconds = 1f;
    public float strokeInputBufferSeconds = 0.5f; // Time window to buffer stroke input

    private Rigidbody rb;
    private InputAction moveAction;
    private InputAction strokeAction;
    private InputAction thrustUpAction;
    private InputAction thrustDownAction;
    private PlayerInput playerInput;

    private bool strokeInputBuffered = false; 

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        strokeAction = playerInput.actions["Stroke"];
        thrustUpAction = playerInput.actions["Thrust Up"];
        thrustDownAction = playerInput.actions["Thrust Down"];
    }

    private Vector3 GetMovementDirection() {
        Vector2 horizontalDirection = moveAction.ReadValue<Vector2>();

        float verticalDirection = 0;
        if (thrustUpAction.IsPressed() && !thrustDownAction.IsPressed())
        {
            verticalDirection = 1;
        }
        else if (!thrustUpAction.IsPressed() && thrustDownAction.IsPressed())
        {
            verticalDirection = -1;
        }

        Vector3 cameraForward = playerInput.camera.transform.forward;
        Vector3 cameraRight = playerInput.camera.transform.right;
        Vector3 cameraUp = playerInput.camera.transform.up;
        return (cameraForward * horizontalDirection.y + cameraRight * horizontalDirection.x + cameraUp * verticalDirection).normalized;
    }

    private void ApplyStroke(Vector3 direction)
    {
        if (strokeInputBuffered)
        {
            StopBufferStrokeInput();
        }
        rb.AddForce(direction * strokeForce, ForceMode.VelocityChange);
        strokeAction.Disable();
        Invoke(nameof(EnableStrokeAction), strokeCooldownSeconds);
    }

    private void ApplyStroke()
    {
        Vector3 strokeDirection = GetMovementDirection();
        ApplyStroke(strokeDirection);
    }

    /**
     * Called when the stroke input action is performed.
     * (default spacebar)
     */
    void OnStroke()
    {

        Vector3 strokeDirection = GetMovementDirection();
        if (strokeDirection == Vector3.zero) {
            BufferStrokeInput();
        } else {
            ApplyStroke(strokeDirection);
        }
    }

    /**
     * Called when movement input is detected.
     * Checks if there is a buffered stroke input to apply.
     */
    void OnMove()
    {
        if (strokeInputBuffered)
        {
            ApplyStroke();
        }
    }

    void OnThrustUp() {
        OnMove();
    }

    void OnThrustDown() {
        OnMove();
    }

    void EnableStrokeAction()
    {
        strokeAction.Enable();
    }

    void BufferStrokeInput()
    {
        strokeInputBuffered = true;
        Invoke(nameof(StrokeBufferTimeout), strokeInputBufferSeconds);
    }

    void StopBufferStrokeInput()
    {
        strokeInputBuffered = false;
        CancelInvoke(nameof(StrokeBufferTimeout));
    }

    void StrokeBufferTimeout()
    {
        strokeInputBuffered = false;
    }
}
