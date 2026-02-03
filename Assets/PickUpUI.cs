using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PickUpUI : MonoBehaviour
{
    [SerializeField] private Vector3 origin = new Vector3(0, 100, 0);
    [SerializeField] private int fontSize = 24;
    [SerializeField] private float offset = 30f;
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private string pickupPrefix = "Picked up: ";

    private Coroutine clearTextCoroutine;
    private int numberOfPickups = 0;

    
    
    // Start is called before the first frame update
    void Start()
    {
        // Subscribe to the pickup event
        PickupLogic.OnPickedUp += HandlePickup;
        // Subscribe to the UITextDestroyed event
        UIPopUp.UITextDestroyed += HandleTextDestroyed;
    }

    void HandlePickup(PickupLogic pickup)
    {
        Debug.Log("Pickup event received for: " + pickup.name);
        addPickupText(pickup.name);
    }

    void addPickupText(string pickupName)
    {
        GameObject textObject = new GameObject("PickUpText");
        textObject.transform.SetParent(this.transform);

        RectTransform rectTransform = textObject.AddComponent<RectTransform>();
        TextMeshProUGUI tmp = textObject.AddComponent<TextMeshProUGUI>();
        UIPopUp uiPopUp = textObject.AddComponent<UIPopUp>();

        uiPopUp.fadeDuration = fadeDuration;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.text = pickupPrefix + pickupName;
        rectTransform.localPosition = Vector3.zero + origin;
        numberOfPickups++;
        // Adjust position based on number of pickups
        rectTransform.localPosition += new Vector3(0, (numberOfPickups - 1) * offset, 0);
    }

    void HandleTextDestroyed(PickupLogic pickup)
    {
        numberOfPickups--;
    }
}
