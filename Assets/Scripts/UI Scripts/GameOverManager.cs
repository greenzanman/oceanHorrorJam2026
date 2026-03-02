using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // Added for Any Button support
using FMODUnity;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    [Header("External References")]
    public PlayerSystemsManager playerSystems;

    [Header("Debug / Testing")]
    public bool testMode = false; // Check this in inspector to auto-play!
    
    [Header("UI References")]
    public GameObject gameOverCanvas;
    public CanvasGroup screenFadeGroup;
    public Image screenFadeImage;
    public RectTransform heartMonitorLine;
    public CanvasGroup heartMonitorGroup;
    public Button redoButton;
    public CanvasGroup redoButtonGroup;
    public Button invisibleAdvanceButton; 

    [Header("Animation Curves")]
    [Tooltip("How the screen fades to black (0 to 1 over time)")]
    public AnimationCurve fadeToBlackCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Tooltip("How the screen snaps to red (Make this an exponential ease-in curve!)")]
    public AnimationCurve redSnapCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Tooltip("How the heart monitor slides across the screen")]
    public AnimationCurve monitorSlideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Text References")]
    public TextMeshProUGUI redText;
    public TextMeshProUGUI yellowText;
    public float charactersPerSecond = 30f;

    [Header("Audio Events")]
    public EventReference hypnoticBellEvent;
    public EventReference flatlineBeepEvent;
    public EventReference uiErrorEvent;
    public EventReference uiSuccessJingleEvent;
    public EventReference uiTextBlipEvent; 

    private bool _waitingForPlayerClick = false;
    private int _idlePhase = 0; // 0 = sequence, 1 = idle/ready to restart

    // Input System
    private InputAction anyButtonAction;

    // Track the current state so the "Any Button" knows what to do
    private enum SequenceState { PlayingAnimation, WaitingForFirstRedo, WaitingForDialogue, WaitingForFinalRedo }
    private SequenceState _currentState = SequenceState.PlayingAnimation;

    void Awake()
    {
        // Setup the "Any Button" action
        anyButtonAction = new InputAction("AnyPress");
        anyButtonAction.AddBinding("<Keyboard>/anyKey");
        anyButtonAction.AddBinding("<Gamepad>/<button>");
        anyButtonAction.AddBinding("<Mouse>/leftButton");
        
        anyButtonAction.performed += ctx => OnAnyInputPressed();
    }

    void OnEnable() => anyButtonAction.Enable();
    void OnDisable() => anyButtonAction.Disable();

    void Start()
    {
        screenFadeGroup.alpha = 0;
        heartMonitorGroup.alpha = 0;
        redoButtonGroup.alpha = 0;
        redoButton.gameObject.SetActive(false);
        invisibleAdvanceButton.gameObject.SetActive(false);
        redText.text = "";
        yellowText.text = "";

        // ensure gameover ui hidden at start
        if (gameOverCanvas != null) gameOverCanvas.SetActive(false);

        redoButton.onClick.AddListener(OnRedoClicked);
        invisibleAdvanceButton.onClick.AddListener(OnAdvanceDialogueClicked);

        // Test mode auto-start
        if (testMode)
        {
            Invoke(nameof(StartGameOverSequence), 1f);
        }
    }

    public void StartGameOverSequence()
    {
        // 0. show game over ui
        if (gameOverCanvas != null) gameOverCanvas.SetActive(true);
        
        // 1. KILL PLAYER CONTROLS IMMEDIATELY
        if (playerSystems != null)
        {
            playerSystems.SetAllSystems(false);
        }

        // 2. Start the animation
        _currentState = SequenceState.PlayingAnimation;
        StartCoroutine(GameOverRoutine());
    }

    // Centralized Input Handler
    private void OnAnyInputPressed()
    {
        switch (_currentState)
        {
            case SequenceState.WaitingForFirstRedo:
                OnRedoClicked(); // Trigger the break/dialogue
                break;
            case SequenceState.WaitingForDialogue:
                if (_waitingForPlayerClick) OnAdvanceDialogueClicked(); // Advance text
                break;
            case SequenceState.WaitingForFinalRedo:
                OnRedoClicked(); // Restart scene
                break;
        }
    }

    private IEnumerator GameOverRoutine()
    {
        // Phase 1: Fade to Black & Hypnotic Bell
        if (!hypnoticBellEvent.IsNull) RuntimeManager.PlayOneShot(hypnoticBellEvent);
        screenFadeImage.color = Color.black;
        // Pass the curve into our helper function!
        yield return FadeCanvasGroup(screenFadeGroup, 0f, 1f, 0.4f, fadeToBlackCurve);

        // Phase 2: Fade to Red & Roar
        yield return new WaitForSeconds(1f); 
        screenFadeImage.color = new Color(0.3f, 0f, 0f); 
        
        // Use the serialized redSnapCurve
        float t = 0;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            float normalizedTime = t / 0.2f;
            screenFadeGroup.alpha = redSnapCurve.Evaluate(normalizedTime); 
            yield return null;
        }
        screenFadeGroup.alpha = redSnapCurve.Evaluate(1f); // Ensure it caps exactly at the end

        // Phase 3: Radio Cutoff
        screenFadeImage.color = Color.black;
        // (Assuming you want a standard linear/ease out for this one)
        yield return FadeCanvasGroup(screenFadeGroup, 1f, 1f, 1f, AnimationCurve.Linear(0,1,1,1)); 

        // Phase 4: Heart Monitor Slide
        if (!flatlineBeepEvent.IsNull) RuntimeManager.PlayOneShot(flatlineBeepEvent);

        // 1. Setup Positions
        // Use localScale to ensure it works even if the image is scaled up
        float imageWidth = heartMonitorLine.rect.width * heartMonitorLine.localScale.x;
        
        // Start: Left edge aligned to Screen Left (Pos X = 0)
        Vector2 startPos = new Vector2(0, heartMonitorLine.anchoredPosition.y);
        // End: Right edge aligned to Screen Left (Pos X = -Width)
        Vector2 endPos = new Vector2(-imageWidth, heartMonitorLine.anchoredPosition.y);

        // 2. Initial State
        heartMonitorGroup.alpha = 0f;
        heartMonitorLine.anchoredPosition = startPos;

        // 3. Start the Fade In and Slide in parallel
        // We use StartCoroutine here so the code continues immediately to the slide loop
        StartCoroutine(FadeCanvasGroup(heartMonitorGroup, 0f, 1f, 0.25f, AnimationCurve.EaseInOut(0,0,1,1)));

        float slideDuration = 2.5f; 
        float fadeOutStartTime = 1.8f; // Start fading out before the slide finishes
        bool hasTriggeredFadeOut = false;
        t = 0;

        while (t < slideDuration)
        {
            t += Time.deltaTime;
            float normalizedTime = t / slideDuration;
            
            // Apply the serializable curve to the slide movement
            float curvePercent = monitorSlideCurve.Evaluate(normalizedTime);
            heartMonitorLine.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, curvePercent);

            // 4. Trigger Fade Out Coroutine "as it's nearing the end"
            if (t >= fadeOutStartTime && !hasTriggeredFadeOut)
            {
                hasTriggeredFadeOut = true;
                StartCoroutine(FadeCanvasGroup(heartMonitorGroup, 1f, 0f, 0.7f, AnimationCurve.EaseInOut(0,1,1,0)));
            }

            yield return null;
        }

        // Ensure it finishes at the exact end position
        heartMonitorLine.anchoredPosition = endPos;

        // Wait for the fade-out coroutine to actually finish before moving to Phase 5
        yield return new WaitForSeconds(0.7f); 

        // extra pause b4 redo 
        yield return new WaitForSeconds(2f); 


        // Phase 5: Show REDO Button
        redoButton.gameObject.SetActive(true);
        yield return FadeCanvasGroup(redoButtonGroup, 0f, 1f, 2f, AnimationCurve.EaseInOut(0,0,1,1));
        
        _currentState = SequenceState.WaitingForFirstRedo; 
    }

    private void OnRedoClicked()
    {
        if (_idlePhase > 0) 
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        if (!uiErrorEvent.IsNull) RuntimeManager.PlayOneShot(uiErrorEvent);
        redoButtonGroup.alpha = 0.5f; 
        
        // Disable mouse interaction so we rely strictly on our state machine
        redoButton.interactable = false; 
        
        StartCoroutine(DialogueSequenceRoutine());
    }

    private void OnAdvanceDialogueClicked()
    {
        _waitingForPlayerClick = false;
    }

    private IEnumerator DialogueSequenceRoutine()
    {
        _currentState = SequenceState.WaitingForDialogue;
        invisibleAdvanceButton.gameObject.SetActive(true); 

        yield return StartCoroutine(TypeDialogue(redText, "{p}IN LIFE,{p}{p} THERE ARE NO REDOS."));
        yield return WaitForClick();

        yield return StartCoroutine(TypeDialogue(redText, "{p}..."));
        yield return WaitForClick();

        yield return StartCoroutine(TypeDialogue(yellowText, "wait waitwait-{p} remember,{p} this is the guy{p} who socked his 6'1 mother{p} in the nose{p}{p}\n\nand then BRAGGED about it —{p} are u sure abt this..?"));
        yield return WaitForClick();

        yield return StartCoroutine(TypeDialogue(redText, "{p}.{p}.{p}.{p}"));
        yield return WaitForClick();

        yield return StartCoroutine(TypeDialogue(yellowText, "{p}and also...{p}{p}{p} dontcha have ur thing{p}g{p}g{p}g tonight?  ;—)"));
        yield return WaitForClick();

        yield return StartCoroutine(TypeDialogue(redText, "TRUE.{p} I{p} WOULD PREFER TO CLOCK OUT SOON;{p}{p}"));
        yield return StartCoroutine(TypeDialogue(redText, "\n\n ME AND THE FLOWER HAVE PLANS TO PLAY BACKGAMMON TONIGHT.", true)); 
        yield return WaitForClick();

        yield return StartCoroutine(TypeDialogue(yellowText, "sick.{p}{p}{p} well this guy is def gonna just die again anyways -{p}{p} i mean,{p}{p} he only made it <b>halfway down</b>.{p}{p}\n\n sooo don't worry abt it"));
        yield return WaitForClick();

        yield return StartCoroutine(TypeDialogue(redText, "THANKS.{p} WOULD YOU MIND LETTING NIGHT SHIFT KNOW,{p} WHEN THEY ARRIVE."));
        yield return WaitForClick();

        yield return StartCoroutine(TypeDialogue(yellowText, "yep already sent em a text, everythings taken care of!!{p} go on now, you have a lovely night ahead ;—)"));
        yield return WaitForClick();

        yield return StartCoroutine(TypeDialogue(redText, ":|"));
        yield return new WaitForSeconds(1f);
        redText.text = ":]"; 
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(TypeDialogue(redText, "\nTHANKS. LOVELY NIGHT TO YOU AS WELL.  o/", true)); 
        yield return WaitForClick();

        invisibleAdvanceButton.gameObject.SetActive(false);
        redText.text = "";
        yellowText.text = "";

        yield return new WaitForSeconds(1f);
        if (!uiSuccessJingleEvent.IsNull) RuntimeManager.PlayOneShot(uiSuccessJingleEvent);
        redoButtonGroup.alpha = 1f;
        _idlePhase = 1;
        _currentState = SequenceState.WaitingForFinalRedo;

        StartCoroutine(IdleChatterRoutine());
    }

    private IEnumerator WaitForClick()
    {
        _waitingForPlayerClick = true;
        while (_waitingForPlayerClick) yield return null;
        
        redText.text = "";
        yellowText.text = "";
    }

    private IEnumerator TypeDialogue(TextMeshProUGUI textComponent, string fullText, bool append = false)
    {
        if (!append) textComponent.text = "";
        float delay = 1f / charactersPerSecond;

        for (int i = 0; i < fullText.Length; i++)
        {
            //{p} = pause tag 
            if (i < fullText.Length - 2 && fullText[i] == '{' && fullText[i+1] == 'p' && fullText[i+2] == '}')
            {
                yield return new WaitForSeconds(0.5f); 
                i += 2; 
                continue;
            }

            textComponent.text += fullText[i];

            // text blip sound 
            if (!uiTextBlipEvent.IsNull) RuntimeManager.PlayOneShot(uiTextBlipEvent);

            yield return new WaitForSeconds(delay);
        }
    }

    private IEnumerator IdleChatterRoutine()
    {
        yield return new WaitForSeconds(5f);
        if (_idlePhase == 1) yellowText.text = "...";
        yield return StartCoroutine(TypeDialogue(yellowText, "dammit..."));
        yield return new WaitForSeconds(2f);
        if (_idlePhase == 1) yellowText.text = "...";

        yield return new WaitForSeconds(5f);
        if (_idlePhase == 1) yellowText.text = "um. you can press it now, y'know.";

        yield return new WaitForSeconds(5f); 
        if (_idlePhase == 1) yellowText.text = "...";
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration, AnimationCurve curve)
    {
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            float normalizedTime = t / duration;
            // Get the curve multiplier (0 to 1) and Lerp based on that
            float curveValue = curve.Evaluate(normalizedTime);
            cg.alpha = Mathf.LerpUnclamped(start, end, curveValue);
            yield return null;
        }
        cg.alpha = end;
    }
}