using UnityEngine;
using UnityEngine.UI;

public class EnergyBarUI : MonoBehaviour
{
    [Header("Assign in Inspector")]
    [SerializeField] private Slider energySlider; 
    [SerializeField] private Image fillImage; // The "Fill" child of the slider
    [SerializeField] private CanvasGroup canvasGroup; // For fading in/out

    [Header("Colors")]
    [SerializeField] private Color safeColor = Color.white;
    [SerializeField] private Color dangerColorHigh = new Color(1f, 0.4f, 0.4f); // Light Red
    [SerializeField] private Color dangerColorLow = new Color(1f, 0f, 0f); // Bright Red

    void Update()
    {
        if (SonarManager.Instance == null) return;

        float current = SonarManager.Instance.currentEnergy;
        float max = SonarManager.Instance.maxEnergy;
        bool isSafe = SonarManager.Instance.inSafeZone;

        // 1. Update Slider Value
        energySlider.value = current / max;

        // 2. Handle Visibility (Only visible if NOT full or NOT safe)
        // Or simply: Appear when leaving safe zone.
        bool shouldShow = !isSafe || current < max; 
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, shouldShow ? 1f : 0f, Time.deltaTime * 5f);

        // 3. Handle Colors
        if (isSafe)
        {
            fillImage.color = safeColor;
        }
        else
        {
            // Lerp from Light Red to Bright Red based on energy level
            float t = current / max;
            fillImage.color = Color.Lerp(dangerColorLow, dangerColorHigh, t);
        }
    }
}