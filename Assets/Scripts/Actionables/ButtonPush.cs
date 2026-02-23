using System.Collections;
using UnityEngine;

public class ButtonPush : UIInteractable
{
    [Header("Animation Settings")]
    [SerializeField] private Vector3 pressedOffset = new Vector3(0, -0.1f, 0);
    [SerializeField] private float pressTime = 0.15f;

    private Vector3 originalPosition;
    private bool isAnimating = false;

    void Start() 
    {
        originalPosition = transform.localPosition;
    }

    // This is called directly by the Interactable script on the same object
    public void ExecuteButtonPress()
    {
        if (!isAnimating)
        {
            StartCoroutine(PressRoutine());
        }
    }

    private IEnumerator PressRoutine()
    {
        isAnimating = true;
        Vector3 targetPosition = originalPosition + pressedOffset;

        // Animate Down
        yield return MoveButton(originalPosition, targetPosition);
        
        // Animate Up
        yield return MoveButton(targetPosition, originalPosition);

        isAnimating = false;
    }

    private IEnumerator MoveButton(Vector3 start, Vector3 end)
    {
        float elapsed = 0;
        while (elapsed < pressTime)
        {
            transform.localPosition = Vector3.Lerp(start, end, elapsed / pressTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = end;
    }

    
    public override void Fire()
    {
        // Debug.Log("Interactable fired");
        ExecuteButtonPress();

        base.Fire();
    }
    
}