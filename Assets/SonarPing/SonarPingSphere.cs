using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SonarPingSphere : MonoBehaviour
{

    // Ping id's (so each is individually tracked)
    private static int pingId = 0;
    public int thisPingId;

    public Material material;
    [SerializeField] private float pingDuration = 2;
    [SerializeField] private float pingRadius = 10;
    [SerializeField] private float initialDelay = 0;
    [SerializeField] private float delay = 0;
    

    private float pingAge = 0;
    private float currentRadius;
    [SerializeField] private bool looping = true;

    // Track all 'seen' objects
    private HashSet<SonarShaderObject> shaderObjects
        = new HashSet<SonarShaderObject>();

    // Start is called before the first frame update
    void Start()
    {
        // Grab a ticket
        GrabTicket();

        pingAge = -initialDelay;

        // Reset seens
        shaderObjects.Clear();
    }

    private void GrabTicket()
    {
        thisPingId = pingId;
        pingId++;
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
                pingAge -= pingDuration + delay;
                transform.localScale = Vector3.zero;

                // Grab a ticket
                GrabTicket();
    
                // Reset seens
                shaderObjects.Clear();
    
                return;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Change scale and transform
        currentRadius = pingAge / pingDuration * pingRadius;
        transform.localScale = new Vector3(currentRadius, currentRadius, currentRadius);
        
        // Fade out near edges
        material.SetColor("_Color", new Color(1 - pingAge / pingDuration - 0.25f, 0, 0, 1));
    
        // Handle shader objects
        foreach (SonarShaderObject shaderObject in shaderObjects)
        {
            // Scale of sphere is 0.5, so must divide radii by 2
            shaderObject.HandlePing(transform.position, currentRadius / 2, pingRadius / 2, thisPingId);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        SonarShaderObject shaderObject = other.GetComponent<SonarShaderObject>();
        if (shaderObject != null)
        {
            shaderObjects.Add(shaderObject);
            shaderObject.HandlePing(transform.position, currentRadius / 2, pingRadius / 2, thisPingId);
        }
    }
}
