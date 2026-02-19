using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Interactable : MonoBehaviour, Actionable
{
    // Declare the event with the pickup as parameter
    public static event Action<Interactable> OnInteract;
    public static event Action<Interactable, bool> OnInteractHeld;
    [SerializeField] private bool inView = false;

    void Awake()
    {
        UIController.onInteractInput += SafeFire;
        UIController.OnInteractHeld += SafeFireHeld;
    }
    public void SetInView(bool isInView)
    {
        inView = isInView;
        if (!isInView)
            UIController.OnInteractHeld.Invoke(false); // Reset hold state when changing view
    }

    public bool CheckIsInView()
    {
        Debug.Log("Checking if " + gameObject.name + " is in view: " + inView);
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

    public void SafeFireHeld(bool isHeld)
    {
        OnInteractHeld?.Invoke(this, isHeld && CheckIsInView());
    }
}
