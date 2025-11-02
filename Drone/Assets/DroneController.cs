using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DroneController : MonoBehaviour
{
    public float forwardSpeed = 8f;
    public float liftForce = 5f;
    public float tiltAmount = 15f;
    public AudioSource flightAudioSource; // optional: looped flying sound

    Rigidbody rb;
    bool isCrashed = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = 10f;
        if (flightAudioSource != null)
        {
            flightAudioSource.loop = true;
            flightAudioSource.Play();
        }
    }

    void FixedUpdate()
    {
        if (isCrashed) return;

        // Simple forward thrust
        Vector3 forward = transform.forward * forwardSpeed;
        // Maintain forward movement by velocity assignment on horizontal plane
        Vector3 currentVel = rb.linearVelocity;
        Vector3 targetVel = new Vector3(forward.x, currentVel.y, forward.z);
        rb.linearVelocity = targetVel;

        // Simple lift control to keep some altitude (auto)
        Vector3 upForce = Vector3.up * liftForce;
        rb.AddForce(upForce, ForceMode.Acceleration);

        // Small automatic tilting for visual
        Quaternion targetRot = Quaternion.Euler(-tiltAmount * 0.1f, transform.eulerAngles.y, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * 1.5f);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isCrashed) return;

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            isCrashed = true;
            // stop flight audio
            if (flightAudioSource != null) flightAudioSource.Stop();

            GameManager.Instance.OnDroneCrashed();
            // optional: add physics reaction
            rb.AddForce(-transform.forward * 3f + Vector3.up * 2f, ForceMode.Impulse);
            // disable controls / further movement
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    // public method to stop flight audio from manager if needed
    public void StopFlightAudio()
    {
        if (flightAudioSource != null) flightAudioSource.Stop();
    }
}
