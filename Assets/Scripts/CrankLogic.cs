using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrankLogic : MonoBehaviour
{
    [SerializeField] private Carousel inventory; 
    [SerializeField] private float rotationSpeed = 1f; // progress per second
    [SerializeField] private float maxCrankRotation = 360f;
    [SerializeField] private float maxDoorHeight = 2f;

    private GameObject crank;
    private GameObject panel;
    private GameObject door;

    private bool isCrankVisible = false;
    private bool isHolding = false;

    private bool isDoorOpen = false;

    private float progress = 0f; // 0 = crank fully down, door closed; 1 = crank fully turned, door fully open

    private Quaternion crankStartRotation;
    private Quaternion crankEndRotation;
    private Vector3 doorStartPosition;
    private Vector3 doorEndPosition;

    void Awake()
    {
        Interactable.OnInteractHeld += HandleInteractHeld;
        Interactable.OnInteract += HandleInteract;

        crank = transform.Find("Crank").gameObject;
        panel = transform.Find("Panel").gameObject;
        door = transform.Find("Door").gameObject;

        crank.GetComponent<MeshRenderer>().enabled = false;

        Vector3 euler = crank.transform.localEulerAngles;
        euler.y = 0f; // treat current rotation as 0
        crank.transform.localRotation = Quaternion.Euler(euler);
        crankStartRotation = crank.transform.localRotation;
        crankEndRotation = crankStartRotation * Quaternion.Euler(0f, maxCrankRotation, 0f);


        doorStartPosition = door.transform.position;
        doorEndPosition = doorStartPosition + Vector3.up * maxDoorHeight;
    }

    void HandleInteract(Interactable interactable)
    {
        if (!isCrankVisible && interactable.gameObject == crank && inventory.Contains("Crank"))
        {
            isCrankVisible = true;
            crank.GetComponent<MeshRenderer>().enabled = true;
        }
    }

    void HandleInteractHeld(Interactable interactable, bool held)
    {
        if (interactable.gameObject == crank && isCrankVisible)
        {
            isHolding = held;
        }
    }

    void Update()
    {
        if (!isCrankVisible || isDoorOpen) return;

        // Update progress
        float delta = rotationSpeed * Time.deltaTime * (isHolding ? 1f : -1f);
        progress = Mathf.Clamp01(progress + delta);

        // Rotate crank absolutely around its local Y axis
        float targetAngle = maxCrankRotation * progress;
        crank.transform.localRotation = crankStartRotation * Quaternion.Euler(0f, targetAngle, 0f);
        Debug.Log("Crank rotation: " + crank.transform.localRotation.eulerAngles.y);
        // Interpolate door position
        door.transform.position = Vector3.Lerp(doorStartPosition, doorEndPosition, progress);
        if (!isDoorOpen && progress >= 1f)
        {
            isDoorOpen = true;
        }
    }
}
