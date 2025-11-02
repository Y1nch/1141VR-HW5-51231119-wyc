using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Audio")]
    public AudioClip flyClip;   // looped handled by DroneController's AudioSource
    public AudioClip hitClip;   // 撞到障礙物
    public AudioClip passClip;  // 順利通過障礙物

    AudioSource sfxSource;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        sfxSource = gameObject.AddComponent<AudioSource>();
    }

    public void OnDroneCrashed()
    {
        PlayHit();
        Debug.Log("Drone crashed!");
        // TODO: 顯示UI、重設場景等
    }

    public void OnDronePassed()
    {
        PlayPass();
        Debug.Log("Drone passed obstacle!");
        // TODO: 加分、下一關等
    }

    void PlayHit()
    {
        if (hitClip != null) sfxSource.PlayOneShot(hitClip);
    }

    void PlayPass()
    {
        if (passClip != null) sfxSource.PlayOneShot(passClip);
    }
}
