using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraTagger : MonoBehaviour
{
    // Maximum distance to check for pickups
    [SerializeField] private float maxDistance = 10f;
    // Enable debug mode to log when looking at pickups
    [SerializeField] private bool debugMode = false;

    private GameObject objectInView = null;

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        bool hitSomething = false;
        
    
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance))
        {
            if (hit.collider.CompareTag("Pickup"))
            {
                hitSomething = true;
                if (debugMode)
                {
                    Debug.Log("Looking at a pickup: " + hit.collider.name);
                }
                // Notify the PickupLogic component that it is being looked at.
                PickupLogic pickup = hit.collider.GetComponent<PickupLogic>();
                if (pickup != null)
                {
                    pickup.SetInView(true);
                    SetObjectInView(hit.collider.gameObject);
                    AddOutline(hit.collider.gameObject);
                }
            }
        }

        // If we're not looking at a pickup anymore, clear it
        if (!hitSomething && objectInView != null)
        {
            PickupLogic previousPickup = objectInView.GetComponent<PickupLogic>();
            if (previousPickup != null)
            {
                previousPickup.SetInView(false);
                RemoveOutline(objectInView);
            }
            objectInView = null;
        }
    }

    /* 
        * Called by the Input System when the interact action is performed.
        Sets the object in view to be picked up if it exists and is in view.
    */
    public void OnInteract()
    {
        Debug.Log("Interact called on: " + gameObject.name);
        if (objectInView != null)
        {   
            // Check if the object is indeed in view before picking up.
            PickupLogic pickup = objectInView.GetComponent<PickupLogic>();
            if (pickup != null && pickup.CheckIsInView())
            {
                // Implement pickup logic here
                pickup.PickUp();
                if (debugMode)
                {
                    Debug.Log("Picked up: " + objectInView.name);
                }
            }
        }
    }


    
    /**
        * Sets the currently viewed object.
        * Clears the previous object's in-view status.
    */
    private void SetObjectInView(GameObject obj)
    {
        // Clear previous object's in-view status
        if (objectInView != null && objectInView != obj)
        {
            PickupLogic previousPickup = objectInView.GetComponent<PickupLogic>();
            previousPickup.SetInView(false);
            RemoveOutline(previousPickup.gameObject);
        }
        // Set new object in view
        objectInView = obj;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * maxDistance);
    }

    /*
        * Adds an outline to the specified object.
    */
    void AddOutline(GameObject obj)
    {
        Outline outline = obj.GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = true;
        }
    }

    /*
        * Removes the outline from the specified object.
    */
    void RemoveOutline(GameObject obj)
    {
        Outline outline = obj.GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = false;
        }
    }
}
