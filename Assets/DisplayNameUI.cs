using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplayNameUI : MonoBehaviour
{
    [SerializeField] private GameObject displayNameText = null;
    [SerializeField] private GameObject displayDescriptionText = null;
    [SerializeField] private string defaultText = "";
    private TextMeshProUGUI tmpName;
    private TextMeshProUGUI tmpDescription;
    // Start is called before the first frame update
    void Awake()
    {
        
        Carousel.onItemChanged += HandleOnItemChanged;
        tmpName = displayNameText.GetComponent<TextMeshProUGUI>();
        tmpDescription = displayDescriptionText.GetComponent<TextMeshProUGUI>();
        if (tmpName == null)
        {
            Debug.LogError("TextMeshProUGUI component not found on displayNameText GameObject.");
        } else if (string.IsNullOrEmpty(defaultText))
        {
            defaultText = tmpName.text;
        }

        if (tmpDescription == null)
        {
            Debug.LogError("TextMeshProUGUI component not found on displayDescriptionText GameObject.");
        }
        tmpName.text = defaultText;
    }

    void HandleOnItemChanged(string newName, string newDescription)
    {
        UpdateDisplayName(newName);
        UpdateDescription(newDescription);
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
