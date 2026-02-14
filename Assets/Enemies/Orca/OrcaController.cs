using UnityEngine;

public class OrcaController : MonoBehaviour
{
    [Header("Second Order Dynamics Tuning")]
    [SerializeField] private float frequency = 1.0f;        // Snappy horizontal
    [SerializeField] private float verticalFrequency = 0.4f; // Heavy vertical
    [SerializeField] private float damping = 1f;
    [SerializeField] private float response = 2f;

    [Header("Orca Speeds")]
    [SerializeField] private float wanderSpeed = 1f;
    [SerializeField] private float escapeSpeed = 4f;
    private float swimSpeed;

    [Header("Vision Threshold")]
    [SerializeField] private float visionThreshold = 0.1f;

    [Header("Horizontal Baseline")]
    [SerializeField] private float baselinePullStrength = 2.0f; 
    [Range(0, 1)] 
    [SerializeField] private float verticalIntentSquash = 0.2f;

    private SecondOrderDynamics xzDynamics;
    private SecondOrderDynamics yDynamics;

    void Start()
    {
        // horizontal and vertical dynamics
        xzDynamics = new SecondOrderDynamics(transform.position, frequency, damping, response);
        yDynamics = new SecondOrderDynamics(transform.position, verticalFrequency, damping, response);
    }

    void Update()
    {
        if (PlayerTester.playerInstance != null)
        {
            Vector3 swimDirection;
            Vector3 playerPosition = PlayerTester.playerInstance.transform.position;

            // --- STEP 1: GOAL / INTENT ---
            // swim to player if not spotted, otherwise flee
            if (!PlayerTester.playerInstance.InVision(transform.position, visionThreshold))
            {
                swimDirection = (playerPosition - transform.position).normalized;
                swimSpeed = wanderSpeed;
            }
            else
            {
                Vector2 visionPosition = PlayerTester.playerInstance.InVisionPos(transform.position);

                Vector3 camDirection = PlayerTester.playerInstance.CameraFacing();
                Vector3 cameraX = Vector3.Cross(camDirection, Vector3.up).normalized;
                Vector3 cameraY = Vector3.Cross(camDirection, cameraX).normalized;

                visionPosition.x -= 0.5f;
                visionPosition.y -= 0.5f;
                
                // Bias Y to favor horizontal exits
                visionPosition.y *= 4.0f; 

                visionPosition.Normalize();
                visionPosition *= -1;

                swimDirection = visionPosition.x * cameraX + visionPosition.y * cameraY;
                swimSpeed = escapeSpeed;
            }

            // --- STEP 2: SQUASH VERTICAL, YO ---
            Vector3 targetVelocity = swimDirection * swimSpeed;

            // reduce vertical intent
            float verticalDesire = targetVelocity.y * verticalIntentSquash;
            // pull toward player's Y level
            float distToBaseline = playerPosition.y - transform.position.y;
            float targetVelY = verticalDesire + (distToBaseline * baselinePullStrength);

            // --- STEP 3: UPDATE PHYSICS ---
            xzDynamics.Increment(new Vector3(targetVelocity.x, 0, targetVelocity.z), Time.deltaTime);
            yDynamics.Increment(new Vector3(0, targetVelY, 0), Time.deltaTime);

            // --- STEP 4: APPLY FINAL POSITION & ROTATION ---
            transform.position = new Vector3(
                xzDynamics.smoothedPosition.x, 
                yDynamics.smoothedPosition.y, 
                xzDynamics.smoothedPosition.z
            );

            Vector3 combinedVelocity = new Vector3(
                xzDynamics.smoothedVelocity.x, 
                yDynamics.smoothedVelocity.y, 
                xzDynamics.smoothedVelocity.z
            );

            if (combinedVelocity.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(combinedVelocity);
            }
        }
    }
}