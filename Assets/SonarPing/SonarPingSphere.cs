using UnityEngine;

// sonar ping sphere used to track individual expanding rings
// - their radius and intensity are read by the SonarManager and passed to the shader
// - the sphere DOES NOT RENDER right now, but helpful for debugging and future visual effects (like a mesh ring or particle burst at the ping origin) 
public class SonarPingSphere : MonoBehaviour
{
    // Public Properties that the SonarManager reads
    public float CurrentRadius { get; private set; }
    public float CurrentIntensity { get; private set; }

    private float maxRange;
    private float speed;
    private float age;
    private bool isInitialized = false;

    // get our owwn renderer for the color
    [SerializeField] private Renderer meshRenderer; 

    public void Initialize(float range, float scannerSpeed)
    {
        this.maxRange = range;
        this.speed = scannerSpeed;
        this.age = 0;
        this.CurrentRadius = 0;
        this.CurrentIntensity = 1;
        
        isInitialized = true;

        // Register with Manager so the Terrain knows about us
        if (SonarManager.Instance != null)
            SonarManager.Instance.RegisterPing(this);
    }

    void Update()
    {
        if (!isInitialized) return;

        // 1. Calculate Growth
        age += Time.deltaTime;
        CurrentRadius = age * speed;

        // 2. Calculate Fade (Intensity goes from 1 to 0)
        // We fade out as we approach max range
        float percentComplete = CurrentRadius / maxRange;
        CurrentIntensity = 1.0f - percentComplete;

        // 3. Physical Scale (Visual Sphere size)
        transform.localScale = Vector3.one * CurrentRadius * 2; // *2 because radius vs diameter

        // 4. Update own material color (Optional, for the sphere mesh itself)
        if (meshRenderer != null)
        {
            Color c = meshRenderer.material.color;
            c.a = CurrentIntensity; // Fade out alpha
            meshRenderer.material.color = c;
        }

        // 5. Kill when done
        if (CurrentRadius > maxRange || CurrentIntensity <= 0)
        {
            DestroyPing();
        }
    }

    void OnDestroy()
    {
        DestroyPing();
    }

    private bool isDestroyed = false;

    private void DestroyPing()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        // Unregister before dying so the shader stops drawing the ring
        if (SonarManager.Instance != null)
            SonarManager.Instance.UnregisterPing(this);
        Destroy(gameObject);
    }
}