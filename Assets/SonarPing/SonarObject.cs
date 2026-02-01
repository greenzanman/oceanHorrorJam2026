using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SonarObject : MonoBehaviour
{
    private Material opacityMaterial;
    private float opacityAmount;
    // Start is called before the first frame update
    void Start()
    {
        opacityMaterial = GetComponent<Renderer>().materials[0];
        opacityMaterial.SetFloat("_Visible", 0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (opacityAmount != 0)
        {
            opacityAmount -= Time.deltaTime;
            opacityAmount = math.max(opacityAmount, 0);
            opacityMaterial.SetFloat("_Visible", math.min(0.25f, opacityAmount));
        }
    }

    public void SetOpacity(float opacity)
    {
        opacityAmount = opacity;
    }
}
