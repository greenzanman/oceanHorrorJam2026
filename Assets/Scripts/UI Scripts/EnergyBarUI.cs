using UnityEngine;
using UnityEngine.UI;

public class EnergyBarUI : MonoBehaviour
{
    [Header("Assign in Inspector")]
    [SerializeField] private Image backgroundFillImage; 
    [SerializeField] private Image afterimageFillImage; 
    [SerializeField] private Image mainFillImage;       
    [SerializeField] private RectTransform tickMarkPivot; 
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private CanvasGroup glowCanvasGroup; 
    
    [Header("Scripts")]
    [SerializeField] private StrokeStaling strokeStaling;

    [Header("Afterimage Settings")]
    [SerializeField] private float afterimageDelay = 0.1f;
    [SerializeField] private float afterimageFadeDuration = 0.4f;

    [Header("Fill Mapping Constraints")]
    [SerializeField] private float emptyFill = 0.15f; 
    [Tooltip("Tweak this slightly higher (e.g., 0.36) to fix the gap at the top of the bar!")]
    [SerializeField] private float fullFill = 0.35f;

    [Header("Colors & Curves")]
    [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.5f); 
    [SerializeField] private Color safeColor = Color.white;
    [SerializeField] private Color dangerColorHigh = new Color(1f, 0.4f, 0.4f);
    [SerializeField] private Color dangerColorLow = new Color(1f, 0f, 0f);
    [SerializeField] private AnimationCurve dangerColorCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Glow Fading")]
    [SerializeField] private Color tickColor = new Color(1f, 1f, 0f); 
    [SerializeField] private Color afterimageColor = new Color(1f, 1f, 1f, 1f);
    
    [SerializeField] private float glowFadeInDuration = 0.15f;
    [SerializeField] private float glowFadeOutDuration = 0.4f;
    [SerializeField] private AnimationCurve glowFadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Image _tickGraphicImage;
    private float _previousEnergy;
    private float _afterimageStoredEnergy;
    private float _afterimageAlpha = 0f;
    private float _afterimageTimer;
    
    private float _glowTimer = 0f; 
    private float _tickAlpha = 0f; 

    void Start()
    {
        if (SonarManager.Instance != null)
        {
            _previousEnergy = SonarManager.Instance.currentEnergy;
            _afterimageStoredEnergy = _previousEnergy;
        }

        Color startColor = afterimageColor;
        startColor.a = 0f;
        afterimageFillImage.color = startColor;

        if (backgroundFillImage != null) backgroundFillImage.color = backgroundColor;
        
        if (glowCanvasGroup != null)
        {
            glowCanvasGroup.alpha = 0f;
            glowCanvasGroup.gameObject.SetActive(false);
        }

        if (tickMarkPivot != null)
        {
            _tickGraphicImage = tickMarkPivot.GetComponentInChildren<Image>();
        }
    }

    void Update()
    {
        if (SonarManager.Instance == null) return;

        float current = SonarManager.Instance.currentEnergy;
        float max = SonarManager.Instance.maxEnergy;
        bool isSafe = SonarManager.Instance.inSafeZone;
        bool isBroken = SonarManager.Instance.IsEmptyPenaltyActive;

        // 1. Afterimage Logic
        if (current < _previousEnergy)
        {
            _afterimageStoredEnergy = _previousEnergy;
            _afterimageAlpha = 1f; 
            _afterimageTimer = afterimageDelay;
        }
        
        if (_afterimageAlpha > 0f)
        {
            if (_afterimageTimer > 0f) _afterimageTimer -= Time.deltaTime;
            else _afterimageAlpha = Mathf.MoveTowards(_afterimageAlpha, 0f, (1f / afterimageFadeDuration) * Time.deltaTime);
        }

        if (current >= _afterimageStoredEnergy) _afterimageAlpha = 0f;
        _previousEnergy = current;

        Color currentAiColor = afterimageColor;
        currentAiColor.a = _afterimageAlpha;
        afterimageFillImage.color = currentAiColor;
        afterimageFillImage.fillAmount = Mathf.Lerp(emptyFill, fullFill, _afterimageStoredEnergy / max);

        // 2. Main Bar Logic (Always colors based on current energy now)
        float predictedCost = strokeStaling.GetNextStrokeCost();
        float currentPercent = current / max;
        bool isFullyUnstaled = strokeStaling.IsFullyUnstaled;

        mainFillImage.fillAmount = Mathf.Lerp(emptyFill, fullFill, currentPercent);
        float colorCurveValue = dangerColorCurve.Evaluate(currentPercent);
        mainFillImage.color = isSafe ? safeColor : Color.Lerp(dangerColorLow, dangerColorHigh, colorCurveValue);

        // 3. Tick Mark Logic
        // Fade in ONLY if we have cost, aren't completely fresh, AND we aren't locked out!
        float targetTickAlpha = (predictedCost > 0 && !isFullyUnstaled && !isBroken) ? 1f : 0f;
        _tickAlpha = Mathf.MoveTowards(_tickAlpha, targetTickAlpha, Time.deltaTime * 6f); 

        if (_tickAlpha > 0f)
        {
            tickMarkPivot.gameObject.SetActive(true);
            if (_tickGraphicImage != null)
            {
                Color fadedTickColor = tickColor;
                fadedTickColor.a = _tickAlpha;
                _tickGraphicImage.color = fadedTickColor;
            }

            // Calculate where it will drop to (clamped so the tick doesn't fly off the bottom of the bar)
            float predictedRemainingPercent = Mathf.Clamp01((current - predictedCost) / max);
            float mappedPredictedFill = Mathf.Lerp(emptyFill, fullFill, predictedRemainingPercent);
            tickMarkPivot.localEulerAngles = new Vector3(0, 0, mappedPredictedFill * 360f);
        }
        else
        {
            tickMarkPivot.gameObject.SetActive(false); 
        }

        // 4. Glow Logic
        if (isBroken) _glowTimer += Time.deltaTime / glowFadeInDuration;
        else _glowTimer -= Time.deltaTime / glowFadeOutDuration;
        
        _glowTimer = Mathf.Clamp01(_glowTimer);

        if (glowCanvasGroup != null)
        {
            if (_glowTimer > 0f)
            {
                if (!glowCanvasGroup.gameObject.activeSelf) glowCanvasGroup.gameObject.SetActive(true);
                glowCanvasGroup.alpha = glowFadeCurve.Evaluate(_glowTimer);
            }
            else
            {
                if (glowCanvasGroup.gameObject.activeSelf) glowCanvasGroup.gameObject.SetActive(false);
            }
        }

        // 5. Overall Visibility
        if (isSafe && current >= max) canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 0f, 2.5f * Time.deltaTime);
        else canvasGroup.alpha = 1f;
    }
}