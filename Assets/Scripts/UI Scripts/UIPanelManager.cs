using System;
using System.Collections.Generic;
using UnityEngine;

public enum PanelType
{
    DefaultPanel,
    InventoryPanel,
    MenuPanel,
    SettingsPanel
    // DescriptionPanel removed as it is now part of InventoryPanel
}

public class UIPanelManager : MonoBehaviour
{
    public static UIPanelManager Instance { get; private set; }

    [System.Serializable]
    public struct PanelEntry
    {
        public PanelType type;
        public GameObject panelObj;
    }

    [Header("Setup")]
    [SerializeField] private List<PanelEntry> panelEntries;
    [SerializeField] private PanelType defaultPanelType = PanelType.DefaultPanel;

    private Dictionary<PanelType, GameObject> panelLookup = new Dictionary<PanelType, GameObject>();
    private GameObject currentPanel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Initialize Lookup
        foreach (var entry in panelEntries)
        {
            if (!panelLookup.ContainsKey(entry.type))
            {
                panelLookup.Add(entry.type, entry.panelObj);
                entry.panelObj.SetActive(false); // Ensure all start hidden
            }
        }
    }

    private void Start()
    {
        // 1. Show the default gameplay UI (usually the HUD/Crosshair)
        ShowPanel(defaultPanelType);
    }

    public void ShowPanel(PanelType panelType)
    {
        // Hide previous
        if (currentPanel != null) currentPanel.SetActive(false);

        // Show new
        if (panelLookup.TryGetValue(panelType, out GameObject panelObj))
        {
            panelObj.SetActive(true);
            currentPanel = panelObj;
        }
        else
        {
            Debug.LogWarning($"Panel {panelType} not found in lookup.");
        }
    }

    public void HideCurrent()
    {
        ShowPanel(defaultPanelType);
    }

    public bool IsCurrentPanel(PanelType panelType)
    {
        if (currentPanel == null) return false;
        
        if (panelLookup.TryGetValue(panelType, out GameObject panelObj))
        {
            return currentPanel == panelObj;
        }
        return false;
    }

    public void TogglePanel(PanelType panelType)
    {
        bool isOpen = IsCurrentPanel(panelType);

        if (!isOpen)
        {
            ShowPanel(panelType);
            Time.timeScale = 0f; // Pause game
            Cursor.lockState = CursorLockMode.None; // Optional: Show mouse if needed
            Cursor.visible = true;
        }
        else
        {
            HideCurrent();
            Time.timeScale = 1f; // Unpause game
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // --- SHORTCUTS ---

    public void ToggleInventory()
    {
        TogglePanel(PanelType.InventoryPanel);
    }

    public void ToggleMenu()
    {
        TogglePanel(PanelType.MenuPanel);
    }

    // REMOVED: ToggleDescription (No longer its own screen)
    // REMOVED: HandleMoveInput (Carousel now listens to UIController events directly)
}