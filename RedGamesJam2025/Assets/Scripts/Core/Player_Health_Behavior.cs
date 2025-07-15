using UnityEngine;
using DG.Tweening;

public class Player_Health_Behavior : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 1;
    public int currentHealth;

    public Canvas gameOverScreen;

    [Header("Hit Effects")]
    public GameObject playerHitFX;
    public float hitFXDuration = 2f;

    [Header("Audio")]
    public AudioClip hitSound;
    public AudioClip deathSound;

    private AudioSource audioSource;


    [Header("Death Settings")]
    public float deathFallForce = 10f;
    public float deathTorque = 5f;
    public bool enableRagdoll = true;

    [Header("Invincibility")]
    public bool hasInvincibilityFrames = false;
    public float invincibilityDuration = 1f;

    [Header("Visual Feedback")]
    public bool flashOnHit = true;
    public Color flashColor = Color.red;
    public float flashDuration = 0.1f;
    public int flashCount = 3;

    [Header("Camera")]
    public Transform playerCamera;
    public float cameraShakeIntensity = 0.5f;
    public float cameraShakeDuration = 0.8f;

    private Rigidbody2D rb;
    private Player_Movement playerMovement;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isDead = false;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;

    public System.Action OnPlayerHit;
    public System.Action OnPlayerDeath;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<Player_Movement>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        currentHealth = maxHealth;

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

    }

    void Update()
    {
        HandleInvincibility();
        
    }

    void HandleInvincibility()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Drone") && !isDead && !isInvincible)
        {
            TakeDamage(1);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Drone") && !isDead && !isInvincible)
        {
            TakeDamage(1);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInvincible) return;

        currentHealth -= damage;

        OnPlayerHit?.Invoke();
        PlayHitEffects();

        if (flashOnHit)
        {
            StartCoroutine(FlashEffect());
        }

        if (hasInvincibilityFrames)
        {
            isInvincible = true;
            invincibilityTimer = invincibilityDuration;
        }

        if (currentHealth <= 0)
        {
            Die();
        }

    }

    void PlayHitEffects()
    {
        if (playerHitFX != null)
        {
            GameObject hitFXInstance = Instantiate(playerHitFX, transform.position, Quaternion.identity);
            Destroy(hitFXInstance, hitFXDuration);
        }
    }

    System.Collections.IEnumerator FlashEffect()
    {
        if (spriteRenderer == null) yield break;

        for (int i = 0; i < flashCount; i++)
        {
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashDuration);
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        OnPlayerDeath?.Invoke();

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        if (playerCamera != null)
        {
            playerCamera.DOShakePosition(cameraShakeDuration, cameraShakeIntensity, 10, 90f, false, true);
        }

        if (enableRagdoll)
        {
            EnableRagdoll();
        }

        PlayHitEffects();

        if (gameOverScreen != null)
        {
            DOVirtual.DelayedCall(1f, () => gameOverScreen.gameObject.SetActive(true));
        }

        Debug.Log("Player Died!");

                
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

    }

    void EnableRagdoll()
    {
        if (rb != null)
        {
            rb.gravityScale = 1f;
            Vector2 deathForceDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(0.5f, 1f)).normalized;
            rb.AddForce(deathForceDirection * deathFallForce, ForceMode2D.Impulse);
            float randomTorque = Random.Range(-deathTorque, deathTorque);
            rb.AddTorque(randomTorque, ForceMode2D.Impulse);
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    public bool IsAlive() => !isDead;
    public bool IsInvincible() => isInvincible;
    public float GetHealthPercentage() => (float)currentHealth / maxHealth;

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        isInvincible = false;
        invincibilityTimer = 0f;

        if (playerMovement != null) playerMovement.enabled = true;

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    public void SetInvincible(bool invincible, float duration = 0f)
    {
        isInvincible = invincible;
        if (invincible && duration > 0f)
        {
            invincibilityTimer = duration;
        }
    }
}
