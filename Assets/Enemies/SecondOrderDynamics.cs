
using UnityEngine.Events;
using UnityEngine;


public class SecondOrderDynamics
{
    public Vector3 smoothedPosition; // Smoothed position and velocity
    public Vector3 smoothedVelocity;
    private Vector3 lead; // Followed position
    private float k1, k2, k3; // Dynamics values

    public SecondOrderDynamics(Vector3 start, float freq, float damp, float resp)
    {
        lead = start;
        smoothedPosition = start;
        smoothedVelocity = Vector3.zero;

        // Calculate coefficients
        k1 = resp / (Mathf.PI * freq);
        k2 = 1 / (4 * Mathf.PI * Mathf.PI * freq * freq);
        k3 = resp * damp / (2 * Mathf.PI * freq);
    }

    public void Increment(Vector3 velocity, float delta)
    {
        float k2_stab = Mathf.Max(k2, 1.1f * (delta * delta / 4 + delta * k1 / 2)); // stabalize
        smoothedPosition += delta * smoothedVelocity;
        lead += delta * velocity;
        smoothedVelocity += delta *
            (lead + k3 * velocity - smoothedPosition - k1 * smoothedVelocity)
            / k2_stab;
    }

    public void IncrementPosition(Vector3 position, float delta)
    {
        if (delta != 0)
        {
            Vector3 velocity = (position - lead) / delta;
            Increment(velocity, delta);
        }
    }
}
