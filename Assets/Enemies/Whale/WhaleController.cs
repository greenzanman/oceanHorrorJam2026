using UnityEngine;
using UnityEngine.Events;

// Floats in random directions, goes towards sounds
public class WhaleController : MonoBehaviour
{
    private Vector3 facingDirection;
    private Vector3 goalDirection;
    private Vector3 flatFacing;
    private SecondOrderDynamics dynamics; 

    // Start is called before the first frame update
    void Start()
    {
        // Create event system?
        if (PlayerTester.soundEvent == null)
            PlayerTester.soundEvent = new UnityEvent<Vector3>();

        PlayerTester.soundEvent.AddListener(OnSound);

        // Random starting angle
        float startAngle = Random.Range(0, 2 * Mathf.PI);
        facingDirection = new Vector3(Mathf.Sin(startAngle), 0, Mathf.Cos(startAngle));
        goalDirection = facingDirection;

        // SOD initializer
        dynamics = new SecondOrderDynamics(transform.position, 0.5f, 1, 2);
    }

    // Update is called once per frame
    void Update()
    {
        // Increment SOD
        dynamics.Increment(facingDirection, Time.deltaTime);
        transform.position = dynamics.smoothedPosition;

        flatFacing = Vector3.MoveTowards(flatFacing, new Vector3(dynamics.smoothedVelocity.x, 0, dynamics.smoothedVelocity.z), Time.deltaTime);

        // Facing the right direction
        transform.rotation = Quaternion.LookRotation(flatFacing);    
    }

    void OnSound(Vector3 soundPosition)
    {
        facingDirection = (soundPosition - transform.position).normalized;
    }
}
