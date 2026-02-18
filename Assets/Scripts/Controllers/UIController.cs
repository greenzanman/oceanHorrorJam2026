using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }

    // Events for the Carousel to listen to
    public static Action OnNextItem; 
    public static Action OnPrevItem;
    
    // Existing events
    public static Action onInteractInput;
    public static Action onMenu;

    private PlayerInput playerInput;
    private InputAction strokeAction; // Left Trigger
    private InputAction fireAction;   // Right Trigger
    private InputAction menuAction;   // Escape/Start

    void Awake()
    {
        Instance = this;
        playerInput = GetComponent<PlayerInput>();
        
        // precise names depend on your Input Action Asset
        strokeAction = playerInput.actions["Stroke"]; 
        fireAction = playerInput.actions["Fire"];
        menuAction = playerInput.actions["Menu"];
    }

    void OnEnable()
    {
        strokeAction.performed += HandleStroke;
        fireAction.performed += HandleFire;
        menuAction.performed += HandleMenu;
    }

    void OnDisable()
    {
        strokeAction.performed -= HandleStroke;
        fireAction.performed -= HandleFire;
        menuAction.performed -= HandleMenu;
    }

    private void HandleStroke(InputAction.CallbackContext context)
    {
        if (IsInventoryOpen())
        {
            // NAVIGATION: Previous Item
            OnPrevItem?.Invoke();
        }
        else
        {
            // GAMEPLAY: Normal Stroke Logic handled by other scripts
            // (If other scripts listen to .performed, they will still fire unless you disable them)
        }
    }

    private void HandleFire(InputAction.CallbackContext context)
    {
        if (IsInventoryOpen())
        {
            // NAVIGATION: Next Item
            OnNextItem?.Invoke();
        }
        else
        {
            // GAMEPLAY: Normal Fire Logic
        }
    }

    private void HandleMenu(InputAction.CallbackContext context)
    {
        // Always toggle menu/inventory on Escape/Start
        // If inventory is open, this will close it (handled by UIPanelManager usually)
        if (IsInventoryOpen())
        {
             UIPanelManager.Instance.ToggleInventory();
        }
        else
        {
             UIPanelManager.Instance.ToggleMenu();
        }
    }

    // Helper to check state
    private bool IsInventoryOpen()
    {
        return UIPanelManager.Instance.IsCurrentPanel(PanelType.InventoryPanel);
    }
}