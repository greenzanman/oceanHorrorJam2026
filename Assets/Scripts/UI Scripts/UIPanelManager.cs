using System;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;

public enum PanelType
{
    // these will  be the keys for the panel lookup dictionary.
    DefaultPanel,
    InventoryPanel,
    DescriptionPanel,
    MenuPanel,
    SettingsPanel
}

/*
Manages the display of UI panels. Panels must be tagged with "UIPanel" and added to the panels list in the inspector.
Panels can specify which panels are valid next panels using the NextPanels component. If a panel does
not have a NextPanels component, it can be displayed from any panel. If a panel has a NextPanels component, it can only be displayed from the panels specified in the NextPanels list.
*/
public class UIPanelManager : MonoBehaviour
{
    public static UIPanelManager Instance { get; private set; }

    [System.Serializable]
    public struct PanelEntry
    {
        public PanelType type;      // The Key (Enum)
        public GameObject panelObj; // The Value (Prefab/Object)
    }

    public static Action<Vector2> onMoveInput;

    [SerializeField] private List<PanelEntry> panelEntries;
    private Dictionary<PanelType, GameObject> panelLookup;

    [Tooltip("the panel to show when others are closed")]
    [SerializeField] private PanelType defaultPanelType = PanelType.DefaultPanel;

    private GameObject currentPanel;

    private void Awake()
    {
        // Debug.Log("Panel manager awake.");
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        panelLookup = new Dictionary<PanelType, GameObject>();
        foreach (var entry in panelEntries)
        {
            if (entry.panelObj != null && !panelLookup.ContainsKey(entry.type))
            {
                panelLookup.Add(entry.type, entry.panelObj);
                // Ensure panels start hidden
                entry.panelObj.SetActive(false); 
            }
        }
        // If a current panel is set with defaultPanel, display it.
        ShowPanel(defaultPanelType);
    }

    void Start()
    {
        //UIController.onInventory += ToggleInventory;
        UIController.onMenu += ToggleMenu;
        UIController.onDescription += ToggleDescription;
        UIController.onMoveInput += HandleMoveInput;
    }

    // show panel and update ref to currentPanel gameobject 
    public void ShowPanel(PanelType panelType)
    {
        if (panelLookup.TryGetValue(panelType, out GameObject panelToOpen))
        {
            // abort if already on the panel to open
            if (currentPanel == panelToOpen) return;

            // Close current, open new
            if (currentPanel != null) currentPanel.SetActive(false);
            
            panelToOpen.SetActive(true);
            Debug.Log($"UIPanelManager: Switched from panel: {currentPanel} to panel: {panelToOpen}");
            currentPanel = panelToOpen;
        }
        else
        {
            Debug.LogError($"UIPanelManager: No panel registered for panelType: {panelType}");
        }
    }

    public void HideCurrent()
    {        
        ShowPanel(defaultPanelType);
    }

    public bool IsCurrentPanel(PanelType panelType)
    {
        // check if we have a current panel
        if (currentPanel == null) {
            Debug.LogWarning("No current panel is active.");
            return false;
        }

        // try to return the gameobj for this panel type
        if (panelLookup.TryGetValue(panelType, out GameObject panelObj))
        {
            return currentPanel == panelObj;
        }

        Debug.LogError($"BAD: No panel registered for {panelType}. Check UIPanelManager inspector.");
        return false;
    }

    public void TogglePanel(PanelType panelType)
    {
        bool isOpen = IsCurrentPanel(panelType);

        if (!isOpen)
        {
            ShowPanel(panelType);
            Time.timeScale = 0f; // pause gameplay
        }
        else
        {
            HideCurrent(); // go back to default ui
            Time.timeScale = 1f; // unpause gameplay
        }
    }


    // shortcut toggles for individual panels
    public void ToggleInventory()
    {
        TogglePanel(PanelType.InventoryPanel);
    }


    // NOTE: toggling description is different because its WITHIN inventory panel
    // - so dont pause gameplay
    public void ToggleDescription()
    {
        bool isDescOpen = IsCurrentPanel(PanelType.DescriptionPanel);

        if (!isDescOpen)
        {
            ShowPanel(PanelType.DescriptionPanel);
            Time.timeScale = 0f; 
        }
        else
        {
            // CLOSE IT -> GO BACK TO INVENTORY
            ShowPanel(PanelType.InventoryPanel);
            Time.timeScale = 0f; 
        }
    }

    public void ToggleMenu()
    {
        TogglePanel(PanelType.MenuPanel);
    }

    void HandleMoveInput(Vector2 input)
    {
        // Debug.Log("Received move input: " + input);
        if (!IsCurrentPanel(PanelType.InventoryPanel))
        {
            // IF NOT IN INVENTORY Do nothing on move input, just prevent further movement handling
            
            return;
        }
        onMoveInput?.Invoke(input);
    }

}
