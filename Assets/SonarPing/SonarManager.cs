using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class SonarManager : MonoBehaviour
{
    [Header("reference objects")]
    [SerializeField] private GameObject pingPrefab;
    [SerializeField] private PlayerInput playerInput;


    [Header("ping settings")]
    [SerializeField] private float pingDuration = 10;
    [SerializeField] private float pingRadius = 20;
    [SerializeField] private float revealDelay = 1;
    private InputAction fireAction;

    [Header("ping burst settings")]
    [SerializeField] private int pingsPerFire = 3;
    [SerializeField] private float burstInterval = 0.12f;  // 120ms

    // Start is called before the first frame update
    void Start()
    {
        // validate inputs
        if (playerInput == null)
        {
            Debug.LogError("SonarManager: pls assign playerObject in inspector");
            return;
        }

        if (pingPrefab == null) 
        {
            Debug.LogError("SonarManager: pls assign pingPrefab in inspector");
            return;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnFire()
    {
        StartCoroutine(PingBurstRoutine());
    }

    private IEnumerator PingBurstRoutine()
    {
        for (int i = 0; i < pingsPerFire; i++)
        {
            SpawnPing();
            
            // Wait for 120ms before the next loop iteration
            yield return new WaitForSeconds(burstInterval);
        }
    }

    private void SpawnPing()
    {
        Instantiate(pingPrefab, playerInput.transform.position, Quaternion.identity)
            .GetComponent<SonarPingSphere>()
            .Initialize(pingDuration, pingRadius, revealDelay);
    }
}
