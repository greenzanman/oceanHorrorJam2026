using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Runtime.CompilerServices;

public class DescriptionUI : MonoBehaviour
{
    [SerializeField] DescriptionModel model;
    [SerializeField] private GameObject descriptionTitleText = null;
    [SerializeField] private GameObject descriptionBodyText = null;
    private TextMeshProUGUI tmpName;
    private TextMeshProUGUI tmpDescription;
    // Start is called before the first frame update
    void OnEnable()
    {
        DescriptionModel.Updated += RefreshUI;
        tmpName = descriptionTitleText.GetComponent<TextMeshProUGUI>();
        tmpDescription = descriptionBodyText.GetComponent<TextMeshProUGUI>();
        if (tmpName == null)
        {
            Debug.LogError("TextMeshProUGUI component not found on descriptionTitleText GameObject.");
        }

        if (tmpDescription == null)
        {
            Debug.LogError("TextMeshProUGUI component not found on descriptionBodyText GameObject.");
        }
        RefreshUI();
    }

    void RefreshUI()
    {
        UpdateDisplayName();
        UpdateDescription();
    }


    void UpdateDisplayName()
    {
        // Debug.Log(tmpName);
        tmpName.text = model.GetName();
    }

    void UpdateDescription()
    {
        // Debug.Log(tmpDescription);
        tmpDescription.text = model.GetDescription();
    }
}
