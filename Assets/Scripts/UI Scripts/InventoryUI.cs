using TMPro;
using UnityEngine;
using UnityEngine.InputSystem; 
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private InventoryModel model;
    
    [Header("Text References")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI controlsHintText; 

    [Header("Button References")]
    [SerializeField] private Button leftArrowButton;
    [SerializeField] private Button rightArrowButton;
    [SerializeField] private Button closeButton;

    private PlayerInput playerInput;

    void Awake()
    {
        // Find PlayerInput (Assuming it's on the player character)
        playerInput = FindObjectOfType<PlayerInput>();

        // Wire up the physical UI buttons to our methods
        if (leftArrowButton != null) leftArrowButton.onClick.AddListener(OnPreviousButtonClicked);
        if (rightArrowButton != null) rightArrowButton.onClick.AddListener(OnNextButtonClicked);
        if (closeButton != null) closeButton.onClick.AddListener(OnCloseButtonClicked);
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
            controlsHintText.text = "LT: Prev  |  RT: Next  \n\n  Start: Close";
        }
        else
        {
            // Keyboard Prompts
            controlsHintText.text = "Space: Prev  |  LClick: Next  \n\n  F: Close";
        }
    }

    void RefreshUI()
    {
        if (model == null) return;
        if (nameText) nameText.text = model.GetName();
        if (descriptionText) descriptionText.text = model.GetDescription();
    }

    // --- Button Click Actions ---

    private void OnPreviousButtonClicked()
    {
        // Trigger the exact same event your gamepad's Left Trigger uses
        UIController.OnPrevItem?.Invoke();
    }

    private void OnNextButtonClicked()
    {
        // Trigger the exact same event your gamepad's Right Trigger uses
        UIController.OnNextItem?.Invoke();
    }

    private void OnCloseButtonClicked()
    {
        // Tell the Panel Manager to close the inventory
        if (UIPanelManager.Instance != null)
        {
            UIPanelManager.Instance.ToggleInventory();
        }
    }
}