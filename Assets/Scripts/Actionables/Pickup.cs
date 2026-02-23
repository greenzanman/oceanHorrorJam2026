using System;
using UnityEngine;

// 1. Inherit from UIInteractable instead of MonoBehaviour, Actionable
public class Pickup : UIInteractable 
{
    [Header("Pickup Settings")]
    [TextArea(3, 10)]
    [SerializeField] private string shortDescription = "Put your description here.";

    [TextArea(5, 15)]
    [SerializeField] private string longDescription = "Put your description here.";
    [SerializeField] private bool showBtnPrompt = false;
    
    public static event Action<Pickup> OnInteract;

    public override void Fire()
    {
        Transform originalParent = transform.parent;
        
        OnInteract?.Invoke(this);

        if (transform.parent == originalParent)
        {
            gameObject.SetActive(false);
        }

        // 2. Call the base class to play your FMOD SFX and open your assigned UI Panel!
        base.Fire();
    }

    public override void SetFocus(bool focused)
    {
        // 3. We let the base class do the heavy lifting for the shader highlight...
        base.SetFocus(focused);

        // ...but we override the canvas visibility to respect your unique showBtnPrompt toggle
        if (interactionPromptCanvas != null)
        {
            interactionPromptCanvas.SetActive(focused && showBtnPrompt);
        }
    }

    public override string GetInteractPrompt()
    {
        // 4. Override to include the specific gameObject's name
        string key = InputHelper.GetBinding("Interact");
        return $"PICKUP {gameObject.name} [{key}] ";
    }

    public string GetShortDescription()
    {
        return shortDescription;
    }

    public string GetLongDescription()
    {
        return longDescription;
    }
}