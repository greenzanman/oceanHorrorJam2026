using System.Collections;
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
    [SerializeField] private Color freshAfterimageColor = new Color(1f, 1f, 1f, 1f); 
    [SerializeField] private Color staleAfterimageColor = new Color(0.5f, 0.5f, 0.5f, 1f); 

    [Header("Fill Mapping Constraints")]
    [SerializeField] private float emptyFill = 0.15f; 
    [SerializeField] private float fullFill = 0.36f; // Adjusted for gap

    [Header("Colors & Curves")]
    [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.5f); 
    [SerializeField] private Color safeColor = Color.white;
    [SerializeField] private Color dangerColorHigh = new Color(1f, 0.4f, 0.4f);
    [SerializeField] private Color dangerColorLow = new Color(1f, 0f, 0f);
    [SerializeField] private AnimationCurve dangerColorCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Sonar Icons")]
    [SerializeField] private Image sonarIcon1; // The 50% Icon
    [SerializeField] private Image sonarIcon2; // The 100% Icon
    [SerializeField] private Color litIconColor = Color.white;
    [SerializeField] private Color dimmedIconColor = new Color(1f, 1f, 1f, 0.2f);

    [Header("Tick Mark Logic")]
    [SerializeField] private Color freshTickColor = new Color(1f, 1f, 0f); // NEW
    [SerializeField] private Color staleTickColor = new Color(1f, 0.5f, 0f); // NEW
    
    [Header("Ghost Tick Animation")]
    [SerializeField] private RectTransform ghostTickPivot; // NEW
    [SerializeField] private AnimationCurve ghostTickYCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private float ghostAnimDuration = 0.3f;
    [SerializeField] private float ghostFadeDuration = 0.2f;

    [Header("Glow Fading")]
    [SerializeField] private float glowFadeInDuration = 0.15f;
    [SerializeField] private float glowFadeOutDuration = 0.4f;
    [SerializeField] private AnimationCurve glowFadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Image _tickGraphicImage;
    private Image _ghostTickGraphicImage; // NEW
    private RectTransform _ghostTickRect; // NEW
    private Coroutine _ghostTickCoroutine; // NEW

    private float _previousEnergy;
    private float _afterimageStoredEnergy;
    private float _afterimageAlpha = 0f;
    private float _afterimageTimer;
    
    private float _glowTimer = 0f; 
    private float _tickAlpha = 0f; 

    // Subscribe to the stroke event
    void OnEnable()
    {
        if (strokeStaling != null) strokeStaling.OnStrokeEvent += TriggerGhostTick;
    }

    void OnDisable()
    {
        if (strokeStaling != null) strokeStaling.OnStrokeEvent -= TriggerGhostTick;
    }

    void Start()
    {
        if (SonarManager.Instance != null)
        {
            _previousEnergy = SonarManager.Instance.currentEnergy;
            _afterimageStoredEnergy = _previousEnergy;
        }

        if (backgroundFillImage != null) backgroundFillImage.color = backgroundColor;
        if (glowCanvasGroup != null)
        {
            glowCanvasGroup.alpha = 0f;
            glowCanvasGroup.gameObject.SetActive(false);
        }

        if (tickMarkPivot != null) _tickGraphicImage = tickMarkPivot.GetComponentInChildren<Image>();
        
        if (ghostTickPivot != null)
        {
            _ghostTickGraphicImage = ghostTickPivot.GetComponentInChildren<Image>();
            _ghostTickRect = _ghostTickGraphicImage.GetComponent<RectTransform>();
            ghostTickPivot.gameObject.SetActive(false); // Hide pool initially
        }
    }

    void Update()
    {
        if (SonarManager.Instance == null) return;

        float current = SonarManager.Instance.currentEnergy;
        float max = SonarManager.Instance.maxEnergy;
        bool isSafe = SonarManager.Instance.inSafeZone;
        bool isBroken = SonarManager.Instance.IsEmptyPenaltyActive;
        
        // Get dynamic staleness from 0 (Stale) to 1 (Fresh)
        float staleness = strokeStaling.GetStalenessNormalized();

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

        // Lerp the afterimage color based on staleness
        Color currentAiColor = Color.Lerp(staleAfterimageColor, freshAfterimageColor, staleness);
        currentAiColor.a = _afterimageAlpha;
        afterimageFillImage.color = currentAiColor;
        afterimageFillImage.fillAmount = Mathf.Lerp(emptyFill, fullFill, _afterimageStoredEnergy / max);

        // 2. Main Bar Logic
        float predictedCost = strokeStaling.GetNextStrokeCost();
        float currentPercent = current / max;
        bool isFullyUnstaled = strokeStaling.IsFullyUnstaled;

        mainFillImage.fillAmount = Mathf.Lerp(emptyFill, fullFill, currentPercent);
        float colorCurveValue = dangerColorCurve.Evaluate(currentPercent);
        mainFillImage.color = isSafe ? safeColor : Color.Lerp(dangerColorLow, dangerColorHigh, colorCurveValue);

        // 3. Main Tick Mark Logic
        float targetTickAlpha = (predictedCost > 0 && !isFullyUnstaled && !isBroken) ? 1f : 0f;
        _tickAlpha = Mathf.MoveTowards(_tickAlpha, targetTickAlpha, Time.deltaTime * 6f); 

        if (_tickAlpha > 0f)
        {
            tickMarkPivot.gameObject.SetActive(true);
            if (_tickGraphicImage != null)
            {
                // Lerp the tick color based on staleness
                Color blendedTickColor = Color.Lerp(staleTickColor, freshTickColor, staleness);
                blendedTickColor.a = _tickAlpha;
                _tickGraphicImage.color = blendedTickColor;
            }

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

        // --- 5. SONAR ICONS LOGIC ---
        // We use 0.99f for full to avoid tiny floating-point math errors
        bool isHalfFull = currentPercent >= 0.5f; 
        bool isCompletelyFull = currentPercent >= 0.99f; 
        
        // Ensure SonarManager has an IsSonarReady() method for the cooldown!
        bool isSonarReady = SonarManager.Instance != null && SonarManager.Instance.IsSonarReady();

        // Icon 1 (50%) only lights up if we are at least half full, NOT locked out, and off cooldown
        if (sonarIcon1 != null)
        {
            sonarIcon1.color = (isHalfFull && isSonarReady && !isBroken) ? litIconColor : dimmedIconColor;
        }

        // Icon 2 (100%) only lights up if we are completely full, NOT locked out, and off cooldown
        if (sonarIcon2 != null)
        {
            sonarIcon2.color = (isCompletelyFull && isSonarReady && !isBroken) ? litIconColor : dimmedIconColor;
        }
        

        // 6. Overall Visibility
        if (isSafe && current >= max) canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 0f, 2.5f * Time.deltaTime);
        else canvasGroup.alpha = 1f;
    }

    // --- GHOST TICK COROUTINE ---
    private void TriggerGhostTick()
    {
        if (ghostTickPivot == null || _ghostTickGraphicImage == null) return;

        if (_ghostTickCoroutine != null) StopCoroutine(_ghostTickCoroutine);
        _ghostTickCoroutine = StartCoroutine(GhostTickRoutine());
    }

    private IEnumerator GhostTickRoutine()
    {
        ghostTickPivot.gameObject.SetActive(true);
        
        // 1. Copy the current rotation of the main tick so it overlaps perfectly
        ghostTickPivot.localEulerAngles = tickMarkPivot.localEulerAngles;
        
        // 2. Copy the exact current color (including staleness)
        Color startColor = _tickGraphicImage != null ? _tickGraphicImage.color : freshTickColor;
        startColor.a = 1f; // Force full alpha on spawn
        _ghostTickGraphicImage.color = startColor;

        float elapsed = 0f;
        Vector2 startPos = new Vector2(_ghostTickRect.anchoredPosition.x, -15f);
        Vector2 endPos = new Vector2(_ghostTickRect.anchoredPosition.x, 10f);

        // Stage 1: Animate Position
        while (elapsed < ghostAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / ghostAnimDuration;
            
            // Use Evaluate to get the overshoot value, and LerpUnclamped so it can actually go past 10f
            float curveValue = ghostTickYCurve.Evaluate(t);
            _ghostTickRect.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, curveValue);
            
            yield return null;
        }

        _ghostTickRect.anchoredPosition = endPos; // Snap to final just in case

        // Stage 2: Fade Out
        elapsed = 0f;
        while (elapsed < ghostFadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / ghostFadeDuration);
            
            Color fadeColor = _ghostTickGraphicImage.color;
            fadeColor.a = alpha;
            _ghostTickGraphicImage.color = fadeColor;
            
            yield return null;
        }

        // Hide it to recycle for the next stroke
        ghostTickPivot.gameObject.SetActive(false);
    }
}