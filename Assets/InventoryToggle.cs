using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryToggle : MonoBehaviour
{
    public GameObject inventoryPanel;
    private PlayerInput playerInput;
    private InputAction inventoryAction;

    void Start()
    {
        if (inventoryPanel == null)
        {
            Debug.LogError("Inventory Panel is not assigned in the inspector.");
        }

        playerInput = GetComponent<PlayerInput>();
        inventoryAction = playerInput.actions["Inventory"];
    }

    void Update()
    {
        if (inventoryAction.WasPressedThisFrame())
        {
            ToggleInventory();
        }
    }

    void ToggleInventory()
    {
        bool isActive = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(isActive);

        if (isActive)
        {
            // Pause the game
            Time.timeScale = 0f;
        }
        else
        {
            // Resume the game
            Time.timeScale = 1f;
        }
    }
}
