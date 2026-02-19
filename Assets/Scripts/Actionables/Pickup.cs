using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
using TMPro;

public class Pickup : MonoBehaviour, Actionable
{
    [Header("UI Settings")]
    [SerializeField] private GameObject interactionPrompt; // world space canvas with text child
    [SerializeField] private bool showBtnPrompt = false;

    [Header("Visuals")]
    [SerializeField] private MeshRenderer meshRenderer;
    private MaterialPropertyBlock propBlock;


    [Header("Pickup Settings")]
    [SerializeField] private string shortDescription = "Put your description here.";
    [SerializeField] private string longDescription = "Put your description here.";
    
    // Declare the event with the pickup as parameter
    public static event Action<Pickup> OnInteract;

    void Awake()
    {
        propBlock = new MaterialPropertyBlock();
        if(interactionPrompt) interactionPrompt.SetActive(false);
    }


    public void Fire()
    {
        Transform originalParent = transform.parent;
        
        OnInteract?.Invoke(this);

        // Deactivate only if parent unchanged (not picked up)
        // Stay active if picked up by Carousel (parent changed)
        if (transform.parent == originalParent)
        {
            gameObject.SetActive(false);
        }
    }

    public void SetFocus(bool focused)
    {
        if (interactionPrompt)
        {
            interactionPrompt.SetActive(focused && showBtnPrompt);
            
            if (focused && showBtnPrompt)
            {
                // Get the text component in your world-space prompt
                var txt = interactionPrompt.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null)
                {
                    // Finds the binding for "Interact" and turns it into a string like "E" or "A"
                    var action = PlayerInput.all[0].actions["Interact"];
                    txt.text = $"PICKUP [{action.GetBindingDisplayString()}]";
                }
            }
        }

        // Shader logic remains the same
        if (meshRenderer != null)
        {
            meshRenderer.GetPropertyBlock(propBlock);
            propBlock.SetFloat("_HighlightLevel", focused ? 1.0f : 0.0f);
            meshRenderer.SetPropertyBlock(propBlock);
        }
    }

    public string GetShortDescription()
    {
        return shortDescription;
    }

    public string GetLongDescription()
    {
        return longDescription;
    }
    

    public string GetInteractPrompt()
    {
        // Fetches the current key/button bound to "Interact"
        string key = GetBinding("Interact");
        return $"Press [{key}] to pick up {gameObject.name}";
    }

    // Helper method for dynamic keys
    private string GetBinding(string actionName)
    {
        var playerInput = FindObjectOfType<UnityEngine.InputSystem.PlayerInput>();
        return playerInput.actions[actionName].GetBindingDisplayString();
    }
}
