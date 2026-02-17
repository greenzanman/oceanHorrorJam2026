using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// attach to any script that opens a UI panel
public class UIInteractable : MonoBehaviour, Actionable
{
    [Header("Interaction Settings")]
    [SerializeField] private PanelType panelToShow;
    
    [Header("Visuals")]
    [SerializeField] private GameObject interactionPromptCanvas; // should be world space
    [SerializeField] private MeshRenderer meshRenderer;
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

    public void SetFocus(bool focused)
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
        // 1. Get the player's input instance (assuming single player)
        var playerInput = PlayerInput.all.Count > 0 ? PlayerInput.all[0] : null;

        if (playerInput != null)
        {
            // 2. Find the 'Interact' action
            InputAction action = playerInput.actions["Interact"];
            
            // 3. Get the binding string (e.g., "E", "A", "Cross")
            string keyName = action.GetBindingDisplayString();

            // --- DEBUGGING START ---
            // Remove these lines once it is working
            Debug.Log($"[UIInteractable] Control Scheme: {playerInput.currentControlScheme}");
            Debug.Log($"[UIInteractable] Key Found: '{keyName}'");
            // --- DEBUGGING END ---

            // 4. Set the text
            promptText.text = $"{interactionVerb} [{keyName}]";
        }
        else
        {
            // Fallback if no PlayerInput found
            promptText.text = $"{interactionVerb} [Interact]";
        }
    }

    /// show the ui panel associated with this interactable
    public void Fire()
    {
        if (panelToShow == PanelType.InventoryPanel)
        {
            UIPanelManager.Instance.ToggleInventory();
        }
            
        else
        {
            UIPanelManager.Instance.ShowPanel(panelToShow);
        }
            
    }
}