using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Carousel : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float rotationAngle = 15f;
    [SerializeField] private float spacing = 2f;
    [SerializeField] private float moveDuration = 0.3f;

    public static Action<string, string, string> onItemChanged;

    private List<Transform> items = new List<Transform>();
    private Queue<Pickup> pendingPickups = new Queue<Pickup>();
    private int currentItem = 0;
    private bool isMoving = false;

    void Awake()
    {
        Pickup.OnInteract += HandlePickup;
        
        // Navigation events from UIController (LT/RT)
        UIController.OnNextItem += () => StartNavigation(1);
        UIController.OnPrevItem += () => StartNavigation(-1);
    }

    void OnDestroy()
    {
        Pickup.OnInteract -= HandlePickup;
    }

    void Update()
    {
        RotateItem(); // Restored logic
    }

    // Restored your original rotation logic using unscaled time
    void RotateItem()
    {
        if (items.Count == 0) return;
        for (int i = 0; i < items.Count; i++)
        {
            Vector3 currentEuler = items[i].localEulerAngles;

            // Only rotate the item if it is in the center
            if (Mathf.Abs(items[i].localPosition.x) < spacing / 2)
            {
                // Use unscaledTime so it works while the game is paused
                float targetYRotation = rotationAngle * Mathf.Sin(Time.unscaledTime * rotationSpeed);
                items[i].localEulerAngles = new Vector3(currentEuler.x, targetYRotation, currentEuler.z);
            }
            else
            {
                // Non-center items return to 0 smoothly
                float newY = Mathf.LerpAngle(currentEuler.y, 0f, Time.unscaledDeltaTime * rotationSpeed);
                items[i].localEulerAngles = new Vector3(currentEuler.x, newY, currentEuler.z);
            }
        }
    }

    void StartNavigation(int direction)
    {
        if (isMoving || items.Count <= 1) return;

        // direction: 1 for Next, -1 for Previous
        if (direction > 0)
        {
            StartCoroutine(MoveCarousel(-1));
            currentItem = (currentItem + 1) % items.Count;
        }
        else
        {
            StartCoroutine(MoveCarousel(1));
            currentItem = (currentItem - 1 + items.Count) % items.Count;
        }
    }

    IEnumerator MoveCarousel(int direction)
    {
        isMoving = true;

        Vector3[] startPositions = new Vector3[items.Count];
        Vector3[] endPositions = new Vector3[items.Count];

        for (int i = 0; i < items.Count; i++)
        {
            startPositions[i] = items[i].localPosition;
            endPositions[i] = startPositions[i] + new Vector3(direction * spacing, 0, 0);
        }

        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled for pause menu
            float t = Mathf.Clamp01(elapsed / moveDuration);
            for (int i = 0; i < items.Count; i++)
            {
                items[i].localPosition = Vector3.Lerp(startPositions[i], endPositions[i], t);
            }
            yield return null;
        }

        for (int i = 0; i < items.Count; i++) items[i].localPosition = endPositions[i];

        // Wrap-around logic
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].localPosition.x > spacing * (items.Count / 2f))
                items[i].localPosition -= new Vector3(spacing * items.Count, 0, 0);
            else if (items[i].localPosition.x < -spacing * (items.Count / 2f))
                items[i].localPosition += new Vector3(spacing * items.Count, 0, 0);
        }

        NotifyUI();
        isMoving = false;
        
        while (pendingPickups.Count > 0) AddPickupNow(pendingPickups.Dequeue());
    }

    void HandlePickup(Pickup pickup)
    {
        if (isMoving) { pendingPickups.Enqueue(pickup); return; }
        AddPickupNow(pickup);
    }

    void AddPickupNow(Pickup pickup)
    {
        Transform t = pickup.transform;
        t.SetParent(transform);
        
        // Ensure visibility and layer
        t.gameObject.SetActive(true);
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;

        items.Add(t);
        RebuildLayout();
        
        currentItem = items.Count - 1;
        NotifyUI();
    }

    void RebuildLayout()
    {
        for (int i = 0; i < items.Count; i++)
        {
            items[i].localPosition = new Vector3(i * spacing, 0, 0);
        }
    }

    void NotifyUI()
    {
        if (items.Count == 0) return;
        Pickup p = items[currentItem].GetComponent<Pickup>();
        if (p != null)
        {
            onItemChanged?.Invoke(items[currentItem].name, p.GetShortDescription(), p.GetLongDescription());
        }
    }
}