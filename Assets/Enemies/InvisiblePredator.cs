using UnityEngine;
using FMODUnity;
using System.Collections.Generic;
using System.Collections;

public class StalkerEnemy : MonoBehaviour

{
    // Cooldown for anticipation sound
    private float anticipationSoundCooldown = 1.5f;
    private float anticipationSoundTimer = 0f;

    [Header("Enemy Marker Prototype")]
    public GameObject enemyMarkerPrefab;

    [Header("Core State")]
    [Range(0, 100)] public float panicIntensity = 0f;
    public bool isStalking = false;

    [Header("Movement & Distance")]
    public float maxDistance = 20f;
    public float minDistance = 2f; 
    public float cursorAvoidanceSpeed = 15f; 

    [Header("View Cone & Accuracy")]
    public float viewConeAngle = 45f;
    
    [Header("Panic Tuning")]
    public float basePanicRiseSpeed = 15f;
    public float basePanicDrainSpeed = 25f;
    public AnimationCurve approachEaseCurve = AnimationCurve.Linear(0, 1, 1, 1);

    [Header("Defeat Mechanics")]
    [Range(0, 100)] public float defeatProgress = 0f;
    public float progressChargeSpeed = 35f; 
    public float progressDrainSpeed = 50f;
    public float accuracySmoothingSpeed = 5f;

    [Header("Darting Mechanics")] // NEW SECTION
    public float minDartInterval = 1.5f; 
    public float maxDartInterval = 5.0f; 
    public float dartIntervalRandomness = 1.0f; 
    public float dartDistance = 5f; 
    public float dartVerticalRandomness = 1.5f; 
    public float dartTravelTime = 0.2f; 
    public float anticipationDelay = 0.35f; 

    [Header("Audio")]
    public EventReference spawnRoarEvent;
    public EventReference panicBeepEvent;
    
    [Header("Darting Audio")] // NEW SECTION
    public EventReference anticipationEvent;
    public EventReference dartSwooshEvent;

    private FMOD.Studio.EventInstance _beepInstance;
    private Transform _player;
    private HashSet<SonarPingSphere> _processedPings = new HashSet<SonarPingSphere>();
    
    // Darting State Trackers
    private float _dartTimer;
    private bool _isDarting = false;
    
    public float CurrentAccuracy { get; private set; } 

    void Start()
    {
        _player = Camera.main.transform;
    }

    public void TriggerInitialSpawn()
    {
        isStalking = true;
        panicIntensity = 0f;
        defeatProgress = 0f; // Reset progress just in case

        float side = Random.value > 0.5f ? 1f : -1f;
        Vector3 spawnDirection = (-_player.forward + (_player.right * side)).normalized;
        spawnDirection.y = 0; 
        
        transform.position = _player.position + (spawnDirection * maxDistance);

        if (!spawnRoarEvent.IsNull) RuntimeManager.PlayOneShot(spawnRoarEvent, transform.position);
        
        _beepInstance = RuntimeManager.CreateInstance(panicBeepEvent);
        RuntimeManager.AttachInstanceToGameObject(_beepInstance, gameObject, GetComponent<Rigidbody>());
        _beepInstance.start();

        // Initialize the first dart timer
        _dartTimer = maxDartInterval; 
    }

    void Update()
    {
        // Update anticipation sound timer
        if (anticipationSoundTimer > 0f)
            anticipationSoundTimer -= Time.deltaTime;

        // Now we check for pings continuously, regardless of state
        CheckForPings();

        if (!isStalking)
        {
            return;
        }

        ProcessStalkingMechanics();
    }

    private void CheckForPings()
    {
        if (SonarManager.Instance == null) return;

        foreach (var ping in SonarManager.Instance.GetActivePings())
        {
            if (_processedPings.Contains(ping)) continue;

            float distanceToPing = Vector3.Distance(transform.position, ping.transform.position);

            if (ping.CurrentRadius >= distanceToPing)
            {
                _processedPings.Add(ping);

                // --- REACTION TO SONAR PING ---
                if (isStalking)
                {
                    // Check if player is looking at the monster (in viewcone)
                    Vector3 dirToEnemy = (transform.position - _player.position).normalized;
                    float angle = Vector3.Angle(_player.forward, dirToEnemy);
                    if (angle <= viewConeAngle)
                    {
                        // Play anticipation sound as test, with cooldown
                        if (!anticipationEvent.IsNull && anticipationSoundTimer <= 0f)
                        {
                            FMODUnity.RuntimeManager.PlayOneShot(anticipationEvent, transform.position);
                            anticipationSoundTimer = anticipationSoundCooldown;
                        }
                    }
                }

                if (!isStalking)
                {
                    TriggerInitialSpawn();
                }
                else if (enemyMarkerPrefab != null)
                {
                    // Spawn the marker right where the predator is standing
                    Instantiate(enemyMarkerPrefab, transform.position, Quaternion.identity);
                }

                break;
            }
        }
    }

    private void ProcessStalkingMechanics()
    {
        // 1. Calculate Angles & Accuracy (Always runs)
        Vector3 dirToEnemy = (transform.position - _player.position).normalized;
        float angle = Vector3.Angle(_player.forward, dirToEnemy);

        float rawAccuracy = 0f;
        if (angle <= viewConeAngle) rawAccuracy = 1.0f - (angle / viewConeAngle); 
        else rawAccuracy = -((angle - viewConeAngle) / (180f - viewConeAngle)); 

        CurrentAccuracy = Mathf.Lerp(CurrentAccuracy, rawAccuracy, Time.deltaTime * accuracySmoothingSpeed);

        // 2. Handle Panic & Progress Math (Always runs)
        if (CurrentAccuracy > 0)
        {
            panicIntensity -= CurrentAccuracy * basePanicDrainSpeed * Time.deltaTime;
            defeatProgress += CurrentAccuracy * progressChargeSpeed * Time.deltaTime;
        }
        else
        {
            float curveMultiplier = approachEaseCurve.Evaluate(panicIntensity / 100f);
            panicIntensity += Mathf.Abs(CurrentAccuracy) * basePanicRiseSpeed * curveMultiplier * Time.deltaTime;
            defeatProgress -= progressDrainSpeed * Time.deltaTime;
        }

        panicIntensity = Mathf.Clamp(panicIntensity, 0f, 100f);
        defeatProgress = Mathf.Clamp(defeatProgress, 0f, 100f);

        // 3. Physical Movement (ONLY runs if we are not currently darting)
        if (!_isDarting)
        {
            // Cursor Avoidance
            if (CurrentAccuracy > 0)
            {
                float strafeDir = Vector3.SignedAngle(_player.forward, dirToEnemy, Vector3.up) > 0 ? 1f : -1f;
                transform.RotateAround(_player.position, Vector3.up, CurrentAccuracy * cursorAvoidanceSpeed * strafeDir * Time.deltaTime);
            }

            // Update Distance
            float targetDistance = Mathf.Lerp(maxDistance, minDistance, panicIntensity / 100f);
            dirToEnemy = (transform.position - _player.position).normalized; 
            dirToEnemy.y = 0; 
            transform.position = _player.position + (dirToEnemy * targetDistance);

            // Dart Timer Countdown
            _dartTimer -= Time.deltaTime;
            if (_dartTimer <= 0f)
            {
                StartCoroutine(DartCoroutine());
            }
        }

        // 4. Update FMOD Beep
        if (_beepInstance.isValid())
        {
            _beepInstance.setParameterByName("PanicIntensity", panicIntensity / 100f);
            _beepInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
        }

        // 5. Win/Loss Conditions
        if (panicIntensity >= 100f) Die();
        else if (defeatProgress >= 100f) DefeatEnemy(); 
    }

    // --- NEW: THE DARTING LOGIC ---
    private IEnumerator DartCoroutine()
    {
        _isDarting = true;

        // 1. Calculate Target Location
        // Dart away from the center of the player's screen if possible
        Vector3 dirToEnemy = (transform.position - _player.position).normalized;
        float dir = Vector3.SignedAngle(_player.forward, dirToEnemy, Vector3.up) > 0 ? 1f : -1f;
        
        Vector3 targetPos = transform.position + (transform.right * dartDistance * dir);
        
        // Add random vertical offset relative to player eye level
        targetPos.y = _player.position.y + Random.Range(-dartVerticalRandomness, dartVerticalRandomness);

        // 2. The "Tell" (Ghost Emitter)
        if (!anticipationEvent.IsNull) 
        {
            // Plays the sound EXACTLY where the monster is about to be!
            // RuntimeManager.PlayOneShot(anticipationEvent, targetPos);
        }

        // Wait for the player to hear it and react
        yield return new WaitForSeconds(anticipationDelay);

        // 3. The Physical Dash
        if (!dartSwooshEvent.IsNull) RuntimeManager.PlayOneShot(dartSwooshEvent, transform.position);

        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < dartTravelTime)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / dartTravelTime);
            
            // Force FMOD to update the beep position rapidly while dashing
            if (_beepInstance.isValid()) _beepInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
            
            yield return null;
        }

        transform.position = targetPos; // Snap to final position

        // 4. Reset Timer based on current progress
        float baseInterval = Mathf.Lerp(maxDartInterval, minDartInterval, defeatProgress / 100f);
        _dartTimer = baseInterval + Random.Range(-dartIntervalRandomness, dartIntervalRandomness);
        _dartTimer = Mathf.Max(0.5f, _dartTimer); // Safety clamp to prevent double-darting

        _isDarting = false;
    }
    // ------------------------------

    private void Die()
    {
        Debug.Log("PLAYER KILLED BY STALKER");
        isStalking = false; // Stop tracking logic
        
        // Find the manager and start the show
        FindObjectOfType<GameOverManager>().StartGameOverSequence();
        
        // Hide the monster instantly without fully destroying it right away, 
        // or let the OnDestroy handle the audio cleanup if you do Destroy(gameObject).
        Destroy(gameObject); 
    }

    private void DefeatEnemy()
    {
        Debug.Log("STALKER DEFEATED");
        isStalking = false;
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (_beepInstance.isValid())
        {
            // Detach it so it survives the GameObject's destruction
            RuntimeManager.DetachInstanceFromGameObject(_beepInstance);
            
            // Tell FMOD to stop, but allow the AHDSR release envelope to play
            _beepInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _beepInstance.release();
            _beepInstance.clearHandle();
        }
    }
}