using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using FMODUnity;
using FMOD.Studio;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody))]
public class StrokeStaling : MonoBehaviour
{
    [Header("FMOD Audio")]
    public EventReference strokeFmodEvent;

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

    private bool _isStroking = false;
    public bool IsStroking => _isStroking;
    public bool IsFullyUnstaled => currentStaleMultiplier >= 0.99f;

    // tell ppl exactly when stroke happens
    public event System.Action OnStrokeEvent;

    // Returns a number 1.0 to 0.0 (fully fresh, to fully stale)
    public float GetStalenessNormalized()
    {
        return Mathf.InverseLerp(minStaleThreshold, 1f, currentStaleMultiplier);
    }

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

    // get the predicted cost of next stroke
    public float GetNextStrokeCost()
    {
        float t = Mathf.InverseLerp(minStaleThreshold, 1f, currentStaleMultiplier);
        return Mathf.Lerp(maxEnergyCost, minEnergyCost, t);
    }

    private void ExecuteStroke()
    {
        // tell sonar manager to evaluate if we should ping on this stroke (50% or 100% energy)
        if (SonarManager.Instance != null)
        {
            SonarManager.Instance.EvaluateStrokeSonar();
        }

        // 1 & 2. Get the calculated cost for the next stroke
        float energyCost = GetNextStrokeCost();

        // 3. Try to consume energy
        if (SonarManager.Instance != null && !SonarManager.Instance.TryConsumeEnergy(energyCost))
        {
            // Not enough energy to stroke
            return; 
        }

        _isStroking = true;

        // tell listeners we stroked
        OnStrokeEvent?.Invoke();

        // AUDIO TRIGGER
        if (!strokeFmodEvent.IsNull)
        {
            // Create an instance, set the Staleness parameter, play, and release memory
            EventInstance strokeInst = RuntimeManager.CreateInstance(strokeFmodEvent);

            // attach to player
            RuntimeManager.AttachInstanceToGameObject(strokeInst, gameObject, rb);
            
            strokeInst.setParameterByName("Staleness", currentStaleMultiplier);
            strokeInst.start();
            strokeInst.release(); 
        }


        // 4. Apply Physics (Force still gets weaker when stale)
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
        
        _isStroking = false;
    }
}