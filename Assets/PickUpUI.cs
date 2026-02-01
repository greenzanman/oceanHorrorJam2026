using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PickUpUI : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 2f;
    private TextMeshProUGUI pickupText;

    private Coroutine clearTextCoroutine;

    
    
    // Start is called before the first frame update
    void Start()
    {
        // Subscribe to the pickup event
        PickupLogic.OnPickedUp += HandlePickup;
        pickupText = GetComponent<TextMeshProUGUI>();
    }

    void HandlePickup(PickupLogic pickup)
    {
        if (clearTextCoroutine != null)
        {
            StopCoroutine(clearTextCoroutine);
        }
        Debug.Log("Pickup event received for: " + pickup.name);
        // Here you can add UI update logic, like showing a message or updating inventory
        if (pickupText != null)
        {
            pickupText.text = "Picked up: " + pickup.name;
            // Optionally, you can add code to fade out the text after a few seconds
        } 
        clearTextCoroutine = StartCoroutine(ClearPickupTextAfterDelay(2f));
    }

    IEnumerator ClearPickupTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        float elapsedTime = 0f;
        Color originalColor = pickupText.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            pickupText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        pickupText.text = "";
        pickupText.color = originalColor; // Reset color
    }
}
