using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioClip flyLoop;
    public AudioClip hit;
    public AudioClip pass;

    public AudioSource musicSource;
    public AudioSource sfxSource;

    void Awake()
    {
        if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        if (flyLoop != null)
        {
            musicSource.clip = flyLoop;
            musicSource.loop = true;
            musicSource.Play();
        }

        // 將 clip 給 GameManager（選擇性）
        GameManager gm = GameManager.Instance;
        if (gm != null)
        {
            gm.flyClip = flyLoop;
            gm.hitClip = hit;
            gm.passClip = pass;
        }
    }

    public void PlayHit() => sfxSource.PlayOneShot(hit);
    public void PlayPass() => sfxSource.PlayOneShot(pass);
}
