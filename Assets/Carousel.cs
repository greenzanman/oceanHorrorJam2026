using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Carousel : MonoBehaviour
{
    // TODO: Decouple initial rotation of pickup items from carousel logic
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float rotationAngle = 15f;
    [SerializeField] private float spacing = 2f;
    [SerializeField] private float moveDuration = 0.3f;

    public static Action<string, string> onItemChanged;

    private List<Transform> items;
    private Queue<PickupLogic> pendingPickups = new();

    private int currentItem = 0;

    private InputAction moveAction;
    private bool isMoving = false;

    void Awake()
    {
        PickupLogic.OnPickedUp += HandlePickup;
        moveAction = playerInput.actions["Move"];

        items = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            items.Add(transform.GetChild(i));
            if (items[i].GetComponent<PickupLogic>() == null)
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
        Debug.Log("Item:", items[currentItem]);
        bool isNNull = items[currentItem].GetComponent<PickupLogic>() == null;
        Debug.Log("Is Null:" + isNNull);
        Debug.Log("PickupLogic:", items[currentItem].GetComponent<PickupLogic>());
        string description = items[currentItem].GetComponent<PickupLogic>().GetDescription();
        string name = items[currentItem].name;
        onItemChanged?.Invoke(name, description);
    }

    void Update()
    {
        if (isMoving) return;

        Vector2 moveInput = moveAction.ReadValue<Vector2>();

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
        RotateItem();
    }

    void RotateItem()
    {
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
        string description = items[currentItem].GetComponent<PickupLogic>().GetDescription();
        string name = items[currentItem].name;
        onItemChanged?.Invoke(name, description);
        isMoving = false;
        while (pendingPickups.Count > 0)
        {
            AddPickupNow(pendingPickups.Dequeue());
        }
    }

    void HandlePickup(PickupLogic pickup)
    {
        if (isMoving)
        {
            pendingPickups.Enqueue(pickup);
            return;
        }

        AddPickupNow(pickup);
    }

    void AddPickupNow(PickupLogic pickup)
    {
        Transform t = pickup.transform;
        t.SetParent(transform);
        items.Add(t);

        RebuildLayout();
    }

    void RebuildLayout()
    {
        for (int i = 0; i < items.Count; i++)
        {
            items[i].localPosition = new Vector3(i * spacing, 0, 0);
        }
    }
}
