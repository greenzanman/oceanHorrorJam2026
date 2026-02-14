using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

// Floats around aimlessly, scatters if there's a sound nearby
public class MinnowSchool : MonoBehaviour
{
    [Header("Minnow School Tuning")]
    [SerializeField] private float alertDistance = 12f;
    private MinnowIndividual[] minnows;
    // Start is called before the first frame update
    void Start()
    {
        // Create event system?
        if (PlayerTester.soundEvent == null)
            PlayerTester.soundEvent = new UnityEvent<Vector3>();

        PlayerTester.soundEvent.AddListener(OnSound);

        minnows = GetComponentsInChildren<MinnowIndividual>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (MinnowIndividual minnow in minnows)
        {
            // Pass the array of minnows so they can calculate neighbor forces
            minnow.ProcessMinnow(Time.deltaTime, minnows);
        }
    }

    void OnSound(Vector3 soundPosition)
    {
        if ((soundPosition - transform.position).sqrMagnitude < alertDistance * alertDistance)
        {
            foreach (MinnowIndividual minnow in minnows)
            {
                minnow.Fear();
            }
        }
    }
}
