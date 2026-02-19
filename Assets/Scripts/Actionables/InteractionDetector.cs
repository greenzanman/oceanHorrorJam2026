using UnityEngine;

public class InteractionDetector : MonoBehaviour
{
    [SerializeField] private float maxReach = 5f;
    [SerializeField] private LayerMask interactableLayer;
    private Actionable currentTarget;

    // This method is called by PlayerInput Broadcast Messages (Action: Interact)
    void OnInteract() 
    {
        currentTarget?.Fire();
    }

    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxReach, interactableLayer))
        {
            // 1. Look for the script on the object hit OR any of its parents
            Actionable actionable = hit.collider.GetComponentInParent<Actionable>();
            // Debug.Log("Ray hit: " + hit.collider.gameObject.name + " which has script: " + actionable);

            // 2. If we found one, handle the focus logic
            if (actionable != null)
            {
                if (currentTarget != actionable)
                {
                    currentTarget?.SetFocus(false);
                    currentTarget = actionable;
                    currentTarget.SetFocus(true);
                }
                return;
            }
        }

        // Clear focus if looking away
        if (currentTarget != null)
        {
            currentTarget.SetFocus(false);
            currentTarget = null;
        }
    }
}