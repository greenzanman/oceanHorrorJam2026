using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

// sonar ping sphere expands from player location of when "Fire" action pressed
// - after pressing Fire, the pingage starts at 0 and increases over time until pingDuration is reached
// - the sphere goes bigger wrt pingAge 
public class SonarPingSphere : MonoBehaviour
{

    [SerializeField] private float pingDuration = 2;
    [SerializeField] private float pingRadius = 10;
    [SerializeField] private float delay = 0;

    private float pingAge = -1f;
    [SerializeField] private bool looping = true;

    private PlayerInput playerInput;
    private InputAction fireAction;

    // Start is called before the first frame update
    void Start()
    {
        pingAge = -1f;
        playerInput = FindObjectOfType<PlayerInput>();
        if (playerInput != null)
        {
            fireAction = playerInput.actions["Fire"];
            if (fireAction != null)
            {
                fireAction.performed += OnFire;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (pingAge < 0)
            return;

        pingAge += Time.deltaTime;
        if (pingAge > pingDuration)
        {
            pingAge = -1f;
            transform.localScale = Vector3.zero;
            return;
        }

        float scale = pingAge / pingDuration * pingRadius;
        transform.localScale = new Vector3(scale, scale, scale);
    }

    private void OnFire(InputAction.CallbackContext context)
    {
        // set sonar ping origin to the location of player when they fired
        var player = FindObjectOfType<StrokePlayerController>();
        if (player != null)
        {
            transform.position = player.transform.position;
        }
        pingAge = 0f;
    }

    void OnTriggerEnter(Collider other)
    {
        SonarObject sonarObject = other.GetComponent<SonarObject>();
        if (sonarObject)
        {
            sonarObject.SetOpacity(1.5f);
        }
    }

    void OnDestroy()
    {
        if (fireAction != null)
        {
            fireAction.performed -= OnFire;
        }
    }
}
