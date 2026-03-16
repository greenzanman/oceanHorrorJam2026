using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
public class SonarShaderObject : MonoBehaviour {
    private List<Vector4> positionIntensities = new List<Vector4>(); // (x, y, z, intensity)
    private const int MAX_SIMULTANEOUS_PINGS = 8;
    private const float START_INTENSITY = 0.7f;
    private const float FADEOUT_TIME = 1.5f;
    private List<float> ages = new List<float>();
    private List<float> lifespans = new List<float>();

    // Map from ids to position intensities
    private Dictionary<int, int> idToPosition = new Dictionary<int, int>();
    private List<float> radii = new List<float>();
    private float maxRadius = -1;
    private Material shaderMaterial;
    private bool requiresUpdate = false; // To avoid repeatedly updating with the same values
    void Start ()
    {
        shaderMaterial = GetComponent<Renderer>().materials[0];

        // Initialize intensities
        for (int i = 0; i < MAX_SIMULTANEOUS_PINGS; i++)
        {
            positionIntensities.Add(new Vector4(0, 0, 0, 0));
            radii.Add(0);
            ages.Add(0);
            lifespans.Add(1f);
        }
        shaderMaterial.SetInteger("_PointCount", MAX_SIMULTANEOUS_PINGS);
    }

    void Update()
    {
        if (idToPosition.Count > 0)
        {
            foreach (int id in idToPosition.Keys.ToList())
            {
                int index = idToPosition[id];
                Vector4 positionIntensity = positionIntensities[index];
                
                if (positionIntensity.w > 0)
                    requiresUpdate = true;

                // Increment age
                ages[index] += Time.deltaTime;

                // Normalize age and evaluate curve
                float normalizedAge = ages[index] / lifespans[index];
                positionIntensity.w = SonarManager.Instance.fadeCurve.Evaluate(normalizedAge);

                positionIntensities[index] = positionIntensity;

                if (positionIntensity.w <= 0 || normalizedAge >= 1f)
                {
                    idToPosition.Remove(id);
                }
            }

            if (requiresUpdate)
            {
                shaderMaterial.SetFloatArray("_Radii", radii);
                shaderMaterial.SetVectorArray("_PointIntensities", positionIntensities);
                requiresUpdate = false;
            } 
        }
    }

    public void HandlePing(Vector3 pingSource, float pingRadius, float maxRadius, int pingId, float lifespan = -1f)
    {
        Vector3 localSource = pingSource - transform.position;

        // Try to assign a new vaue
        if (!idToPosition.ContainsKey(pingId))
        {
            bool assigned = false;
            for (int i = 0; i <positionIntensities.Count; i++)
            {
                // If this position intensity is no longer assigned
                if (positionIntensities[i].w <= 0)
                {
                    idToPosition[pingId] = i;
                    assigned = true;
                    break;
                }
            }
            if (!assigned) // This should never happen
            {
                print("Grevious error, skipping this ping");
            }
        }

        positionIntensities[idToPosition[pingId]] = new Vector4(localSource.x, localSource.y, localSource.z, SonarManager.Instance.fadeCurve.Evaluate(0));
        radii[idToPosition[pingId]] = pingRadius;
        ages[idToPosition[pingId]] = 0f; // Reset age for new ping
        lifespans[idToPosition[pingId]] = lifespan;

        if (this.maxRadius == -1)
        {
            this.maxRadius = maxRadius;
            // This only needs to be set once
            shaderMaterial.SetFloat("_MaxRadius", maxRadius);
        }

        requiresUpdate = true;
    }
}