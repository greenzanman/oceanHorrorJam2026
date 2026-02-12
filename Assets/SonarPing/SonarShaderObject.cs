using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
public class SonarShaderObject : MonoBehaviour {
    private List<Vector4> positionIntensities = new List<Vector4>(); // (x, y, z, intensity)
    private const int MAX_SIMULTANEOUS_PINGS = 8;
    private const float START_INTENSITY = 0.7f;
    private const float FADEOUT_TIME = 12;
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
        }
        shaderMaterial.SetInteger("_PointCount", MAX_SIMULTANEOUS_PINGS);
    }

    void Update()
    {
        if (idToPosition.Count > 0)
        {
            Vector4 positionIntensity;
            foreach (int id in idToPosition.Keys.ToList())
            {
                positionIntensity = positionIntensities[idToPosition[id]];
                if (positionIntensity.w > 0)
                    requiresUpdate = true;

                positionIntensity.w -= Time.deltaTime * START_INTENSITY / FADEOUT_TIME;
                positionIntensities[idToPosition[id]] = positionIntensity;

                if (positionIntensity.w <= 0)
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

    public void HandlePing(Vector3 pingSource, float pingRadius, float maxRadius, int pingId)
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

        positionIntensities[idToPosition[pingId]] = new Vector4(localSource.x, localSource.y, localSource.z, START_INTENSITY);
        radii[idToPosition[pingId]] = pingRadius;

        if (this.maxRadius == -1)
        {
            this.maxRadius = maxRadius;
            // This only needs to be set once
            shaderMaterial.SetFloat("_MaxRadius", maxRadius);
        }

        requiresUpdate = true;
    }
}