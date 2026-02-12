using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/*
Manages the display of UI panels. Panels must be tagged with "UIPanel" and added to the panels list in the inspector.
Panels can specify which panels are valid next panels using the NextPanels component. If a panel does
not have a NextPanels component, it can be displayed from any panel. If a panel has a NextPanels component, it can only be displayed from the panels specified in the NextPanels list.
*/
public class UIPanelManager : MonoBehaviour
{
    public static UIPanelManager Instance { get; private set; }

    public static Action<Vector2> onMoveInput;

    [SerializeField] private List<GameObject> panels;

    [SerializeField] private GameObject defaultPanel = null;
    private Dictionary<string, GameObject> panelLookup;

    private GameObject currentPanel;

    private void Awake()
    {
        Debug.Log("Panel manager awake.");
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        panelLookup = new Dictionary<string, GameObject>();
        foreach (var panel in panels)
        {
            if (!panel.CompareTag("UIPanel"))
            {
                Debug.LogWarning("Panel " + panel.name + " does not have the UIPanel tag. Skipping.");
            }
            panelLookup[panel.name] = panel;
            panel.SetActive(true);
            panel.SetActive(false);
        }
        // Add current panel to lookup if it's not already there and has the correct tag
        if(defaultPanel != null && !panelLookup.ContainsKey(defaultPanel.name) && defaultPanel.CompareTag("UIPanel"))
        {
            panelLookup[defaultPanel.name] = defaultPanel;
        }
        // If a current panel is set, display it.
        DisplayPanel(panelLookup[defaultPanel.name]);
        currentPanel = defaultPanel;
    }

    void Start()
    {
        //UIController.onInventory += ToggleInventory;
        UIController.onMenu += ToggleMenu;
        Interactable.OnInteract += HandleInteract;
        UIController.onDescription += ToggleDescription;
        UIController.onMoveInput += HandleMoveInput; // Ensure we start with no movement input
    }

    public bool ShowPanel(string panelName)
    {
        if (!panelLookup.ContainsKey(panelName))
        {
            Debug.LogError("Panel " + panelName + " not found in UIPanelManager.");
            return false;
        }

        return DisplayPanel(panelLookup[panelName]);
    }
    private bool DisplayPanel(GameObject panelNode)
    {
        if (panelNode == null)
        {
            Debug.LogError("Attempting to display a null panel node.");
            return false;
        }

        if (currentPanel != null && currentPanel.name == panelNode.name)
        {
            Debug.LogWarning("Panel " + panelNode.name + " is already active.");
            return false;
        }

        if(panelNode.GetComponent<NextPanels>() == null)
        {
            Debug.LogWarning("Panel " + panelNode.name + " does not have a NextPanels component. It will be displayed without validation.");
        }

        if(currentPanel != null && 
        currentPanel.GetComponent<NextPanels>() != null && 
        !currentPanel.GetComponent<NextPanels>().GetNextPanels().Contains(panelNode))
        {
            Debug.LogWarning("Panel " + panelNode.name + " is not a valid next panel for " + currentPanel.name);
            return false;
        }
        if (currentPanel != null)
            currentPanel.SetActive(false);
        panelNode.SetActive(true);
        currentPanel = panelNode;
        return true;
    }

    public bool HideCurrent()
    {        
        if (currentPanel == null)
        {
            Debug.LogWarning("No current panel to hide.");
            return false;
        }
        return ShowPanel(defaultPanel.name);
    }

    public bool IsCurrentPanel(string panelName)
    {
        return currentPanel != null && currentPanel.name == panelName;
    }

    void ToggleInventory()
    {
        bool isInventoryOpen = IsCurrentPanel("Inventory Panel");
        if (!isInventoryOpen && ShowPanel("Inventory Panel"))
        {
            Time.timeScale = 0f;
        } else if (isInventoryOpen && HideCurrent())
        {
            Time.timeScale = 1f;
        } else
        {
            Debug.LogWarning("Failed to toggle inventory panel.");
        }
    }

    void ToggleDescription()
    {
        bool isDescriptionOpen = IsCurrentPanel("Description Panel");
        if (!isDescriptionOpen && ShowPanel("Description Panel"))
        {
            Time.timeScale = 0f;
        } else if (isDescriptionOpen && HideCurrent())
        {
            ShowPanel("Inventory Panel");
            Time.timeScale = 1f;
        } else
        {
            Debug.LogWarning("Failed to toggle description panel.");
        }
    }   

    void ToggleMenu()
    {
        bool isMenuOpen = !IsCurrentPanel("Default Panel");
        if (!isMenuOpen && ShowPanel("Menu Panel"))
        {
            Time.timeScale = 0f;
        } else if (isMenuOpen && HideCurrent())
        {
            Time.timeScale = 1f;
        } else
        {
            Debug.LogWarning("Failed to toggle menu panel.");
        }
    }


    void HandleInteract(Interactable interactable)
    {
        // Toggles inventory if interactable is the computer. 
        if (interactable.name == "Computer")
        {
            ToggleInventory();
        }
    }

    void HandleMoveInput(Vector2 input)
    {
        Debug.Log("Received move input: " + input);
        if (!IsCurrentPanel("Inventory Panel"))
        {
            // Do nothing on move input, just prevent further movement handling
            
            return;
        }
        onMoveInput?.Invoke(input);
    }

}
