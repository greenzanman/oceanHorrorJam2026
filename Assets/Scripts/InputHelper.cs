using UnityEngine;
using UnityEngine.InputSystem;

public static class InputHelper
{
    // The cache is now static, meaning it's shared across your whole game!
    private static PlayerInput cachedPlayerInput;

    // The method is now public and static, so anyone can call it.
    public static string GetBinding(string actionName)
    {
        // 1. Try to find the PlayerInput if we haven't already
        if (cachedPlayerInput == null)
        {
            cachedPlayerInput = Object.FindAnyObjectByType<PlayerInput>();
        }

        // 2. Safe null checks 
        if (cachedPlayerInput == null || cachedPlayerInput.actions == null)
        {
            return "?";  //  safe fallback
        }

        // 3. Find the action and return the binding string
        var action = cachedPlayerInput.actions.FindAction(actionName);
        return action != null ? action.GetBindingDisplayString() : "?";
    }


    // i think can later add here like rebinding functionality handly
}