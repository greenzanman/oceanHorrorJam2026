using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody))]
public class StrokeStaling : MonoBehaviour
{
    [Header("Horizontal Burst")]
    public float horizontalStrokeForce = 5f;

    [Header("Vertical Curve Settings")]
    public float verticalDuration = 0.5f;
    public float verticalForceMultiplier = 10f;
    public AnimationCurve verticalForceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Staling Settings")]
    [Tooltip("How much power is kept after one use. 0.7 = 30% reduction per use.")]
    public float staleFactor = 0.7f;
    [Tooltip("How fast the power returns to 1.0 per second.")]
    public float recoveryRate = 0.5f;
    [Tooltip("The minimum power floor so strokes never reach 0 force.")]
    public float minStaleThreshold = 0.2f;

    [Header("Energy Costs")]
    public float maxEnergyCost = 10f; // 10% of 100
    public float minEnergyCost = 2f;  // 2% of 100

    private Rigidbody rb;
    private PlayerInput playerInput;
    private float currentStaleMultiplier = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
    }

    void Update()
    {
        // Recover power over time
        if (currentStaleMultiplier < 1f)
        {
            currentStaleMultiplier = Mathf.MoveTowards(currentStaleMultiplier, 1f, recoveryRate * Time.deltaTime);
        }
    }

    void OnStroke() // Called by PlayerInput
    {
        ExecuteStroke();
    }

    private void ExecuteStroke()
    {
        // 1. Calculate "Freshness" (0.0 = Fully Stale, 1.0 = Fully Fresh)
        float t = Mathf.InverseLerp(minStaleThreshold, 1f, currentStaleMultiplier);

        // 2. CALCULATE COST (INVERTED)
        // If t = 1.0 (Fresh) -> Use minEnergyCost (Efficient)
        // If t = 0.0 (Stale) -> Use maxEnergyCost (Punishing)
        float energyCost = Mathf.Lerp(maxEnergyCost, minEnergyCost, t);

        // 3. Try to consume energy
        if (SonarManager.Instance != null && !SonarManager.Instance.TryConsumeEnergy(energyCost))
        {
            // Not enough energy to stroke
            return; 
        }

        // 4. Apply Physics (Force still gets weaker when stale, that remains the same)
        float modifiedForce = horizontalStrokeForce * currentStaleMultiplier;
        
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 horizontalDir = new Vector3(camForward.x, 0, camForward.z).normalized;
        
        rb.AddForce(horizontalDir * modifiedForce, ForceMode.VelocityChange);
        StartCoroutine(ApplyVerticalCurveRoutine(currentStaleMultiplier));

        // 5. Apply Staling
        currentStaleMultiplier *= staleFactor;
        if (currentStaleMultiplier < minStaleThreshold) currentStaleMultiplier = minStaleThreshold;
    }

    private IEnumerator ApplyVerticalCurveRoutine(float staleAtTimeOfStart)
    {
        float elapsed = 0f;

        while (elapsed < verticalDuration)
        {
            elapsed += Time.fixedDeltaTime; // Use fixedDeltaTime for physics consistency
            
            float normalizedTime = elapsed / verticalDuration;
            float curveValue = verticalForceCurve.Evaluate(normalizedTime);
            float verticalForce = curveValue * verticalForceMultiplier * staleAtTimeOfStart;
            
            rb.AddForce(Vector3.up * verticalForce, ForceMode.Force);

            yield return new WaitForFixedUpdate(); // <--- WAIT FOR PHYSICS STEP
        }
    }
}