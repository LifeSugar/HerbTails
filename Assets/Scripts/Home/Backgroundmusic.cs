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
            DontDestroyOnLoad(gameObject);  // ��֤�����ڳ��������
            bgmExists = true;
        }
        else
        {
            Destroy(gameObject);  // ��ֹ��� BGM �ص�
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