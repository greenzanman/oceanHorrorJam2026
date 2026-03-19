using UnityEngine;

public class EnemyMarker : MonoBehaviour
{
    [Header("Fade Settings")]
    public float lifetime = 2.0f;
    [Tooltip("Curve determining alpha over time. X is time (0 to 1), Y is Alpha (0 to 1).")]
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private Material _material;
    private float _timer = 0f;
    private static readonly int AlphaProp = Shader.PropertyToID("_Alpha");

    void Start()
    {
        // Remember to create a new material instance so we don't fade all markers at once
        _material = GetComponent<Renderer>().material; 
    }

    void Update()
    {
        _timer += Time.deltaTime;
        float normalizedTime = Mathf.Clamp01(_timer / lifetime);
        
        // Evaluate the curve and apply it to the shader
        float currentAlpha = fadeCurve.Evaluate(normalizedTime);
        _material.SetFloat(AlphaProp, currentAlpha);

        if (_timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}