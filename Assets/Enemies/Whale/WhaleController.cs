using UnityEngine;
using UnityEngine.Events;

public class WhaleController : MonoBehaviour
{
    public static UnityEvent<Vector3> soundEvent;
    private Vector3 facingDirection;
    private Vector3 goalDirection;
    private Vector3 flatFacing;

    // Start is called before the first frame update
    void Start()
    {
        // Create event system?
        if (soundEvent == null)
            soundEvent = new UnityEvent<Vector3>();

        soundEvent.AddListener(OnSound);

        // Random starting angle
        float startAngle = Random.Range(0, 2 * Mathf.PI);
        facingDirection = new Vector3(Mathf.Sin(startAngle), 0, Mathf.Cos(startAngle))
            * Mathf.Rad2Deg;
        goalDirection = facingDirection;
    }

    // Update is called once per frame
    void Update()
    {
        facingDirection = Vector3.RotateTowards(facingDirection, goalDirection, 
            Time.deltaTime, 0);
        transform.position += facingDirection.normalized * Time.deltaTime;
        flatFacing = facingDirection;
        flatFacing.y = 0;
        
        // Facing the right direction
        transform.rotation = Quaternion.LookRotation(flatFacing);    
    }

    void OnSound(Vector3 soundPosition)
    {
        goalDirection = (soundPosition - transform.position).normalized;
    }
}
