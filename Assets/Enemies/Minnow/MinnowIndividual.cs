using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinnowIndividual : MonoBehaviour
{
    Vector3 localGoal;
    const float goalRadius = 2.5f;
    const float moveSpeed = 1f;

    Vector3 fearOffset = Vector3.zero;
    int fearCount;
    // Start is called before the first frame update
    void Start()
    {
        // Choose a random local goal in a radius
        GenerateNewGoal();
    }


    public void ProcessMinnow(float deltaTime)
    {
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, localGoal,
            (fearCount > 0 ? moveSpeed * 2.5f : moveSpeed) * deltaTime);

        if ((transform.localPosition - localGoal).sqrMagnitude < 0.1f)
        {
            GenerateNewGoal();
        }
    }

    public void Fear()
    {
        fearCount = 10;
        fearOffset = Random.onUnitSphere * Random.Range(7, 12);
        GenerateNewGoal();
    }

    private void GenerateNewGoal()
    {
        if (fearCount > 0)
        {
            localGoal = fearOffset + Random.insideUnitSphere * goalRadius;
            fearCount -= 1;
        }
        else
        {
            localGoal = Random.insideUnitSphere * goalRadius;
        }
    }
}
