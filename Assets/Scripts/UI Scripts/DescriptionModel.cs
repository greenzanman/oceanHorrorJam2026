using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DescriptionModel : MonoBehaviour
{
    public static Action Updated;
    [SerializeField] private string currentTitle = null;
    [SerializeField] private string currentDescription = null;
    
    // Start is called before the first frame update
    void Awake()
    {
        Carousel.onItemChanged += HandleOnItemChanged;
    }

    void HandleOnItemChanged(string newName, string shortDescription, string longDescription)
    {
        UpdateDisplayName(newName);
        UpdateDescription(longDescription);
        Updated?.Invoke();
    }

    void UpdateDisplayName(string newName)
    {
        currentTitle = newName;
    }

    void UpdateDescription(string newDescription)
    {
        currentDescription = newDescription;
    }

     public string GetName()
    {
        return currentTitle;
    }

    public string GetDescription()
    {
        return currentDescription;
    }
}
