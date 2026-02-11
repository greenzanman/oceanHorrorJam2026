using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody))]
public class StrokeCamDirection : MonoBehaviour
{
    [Header("Horizontal Burst")]
    public float horizontalStrokeForce = 5f;
    
    [Header("Vertical Curve Settings")]
    [Tooltip("The total duration of the vertical force application.")]
    public float verticalDuration = 0.5f;
    [Tooltip("How much strength the vertical force has.")]
    public float verticalForceMultiplier = 10f;
    [Tooltip("0 to 0.3s should be flat/low, then curve sharply upward.")]
    public AnimationCurve verticalForceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Cooldown & Buffer")]
    public float strokeCooldownSeconds = 1f;
    public float strokeInputBufferSeconds = 0.3f;

    private Rigidbody rb;
    private PlayerInput playerInput;
    private InputAction strokeAction;
    
    private bool isCooldown = false;
    private bool strokeInputBuffered = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        strokeAction = playerInput.actions["Stroke"];
    }

    /// <summary>
    /// Triggered by PlayerInput "Stroke" action (e.g., Spacebar)
    /// </summary>
    void OnStroke()
    {
        if (isCooldown)
        {
            BufferStrokeInput();
            return;
        }

        ExecuteStroke();
    }

    private void ExecuteStroke()
    {
        StopBufferStrokeInput();
        StartCoroutine(CooldownRoutine());

        // 1. Calculate Horizontal Direction (Flatten the camera forward)
        Vector3 camForward = playerInput.camera.transform.forward;
        Vector3 horizontalDir = new Vector3(camForward.x, 0, camForward.z).normalized;

        // 2. Apply Instant Horizontal Burst
        rb.AddForce(horizontalDir * horizontalStrokeForce, ForceMode.VelocityChange);

        // 3. Start Vertical Curved Force
        StartCoroutine(ApplyVerticalCurveRoutine());
    }

    private IEnumerator ApplyVerticalCurveRoutine()
    {
        float elapsed = 0f;

        while (elapsed < verticalDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / verticalDuration;

            // Evaluate the curve (Ease-in -> Sharp Rise)
            float curveValue = verticalForceCurve.Evaluate(normalizedTime);
            
            // Apply as Force (continuous) rather than Impulse
            rb.AddForce(Vector3.up * curveValue * verticalForceMultiplier, ForceMode.Force);

            yield return null;
        }
    }

    private IEnumerator CooldownRoutine()
    {
        isCooldown = true;
        yield return new WaitForSeconds(strokeCooldownSeconds);
        isCooldown = false;

        // If the player pressed the button during cooldown, fire it now
        if (strokeInputBuffered)
        {
            ExecuteStroke();
        }
    }

    #region Buffering Logic

    void BufferStrokeInput()
    {
        if (strokeInputBuffered) return; // Already buffered
        
        strokeInputBuffered = true;
        CancelInvoke(nameof(StopBufferStrokeInput));
        Invoke(nameof(StopBufferStrokeInput), strokeInputBufferSeconds);
    }

    void StopBufferStrokeInput()
    {
        strokeInputBuffered = false;
    }

    #endregion
}