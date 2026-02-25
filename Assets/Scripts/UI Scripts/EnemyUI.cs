using UnityEngine;
using UnityEngine.UI;

public class EnemyUI : MonoBehaviour
{
    [Header("References")]
    public StalkerEnemy stalker; // Drag your enemy here, or find it dynamically
    public Image centerDot;
    public Image yellowBorder;

    [Header("Visual Settings")]
    public Color lowPanicColor = new Color(1f, 0.5f, 0f); // Orange
    public Color highPanicColor = new Color(0.6f, 0f, 0f); // Deep Red
    public float minDotScale = 0.5f;
    public float maxDotScale = 2.0f;
    public float maxBlinkSpeed = 20f; // Speed of the sine wave at max panic

    [Header("Success Ring Settings")]
    public Color successRingColor = Color.yellow;
    public float maxRingThickness = 1.5f;

    void Update()
    {
        if (stalker == null || !stalker.isStalking)
        {
            centerDot.enabled = false;
            yellowBorder.enabled = false;
            return;
        }

        centerDot.enabled = true;
        
        // --- CENTER DOT LOGIC ---
        float panicNorm = stalker.panicIntensity / 100f; 
        centerDot.color = Color.Lerp(lowPanicColor, highPanicColor, panicNorm);
        centerDot.rectTransform.localScale = Vector3.one * Mathf.Lerp(minDotScale, maxDotScale, panicNorm);

        float currentBlinkSpeed = Mathf.Lerp(2f, maxBlinkSpeed, panicNorm);
        float alpha = (Mathf.Sin(Time.time * currentBlinkSpeed) + 1f) / 2f; 
        
        Color dotColor = centerDot.color;
        dotColor.a = alpha;
        centerDot.color = dotColor;

        // --- SUCCESS RING LOGIC (ACCURACY BASED) ---
        // Grab the smoothed accuracy. 
        // 0.0 means edge of the cone, 1.0 means dead center.
        float accuracyNorm = stalker.CurrentAccuracy;

        // Only show the ring if the enemy is inside the view cone
        if (accuracyNorm > 0.01f)
        {
            yellowBorder.enabled = true;
            
            // Set the custom color, but use ACCURACY to drive the alpha (opacity)
            Color ringColor = successRingColor;
            ringColor.a = accuracyNorm; // Gets fully solid when perfectly centered
            yellowBorder.color = ringColor;
            
            // The ring gets thicker the closer the enemy is to the center of the screen
            yellowBorder.rectTransform.localScale = Vector3.one * Mathf.Lerp(1.0f, maxRingThickness, accuracyNorm);
        }
        else
        {
            yellowBorder.enabled = false;
        }
    }
}