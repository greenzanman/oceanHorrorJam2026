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
}
