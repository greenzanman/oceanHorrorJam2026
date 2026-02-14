using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/*
This script listens for pickup events and displays a temporary text on the screen indicating what item was picked up.
It also listens for the UITextDestroyed event to keep track of how many 
pickup texts are currently displayed and adjust the position of new texts accordingly.
*/
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
        Pickup.OnInteract += AddPickupText;
        // Subscribe to the UITextDestroyed event
        UIPopUp.UITextDestroyed += HandleTextDestroyed;
    }

    void AddPickupText(Pickup pickup)
    {
        Debug.Log("Pickup event received for: " + pickup.name);
        string pickupName = pickup.name;
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

    void HandleTextDestroyed(Pickup pickup)
    {
        numberOfPickups--;
    }
}


/*
This script is attached to the temporary text objects created by PickUpUI.
It automatically destroys itself after a certain duration and notifies the PickUpUI to update 
the count of active pickup texts.
*/
public class UIPopUp : MonoBehaviour
{
    public static event Action<Pickup> UITextDestroyed;
    [SerializeField] public float fadeDuration = 2f;

    private Coroutine destroyCoroutine;
    
    // Start is called before the first frame update
    void Start()
    {
        // Start destroy coroutine
        destroyCoroutine = StartCoroutine(DestroyAfterTime(fadeDuration)); 
    }

    IEnumerator DestroyAfterTime(float duration)
    {
        yield return new WaitForSeconds(duration);
        Destroy(this.gameObject);
        UITextDestroyed?.Invoke(null);
    }
}
