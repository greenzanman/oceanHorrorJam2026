using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Moves towards player when they're not looking TODO: Change it to when they're spooked? or pinged?
public class SharkController : MonoBehaviour
{
    [Header("Shark Behavior Tuning")]
    [SerializeField] private float approachSpeed = 1f;
    [SerializeField] private float retreatSpeed = -0.3f;
    [SerializeField] private float detectionAngle = 0.1f;

    // Start is called before the first frame update
    void Start()
    {        
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerTester.playerInstance != null)
        {
            Vector3 playerPosition = PlayerTester.playerInstance.transform.position;
            float moveRate;
            if (PlayerTester.playerInstance.InVision(transform.position, detectionAngle))
            {
                moveRate = retreatSpeed;
            }
            else
            {
                moveRate = approachSpeed;
            }

            transform.rotation = Quaternion.LookRotation(playerPosition - transform.position);

            transform.position = Vector3.MoveTowards(transform.position,
                playerPosition, moveRate * Time.deltaTime);
        }
    }
}
