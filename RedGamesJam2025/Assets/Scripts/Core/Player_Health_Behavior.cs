using UnityEngine;
using DG.Tweening;

public class Player_Health_Behavior : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 1;
    public int currentHealth;
    
    [Header("Hit Effects")]
    public GameObject playerHitFX;
    public float hitFXDuration = 2f;
    
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
    
    // Events
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
        // Check if hit by drone
        if (other.CompareTag("Drone") && !isDead && !isInvincible)
        {
            TakeDamage(1);
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Alternative collision detection for drones
        if (collision.gameObject.CompareTag("Drone") && !isDead && !isInvincible)
        {
            TakeDamage(1);
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (isDead || isInvincible) return;
        
        currentHealth -= damage;
        
        // Trigger hit event
        OnPlayerHit?.Invoke();
        
        // Play hit effects
        PlayHitEffects();
        
        // Visual feedback
        if (flashOnHit)
        {
            StartCoroutine(FlashEffect());
        }
        
        // Set invincibility frames
        if (hasInvincibilityFrames)
        {
            isInvincible = true;
            invincibilityTimer = invincibilityDuration;
        }
        
        // Check if dead
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
            
            // Destroy hit FX after duration
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
    
    // Enable ragdoll physics
    if (enableRagdoll)
    {
        EnableRagdoll();
    }
    
    // Play death effects
    PlayHitEffects();
    
    Debug.Log("Player Died!");
}
    
    void EnableRagdoll()
    {
        if (rb != null)
        {
            // Enable gravity and physics
            rb.gravityScale = 1f;
            
            // Add random death force
            Vector2 deathForceDirection = new Vector2(
                Random.Range(-1f, 1f), 
                Random.Range(0.5f, 1f)
            ).normalized;
            
            rb.AddForce(deathForceDirection * deathFallForce, ForceMode2D.Impulse);
            
            // Add random torque for spinning
            float randomTorque = Random.Range(-deathTorque, deathTorque);
            rb.AddTorque(randomTorque, ForceMode2D.Impulse);
        }
    }
    
    // Public methods for external use
    public void Heal(int amount)
    {
        if (isDead) return;
        
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }
    
    public bool IsAlive()
    {
        return !isDead;
    }
    
    public bool IsInvincible()
    {
        return isInvincible;
    }
    
    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }
    
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        isInvincible = false;
        invincibilityTimer = 0f;
        
        // Re-enable player movement
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
        
        // Reset physics
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        
        // Reset visual
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