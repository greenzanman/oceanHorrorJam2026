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

    public IEnumerator FadeToOpacity(float targetOpacity, float duration)
    {
        float startOpacity = opacityAmount;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Cubic ease-in: t^3
            float easedT = t * t * t;
            opacityAmount = Mathf.Lerp(startOpacity, targetOpacity, easedT);
            yield return null;
        }
        opacityAmount = targetOpacity;
    }
}
