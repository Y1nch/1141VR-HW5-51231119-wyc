using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;       // Drone (目標)
    public Vector3 offset = new Vector3(0, 5, -8); // 預設攝影機位置偏移
    public float smoothSpeed = 5f; // 跟隨平滑速度

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // 可選，讓攝影機永遠看向 drone
        transform.LookAt(target);
    }
}
