using UnityEngine;
using UnityEngine.Rendering.Universal; 

public class OrientationCue : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private GameObject cuePrefab; 
    
    [Header("Raycast Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float raycastStartHeight = 1f;
    [SerializeField] private float maxDistance = 50f;

    [Header("Dynamic Scaling & Fading")]
    [SerializeField] private float maxScale = 1.5f; 
    [SerializeField] private float minScale = 0.4f; 
    [SerializeField] private float maxHeight = 2.5f; // CHANGED: Now set to 2.5 units (a realistic jump height)
    [SerializeField] private float maxAlpha = 1.0f; 
    [SerializeField] private float minAlpha = 0.0f; 

    private GameObject cueInstance;
    private DecalProjector decalProjector;
    private Material decalMaterial; // Added to control our custom shader

    void Start()
    {
        if (cuePrefab != null)
        {
            cueInstance = Instantiate(cuePrefab);
            decalProjector = cueInstance.GetComponent<DecalProjector>();
            
            // Create a unique instance of the material so we can fade it safely
            if (decalProjector != null) {
                decalMaterial = new Material(decalProjector.material);
                decalProjector.material = decalMaterial;
            }

            cueInstance.SetActive(false);
        }
    }

    void Update()
    {
        if (cueInstance == null || decalProjector == null) return;

        Vector3 rayStart = transform.position + (Vector3.up * raycastStartHeight);
        RaycastHit hit;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, maxDistance, groundLayer))
        {
            cueInstance.SetActive(true);

            // 1. POSITION & ROTATION
            cueInstance.transform.position = hit.point + (Vector3.up * 0.5f);
            cueInstance.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            // 2. CALCULATE HEIGHT PERCENTAGE
            float actualHeight = hit.distance - raycastStartHeight; 
            float heightPercent = Mathf.Clamp01(actualHeight / maxHeight);

            // 3. APPLY DYNAMIC SCALE
            float currentSize = Mathf.Lerp(minScale, maxScale, heightPercent);
            decalProjector.size = new Vector3(currentSize, currentSize, 2f); 

            // 4. APPLY DYNAMIC TRANSPARENCY TO CUSTOM SHADER
            if (decalMaterial != null)
            {
                float currentAlpha = Mathf.Lerp(maxAlpha, minAlpha, heightPercent);
                decalMaterial.SetFloat("_FadeAmount", currentAlpha);
            }
        }
        else
        {
            cueInstance.SetActive(false);
        }
    }
}