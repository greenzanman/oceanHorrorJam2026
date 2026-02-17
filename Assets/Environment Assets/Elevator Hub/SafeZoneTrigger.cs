using UnityEngine;

public class SafeZoneTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Entered Safe Zone: {other.name}"); // DEBUG
        if (other.CompareTag("Player"))
        {
            SonarManager.Instance.SetSafeZone(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"Exited Safe Zone: {other.name}"); // DEBUG
        if (other.CompareTag("Player"))
        {
            SonarManager.Instance.SetSafeZone(false);
        }
    }
}