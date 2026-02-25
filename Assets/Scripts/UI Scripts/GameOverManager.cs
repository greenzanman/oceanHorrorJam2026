using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // Added for Any Button support
using FMODUnity;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    [Header("Debug / Testing")]
    public bool testMode = false; // Check this in inspector to auto-play!
    
    [Header("UI References")]
    public CanvasGroup screenFadeGroup;
    public Image screenFadeImage;
    public RectTransform heartMonitorLine;
    public CanvasGroup heartMonitorGroup;
    public Button redoButton;
    public CanvasGroup redoButtonGroup;
    public Button invisibleAdvanceButton; 
    
    [Header("Text References")]
    public TextMeshProUGUI redText;
    public TextMeshProUGUI yellowText;
    public float charactersPerSecond = 30f;

    [Header("Audio Events")]
    public EventReference hypnoticBellEvent;
    public EventReference roarEvent;
    public EventReference radioCrackleEvent;
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
        yield return FadeCanvasGroup(screenFadeGroup, 0f, 1f, 0.4f);

        // Phase 2: Fade to Red & Roar
        yield return new WaitForSeconds(1f); 
        screenFadeImage.color = new Color(0.3f, 0f, 0f); 
        if (!roarEvent.IsNull) RuntimeManager.PlayOneShot(roarEvent);
        
        float t = 0;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            screenFadeGroup.alpha = Mathf.Pow(t / 0.2f, 2f); 
            yield return null;
        }

        // Phase 3: Radio Cutoff
        if (!radioCrackleEvent.IsNull) RuntimeManager.PlayOneShot(radioCrackleEvent);
        screenFadeImage.color = Color.black;
        yield return FadeCanvasGroup(screenFadeGroup, 1f, 1f, 1f); 

        // Phase 4: Heart Monitor Slide
        heartMonitorGroup.alpha = 1f;
        FMOD.Studio.EventInstance flatline = RuntimeManager.CreateInstance(flatlineBeepEvent);
        flatline.start();

        Vector2 startPos = new Vector2(Screen.width, heartMonitorLine.anchoredPosition.y);
        Vector2 endPos = new Vector2(0, heartMonitorLine.anchoredPosition.y);
        
        t = 0;
        while (t < 2f) 
        {
            t += Time.deltaTime;
            heartMonitorLine.anchoredPosition = Vector2.Lerp(startPos, endPos, t / 2f);
            yield return null;
        }

        flatline.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        flatline.release();
        yield return FadeCanvasGroup(heartMonitorGroup, 1f, 0f, 1f);

        // Phase 5: Show REDO Button
        redoButton.gameObject.SetActive(true);
        yield return FadeCanvasGroup(redoButtonGroup, 0f, 1f, 2f);
        
        // Update state so Any Button works
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

        yield return StartCoroutine(TypeDialogue(yellowText, "wait. but this is a guy{p} who socked his 6'1 mother{p} in the nose{p}{p}\nand then BRAGGED about it —{p} <i>you know the type</i>..."));
        yield return WaitForClick();

        yield return StartCoroutine(TypeDialogue(redText, "{p}.{p}.{p}.{p}"));
        yield return WaitForClick();

        yield return StartCoroutine(TypeDialogue(yellowText, "{p}and remember...{p}{p}{p} u have ur thingg{p}g{p}g tonight  ;—)"));
        yield return WaitForClick();

        yield return StartCoroutine(TypeDialogue(redText, "TRUE.{p} I WOULD PREFER TO CLOCK OUT SOON;{p} ME AND THE FLOWER HAVE PLANS TO PLAY BACKGAMMON TONIGHT."));
        yield return WaitForClick();

        yield return StartCoroutine(TypeDialogue(yellowText, "sick.{p}{p}{p} well this guy is def gonna just right up and die anyways -{p}{p} i mean,{p}{p} he only made it <b>halfway down</b>.{p}{p} sooo don't worry abt it"));
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
        yield return new WaitForSeconds(8f);
        if (_idlePhase == 1) yellowText.text = "um. you can press it now, y'know.";

        yield return new WaitForSeconds(5f); 
        if (_idlePhase == 1) yellowText.text = "...";
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, t / duration);
            yield return null;
        }
        cg.alpha = end;
    }
}