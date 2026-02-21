using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ElevatorIntroManager : MonoBehaviour
{
    [Header("UI & Player Control")]
    [SerializeField] private Image blackScreen;
    [SerializeField] private PlayerInput playerInput; 

    [Header("Elevator Elements")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private Transform doorTransform; 
    [SerializeField] private Transform elevatorBackTransform;

    [Header("FMOD Events")]
    [SerializeField] private FMODUnity.EventReference descendEvent;
    [SerializeField] private FMODUnity.EventReference doorOpenEvent;

    [Header("Camera Shake")]
    [SerializeField] private Transform cameraTransform; // Drag your Player's Main Camera here
    [SerializeField] private float shakeIntensity = 0.02f; // Keep this low so it's a rumble, not an earthquake
    [SerializeField] private float shakeDuration = 12f;

    void Start()
    {
        blackScreen.gameObject.SetActive(true);
        StartCoroutine(PlayIntroSequence());
        StartCoroutine(ShakeCamera()); // Start the shake at the exact same time
    }

    private IEnumerator PlayIntroSequence()
    {
        // ==========================================
        // 0 SECONDS: PITCH BLACK & NO CONTROL
        // ==========================================
        if (playerInput != null) playerInput.enabled = false;
        
        Color screenColor = blackScreen.color;
        screenColor.a = 1f; // 100% Black
        blackScreen.color = screenColor;
        blackScreen.raycastTarget = true; // Block UI clicks

        FMODUnity.RuntimeManager.PlayOneShot(descendEvent, elevatorBackTransform.position);

        // Wait in the dark for exactly 6 seconds
        yield return new WaitForSeconds(6f);


        // ==========================================
        // 6 SECONDS: RESTORE CONTROL & START FADE
        // ==========================================
        if (playerInput != null) playerInput.enabled = true;
        
        // Turn off raycast blocking so the player can interact while it fades
        blackScreen.raycastTarget = false; 

        // Now we slowly fade from black to clear over the NEXT 6 seconds
        float fadeDuration = 6f; 
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            screenColor.a = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            blackScreen.color = screenColor;
            yield return null; 
        }

        // Make sure it is completely invisible at the end
        screenColor.a = 0f;
        blackScreen.color = screenColor;


        // ==========================================
        // 12 SECONDS: OPEN DOORS
        // ==========================================
        // (Since we waited 6s in the dark, and the fade took 6s, we are now at exactly 12 seconds)
        
        doorAnimator.SetTrigger("Open");
        FMODUnity.RuntimeManager.PlayOneShot(doorOpenEvent, doorTransform.position);
    }


    // THE PARALLEL SHAKE COROUTINE
    private IEnumerator ShakeCamera()
    {
        if (cameraTransform == null) yield break;

        // Save the original position so we can snap back to it perfectly
        Vector3 originalPos = cameraTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            // Generate a tiny random offset
            float x = Random.Range(-1f, 1f) * shakeIntensity;
            float y = Random.Range(-1f, 1f) * shakeIntensity;

            // Apply the offset
            cameraTransform.localPosition = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // Snap the camera back to its exact original spot when the elevator stops
        cameraTransform.localPosition = originalPos;
    }
}