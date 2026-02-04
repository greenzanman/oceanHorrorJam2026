using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PickupLogic : MonoBehaviour
{
    [SerializeField] private string description = "Put your description here.";
    // Declare the event with the pickup as parameter
    public static event Action<PickupLogic> OnPickedUp;
    [SerializeField] private bool inView = false;
    public void SetInView(bool isInView)
    {
        inView = isInView;
    }

    public bool CheckIsInView()
    {
        return inView;
    }

    public void PickUp()
    {
        // Trigger the event
        OnPickedUp?.Invoke(this);
        // Destroy the pickup object
        //Destroy(gameObject);
    }

    public string GetDescription()
    {
        return description;
    }
    
}
