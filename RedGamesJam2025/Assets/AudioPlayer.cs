using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SimpleAudioNode : MonoBehaviour
{
    public AudioClip clip;
    public float volume = 1f;
    public bool loop = false;
    public bool playOnStart = true;
    public bool destroyAfterPlay = true;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = loop;
        audioSource.volume = volume;
        audioSource.clip = clip;
    }

    void Start()
    {
        if (playOnStart && clip != null)
        {
            audioSource.Play();

            if (!loop && destroyAfterPlay)
            {
                Destroy(gameObject, clip.length + 0.1f); // destroy after playback
            }
        }
    }

    /// <summary>
    /// Play the clip manually if playOnStart is false.
    /// </summary>
    public void Play()
    {
        if (clip != null)
        {
            audioSource.Play();

            if (!loop && destroyAfterPlay)
            {
                Destroy(gameObject, clip.length + 0.1f);
            }
        }
    }
}
