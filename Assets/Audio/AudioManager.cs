using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] EventReference ScubaBreathEvent;
    [SerializeField] EventReference AmbienceUnderwater;
    [SerializeField] EventReference SonarPingEvent;
    [SerializeField] float rate;
    [SerializeField] GameObject player;
    [SerializeField] StrokeStaling strokeController;

    [SerializeField] SonarManager sonarController;

    [Header("Sonar & Energy Events")]
    [SerializeField] EventReference DepletedEnergyEvent;
    [SerializeField] EventReference SuperSonarEvent;
    [SerializeField] EventReference SonarReady50Event;
    [SerializeField] EventReference SonarReady100Event;

    float time;
    public void PlayScubaBreath()
    {
        RuntimeManager.PlayOneShotAttached(ScubaBreathEvent, player);
    }

    public void PlayAmbienceUnderwater()
    {
        RuntimeManager.PlayOneShot(AmbienceUnderwater);
    }

    public void PlaySonarPing()
    {
        RuntimeManager.PlayOneShotAttached(SonarPingEvent, player);
    }


    // Start is called before the first frame update
    void Start()
    {
        PlayAmbienceUnderwater();
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;

        if (strokeController.IsStroking)
        {
            if (time >= rate)
            {
                PlayScubaBreath();
                time = 0f;
            }
        }
    }

    public void PlayDepletedEnergy()
    {
        // 2d ui sound
        if (!DepletedEnergyEvent.IsNull) RuntimeManager.PlayOneShot(DepletedEnergyEvent);
    }

    public void PlaySuperSonar()
    {
        if (!SuperSonarEvent.IsNull) RuntimeManager.PlayOneShotAttached(SuperSonarEvent, player);
    }

    public void PlaySonarReady50()
    {
        // 2d ui sound
        if (!SonarReady50Event.IsNull) RuntimeManager.PlayOneShot(SonarReady50Event);
    }

    public void PlaySonarReady100()
    {
        // 2d ui sound
        if (!SonarReady100Event.IsNull) RuntimeManager.PlayOneShot(SonarReady100Event);
    }
}
