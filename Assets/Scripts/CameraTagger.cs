using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Tags objects that the camera is looking at as "in view" for pickups and interactables.
This allows the UI controller to know which object to interact with when the player presses the interact button.
*/
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
    
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance))
        {
            if (hit.collider.CompareTag("Pickup"))
            {
                MarkPickup(hit);
                return;
            }
            if(hit.collider.CompareTag("Interact"))
            {
                MarkInteractable(hit);
                return;
            }
        }

        // If we're not looking at a pickup anymore, clear it
        if (objectInView != null)
        {
            ClearObjectInView();
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
            Pickup previousPickup = objectInView.GetComponent<Pickup>();
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
        if(outline == null)
        {
            return;
        }
        outline.EnableOutline();
    }

    /*
        * Removes the outline from the specified object.
    */
    void RemoveOutline(GameObject obj)
    {
        Outline outline = obj.GetComponent<Outline>();
        if(outline == null)
        {
            return;
        }
        outline.DisableOutline();
    }

    void MarkPickup(RaycastHit hit)
    {
        if (debugMode)
        {
            Debug.Log("Looking at a pickup: " + hit.collider.name);
        }
        // Notify the PickupLogic component that it is being looked at.
        Pickup pickup = hit.collider.GetComponent<Pickup>();
        if (pickup != null)
        {
            pickup.SetInView(true);
            SetObjectInView(hit.collider.gameObject);
            AddOutline(hit.collider.gameObject);
        }
    }

    void MarkInteractable(RaycastHit hit)
    {
        if (debugMode)
        {
            Debug.Log("Looking at an interactable: " + hit.collider.name);
        }
        // Notify the Interactable component that it is being looked at.
        Interactable interactable = hit.collider.GetComponent<Interactable>();
        if (interactable != null)
        {
            interactable.SetInView(true);
            SetObjectInView(hit.collider.gameObject);
            AddOutline(hit.collider.gameObject);
        }
    }

    void ClearObjectInView()
    {
        if (objectInView != null)
        {
            Pickup pickup = objectInView.GetComponent<Pickup>();
            if (pickup != null)
            {
                pickup.SetInView(false);
                RemoveOutline(pickup.gameObject);
            }
            Interactable interactable = objectInView.GetComponent<Interactable>();
            if (interactable != null)
            {
                interactable.SetInView(false);
                RemoveOutline(interactable.gameObject);
            }
            objectInView = null;
        }
    }
}
