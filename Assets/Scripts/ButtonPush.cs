using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPush : MonoBehaviour
{
    [SerializeField] private Vector3 pressedOffset = new Vector3(0, -0.1f, 0);
    [SerializeField] private float pressTime = 0.2f;

    private Vector3 originalPosition;
    private bool isPressed = false;

    void Start() 
    {
        originalPosition = transform.localPosition;
        Interactable.OnInteract += Press;
    }

    public void Press(Interactable interactable)
    {
        if (interactable.gameObject == this.gameObject)
            StartCoroutine(PressRoutine());
    }

    IEnumerator PressRoutine()
    {
        isPressed = true;

        // Move down
        Vector3 target = originalPosition + pressedOffset;
        float elapsed = 0;
        while (elapsed < pressTime)
        {
            transform.localPosition = Vector3.Lerp(originalPosition, target, elapsed / pressTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = target;

        // Move back up
        elapsed = 0;
        while (elapsed < pressTime)
        {
            transform.localPosition = Vector3.Lerp(target, originalPosition, elapsed / pressTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalPosition;

        isPressed = false;
    }
}
