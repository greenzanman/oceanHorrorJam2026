using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyUI : MonoBehaviour
{
    [Header("References")]
    public StalkerEnemy stalker;
    public Image centerDot;
    public Image yellowBorder;
    
    [Header("Warning Text References")]
    public TMP_Text leftWarningText;  // Assign your "< !" text here
    public TMP_Text rightWarningText; // Assign your "! >" text here

    [Header("Visual Settings")]
    public Color lowPanicColor = new Color(1f, 0.5f, 0f);
    public Color highPanicColor = new Color(0.6f, 0f, 0f);
    public float minDotScale = 0.5f;
    public float maxDotScale = 2.0f;
    public float maxBlinkSpeed = 20f;

    [Header("Success Ring Settings")]
    public Color successRingColor = Color.green;
    public float maxRingThickness = 1.5f;
    [Range(0.05f, 0.5f)] public float minRingArc = 0.1f; 

    [Header("Warning Text Settings")]
    public float minWarningBlinkSpeed = 2f;  // Slower blink when monster is barely off-screen
    public float maxWarningBlinkSpeed = 15f; // Frantic blink when exactly 180 degrees behind

    private Camera _mainCam;

    void Start()
    {
        _mainCam = Camera.main;
        
        // Ensure text starts hidden
        if (leftWarningText != null) leftWarningText.enabled = false;
        if (rightWarningText != null) rightWarningText.enabled = false;
    }

    void Update()
    {
        if (stalker == null || !stalker.isStalking)
        {
            centerDot.enabled = false;
            yellowBorder.enabled = false;
            if (leftWarningText) leftWarningText.enabled = false;
            if (rightWarningText) rightWarningText.enabled = false;
            return;
        }

        centerDot.enabled = true;
        
        // --- CENTER DOT LOGIC ---
        float panicNorm = stalker.panicIntensity / 100f; 
        centerDot.color = Color.Lerp(lowPanicColor, highPanicColor, panicNorm);
        centerDot.rectTransform.localScale = Vector3.one * Mathf.Lerp(minDotScale, maxDotScale, panicNorm);

        float currentDotBlinkSpeed = Mathf.Lerp(2f, maxBlinkSpeed, panicNorm);
        float dotAlpha = (Mathf.Sin(Time.time * currentDotBlinkSpeed) + 1f) / 2f; 
        
        Color dotColor = centerDot.color;
        dotColor.a = dotAlpha;
        centerDot.color = dotColor;

        // --- ACCURACY AND RING LOGIC ---
        float accuracyNorm = stalker.CurrentAccuracy;

        if (accuracyNorm > 0.01f)
        {
            // We are looking at the monster! Show the ring, hide the warning text.
            yellowBorder.enabled = true;
            if (leftWarningText) leftWarningText.enabled = false;
            if (rightWarningText) rightWarningText.enabled = false;
            
            Vector3 dirToMonster = stalker.transform.position - _mainCam.transform.position;
            Vector3 localDir = _mainCam.transform.InverseTransformDirection(dirToMonster);
            float angleToMonster = Mathf.Atan2(localDir.x, localDir.y) * Mathf.Rad2Deg;

            float currentFill = Mathf.Lerp(minRingArc, 1.0f, accuracyNorm);
            yellowBorder.fillAmount = currentFill;

            float rotationZ = -angleToMonster + (currentFill * 180f);
            yellowBorder.rectTransform.localEulerAngles = new Vector3(0, 0, rotationZ);
            
            Color ringColor = successRingColor;
            ringColor.a = Mathf.Lerp(0.2f, 1.0f, accuracyNorm); 
            yellowBorder.color = ringColor;
            yellowBorder.rectTransform.localScale = Vector3.one * Mathf.Lerp(1.0f, maxRingThickness, accuracyNorm);
        }
        else
        {
            // The monster is off-screen. Hide the ring, show the warning text.
            yellowBorder.enabled = false;

            // --- OFF-SCREEN WARNING LOGIC ---
            // 1. Calculate the flat angle (ignoring height differences)
            Vector3 dirToMonsterFlat = stalker.transform.position - _mainCam.transform.position;
            dirToMonsterFlat.y = 0;
            Vector3 camForwardFlat = _mainCam.transform.forward;
            camForwardFlat.y = 0;

            // Signed angle tells us left (-) or right (+)
            float signedAngle = Vector3.SignedAngle(camForwardFlat, dirToMonsterFlat, Vector3.up);
            float absAngle = Mathf.Abs(signedAngle); // 0 to 180

            // 2. Calculate Blink Speed based on how far behind us it is
            // 0 = barely off-screen, 1 = perfectly 180 degrees behind
            float rearSeverity = (absAngle - stalker.viewConeAngle) / (180f - stalker.viewConeAngle);
            rearSeverity = Mathf.Clamp01(rearSeverity);

            float currentWarningBlink = Mathf.Lerp(minWarningBlinkSpeed, maxWarningBlinkSpeed, rearSeverity);
            float warningAlpha = (Mathf.Sin(Time.time * currentWarningBlink) + 1f) / 2f;

            // 3. Determine which side to show
            // To prevent flipping out when EXACTLY 180, we use >= 179.5f to lock it to the right side
            bool isDirectlyBehind = absAngle >= 179.5f;

            if (isDirectlyBehind || signedAngle >= 0)
            {
                // Monster is to the Right (or directly behind)
                if (leftWarningText) leftWarningText.enabled = false;
                if (rightWarningText) 
                {
                    rightWarningText.enabled = true;
                    Color c = rightWarningText.color;
                    c.a = warningAlpha;
                    rightWarningText.color = c;
                }
            }
            else
            {
                // Monster is to the Left
                if (rightWarningText) rightWarningText.enabled = false;
                if (leftWarningText) 
                {
                    leftWarningText.enabled = true;
                    Color c = leftWarningText.color;
                    c.a = warningAlpha;
                    leftWarningText.color = c;
                }
            }
        }
    }
}