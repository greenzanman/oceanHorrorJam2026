using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class Interactable : MonoBehaviour, Actionable
{
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private bool showBtnPrompt = true;
    [SerializeField] private MeshRenderer meshRenderer;
    
    private MaterialPropertyBlock propBlock;

    void Awake() => propBlock = new MaterialPropertyBlock();

    public void SetFocus(bool focused)
    {
        if(interactionPrompt) interactionPrompt.SetActive(focused && showBtnPrompt);
        if (meshRenderer != null)
        {
            meshRenderer.GetPropertyBlock(propBlock);
            propBlock.SetFloat("_HighlightLevel", focused ? 1.0f : 0.0f); 
            meshRenderer.SetPropertyBlock(propBlock);
        }
    }

    public void Fire()
    {
        Debug.Log("Interactable fired");
    }

    public string GetInteractPrompt()
    {
        string key = GetBinding("Interact");
        return $"Press [{key}] to Interact";
    }

    private string GetBinding(string actionName)
    {
        var playerInput = FindObjectOfType<UnityEngine.InputSystem.PlayerInput>();
        return playerInput.actions[actionName].GetBindingDisplayString();
    }
    
}
