using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// attach to any script that opens a UI panel
public class UIInteractable : MonoBehaviour, Actionable
{
    [Header("Interaction Settings")]
    [SerializeField] private PanelType panelToShow;

    [Header("Audio Settings")]
    [SerializeField] private FMODUnity.EventReference interactSound;
    
    [Header("Visuals")]
    [SerializeField] protected GameObject interactionPromptCanvas; // should be world space
    [SerializeField] protected MeshRenderer meshRenderer;
    [SerializeField] private string interactionVerb = "Access";
    
    private MaterialPropertyBlock propBlock;
    private TextMeshProUGUI promptText;

    void Awake() 
    {
        propBlock = new MaterialPropertyBlock();
        
        // Cache the text component if prompt is assigned
        if (interactionPromptCanvas != null)
        {
            promptText = interactionPromptCanvas.GetComponentInChildren<TextMeshProUGUI>();
            interactionPromptCanvas.SetActive(false); // Ensure it starts hidden
        }
    }

    public virtual void SetFocus(bool focused)
    {
        if (interactionPromptCanvas)
        {
            interactionPromptCanvas.SetActive(focused);

            // Only update text when we are turning it ON
            if (focused && promptText != null)
            {
                UpdatePromptText();
            }
        }
        
        // Shader highlight logic
        if (meshRenderer != null)
        {
            meshRenderer.GetPropertyBlock(propBlock);
            propBlock.SetFloat("_HighlightLevel", focused ? 1.0f : 0.0f); 
            meshRenderer.SetPropertyBlock(propBlock);
        }
    }

    private void UpdatePromptText()
    {
        string key = InputHelper.GetBinding("Interact");
        promptText.text = $"{interactionVerb} [{key}]";
        
    }

    /// show the ui panel associated with this interactable
    /// play the interact sound
    public virtual void Fire()
    {
        // 1. Play the sound first
        if (!interactSound.IsNull)
        {
            FMODUnity.RuntimeManager.PlayOneShot(interactSound, transform.position);
        }
        else
        {
            Debug.LogWarning($"No interaction sound assigned on {gameObject.name}!");
        }

        // 2. Then do the UI logic
        if (panelToShow == PanelType.InventoryPanel)
        {
            UIPanelManager.Instance.ToggleInventory();
        }
        else
        {
            UIPanelManager.Instance.ShowPanel(panelToShow);
        }
    }

    public virtual string GetInteractPrompt()
    {
        string keyName = InputHelper.GetBinding("Interact");
        
        return $"{interactionVerb} [{keyName}]";
    }
}