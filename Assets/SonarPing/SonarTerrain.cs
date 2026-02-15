using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SonarTerrain : MonoBehaviour {
    
    // Arrays matching shader size
    private float[] radiiArray = new float[16];
    private Vector4[] positionsArray = new Vector4[16];

    // Tracking active pings
    // Key: Ping ID, Value: Index in the array (0-15)
    private Dictionary<int, int> activePings = new Dictionary<int, int>();

    private const int MAX_PINGS = 16;
    private const float FADE_SPEED = 0.5f; 
    
    private Material terrainMat;
    private Terrain terrain;
    private bool isDirty = false;

    void Start () {
        terrain = GetComponent<Terrain>();
        terrainMat = terrain.materialTemplate; // Access the Custom Material
        
        // Clear arrays
        for(int i=0; i<MAX_PINGS; i++) {
            positionsArray[i] = Vector4.zero;
            radiiArray[i] = 0f;
        }
        
        terrainMat.SetInteger("_PointCount", MAX_PINGS);
        UpdateShader();
    }

    void Update() {
        if (activePings.Count == 0) return;

        List<int> idsToRemove = new List<int>();
        isDirty = false;

        foreach (var kvp in activePings) {
            int id = kvp.Key;
            int index = kvp.Value;

            // Decay Intensity (stored in W)
            float currentIntensity = positionsArray[index].w;
            currentIntensity -= Time.deltaTime * FADE_SPEED;

            if (currentIntensity <= 0) {
                currentIntensity = 0;
                idsToRemove.Add(id);
            }

            positionsArray[index].w = currentIntensity;
            isDirty = true;
        }

        // Cleanup finished pings
        foreach (int id in idsToRemove) {
            int index = activePings[id];
            activePings.Remove(id);
            radiiArray[index] = 0; // Clear radius effectively disables it in shader
        }

        if (isDirty) {
            UpdateShader();
        }
    }

    public void HandlePing(Vector3 worldPos, float radius, int pingId) {
        
        int index = -1;

        // 1. Is this ping already active? Update it.
        if (activePings.ContainsKey(pingId)) {
            index = activePings[pingId];
        } 
        // 2. Otherwise find a free slot
        else if (activePings.Count < MAX_PINGS) {
            // Build a set of occupied indices for O(1) lookup
            var occupied = new HashSet<int>(activePings.Values);
            for (int i = 0; i < MAX_PINGS; i++) {
                if (!occupied.Contains(i) && positionsArray[i].w <= 0) {
                    index = i;
                    activePings.Add(pingId, index);
                    break;
                }
            }
        }

        if (index != -1) {
            // Send World Position directly
            positionsArray[index] = new Vector4(worldPos.x, worldPos.y, worldPos.z, 1.0f); // 1.0 = Start Intensity
            radiiArray[index] = radius;
            isDirty = true;
            UpdateShader();
        }
    }

    private void UpdateShader() {
        // Use SetGlobal instead of material.Set
        // This ensures ALL shaders (including the terrain) get the data
        Shader.SetGlobalFloatArray("_Radii", radiiArray);
        Shader.SetGlobalVectorArray("_PointIntensities", positionsArray);
        Shader.SetGlobalInteger("_PointCount", MAX_PINGS);
    }
}