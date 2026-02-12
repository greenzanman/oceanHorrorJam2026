using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinnowIndividual : MonoBehaviour
{
    Vector3 localGoal;

    [Header("Minnow Individual Tuning")]
    [SerializeField] private float goalRadius = 2.5f;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float fearMultiplier = 2.5f;
    [SerializeField] private float scatterMin = 7f;
    [SerializeField] private float scatterMax = 12f;

    [Header("Boid Settings")]
    [SerializeField] private float neighborDistance = 3.0f; // How far they "see"
    [SerializeField] private float separationDistance = 1.0f; // Avoidance bubble

    [Header("Boid Weights")]
    [SerializeField] private float weightSeparation = 1.5f;
    [SerializeField] private float weightAlignment = 1.0f;
    [SerializeField] private float weightCohesion = 1.0f;
    [SerializeField] private float weightGoal = 0.5f;

    Vector3 fearOffset = Vector3.zero;
    int fearCount;
    private SecondOrderDynamics dynamics; 
    // Start is called before the first frame update
    void Start()
    {
        // Choose a random local goal in a radius
        GenerateNewGoal();

        // SOD initializer
        dynamics = new SecondOrderDynamics(transform.localPosition, 0.5f, 1, 2);
    }


    // Backward compatibility: if no neighbors are passed, use only goal direction
    public void ProcessMinnow(float deltaTime)
    {
        Vector3 goalDir = (localGoal - transform.localPosition).normalized;
        float currentSpeed = (fearCount > 0 ? moveSpeed * fearMultiplier : moveSpeed);
        dynamics.Increment(goalDir, currentSpeed * deltaTime);
        transform.localPosition = dynamics.smoothedPosition;
        if ((transform.localPosition - localGoal).sqrMagnitude < 0.1f)
        {
            GenerateNewGoal();
        }
    }

    // Boids-enabled version
    public void ProcessMinnow(float deltaTime, MinnowIndividual[] neighbors)
    {
        Vector3 boidDir = CalculateBoidDirection(neighbors);
        Vector3 goalDir = (localGoal - transform.localPosition).normalized;
        Vector3 finalDir = (boidDir + (goalDir * weightGoal)).normalized;
        float currentSpeed = (fearCount > 0 ? moveSpeed * fearMultiplier : moveSpeed);
        dynamics.Increment(finalDir, currentSpeed * deltaTime);
        transform.localPosition = dynamics.smoothedPosition;
        if ((transform.localPosition - localGoal).sqrMagnitude < 0.1f)
        {
            GenerateNewGoal();
        }
    }

    private Vector3 CalculateBoidDirection(MinnowIndividual[] neighbors)
    {
        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;
        int neighborCount = 0;

        foreach (var other in neighbors)
        {
            if (other == this) continue;

            float dist = Vector3.Distance(transform.localPosition, other.transform.localPosition);

            if (dist < neighborDistance)
            {
                // 1. Separation: Move away if too close
                if (dist < separationDistance)
                {
                    separation += (transform.localPosition - other.transform.localPosition).normalized / dist;
                }

                // 2. Alignment: Face the same way as friends
                alignment += (other.localGoal - other.transform.localPosition).normalized;

                // 3. Cohesion: Move toward the middle of the group
                cohesion += other.transform.localPosition;

                neighborCount++;
            }
        }

        if (neighborCount == 0) return Vector3.zero;

        // Average out the forces
        separation /= neighborCount;
        alignment /= neighborCount;
        cohesion = (cohesion / neighborCount) - transform.localPosition;

         return (separation * weightSeparation) + 
             (alignment * weightAlignment) + 
             (cohesion * weightCohesion);
        }

    public void Fear()
    {
        fearCount = 10;
        fearOffset = Random.onUnitSphere * Random.Range(scatterMin, scatterMax);
        GenerateNewGoal();
    }

    private void GenerateNewGoal()
    {
        if (fearCount > 0)
        {
            localGoal = fearOffset + Random.insideUnitSphere * goalRadius;
            fearCount -= 1;
        }
        else
        {
            localGoal = Random.insideUnitSphere * goalRadius;
        }
    }
}
