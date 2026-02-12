using UnityEngine;
using UnityEngine.Events;

using UnityEngine.InputSystem;

// Testing some random things attached to player input
public class PlayerTester : MonoBehaviour
{
    public static PlayerTester playerInstance;
    public static UnityEvent<Vector3> soundEvent;
    private PlayerInput playerInput;
    private Camera playerCamera;
    void Start()
    {
        playerInstance = this;
        playerInput = GetComponent<PlayerInput>();
    
        playerCamera = GetComponentInChildren<Camera>();
    }

    void FixedUpdate()
    {
        if (playerInput.actions["Debug1"].WasPressedThisFrame())
        {
           soundEvent.Invoke(transform.position);
        }
    }

    /// <summary>
    /// Returns if a given position is in the view cone
    /// </summary>
    /// <param name="position"></param>
    public bool InVision(Vector3 position, float border = 0.0f)
    {
        Vector3 pos = playerCamera.WorldToScreenPoint(position);
        if (pos.x / Screen.width < -border || pos.y / Screen.height < -border 
            || pos.x / Screen.width > 1 + border || pos.y / Screen.height > 1 + border)
            return false;
        return true;
    }

    public Vector3 CameraFacing()
    {
        return playerCamera.transform.forward;
    }

    public Vector2 InVisionPos(Vector3 position)
    {
        
        Vector3 pos = playerCamera.WorldToScreenPoint(position);
        return new Vector2(pos.x / Screen.width, pos.y / Screen.height);
    }

}
