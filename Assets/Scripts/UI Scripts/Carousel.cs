using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/*
Manages the carousel of pickups that the player can interact with. 
The carousel rotates the center item back and forth, and allows the player to scroll through items using horizontal input. 
When a new pickup is added to the carousel, it is added to the end of the list and the layout is rebuilt. 
The carousel also handles wrapping around when scrolling past the first or last item. 
The carousel listens for pickup events and adds new pickups to the carousel when they are picked up in the world. 
The carousel also exposes an event when the current item changes, so that the UI can update accordingly.
*/
public class Carousel : MonoBehaviour
{
    // TODO: Decouple initial rotation of pickup items from carousel logic
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float rotationAngle = 15f;
    [SerializeField] private float spacing = 2f;
    [SerializeField] private float moveDuration = 0.3f;

    public static Action<string, string, string> onItemChanged;

    private List<Transform> items;
    private Queue<Pickup> pendingPickups = new();

    private int currentItem = 0;

    private bool isMoving = false;

    void Awake()
    {
        Pickup.OnInteract += HandlePickup;
        UIPanelManager.onMoveInput += HandleMoveInput; // Ensure we start with no movement input

        items = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            items.Add(transform.GetChild(i));
            if (items[i].GetComponent<Pickup>() == null)
            {
                Debug.LogError("Child transform is missing pickup logic: " + i);
            }
        }

        // Initial horizontal layout
        for (int i = 0; i < items.Count; i++)
        {
            items[i].localPosition = new Vector3(i * spacing, 0, 0);
        }
        foreach (Transform item in items)
        {
            Debug.Log("Item in carousel at start: " + item.name);
        }
        
    }

    void Start()
    {
        if (items.Count == 0) return;
        Debug.Log("Item:", items[currentItem]);
        Pickup pickupComponent = items[currentItem].GetComponent<Pickup>();
        bool isNull = pickupComponent == null;
        if (items.Count > 0)
        {
            Pickup pickup = items[currentItem].GetComponent<Pickup>();
            if (pickup != null)
            {
                string shortDescription = pickup.GetShortDescription();
                string longDescription = pickup.GetLongDescription();
                string name = items[currentItem].name;
                onItemChanged?.Invoke(name, shortDescription, longDescription);
            }
        }
    }

    void HandleMoveInput(Vector2 input)
    {
        if (isMoving) return;
        if(items.Count <= 1) return;
        // Do nothing on move input, just prevent further movement handling
        Debug.Log("Received move input: " + input);
        Vector2 moveInput = input.normalized;
        if (moveInput.x > 0.5f)
        {
            StartCoroutine(MoveCarousel(-1));
            currentItem = (currentItem + 1) % items.Count;
        }
        else if (moveInput.x < -0.5f)
        {
            StartCoroutine(MoveCarousel(1));
            currentItem = (currentItem - 1 + items.Count) % items.Count;
        }
    }

    void Update()
    {
        RotateItem();
    }

    void RotateItem()
    {
        if (items.Count == 0) return;
        for (int i = 0; i < items.Count; i++)
        {
            Vector3 currentEuler = items[i].localEulerAngles;

            if (Mathf.Abs(items[i].localPosition.x) < spacing / 2)
            {
                // Smooth back-and-forth using sine
                float targetYRotation = rotationAngle * Mathf.Sin(Time.unscaledTime * rotationSpeed);
                items[i].localEulerAngles = new Vector3(currentEuler.x, targetYRotation, currentEuler.z);
            }
            else
            {
                // Non-center items go back to 0 Y smoothly
                float newY = Mathf.LerpAngle(currentEuler.y, 0f, Time.unscaledDeltaTime * rotationSpeed);
                items[i].localEulerAngles = new Vector3(currentEuler.x, newY, currentEuler.z);
            }
        }
    }

    IEnumerator MoveCarousel(int direction) // direction = -1 (right) / 1 (left)
    {
        if (items.Count == 0)
            yield break;
        isMoving = true;

        Vector3[] startPositions = new Vector3[items.Count];
        Vector3[] endPositions = new Vector3[items.Count];

        for (int i = 0; i < items.Count; i++)
        {
            startPositions[i] = items[i].localPosition;
            endPositions[i] = startPositions[i] + new Vector3(direction * spacing, 0, 0);
        }

        // Tween over time
        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);

            for (int i = 0; i < items.Count; i++)
            {
                items[i].localPosition = Vector3.Lerp(startPositions[i], endPositions[i], t);
            }

            yield return null;
        }

        // Apply final positions exactly
        for (int i = 0; i < items.Count; i++)
        {
            items[i].localPosition = endPositions[i];
        }

        // Wrap-around **after tweening**
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].localPosition.x > spacing * (items.Count / 2))
            {
                items[i].localPosition = new Vector3(
                    items[i].localPosition.x - spacing * items.Count,
                    items[i].localPosition.y,
                    items[i].localPosition.z
                );
            }
            else if (items[i].localPosition.x < -spacing * (items.Count / 2))
            {
                items[i].localPosition = new Vector3(
                    items[i].localPosition.x + spacing * items.Count,
                    items[i].localPosition.y,
                    items[i].localPosition.z
                );
            }
        }
        if (items.Count > 0)
        {
            Pickup pickup = items[currentItem].GetComponent<Pickup>();
            if (pickup != null)
            {
                string shortDescription = pickup.GetShortDescription();
                string longDescription = pickup.GetLongDescription();
                string name = items[currentItem].name;
                onItemChanged?.Invoke(name, shortDescription, longDescription);
            }
        }

        isMoving = false;
        while (pendingPickups.Count > 0)
        {
            AddPickupNow(pendingPickups.Dequeue());
        }
    }

    void HandlePickup(Pickup pickup)
    {
        if (isMoving)
        {
            pendingPickups.Enqueue(pickup);
            return;
        }

        AddPickupNow(pickup);
    }

    void AddPickupNow(Pickup pickup)
    {
        Transform t = pickup.transform;
        t.SetParent(transform);
        items.Add(t);

        RebuildLayout();
        currentItem = items.Count - 1;
        string shortDescription = pickup.GetShortDescription();
        string longDescription = pickup.GetLongDescription();
        string name = items[currentItem].name;
        onItemChanged?.Invoke(name, shortDescription, longDescription);
    }

    void RebuildLayout()
    {
        for (int i = 0; i < items.Count; i++)
        {
            items[i].localPosition = new Vector3(i * spacing, 0, 0);
        }
    }
}
