using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioClip backgroundMusic;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = backgroundMusic;
        audioSource.loop = true;
        audioSource.volume = 0.6f;
        audioSource.Play();
    }
}
