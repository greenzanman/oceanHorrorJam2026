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
    private float fadeMultiplier = 1f;
    private float age;
    private bool isInitialized = false;

    // get our owwn renderer for the color
    [SerializeField] private Renderer meshRenderer; 

    public void Initialize(float range, float scannerSpeed, float fadeMult = 1f)
    {
        this.maxRange = range;
        this.speed = scannerSpeed;
        this.fadeMultiplier = fadeMult;
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

        age += Time.deltaTime;

        // 1. Calculate Growth (Mathf.Min ensures it stops expanding when it hits maxRange!)
        CurrentRadius = Mathf.Min(age * speed, maxRange);

        // 2. Calculate Fade based on TIME, not radius
        float standardLifespan = maxRange / speed; 
        float totalLifespan = standardLifespan * fadeMultiplier; // Apply the linger!
        
        CurrentIntensity = 1.0f - (age / totalLifespan);

        // 3. Physical Scale
        transform.localScale = Vector3.one * CurrentRadius * 2; 

        // 4. Visual Mesh Fade
        if (meshRenderer != null)
        {
            Color c = meshRenderer.material.color;
            c.a = CurrentIntensity; 
            meshRenderer.material.color = c;
        }

        // 5. Kill ONLY when completely faded out
        if (CurrentIntensity <= 0)
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