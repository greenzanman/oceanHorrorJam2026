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

    [Header("3. Visuals (Colors & Grid)")]
    public Color scannerColor = Color.red;
    [Range(0.1f, 5f)] public float fadeStrength = 1.0f;
    [Tooltip("Higher = Smaller Dots")] public float gridScale = 50.0f; 
    [Range(0.01f, 0.99f)] public float dotSize = 0.5f;

    [Header("4. Energy System")]
    public float maxEnergy = 100f;
    public float safeZoneRechargeRate = 50f; // Rapid fill in elevator
    
    [Header("Outside Regeneration Curve")]
    [Tooltip("Maximum regen speed when energy is 0.")]
    public float maxRegenRate = 20f; 
    [Tooltip("Minimum regen speed when energy is 100 (Full).")]
    public float minRegenRate = 2f;

    [Tooltip("X axis = Current Energy %. Y axis = 1 is max energy and 0 is min energy.")]
    public AnimationCurve energyRegenCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));
    
    [Header("Debug View")]
    public float currentEnergy;
    public bool inSafeZone = true; // Default to true (start in elevator)

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
        // Debug Key
        if (Input.GetKeyDown(KeyCode.Semicolon)) OnFire();

        // 1. Update Wave Data (Positions/Radii)
        UpdateWaveData();

        // 2. Update Visual Data (Colors/Grid)
        UpdateVisualData();

        // 3. Update Energy System
        HandleEnergy();
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
        }

        // Clear empty slots
        for (int i = count; i < 16; i++) {
            _radii[i] = 0;
            _intensities[i] = Vector4.zero;
        }

        // Send to GPU
        Shader.SetGlobalInteger("_PointCount", count);
        Shader.SetGlobalFloatArray("_Radii", _radii);
        Shader.SetGlobalVectorArray("_PointIntensities", _intensities);
    }

    void UpdateVisualData()
    {
        // This makes sure the shader is never Black/Invisible
        Shader.SetGlobalColor("_SonarBaseColor", scannerColor);
        Shader.SetGlobalFloat("_SonarFadeStrength", fadeStrength);
        Shader.SetGlobalFloat("_SonarGridScale", gridScale);
        Shader.SetGlobalFloat("_SonarDotSize", dotSize);
    }

    void HandleEnergy()
    {
        float oneThird = maxEnergy / 3f;

        if (inSafeZone)
        {
            // RAPID RECHARGE IN SAFE ZONE
            if (currentEnergy < maxEnergy)
            {
                currentEnergy += safeZoneRechargeRate * Time.deltaTime;
            }
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
    }

    // Tries to consume energy. Returns true if successful, false if not enough.
    public bool TryConsumeEnergy(float amount)
    {
        // If inside safe zone, actions are free!
        if (inSafeZone) return true;

        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;
            return true;
        }
        
        return false;
    }

    // --- REGISTRATION ---
    public void RegisterPing(SonarPingSphere ping)
    {
        if (!activeSpheres.Contains(ping)) activeSpheres.Add(ping);
    }

    // called by the ping itself when it dies, so we stop sending data to the shader for it
    public void UnregisterPing(SonarPingSphere ping)
    {
        if (activeSpheres.Contains(ping)) activeSpheres.Remove(ping);
    }

    // --- SPAWNING ---
    public void OnFire()
    {
        float pingCost = maxEnergy / 3f;

        // Can fire if in Safe Zone OR if we have enough energy
        if (inSafeZone || currentEnergy >= pingCost)
        {
            // Only deduct energy if OUTSIDE safe zone
            if (!inSafeZone)
            {
                currentEnergy -= pingCost;
            }

            StartCoroutine(PingBurstRoutine());
        }
        else
        {
            Debug.Log("Not enough energy to ping!");
            // TODO: sfx add errory sound
        }
    }

    public void SetSafeZone(bool isSafe)
    {
        inSafeZone = isSafe;
    }

    private IEnumerator PingBurstRoutine()
    {
        for (int i = 0; i < pingsPerFire; i++)
        {
            SpawnPing();
            yield return new WaitForSeconds(burstInterval);
        }
    }

    private void SpawnPing()
    {
        if (pingPrefab != null)
        {
            GameObject go = Instantiate(pingPrefab, transform.position, Quaternion.identity);
            SonarPingSphere sphereScript = go.GetComponent<SonarPingSphere>();
            if (sphereScript != null)
            {
                sphereScript.Initialize(maxRange, scannerSpeed);
            }
        }
    }
}