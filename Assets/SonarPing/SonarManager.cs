using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

// this manager should be on the player
// - tracks all active ping spheres and sends their data to the shader each frame
[RequireComponent(typeof(PlayerInput))]
public class SonarManager : MonoBehaviour
{
    public static SonarManager Instance;

    [Header("1. Object Links")]
    [SerializeField] private GameObject pingPrefab; 

    [Header("2. Scanner Gameplay")]
    public float scannerSpeed = 15f;
    public float maxRange = 30f;
    public int pingsPerFire = 1;
    public float burstInterval = 0.2f;

    [Header("Cone Settings")]
    [Range(10f, 360f)] public float coneAngle = 45f;
    [Range(0.01f, 0.5f)] public float wedgeFeather = 0.05f;
    public float minOmniRadius = 5.0f;

    private Vector4[] _pingDirections = new Vector4[16];

    [Header("3. Visuals (Colors & Grid)")]
    public Color scannerColor = Color.red;
    [Range(0.1f, 5f)] public float fadeStrength = 1.0f;
    [SerializeField] public AnimationCurve fadeCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));
    [Tooltip("Higher = Smaller Dots")] public float gridScale = 50.0f; 
    [Range(0.01f, 0.99f)] public float dotSize = 0.5f;


    [Header("Altitude Gradient")]
    public Color colorLow = Color.blue;
    public Color colorHigh = Color.red;
    public Transform lowestPoint;
    public Transform highestPoint;

    [Header("Audio")]
    private bool _was50Ready = false;
    private bool _was100Ready = false;

    [Header("4. Energy System")]
    public float maxEnergy = 100f;
    public float safeZoneRechargeRate = 50f; // Rapid refill in safe zone (elevator) 
    public float refillDelay = 0.2f; // Delay before regeneration starts
    public float emptyRefillDelay = 2.0f;
    private float _currentRefillTimer = 0f;
    public bool IsEmptyPenaltyActive { get; private set; }

    [Header("Sonar Auto-Ping Settings")]
    public float sonarCooldown = 0f; // How often the 50% ping can happen
    private float _sonarCooldownTimer = 0f;
    public bool IsSonarReady() { return _sonarCooldownTimer <= 0f; }

    [Header("Super Sonar (100% Energy)")]
    [Tooltip("Enable or disable the super sonar (100% energy) feature.")]
    public bool enableSuperSonar = false;
    public float superRangeMultiplier = 2.0f;
    public float superSpeedMultiplier = 1.5f;
    public float superFadeMultiplier = 3.0f;

    
    [Header("Outside Regeneration Curve")]
    [Tooltip("Maximum regen speed when energy is 0.")]
    public float maxRegenRate = 20f; 
    [Tooltip("Minimum regen speed when energy is 100 (Full).")]
    public float minRegenRate = 2f;

    [Tooltip("X axis = Current Energy %. Y axis = 1 is max energy and 0 is min energy.")]
    public AnimationCurve energyRegenCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));
    
    [Header("Debug View")]
    public float currentEnergy;
    public bool inSafeZone = true; // Default to true (since start in elevator)

    [Header("Audio")]
    [SerializeField] private AudioManager audioManager;

    // --- INTERNAL DATA ---
    private List<SonarPingSphere> activeSpheres = new List<SonarPingSphere>();
    private float[] _radii = new float[16];
    private Vector4[] _intensities = new Vector4[16];

    void Awake()
    {
        Instance = this;
        currentEnergy = maxEnergy;
    }

    void Update()
    {
        // Debug Key (only in development)
        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Semicolon)) EvaluateStrokeSonar();
        #endif

        // 1. Update Wave Data (Positions/Radii)
        UpdateWaveData();

        // 2. Update Visual Data (Colors/Grid)
        UpdateVisualData();

        // 3. Update Energy System
        HandleEnergy();

        // tick cooldown timer down
        if (_sonarCooldownTimer > 0f) _sonarCooldownTimer -= Time.deltaTime;
    }

    void UpdateWaveData()
    {
        int count = Mathf.Min(activeSpheres.Count, 16);

        for (int i = 0; i < count; i++)
        {
            SonarPingSphere sphere = activeSpheres[i];
            
            // Safety check: if a sphere was destroyed but is still in list
            if (sphere == null) continue; 

            _radii[i] = sphere.CurrentRadius;
            _intensities[i] = new Vector4(
                sphere.transform.position.x, 
                sphere.transform.position.y, 
                sphere.transform.position.z, 
                sphere.CurrentIntensity 
            );

            float cosAngle = Mathf.Cos((coneAngle * 0.5f) * Mathf.Deg2Rad);
            _pingDirections[i] = new Vector4(sphere.ForwardDir.x, sphere.ForwardDir.y, sphere.ForwardDir.z, cosAngle);
        }

        // Clear empty slots
        for (int i = count; i < 16; i++) {
            _radii[i] = 0;
            _intensities[i] = Vector4.zero;
            _pingDirections[i] = Vector4.zero;
        }

        // Send to GPU
        Shader.SetGlobalInteger("_PointCount", count);
        Shader.SetGlobalFloatArray("_Radii", _radii);
        Shader.SetGlobalVectorArray("_PointIntensities", _intensities);
        // - cone params
        Shader.SetGlobalVectorArray("_PingDirections", _pingDirections);
        Shader.SetGlobalFloat("_WedgeFeather", wedgeFeather);
        Shader.SetGlobalFloat("_MinOmniRadius", minOmniRadius);
    }

    void UpdateVisualData()
    {
        // Existing parameters
        Shader.SetGlobalColor("_SonarBaseColor", scannerColor);
        Shader.SetGlobalFloat("_SonarFadeStrength", fadeStrength);
        Shader.SetGlobalFloat("_SonarGridScale", gridScale);
        Shader.SetGlobalFloat("_SonarDotSize", dotSize);

        // New gradient parameters
        Shader.SetGlobalColor("_ColorLow", colorLow);
        Shader.SetGlobalColor("_ColorHigh", colorHigh);
        
        if (lowestPoint != null && highestPoint != null)
        {
            Shader.SetGlobalFloat("_MinY", lowestPoint.position.y);
            Shader.SetGlobalFloat("_MaxY", highestPoint.position.y);
        }
    }

    void HandleEnergy()
    {
        // 1. Process Refill Delay (Halt regeneration while timer is active)
        if (_currentRefillTimer > 0f)
        {
            _currentRefillTimer -= Time.deltaTime;
            
            // If the timer just finished, turn off the penalty state
            if (_currentRefillTimer <= 0f) 
            {
                IsEmptyPenaltyActive = false; 
            }
            return; 
        }

        float oneThird = maxEnergy / 3f;

        if (inSafeZone)
        {
            // RAPID RECHARGE IN SAFE ZONE
            if (currentEnergy < maxEnergy)
            {
                currentEnergy += safeZoneRechargeRate * Time.deltaTime;
            }

            currentEnergy = Mathf.Min(currentEnergy, maxEnergy * 0.65f);
        }
        else
        {
            // 1. Calculate how full we are (0.0 to 1.0)
            float energyPercent = currentEnergy / maxEnergy;

            // 2. Evaluate Curve
            float curveValue = energyRegenCurve.Evaluate(energyPercent);

            // 3. Apply Rate
            float currentRate = Mathf.Lerp(minRegenRate, maxRegenRate, curveValue);
            
            currentEnergy += currentRate * Time.deltaTime;
        }

        // Clamp energy between 0 and Max
        currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);



        // Check current percentages
        bool is50Ready = currentEnergy >= (maxEnergy * 0.5f);
        bool is100Ready = currentEnergy >= (maxEnergy * 0.99f);

        // If it just became ready this frame, play the tick!
        // - except at play start
        if (Time.timeSinceLevelLoad > 0.5f)
        {
            if (is50Ready && !_was50Ready) 
            {
                if (audioManager != null) audioManager.PlaySonarReady50();
            }
            
            if (is100Ready && !_was100Ready) 
            {
                if (audioManager != null) audioManager.PlaySonarReady100();
            }
        }
        

        // Update the history for the next frame
        _was50Ready = is50Ready;
        _was100Ready = is100Ready;
    }

    // Tries to consume energy. Returns true if successful, false if not enough.
    public bool TryConsumeEnergy(float amount)
    {
        // GUARD CHECKS:
        // In safe zone: do the action for 0 energy cost
        if (inSafeZone) return true;

        // empty bar penalty: no action
        if (IsEmptyPenaltyActive) return false;


        // take da energy
        currentEnergy -= amount;

        // Check if we depleted energy past 0, apply penalty if so
        if (currentEnergy <= 0.01f) 
        {
            // empty bar penalty
            currentEnergy = 0f;
            _currentRefillTimer = emptyRefillDelay;  // longer refill delay for empty bar
            IsEmptyPenaltyActive = true;            // will trigger ui glow
            if (audioManager != null) audioManager.PlayDepletedEnergy();
        }
        else
        {
            _currentRefillTimer = refillDelay;      // normal refill delay
        }

        return true;
    }

    // OLD: manual ping trigger
    // public void OnFire()
    // {
    //     float pingCost = maxEnergy / 3f;

    //     if (TryConsumeEnergy(pingCost))
    //     {
    //         StartCoroutine(PingBurstRoutine());
    //     }
    //     else
    //     {
    //         Debug.Log("Not enough energy to ping!");
    //     }
    // }

    public void RegisterPing(SonarPingSphere ping)
    {
        if (!activeSpheres.Contains(ping)) activeSpheres.Add(ping);
    }

    public void UnregisterPing(SonarPingSphere ping)
    {
        if (activeSpheres.Contains(ping)) activeSpheres.Remove(ping);
    }

    public void SetSafeZone(bool isSafe)
    {
        inSafeZone = isSafe;

        if (inSafeZone && currentEnergy > maxEnergy * 0.65f)
        {
            currentEnergy = maxEnergy * 0.65f;
        }
    }

    private IEnumerator PingBurstRoutine(float rangeMult, float speedMult, float fadeMult)
    {
        audioManager.PlaySonarPing();
        
        for (int i = 0; i < pingsPerFire; i++)
        {
            SpawnPing(rangeMult, speedMult, fadeMult);
            yield return new WaitForSeconds(burstInterval);
        }
    }

    // Called by StrokeStaling right BEFORE it consumes the normal stroke energy
    public void EvaluateStrokeSonar()
    {
        // 1. Check for Super Sonar (100% Energy)
        if (enableSuperSonar && currentEnergy >= maxEnergy * 0.99f)
        {
            // play super sonar sound
            if (audioManager != null) audioManager.PlaySuperSonar();

            // Start the burst routine WITH multipliers
            StartCoroutine(PingBurstRoutine(superRangeMultiplier, superSpeedMultiplier, superFadeMultiplier));
            _sonarCooldownTimer = sonarCooldown;
            TryConsumeEnergy(maxEnergy + 1f); // 101% drain
            return;
        }

        // 2. Check for Regular Auto-Sonar (>= 50% Energy)
        if (currentEnergy >= maxEnergy * 0.5f && IsSonarReady() && !IsEmptyPenaltyActive)
        {
            // Start the burst routine WITHOUT multipliers (1.0 default)
            StartCoroutine(PingBurstRoutine(1.0f, 1.0f, 1.0f));
            _sonarCooldownTimer = sonarCooldown;
        }
    }

    private void SpawnPing(float rangeMult, float speedMult, float fadeMult)
    {
        if (pingPrefab != null)
        {
            GameObject go = Instantiate(pingPrefab, transform.position, Quaternion.identity);
            SonarPingSphere sphereScript = go.GetComponent<SonarPingSphere>();
            if (sphereScript != null)
            {
                sphereScript.Initialize(maxRange * rangeMult, scannerSpeed * speedMult, transform.forward, fadeMult);
            }
        }
    }

    // so enemies can read the active rings
    public List<SonarPingSphere> GetActivePings()
    {
        return activeSpheres;
    }
}