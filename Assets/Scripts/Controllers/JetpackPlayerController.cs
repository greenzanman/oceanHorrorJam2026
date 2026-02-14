using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class JetpackPlayerController : MonoBehaviour
{

    public float thrust = 10f;

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
        // Thrust direction. Forward/backward/left/right relative to the camera direction.
        Vector2 horizontalThrustDirection = moveAction.ReadValue<Vector2>();

        float verticalThrust = 0;
        if (thrustUpAction.IsPressed() && !thrustDownAction.IsPressed())
        {
            verticalThrust = 1;
        }
        else if (!thrustUpAction.IsPressed() && thrustDownAction.IsPressed())
        {
            verticalThrust = -1;
        }

        Vector3 cameraForward = playerInput.camera.transform.forward;
        Vector3 cameraRight = playerInput.camera.transform.right;
        Vector3 cameraUp = playerInput.camera.transform.up;
        Vector3 thrustDirection = (cameraForward * horizontalThrustDirection.y + cameraRight * horizontalThrustDirection.x + cameraUp * verticalThrust).normalized;

        rb.AddForce(thrustDirection * thrust, ForceMode.Acceleration);
    }
        
}
