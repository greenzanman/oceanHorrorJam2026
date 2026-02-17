using System;
using UnityEngine;
using UnityEngine.InputSystem; // Required for InputValue

public class UIController : MonoBehaviour
{
    // static events that the UIPanelManager script can subscribe to
    public static Action onInteractInput;
    public static Action<Vector2> onMoveInput;
    public static Action onMenu;
    public static Action onDescription;
    
    void OnInteract()
    {
        onInteractInput?.Invoke();
    }

    void OnMove(InputValue value)
    {
        Vector2 moveInput = value.Get<Vector2>();
        onMoveInput?.Invoke(moveInput);
    }

    void OnMenu()
    {
        onMenu?.Invoke();
    }


    void OnStroke()
    {
        bool isInventoryOpen = UIPanelManager.Instance.IsCurrentPanel(PanelType.InventoryPanel);
        bool isDescriptionOpen = UIPanelManager.Instance.IsCurrentPanel(PanelType.DescriptionPanel);
        
        if (isInventoryOpen || isDescriptionOpen)
        {
             UIPanelManager.Instance.ToggleInventory();
        }
        else
        {
            // HERE: must be gameplay input, which is handled by stroke controller (not here)
        }
    }

    // put shit in here for debugging 
    void Update()
    {
        // if(Input.GetKeyDown(KeyCode.E))
        // {
        //     onDescription?.Invoke();
        // }
    }
}