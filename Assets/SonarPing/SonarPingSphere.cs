using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SonarPingSphere : MonoBehaviour
{

    [SerializeField] private float pingDuration = 2;
    [SerializeField] private float pingRadius = 10;
    [SerializeField] private float delay = 0;

    private float pingAge = 0;
    [SerializeField] private bool looping = true;

    // Start is called before the first frame update
    void Start()
    {
        pingAge = -delay;
    }

    // Update is called once per frame
    void Update()
    {
        pingAge += Time.deltaTime;
        if (pingAge < 0)
            return;

        if (pingAge > pingDuration)
        {
            if (looping)
            {
                pingAge -= pingDuration;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        float scale = pingAge / pingDuration * pingRadius;
        transform.localScale = new Vector3(scale, scale, scale);
        
    }

    void OnTriggerEnter(Collider other)
    {
        SonarObject sonarObject = other.GetComponent<SonarObject>();
        if (sonarObject)
        {
            sonarObject.SetOpacity(1.5f);
        }
    }
}
