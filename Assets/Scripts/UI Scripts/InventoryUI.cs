using TMPro;
using UnityEngine;
using UnityEngine.InputSystem; // Needed for detecting Gamepad

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private InventoryModel model;
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI controlsHintText; // <--- Drag your hint text here

    private PlayerInput playerInput; // Reference to player input

    void Awake()
    {
        // Find PlayerInput (Assuming it's on the player character)
        playerInput = FindObjectOfType<PlayerInput>();
    }

    void OnEnable()
    {
        InventoryModel.Updated += RefreshUI;
        
        // Listen for control scheme changes (Keyboard -> Gamepad switching)
        if (playerInput != null)
        {
            playerInput.onControlsChanged += UpdateControlHints;
            UpdateControlHints(playerInput); // Set initial text
        }

        RefreshUI();
    }

    void OnDisable()
    {
        InventoryModel.Updated -= RefreshUI;
        if (playerInput != null)
        {
            playerInput.onControlsChanged -= UpdateControlHints;
        }
    }

    // Automatically called when you plug in a controller or touch the keyboard
    void UpdateControlHints(PlayerInput input)
    {
        if (controlsHintText == null) return;

        if (input.currentControlScheme == "Gamepad")
        {
            // Gamepad Prompts
            controlsHintText.text = "LT: Prev  |  RT: Next  |  Start: Close";
        }
        else
        {
            // Keyboard Prompts
            controlsHintText.text = "Space: Prev  |  LClick: Next  |  F: Close";
        }
    }

    void RefreshUI()
    {
        if (model == null) return;
        if (nameText) nameText.text = model.GetName();
        if (descriptionText) descriptionText.text = model.GetDescription();
    }
}