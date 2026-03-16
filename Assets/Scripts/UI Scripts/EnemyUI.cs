using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyUI : MonoBehaviour
{
    [Header("UI Group Setting")]
    public CanvasGroup mainCanvasGroup;
    public float masterFadeSpeed = 3f;

    [Header("References")]
    public StalkerEnemy stalker;
    public Image centerDot;
    
    [Header("Warning Text References")]
    public TMP_Text leftWarningText;  
    public TMP_Text rightWarningText; 

    [Header("Directional Arrow Settings")]
    public TMP_Text directionalArrow; 
    public float arrowRadius = 150f;  
    public float maxArrowScale = 2.0f; 
    public float minArrowScale = 0.5f; 
    public Color arrowColor = Color.red;

    [Header("Visual Settings")]
    public Color lowPanicColor = new Color(1f, 0.5f, 0f);
    public Color highPanicColor = new Color(0.6f, 0f, 0f);
    public float minDotScale = 0.5f;
    public float maxDotScale = 2.0f;
    public float maxBlinkSpeed = 20f;
    [Tooltip("How violently the center dot shakes at 100% panic")]
    public float maxShakeAmount = 15f; // Max pixels the dot will shake

    [Header("Off-Screen Warning Settings")]
    public float minWarningBlinkSpeed = 2f;  
    public float maxWarningBlinkSpeed = 15f; 

    private Camera _mainCam;
    private float _arrowVisibilityWeight = 0f; 
    
    // We need to remember where the dot started so it doesn't drift away while shaking
    private Vector3 _originalDotPos; 

    void Start()
    {
        _mainCam = Camera.main;
        
        if (leftWarningText) leftWarningText.enabled = true;
        if (rightWarningText) rightWarningText.enabled = true;
        if (directionalArrow) directionalArrow.enabled = true;
        if (centerDot) 
        {
            centerDot.enabled = true;
            // Cache the starting position of the dot
            _originalDotPos = centerDot.rectTransform.localPosition;
        }
        
        if (mainCanvasGroup != null) mainCanvasGroup.alpha = 0f;
    }

    void Update()
    {
        // 1. MASTER FADE LOGIC
        bool isActive = stalker != null && stalker.isStalking;
        float targetMasterAlpha = isActive ? 1f : 0f;
        
        if (mainCanvasGroup != null)
        {
            mainCanvasGroup.alpha = Mathf.MoveTowards(mainCanvasGroup.alpha, targetMasterAlpha, Time.deltaTime * masterFadeSpeed);
            if (mainCanvasGroup.alpha <= 0.001f && !isActive) return;
        }
        else if (!isActive)
        {
            return; 
        }

        // --- CENTER DOT LOGIC (WITH SHAKE) ---
        float panicNorm = stalker.panicIntensity / 100f; 
        centerDot.color = Color.Lerp(lowPanicColor, highPanicColor, panicNorm);
        centerDot.rectTransform.localScale = Vector3.one * Mathf.Lerp(minDotScale, maxDotScale, panicNorm);

        float currentDotBlinkSpeed = Mathf.Lerp(2f, maxBlinkSpeed, panicNorm);
        float dotAlpha = (Mathf.Sin(Time.time * currentDotBlinkSpeed) + 1f) / 2f; 
        
        Color dotColor = centerDot.color;
        dotColor.a = dotAlpha;
        centerDot.color = dotColor;

        // NEW: Calculate and apply the shake
        // We square the panicNorm so the shake exponentially ramps up at the very end
        float shakeIntensity = panicNorm * panicNorm; 
        Vector2 shakeOffset = Random.insideUnitCircle * (maxShakeAmount * shakeIntensity);
        centerDot.rectTransform.localPosition = _originalDotPos + (Vector3)shakeOffset;


        // 2. CROSSFADE LOGIC (Arrow vs Warnings)
        float accuracyNorm = stalker.CurrentAccuracy;
        bool inViewCone = accuracyNorm > 0.01f;
        
        float targetArrowWeight = inViewCone ? 1f : 0f;
        _arrowVisibilityWeight = Mathf.MoveTowards(_arrowVisibilityWeight, targetArrowWeight, Time.deltaTime * (masterFadeSpeed * 2f));

        
        // --- DIRECTIONAL ARROW LOGIC ---
        if (directionalArrow != null)
        {
            Vector3 dirToMonster = stalker.transform.position - _mainCam.transform.position;
            Vector3 localDir = _mainCam.transform.InverseTransformDirection(dirToMonster);
            float angle = Mathf.Atan2(localDir.y, localDir.x) * Mathf.Rad2Deg;

            directionalArrow.rectTransform.localEulerAngles = new Vector3(0, 0, angle);
            float rad = angle * Mathf.Deg2Rad;
            directionalArrow.rectTransform.localPosition = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * arrowRadius;

            Color c = arrowColor;
            c.a = Mathf.Lerp(1.0f, 0.0f, accuracyNorm) * _arrowVisibilityWeight; 
            directionalArrow.color = c;
            
            directionalArrow.rectTransform.localScale = Vector3.one * Mathf.Lerp(maxArrowScale, minArrowScale, accuracyNorm);
        }

        // --- OFF-SCREEN WARNING LOGIC ---
        Vector3 dirToMonsterFlat = stalker.transform.position - _mainCam.transform.position;
        dirToMonsterFlat.y = 0;
        Vector3 camForwardFlat = _mainCam.transform.forward;
        camForwardFlat.y = 0;

        float signedAngle = Vector3.SignedAngle(camForwardFlat, dirToMonsterFlat, Vector3.up);
        float absAngle = Mathf.Abs(signedAngle); 

        float rearSeverity = Mathf.Clamp01((absAngle - stalker.viewConeAngle) / (180f - stalker.viewConeAngle));
        float currentWarningBlink = Mathf.Lerp(minWarningBlinkSpeed, maxWarningBlinkSpeed, rearSeverity);
        
        float warningAlpha = ((Mathf.Sin(Time.time * currentWarningBlink) + 1f) / 2f) * (1f - _arrowVisibilityWeight);

        bool isDirectlyBehind = absAngle >= 179.5f;

        if (rightWarningText) 
        {
            Color rc = rightWarningText.color;
            rc.a = (isDirectlyBehind || signedAngle >= 0) ? warningAlpha : 0f;
            rightWarningText.color = rc;
        }

        if (leftWarningText) 
        {
            Color lc = leftWarningText.color;
            lc.a = (!isDirectlyBehind && signedAngle < 0) ? warningAlpha : 0f;
            leftWarningText.color = lc;
        }
    }
}