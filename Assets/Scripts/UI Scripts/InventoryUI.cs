using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private InventoryModel model;
    [SerializeField] private GameObject displayNameText = null;
    [SerializeField] private GameObject displayDescriptionText = null;
    private TextMeshProUGUI tmpName;
    private TextMeshProUGUI tmpDescription;
    // Start is called before the first frame update
    void OnEnable()
    {
        InventoryModel.Updated += RefreshUI;
        //Carousel.onItemChanged += HandleOnItemChanged;
        tmpName = displayNameText.GetComponent<TextMeshProUGUI>();
        tmpDescription = displayDescriptionText.GetComponent<TextMeshProUGUI>();
        if (tmpName == null)
        {
            Debug.LogError("TextMeshProUGUI component not found on displayNameText GameObject.");
        }

        if (tmpDescription == null)
        {
            Debug.LogError("TextMeshProUGUI component not found on displayDescriptionText GameObject.");
        }

        if(model == null)
        {
            Debug.LogError("Model not provided.");
        }

        RefreshUI();
    }

    void RefreshUI()
    {
        Debug.Log("Refreshing UI");
        UpdateDisplayName();
        UpdateDescription();
    }

    void UpdateDisplayName()
    {
        // Debug.Log(tmpName);
        if (tmpName != null)
        {
            tmpName.text = model.GetName();
        }
    }

    void UpdateDescription()
    {
        // Debug.Log(tmpDescription);
        if (tmpDescription != null)
        {
            tmpDescription.text = model.GetDescription();
        }
    }
}
