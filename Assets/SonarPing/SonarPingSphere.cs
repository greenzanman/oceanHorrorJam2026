using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// sonar ping sphere expands from player location of when "Fire" action pressed
// - after pressing Fire, the pingage starts at 0 and increases over time until pingDuration is reached
// - the sphere goes bigger wrt pingAge 
public class SonarPingSphere : MonoBehaviour
{

    private float pingDuration;
    private float pingRadius;
    private float revealDelay;

    private float pingAge = 0f;
    private bool isInitialized = false;
    [SerializeField] private bool looping = false;

    // Start is called before the first frame update
    public void Initialize(float pingDuration, float pingRadius, float revealDelay)
    {
        this.pingDuration = pingDuration;
        this.pingRadius = pingRadius;
        this.revealDelay = revealDelay;

        transform.localScale = Vector3.zero;
        isInitialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isInitialized)
            return;

        pingAge += Time.deltaTime;
        if (pingAge > pingDuration)
        {
            Destroy(gameObject);
            return;
        }

        float scale = pingAge / pingDuration * pingRadius;
        transform.localScale = new Vector3(scale, scale, scale);
    }
    
    void OnTriggerEnter(Collider other)
    {
        SonarObject sonarObject = other.GetComponent<SonarObject>();
        if (sonarObject)
        {
            StartCoroutine(DelayedReveal(sonarObject));
        }
    }

    private IEnumerator DelayedReveal(SonarObject sonarObject)
    {
        yield return new WaitForSeconds(revealDelay);
        sonarObject.SetOpacity(1.5f);
    }
}
