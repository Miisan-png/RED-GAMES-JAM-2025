using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    private AudioSource audioSource;
    private bool collected = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return; // prevent double triggers
        if (other.CompareTag("Player"))
        {
            collected = true;

            // Play sound
            if (audioSource != null)
            {
                audioSource.Play();
            }

            // Hide visuals immediately
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<Collider2D>().enabled = false;

            // Destroy after sound finishes playing
            Destroy(gameObject, audioSource.clip.length);
        }
    }
}
