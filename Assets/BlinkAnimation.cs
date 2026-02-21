using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BlinkAnimation : MonoBehaviour
{
    [Header("Pulse Settings")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float minBrightness = 0.6f;
    [SerializeField] private float maxBrightness = 1f;

    [Header("Target Components (Optional)")]
    [SerializeField] private Image targetImage;
    [SerializeField] private TextMeshProUGUI targetText;

    private Color imageBaseColor;
    private Color textBaseColor;

    private void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        if (targetText == null)
            targetText = GetComponentInChildren<TextMeshProUGUI>();

        if (targetImage != null)
            imageBaseColor = targetImage.color;

        if (targetText != null)
            textBaseColor = targetText.color;
    }

    void Update()
    {
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
        float brightness = Mathf.Lerp(minBrightness, maxBrightness, t);

        if (targetImage != null)
            targetImage.color = imageBaseColor * brightness;

        if (targetText != null)
            targetText.color = textBaseColor * brightness;
    }
}
