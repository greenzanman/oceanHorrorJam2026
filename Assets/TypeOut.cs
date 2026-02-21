using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using UnityEditorInternal;

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

    private void Awake()
    {
        textComponent.text = "";
        button = GetComponentInChildren<Button>();
        button.gameObject.SetActive(false);
        button.onClick.AddListener(NextScene);
    }

    private void Start()
    {
        if (playOnStart)
            StartReveal();
        //audioSource.PlayOneShot(audioSource.clip);
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

    void NextScene()
    {
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
