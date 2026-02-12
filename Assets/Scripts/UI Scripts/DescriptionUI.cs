using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Runtime.CompilerServices;

public class DescriptionUI : MonoBehaviour
{
    [SerializeField] private GameObject descriptionTitleText = null;
    [SerializeField] private GameObject descriptionBodyText = null;
    [SerializeField] private string defaultText = "";
    private TextMeshProUGUI tmpName;
    private TextMeshProUGUI tmpDescription;
    // Start is called before the first frame update
    void Awake()
    {
        
        Carousel.onItemChanged += HandleOnItemChanged;
        tmpName = descriptionTitleText.GetComponent<TextMeshProUGUI>();
        tmpDescription = descriptionBodyText.GetComponent<TextMeshProUGUI>();
        if (tmpName == null)
        {
            Debug.LogError("TextMeshProUGUI component not found on descriptionTitleText GameObject.");
        } else if (string.IsNullOrEmpty(defaultText))
        {
            defaultText = tmpName.text;
        }

        if (tmpDescription == null)
        {
            Debug.LogError("TextMeshProUGUI component not found on descriptionBodyText GameObject.");
        }
        tmpName.text = defaultText;
    }

    void HandleOnItemChanged(string newName, string shortDescription, string longDescription)
    {
        UpdateDisplayName(newName);
        UpdateDescription(longDescription);
    }

    void UpdateDisplayName(string newName)
    {
        // Debug.Log(tmpName);
        if (tmpName != null)
        {
            tmpName.text = newName;
        }
    }

    void UpdateDescription(string newDescription)
    {
        // Debug.Log(tmpDescription);
        if (tmpDescription != null)
        {
            tmpDescription.text = newDescription;
        }
    }
}
