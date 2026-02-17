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
            if (hit.collider.TryGetComponent(out Actionable actionable))
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