using UnityEngine;

public class PassZone : MonoBehaviour
{
    bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (other.CompareTag("Player") || other.CompareTag("Drone"))
        {
            triggered = true;
            GameManager.Instance.OnDronePassed();
        }
    }
}
