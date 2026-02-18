using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class JetpackPlayerController : MonoBehaviour
{

    public bool disableUpDown = true;
    public float thrust = 10f;
    public float gravity = 1.62f;

    private Rigidbody rb;
    private InputAction moveAction;
    private InputAction thrustUpAction;
    private InputAction thrustDownAction;
    private PlayerInput playerInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        thrustUpAction = playerInput.actions["Thrust Up"];
        thrustDownAction = playerInput.actions["Thrust Down"];
    }


    void FixedUpdate()
    {
        // apply gravity
        rb.AddForce(Vector3.down * gravity, ForceMode.Acceleration);

        // Thrust direction. Forward/backward/left/right relative to the camera direction.
        Vector2 horizontalThrustDirection = moveAction.ReadValue<Vector2>();

        float verticalThrust = 0;
        if (!disableUpDown)
        {
            if (thrustUpAction.IsPressed() && !thrustDownAction.IsPressed())
            {
                verticalThrust = 1;
            }
            else if (!thrustUpAction.IsPressed() && thrustDownAction.IsPressed())
            {
                verticalThrust = -1;
            }
        }
        

        Vector3 cameraForward = playerInput.camera.transform.forward;
        Vector3 cameraRight = playerInput.camera.transform.right;
        Vector3 cameraUp = playerInput.camera.transform.up;
        Vector3 thrustDirection = (cameraForward * horizontalThrustDirection.y + cameraRight * horizontalThrustDirection.x + cameraUp * verticalThrust).normalized;

        RaycastHit hit;
        // Cast a sphere slightly ahead of the player. 
        // '0.5f' (radius) and '0.6f' (distance)
        if (Physics.SphereCast(transform.position, 0.375f, thrustDirection, out hit, 0.45f))
        {
            // Project the thrust vector onto the wall's surface so we slide instead of sticking
            thrustDirection = Vector3.ProjectOnPlane(thrustDirection, hit.normal).normalized;
        }

        rb.AddForce(thrustDirection * thrust, ForceMode.Acceleration);
    }
        
}
