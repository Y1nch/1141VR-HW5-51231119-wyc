using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DroneController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;         // 水平移動速度
    public float ascendSpeed = 5f;       // 上升速度（空白鍵）
    public float tiltAmount = 10f;       // 視覺傾斜角度
    public float tiltSmooth = 2f;        // 傾斜平滑速度

    [Header("Physics")]
    public float hoverForce = 12f;     // 懸浮力（抵銷重力）
    public float stability = 2f;         // 穩定回正速度

    [Header("Audio")]
    public AudioSource flightAudioSource; // 飛行音效（Loop）
    
    private Rigidbody rb;
    private Vector3 inputDir;
    private bool isGrounded = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.linearDamping = 2f;
        rb.angularDamping = 5f;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; // ❗ 防翻倒

        if (flightAudioSource != null)
        {
            flightAudioSource.loop = true;
            flightAudioSource.Play();
        }
    }

    void Update()
    {
        // 取得鍵盤輸入
        float h = Input.GetAxis("Horizontal"); // A,D
        float v = Input.GetAxis("Vertical");   // W,S
        inputDir = new Vector3(h, 0, v).normalized;

        // 空白鍵上升
        if (Input.GetKey(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * ascendSpeed, ForceMode.Acceleration);
            isGrounded = false;
        }

        // 飛行音效控制（速度越快越大聲）
        if (flightAudioSource != null)
        {
            flightAudioSource.pitch = 0.8f + rb.linearVelocity.magnitude * 0.05f;
        }
    }

    void FixedUpdate()
    {
        // 基本懸浮
        if (!isGrounded)
        {
            rb.AddForce(Vector3.up * hoverForce * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }

        // 方向移動（相對於自身前方）
        Vector3 move = transform.TransformDirection(inputDir) * moveSpeed;
        rb.AddForce(move, ForceMode.Acceleration);

        // 模擬視覺傾斜（但不真正旋轉剛體）
        float tiltZ = Mathf.LerpAngle(transform.localEulerAngles.z, -inputDir.x * tiltAmount, Time.fixedDeltaTime * tiltSmooth);
        float tiltX = Mathf.LerpAngle(transform.localEulerAngles.x, inputDir.z * tiltAmount, Time.fixedDeltaTime * tiltSmooth);
        transform.localRotation = Quaternion.Euler(tiltX, transform.localEulerAngles.y, tiltZ);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.contacts.Length > 0)
        {
            // 偵測是否碰到平台或地面
            ContactPoint contact = collision.contacts[0];
            if (Vector3.Dot(contact.normal, Vector3.up) > 0.5f)
            {
                isGrounded = true;
            }

            // 若是障礙物
            if (collision.gameObject.CompareTag("Obstacle"))
            {
                if (flightAudioSource != null) flightAudioSource.Stop();
                GameManager.Instance?.OnDroneCrashed();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PassZone"))
        {
            GameManager.Instance?.OnDronePassed();
        }
    }
}
