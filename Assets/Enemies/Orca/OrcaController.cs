using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Swims towards player, if seen, attempts to swim out of view
public class OrcaController : MonoBehaviour
{
    [Header("Second Order Dynamics Tuning")]
    [SerializeField] private float frequency = 0.5f;
    [SerializeField] private float damping = 1f;
    [SerializeField] private float response = 2f;

    [Header("Orca Speeds")]
    [SerializeField] private float wanderSpeed = 1f;
    [SerializeField] private float escapeSpeed = 4f;
    private float swimSpeed;

    [Header("Vision Threshold")]
    [SerializeField] private float visionThreshold = 0.1f;

    private SecondOrderDynamics dynamics;
    // Start is called before the first frame update
    void Start()
    {
        
        // SOD initializer
        dynamics = new SecondOrderDynamics(transform.position, frequency, damping, response);
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerTester.playerInstance != null)
        {
            Vector3 swimDirection;
            Vector3 playerPosition = PlayerTester.playerInstance.transform.position;

            if (!PlayerTester.playerInstance.InVision(transform.position, visionThreshold))
            {
                swimDirection = (PlayerTester.playerInstance.transform.position - transform.position).normalized;
                swimSpeed = wanderSpeed;
            }
            else
            {
                // If in vision, find a good direction to run
                Vector2 visionPosition = PlayerTester.playerInstance.InVisionPos(transform.position);

                Vector3 camDirection = PlayerTester.playerInstance.CameraFacing();
                Vector3 cameraX = Vector3.Cross(camDirection, Vector3.up).normalized;
                Vector3 cameraY = Vector3.Cross(camDirection, cameraX).normalized;

                // Find closest direction out of camera
                visionPosition.x -= 0.5f;
                visionPosition.y -= 0.5f;
                visionPosition.Normalize();
                visionPosition *= -1;

                swimDirection = visionPosition.x * cameraX + visionPosition.y * cameraY;
                swimSpeed = escapeSpeed;
            }

            dynamics.Increment(swimDirection * swimSpeed, Time.deltaTime);
            transform.position = dynamics.smoothedPosition;
            transform.rotation = Quaternion.LookRotation(dynamics.smoothedVelocity);
        }
    }
}
