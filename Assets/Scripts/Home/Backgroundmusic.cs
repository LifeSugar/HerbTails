using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BGMPlayer : MonoBehaviour
{
    public AudioClip bgmClip;
    private AudioSource audioSource;

    private static bool bgmExists = false;

    void Awake()
    {
        if (!bgmExists)
        {
            DontDestroyOnLoad(gameObject);  // 保证音乐在场景间持续
            bgmExists = true;
        }
        else
        {
            Destroy(gameObject);  // 防止多个 BGM 重叠
            return;
        }
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = bgmClip;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = 0.5f;
        audioSource.Play();
    }
}