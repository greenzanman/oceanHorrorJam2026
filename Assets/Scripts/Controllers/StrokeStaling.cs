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

    private Rigidbody rb;
    private PlayerInput playerInput;
    
    // Tracks current power (1.0 = Full Power)
    private float currentStaleMultiplier = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
    }

    void Update()
    {
        // Recover the multiplier over time back to 1.0
        if (currentStaleMultiplier < 1f)
        {
            currentStaleMultiplier = Mathf.MoveTowards(currentStaleMultiplier, 1f, recoveryRate * Time.deltaTime);
        }
    }

    void OnStroke()
    {
        ExecuteStroke();
    }

    private void ExecuteStroke()
    {
        // 1. Calculate and apply force using CURRENT power before we stale it
        float modifiedForce = horizontalStrokeForce * currentStaleMultiplier;
        
        Vector3 camForward = playerInput.camera.transform.forward;
        Vector3 horizontalDir = new Vector3(camForward.x, 0, camForward.z).normalized;
        rb.AddForce(horizontalDir * modifiedForce, ForceMode.VelocityChange);

        StartCoroutine(ApplyVerticalCurveRoutine(currentStaleMultiplier));

        // 2. IMMEDIATELY drop power to 0 so the 2-second timer starts
        currentStaleMultiplier = 0f; 
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