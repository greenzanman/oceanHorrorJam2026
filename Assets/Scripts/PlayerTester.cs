using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Testing some random things attached to player input
public class PlyerTester : MonoBehaviour
{
    private PlayerInput playerInput;
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    void FixedUpdate()
    {
        if (playerInput.actions["Debug1"].WasPressedThisFrame())
        {
            WhaleController.soundEvent.Invoke(transform.position);
        }
    }

}
