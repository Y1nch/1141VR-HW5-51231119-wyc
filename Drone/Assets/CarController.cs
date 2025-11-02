using UnityEngine;
using UnityEngine.InputSystem; 

public class CarController : MonoBehaviour
{
    [Header("音效設定")]
    public AudioClip runLoopSoundClip;
    public AudioClip hitPlayer2SoundClip; 
    public AudioClip hitPlayer3SoundClip; 
    public AudioClip passOverSoundClip; 

    [Header("移動設定")]
    public float initialMoveSpeed = 0.2f;
    public float dragFactor = 0.98f;
    public float minSpeedThreshold = 0.001f;

    [Header("跳躍設定")]
    public float jumpForce = 5f;
    
    private float currentSpeed = 0f;
    private Rigidbody2D rb; 
    private AudioSource audioSource; 
    private bool isMoving = false; 
    private bool isFrozen = false; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("CarController 需要一個 Rigidbody2D 元件才能執行物理功能！");
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("CarController 需要一個 AudioSource 元件才能播放音效！");
        }

        Application.targetFrameRate = 60;
    }

    void Update()
    {
        if (isFrozen)
        {
            if (rb != null && rb.linearVelocity.magnitude > 0.01f)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
            if (isMoving)
            {
                isMoving = false;
                audioSource.Stop();
            }
            return;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            currentSpeed = initialMoveSpeed;
        }

        transform.Translate(Vector3.right * currentSpeed * Time.deltaTime);
        currentSpeed *= dragFactor;

        if (currentSpeed < minSpeedThreshold)
            currentSpeed = 0;

        HandleMovementSound();

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Jump();
        }
    }
    
    private void Jump()
    {
        if (rb == null) return;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private void HandleMovementSound()
    {
        if (audioSource == null || runLoopSoundClip == null) return;
        
        bool currentlyMoving = currentSpeed > minSpeedThreshold;

        if (currentlyMoving && !isMoving)
        {
            isMoving = true;
            audioSource.clip = runLoopSoundClip;
            audioSource.loop = true; 
            audioSource.Play();
        }
        else if (!currentlyMoving && isMoving)
        {
            isMoving = false;
            audioSource.Stop();
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player2"))
        {
            if (audioSource != null && hitPlayer2SoundClip != null)
            {
                audioSource.PlayOneShot(hitPlayer2SoundClip);
            }
        }
        else if (collision.gameObject.CompareTag("Player3"))
        {
            if (audioSource != null && hitPlayer3SoundClip != null)
            {
                audioSource.PlayOneShot(hitPlayer3SoundClip);
            }
            
            isFrozen = true;
            currentSpeed = 0f; 
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player2_PassOverTrigger")) 
        {
            if (audioSource != null && passOverSoundClip != null)
            {
                audioSource.PlayOneShot(passOverSoundClip);
            }
        }
    }
}
