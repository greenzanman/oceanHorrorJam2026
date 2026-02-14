using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

public class InventoryModel : MonoBehaviour
{
    public static Action Updated;
    [SerializeField] private string currentName = "";
    [SerializeField] private string currentDescription = "";
    void Awake()
    {
        Carousel.onItemChanged += HandleOnItemChanged;
    }

    void HandleOnItemChanged(string newName, string shortDescription, string longDescription)
    {
        UpdateName(newName);
        UpdateDescription(shortDescription);
        Updated?.Invoke();
    }

    void UpdateName(string newName)
    {
        currentName = newName;
    }

    void UpdateDescription(string newDescription)
    {
        currentDescription = newDescription;
    }

    public string GetName()
    {
        return currentName;
    }

    public string GetDescription()
    {
        return currentDescription;
    }
}
