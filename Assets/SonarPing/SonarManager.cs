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
    public float passiveDrainRate = 2f; // How fast it drops outside
    public float lowEnergyRegenRate = 10f; // How fast it regens up to 33%
    public float safeZoneRechargeRate = 50f; // How fast it refills inside elevator
    
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
                currentEnergy = Mathf.Min(currentEnergy, maxEnergy);
            }
        }
        else
        {
            // OUTSIDE SAFE ZONE
            if (currentEnergy > oneThird)
            {
                // Decay down to 1/3
                currentEnergy -= passiveDrainRate * Time.deltaTime;
            }
            else
            {
                // Regenerate up to 1/3
                currentEnergy += lowEnergyRegenRate * Time.deltaTime;
                currentEnergy = Mathf.Min(currentEnergy, oneThird);
            }
        }
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