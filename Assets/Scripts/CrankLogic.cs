using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;   

public class CrankLogic : MonoBehaviour, Actionable
{
    [SerializeField] private Inventory inventory; 
    [SerializeField] private float rotationSpeed = 1f; // progress per second
    [SerializeField] private float maxCrankRotation = 360f;
    [SerializeField] private float maxDoorHeight = 2f;

    private GameObject crank;
    private GameObject panel;
    private GameObject door;

    private bool isFocused;
    private UnityEngine.InputSystem.PlayerInput playerInput;

    private bool isCrankVisible = false;

    private bool isDoorOpen = false;

    private float progress = 0f; // 0 = crank fully down, door closed; 1 = crank fully turned, door fully open

    private Quaternion crankStartRotation;
    private Quaternion crankEndRotation;
    private Vector3 doorStartPosition;
    private Vector3 doorEndPosition;

    [Header("Visuals")]
    [SerializeField] private MeshRenderer meshRenderer;
    private MaterialPropertyBlock propBlock;

    [Header("UI")]
    [SerializeField] private GameObject interactionPromptCanvas;
    private TextMeshProUGUI promptText;

    void Awake()
    {
        propBlock = new MaterialPropertyBlock();

        crank = transform.Find("Crank").gameObject;
        panel = transform.Find("Panel").gameObject;
        door = transform.Find("Door").gameObject;

        crank.GetComponent<MeshRenderer>().enabled = false;

        Vector3 euler = crank.transform.localEulerAngles;
        euler.y = 0f; // treat current rotation as 0
        crank.transform.localRotation = Quaternion.Euler(euler);
        crankStartRotation = crank.transform.localRotation;
        crankEndRotation = crankStartRotation * Quaternion.Euler(0f, maxCrankRotation, 0f);


        doorStartPosition = door.transform.position;
        doorEndPosition = doorStartPosition + Vector3.up * maxDoorHeight;
    }

    void Start()
    {
        // assumes only one playerinput
        playerInput = FindObjectOfType<UnityEngine.InputSystem.PlayerInput>();
    }

    void Update()
    {
        // update ui text if focused
        if (isFocused && promptText != null)
        {
            promptText.text = GetInteractPrompt();
        }

        if (!isCrankVisible || isDoorOpen) return;

        bool isPressing = playerInput.actions["Interact"].IsPressed();

        // Update progress
        float delta = rotationSpeed * Time.deltaTime * (isPressing ? 1f : -1f);
        progress = Mathf.Clamp01(progress + delta);

        // Rotate crank absolutely around its local Y axis
        float targetAngle = maxCrankRotation * progress;
        crank.transform.localRotation = crankStartRotation * Quaternion.Euler(0f, targetAngle, 0f);
        Debug.Log("Crank rotation: " + crank.transform.localRotation.eulerAngles.y);
        // Interpolate door position
        door.transform.position = Vector3.Lerp(doorStartPosition, doorEndPosition, progress);
        if (!isDoorOpen && progress >= 1f)
        {
            isDoorOpen = true;
        }
    }

    public void SetFocus(bool focus)
    {
        isFocused = focus;
        if (interactionPromptCanvas != null)
        {
            interactionPromptCanvas.SetActive(focus);
            if (focus)
            {
                // Update the floating text immediately when looked at
                if (promptText == null) 
                    promptText = interactionPromptCanvas.GetComponentInChildren<TextMeshProUGUI>();
                promptText.text = GetInteractPrompt();
            }
        }
    }

    public void Fire()
    {
        if (inventory == null) {
            Debug.LogError("CrankLogic: Inventory reference is MISSING in the Inspector!");
            return;
        }

        GameObject obj = inventory.Contains("Crank", true);
        Debug.Log("CrankLogic: Checking inventory... Found: " + (obj != null ? obj.name : "NULL"));
        
        if (!isCrankVisible)
        {
            GameObject item = inventory.Contains("Crank", true);
            if (item != null)
            {
                isCrankVisible = true;
                transform.Find("Crank").GetComponent<MeshRenderer>().enabled = true; 
                inventory.RemoveItem(item);
            }
        }
    }

    private void UpdateHighlight()
    {
        if (meshRenderer != null)
        {
            meshRenderer.GetPropertyBlock(propBlock);
            propBlock.SetFloat("_HighlightLevel", isFocused ? 1.0f : 0.0f);
            meshRenderer.SetPropertyBlock(propBlock);
        }
    }

    public string GetInteractPrompt()
    {
        string key = playerInput.actions["Interact"].GetBindingDisplayString();

        if (!isCrankVisible) return $"PLACE CRANK [PRESS {key}]";
        if (progress < 1f) return $"OPEN DOOR [HOLD {key}]";
        
        return "OPENED."; // Hide if done
    }
}
