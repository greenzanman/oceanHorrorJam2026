using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class TypeOut : MonoBehaviour
{
    [SerializeField] private float charactersPerSecond = 40f;
    [SerializeField] private bool playOnStart = true;

    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource buttonPressSound;
    [SerializeField] private string nextSceneName;
    [TextArea(5, 20)]
    [SerializeField] private string fullText;


    private Button button;
    private Coroutine revealCoroutine;
    
    // action to listen for any button press
    private InputAction anyButtonAction;
    private bool isTransitioning = false; // Prevents triggering the transition multiple times

    private void Awake()
    {
        textComponent.text = "";
        button = GetComponentInChildren<Button>();
        button.gameObject.SetActive(false);
        
        // Listen for clicks on the UI button
        button.onClick.AddListener(TransitionNextScene);

        // setup anyButtonAction to listen for any button
        anyButtonAction = new InputAction("AnyPress");
        anyButtonAction.AddBinding("<Keyboard>/anyKey");
        anyButtonAction.AddBinding("<Gamepad>/<button>");
        anyButtonAction.AddBinding("<Mouse>/leftButton");

        // Listen for any button press
        anyButtonAction.performed += OnAnyButtonPressed;
    }

    private void OnEnable()
    {
        anyButtonAction.Enable();
    }

    private void OnDisable()
    {
        anyButtonAction.Disable();
    }

    private void Start()
    {
        if (playOnStart)
            StartReveal();
    }

    private void OnAnyButtonPressed(InputAction.CallbackContext context)
    {
        // Only allow progression if the text is done (the button is visible) 
        // and we haven't already started transitioning.
        if (button.gameObject.activeSelf && !isTransitioning)
        {
            TransitionNextScene();
        }
    }

    public void StartReveal()
    {
        if (revealCoroutine != null)
            StopCoroutine(revealCoroutine);

        revealCoroutine = StartCoroutine(RevealText());
    }

    public void Skip()
    {
        if (revealCoroutine != null)
            StopCoroutine(revealCoroutine);

        textComponent.text = fullText;
        button.gameObject.SetActive(true); // show button if skipped
    }

    private IEnumerator RevealText()
    {
        textComponent.text = "";
        float delay = 1f / charactersPerSecond;

        foreach (char c in fullText)
        {
            textComponent.text += c;
            if (audioSource != null && !char.IsWhiteSpace(c))
            {
                Debug.Log("Playing sound for character: " + c);
                audioSource.PlayOneShot(audioSource.clip);
            }
            yield return new WaitForSeconds(delay);
        }

        yield return new WaitForSeconds(delay*5);

        button.gameObject.SetActive(true);
    }

    public void SetText(string newText)
    {
        fullText = newText;
    }

    void TransitionNextScene()
    {
        if (isTransitioning) return;
        isTransitioning = true; // lock to prevent multiple transitions

        StartCoroutine(TransitionToNextScene());
        // Implement scene transition logic here
    }

    IEnumerator TransitionToNextScene()
        {
            buttonPressSound.PlayOneShot(buttonPressSound.clip);
        // Add any transition effects here (e.g., fade out)
        yield return new WaitForSeconds(3f); // Wait for the effect to finish
        SceneManager.LoadScene(nextSceneName);
    }
}
