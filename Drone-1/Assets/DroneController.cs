using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DroneController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float ascendSpeed = 5f;
    public float tiltAmount = 10f;
    public float tiltSmooth = 2f;

    [Header("Physics")]
    public float hoverForce = 12f;
    public float stability = 2f;

    [Header("Hover Settings")]
    public float hoverHeight = 2.0f; // 起飛後要漂浮的目標高度
    public float hoverTolerance = 0.1f; // 懸浮高度允許誤差

    [Header("Audio")]
    public AudioSource flightAudioSource;

    private Rigidbody rb;
    private Vector3 inputDir;
    private bool isGrounded = true;
    private bool isHovering = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.linearDamping = 2f;
        rb.angularDamping = 5f;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (flightAudioSource != null)
        {
            flightAudioSource.loop = true;
            flightAudioSource.Play();
        }
    }

    void Update()
    {
        // 取得鍵盤輸入
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        inputDir = new Vector3(h, 0, v).normalized;

        // 只在地面時、按空白鍵才起飛
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            isGrounded = false;
            isHovering = true;
        }

        // 按下 Ctrl 鍵降落
        if (isHovering && Input.GetKeyDown(KeyCode.LeftControl))
        {
            isHovering = false;
        }

        // 飛行音效控制（速度越快越大聲）
        if (flightAudioSource != null)
        {
            flightAudioSource.pitch = 0.8f + rb.linearVelocity.magnitude * 0.05f;
        }
    }

    void FixedUpdate()
    {
        // 起飛：持續加速直到到達懸浮高度
        if (isHovering)
        {
            float targetY = GetGroundY() + hoverHeight;
            float heightDiff = targetY - transform.position.y;

            if (Mathf.Abs(heightDiff) > hoverTolerance)
            {
                // 未達到目標高度時，持續施加上升力
                rb.AddForce(Vector3.up * ascendSpeed * Time.fixedDeltaTime, ForceMode.Acceleration);
            }
            else
            {
                // 達到目標高度後，進入懸浮狀態
                isHovering = false;
            }
        }

        // 懸浮（維持漂浮高度，靠 hoverForce 補正）
        if (!isGrounded)
        {
            float targetY = GetGroundY() + hoverHeight;
            float heightDiff = targetY - transform.position.y;

            if (Mathf.Abs(heightDiff) > hoverTolerance)
            {
                // 未達到目標高度時，持續施加上升/下降力
                if (heightDiff > 0)
                {
                    rb.AddForce(Vector3.up * hoverForce * Time.fixedDeltaTime, ForceMode.VelocityChange);
                }
                else
                {
                    rb.AddForce(Vector3.down * hoverForce * Time.fixedDeltaTime, ForceMode.VelocityChange);
                }
            }
            else
            {
                // 達到目標高度時，停止施加力
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            }
        }

        // 水平移動
        Vector3 move = transform.TransformDirection(inputDir) * moveSpeed;
        rb.AddForce(move, ForceMode.Acceleration);

        // 視覺傾斜
        float tiltZ = Mathf.LerpAngle(transform.localEulerAngles.z, -inputDir.x * tiltAmount, Time.fixedDeltaTime * tiltSmooth);
        float tiltX = Mathf.LerpAngle(transform.localEulerAngles.x, inputDir.z * tiltAmount, Time.fixedDeltaTime * tiltSmooth);
        transform.localRotation = Quaternion.Euler(tiltX, transform.localEulerAngles.y, tiltZ);
    }

    // 取得地面 Y 高度（可再加 Raycast 改進）
    float GetGroundY()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f))
        {
            return hit.point.y;
        }
        return 0f; // 預設場景Y=0
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.contacts.Length > 0)
        {
            ContactPoint contact = collision.contacts[0];
            if (Vector3.Dot(contact.normal, Vector3.up) > 0.5f)
            {
                isGrounded = true;
                isHovering = false; // 落地重設
            }

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
