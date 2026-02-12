using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/*
    * The UIController is responsible for handling all UI input and events. It listens for input from the player and fires events that other UI elements can subscribe to.
*/
public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }
    public static Action onInteractInput;
    public static Action<Vector2> onMoveInput;
    //public static Action onInventory;
    public static Action onMenu;
    public static Action onDescription;
    public static Action<Pickup> onPickup;
    [SerializeField] private UIPanelManager panelManager;
    [SerializeField] private Carousel carousel;
    public GameObject inventoryPanel;
    private PlayerInput playerInput;
    private InputAction inventoryAction;
    private InputAction interactAction;

    private InputAction menuAction;
    
    private InputAction moveAction;

    private bool isDescriptionOpen = false;

    void Start()
    {
        if (inventoryPanel == null)
        {
            Debug.LogError("Inventory Panel is not assigned in the inspector.");
        }

        playerInput = GetComponent<PlayerInput>();
        //inventoryAction = playerInput.actions["Inventory"];
        interactAction = playerInput.actions["Interact"];
        menuAction = playerInput.actions["Menu"];
        moveAction = playerInput.actions["Move"];

        //inventoryAction.performed += HandleInventory;
        interactAction.performed += HandleInteractInput;
        menuAction.performed += HandleMenu;
        moveAction.performed += HandleMoveInput;
    }   

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            HandleDescription();
        }
    }

    // void HandleInventory(InputAction.CallbackContext context)
    // {
    //     onInventory?.Invoke();
    // }

    
    void HandleInteractInput(InputAction.CallbackContext context)
    {
        onInteractInput?.Invoke();
    }

    void HandleMenu(InputAction.CallbackContext context)
    {
        onMenu?.Invoke(); 
    }

    void HandleMoveInput(InputAction.CallbackContext context)
    {
        onMoveInput?.Invoke(context.ReadValue<Vector2>());
    }

    void HandleDescription()
    {
        onDescription?.Invoke();
    }
}
