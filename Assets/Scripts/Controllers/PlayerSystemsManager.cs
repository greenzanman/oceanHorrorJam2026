using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSystemsManager : MonoBehaviour
{
    [Header("Scripts to Toggle")]
    [SerializeField] private PlayerInput input;       // Input System component
    [SerializeField] private MonoBehaviour movement;  // Your movement script
    [SerializeField] private MonoBehaviour fireSystem;// Your weapon/firing script
    [SerializeField] private MonoBehaviour energy;    // Your energy/stamina script
    [SerializeField] private FirstPersonCameraController playerCamera; // The camera object or script

    public void SetAllSystems(bool state)
    {
        // Disable Input System first to stop new button presses
        if (input != null) input.enabled = state;

        // Toggle other scripts
        if (movement != null) movement.enabled = state;
        if (fireSystem != null) fireSystem.enabled = state;
        if (energy != null) energy.enabled = state;

        if (playerCamera != null) playerCamera.enabled = state;

        // cursor to appear when systems are disabled
        Cursor.lockState = state ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !state;
    }
}