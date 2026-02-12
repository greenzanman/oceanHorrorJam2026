using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Interactable : MonoBehaviour, Actionable
{
    // Declare the event with the pickup as parameter
    public static event Action<Interactable> OnInteract;
    [SerializeField] private bool inView = false;

    void Awake()
    {
        UIController.onInteractInput += SafeFire;
    }
    public void SetInView(bool isInView)
    {
        inView = isInView;
    }

    public bool CheckIsInView()
    {
        return inView;
    }

    public void Fire()
    {
        // Trigger the event
        OnInteract?.Invoke(this);
        // Destroy the pickup object
        //Destroy(gameObject);
    }

    public void SafeFire()
    {
        if (CheckIsInView())
        {
            Fire();
        }
    }
}
