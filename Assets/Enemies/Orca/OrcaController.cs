using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Swims towards player, if seen, attempts to swim out of view
public class OrcaController : MonoBehaviour
{
    private float swimSpeed = 1;
    private SecondOrderDynamics dynamics; 
    // Start is called before the first frame update
    void Start()
    {
        
        // SOD initializer
        dynamics = new SecondOrderDynamics(transform.position, 0.5f, 1, 2);
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerTester.playerInstance != null)
        {
            Vector3 swimDirection;
            Vector3 playerPosition = PlayerTester.playerInstance.transform.position;

            if (!PlayerTester.playerInstance.InVision(transform.position, 0.1f))
            {
                swimDirection = (PlayerTester.playerInstance.transform.position - transform.position).normalized;
                swimSpeed = 1;
            }
            else
            {
                // If in vision, find a good direction to run
                Vector2 visionPosition = PlayerTester.playerInstance.InVisionPos(transform.position);

                Vector3 camDirection = PlayerTester.playerInstance.cameraFacing();
                Vector3 cameraX = Vector3.Cross(camDirection, Vector3.up).normalized;
                Vector3 cameraY = Vector3.Cross(camDirection, cameraX).normalized;

                // Find closest direction out of camera
                visionPosition.x -= 0.5f;
                visionPosition.y -= 0.5f;
                visionPosition.Normalize();
                visionPosition *= -1;

                swimDirection = visionPosition.x * cameraX + visionPosition.y * cameraY;
                swimSpeed = 4;
            }

            dynamics.Increment(swimDirection * swimSpeed, Time.deltaTime);
            transform.position = dynamics.smoothedPosition;
            transform.rotation = Quaternion.LookRotation(dynamics.smoothedVelocity);
        }
    }
}
