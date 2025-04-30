using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonAudio : MonoBehaviour
{
   public AudioClip clickSound;
    public float volume = 1.0f;

    private AudioSource audioSource;

    void Awake()
    {
        // 在按钮对象上添加或获取 AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;

        // 获取按钮组件并绑定事件
//        Button btn = GetComponent<Button>();
 //       btn.onClick.AddListener(PlayClickSound);
    }

    public void PlayClickSound()
    {
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound, volume);
        }
    }
}
