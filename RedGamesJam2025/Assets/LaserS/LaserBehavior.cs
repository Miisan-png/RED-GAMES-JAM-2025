using UnityEngine;
using DG.Tweening;
using System.Collections;

public class LaserBehavior : MonoBehaviour
{
    [Header("Laser Animation System")]
    public Sprite[] laserFrames = new Sprite[12];
    public float buildupFrameRate = 0.15f; // Time per frame for buildup (frames 0-3)
    public float fireFrameRate = 0.08f; // Time per frame for firing (frames 4-11)
    
    [Header("Laser Settings")]
    public bool fireFromTop = true;
    public float lockOnDuration = 1f;
    public float slideSpeed = 8f;
    public float laserLifetime = 2f;
    public float cooldownTime = 3f;
    
    [Header("Position Settings")]
    public float slideStartOffset = 3f; // How far above/below screen to start slide
    
    [Header("Visual Effects")]
    public GameObject chargingEffect;
    public GameObject fireEffect;
    public Color lockOnColor = Color.red;
    public float lockOnFlashSpeed = 0.2f;
    
    [Header("Audio")]
    public AudioClip chargingSound;
    public AudioClip fireSound;
    
    [Header("Debug")]
    public bool enableDebugLogs = true;
    
    private SpriteRenderer spriteRenderer;
    private Collider2D laserCollider;
    private AudioSource audioSource;
    private Camera mainCamera;
    
    private Vector3 originalPosition; // Store the original position where laser was placed
    private Vector3 slideStartPosition;
    
    private bool isActive = false;
    private bool isCharging = false;
    private bool isFiring = false;
    private int currentFrame = 0;
    private Color originalColor;
    
    private Coroutine laserSequence;
    private Coroutine animationCoroutine;


    
    void Start()
    {
        InitializeLaser();
    }

  
    void InitializeLaser()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        laserCollider = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
        mainCamera = Camera.main;
        
        // Store the original LOCAL position where the laser was placed relative to camera
        originalPosition = transform.localPosition;
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            spriteRenderer.enabled = false;
        }
        
        if (laserCollider != null)
        {
            laserCollider.enabled = false;
            laserCollider.isTrigger = true;
        }
        
        gameObject.tag = "Laser";
        
        if (enableDebugLogs)
            Debug.Log($"Laser initialized at local position: {originalPosition}");
    }
    
    public void ActivateLaser()
    {
        if (isActive) return;
        
        if (laserSequence != null)
        {
            StopCoroutine(laserSequence);
        }
        
        laserSequence = StartCoroutine(LaserAttackSequence());
    }
    
    IEnumerator LaserAttackSequence()
    {
        isActive = true;
        
        if (enableDebugLogs)
            Debug.Log("Laser attack sequence started");
        
        // Phase 1: Buildup and Slide
        yield return StartCoroutine(BuildupPhase());
        
        // Phase 2: Lock-on flash
        yield return StartCoroutine(LockOnPhase());
        
        // Phase 3: Fire laser
        yield return StartCoroutine(FirePhase());
        
        // Phase 4: Cleanup
        yield return StartCoroutine(CleanupPhase());
        
        isActive = false;
        
        if (enableDebugLogs)
            Debug.Log("Laser attack sequence completed");
    }
    
    IEnumerator BuildupPhase()
    {
        isCharging = true;
        spriteRenderer.enabled = true;
        
        if (chargingEffect != null)
        {
            chargingEffect.SetActive(true);
        }
        
        if (chargingSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(chargingSound);
        }
        
        // Set slide start position
        SetSlideStartPosition();
        
        // Start buildup animation
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        
        animationCoroutine = StartCoroutine(PlayBuildupAnimation());
        
        // Slide from start position to original position over the lock-on duration
        yield return StartCoroutine(SlideToPosition());
        
        isCharging = false;
        
        if (enableDebugLogs)
            Debug.Log("Buildup phase completed");
    }
    
    IEnumerator SlideToPosition()
    {
        float elapsedTime = 0f;
        Vector3 startPos = slideStartPosition;
        Vector3 endPos = originalPosition;
        
        while (elapsedTime < lockOnDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / lockOnDuration;
            
            // Smooth slide using ease-out curve
            float smoothT = 1f - (1f - t) * (1f - t);
            
            transform.localPosition = Vector3.Lerp(startPos, endPos, smoothT);
            
            yield return null;
        }
        
        // Ensure we're exactly at the target local position
        transform.localPosition = originalPosition;
    }
    
    IEnumerator LockOnPhase()
    {
        // Flash effect to indicate lock-on
        yield return StartCoroutine(LockOnFlashEffect());
        
        if (enableDebugLogs)
            Debug.Log($"Locked on to position: {originalPosition}");
    }
    
    IEnumerator FirePhase()
    {
        isFiring = true;
        
        // Ensure we're at the exact original local position
        transform.localPosition = originalPosition;
        
        // Enable collider for damage
        if (laserCollider != null)
        {
            laserCollider.enabled = true;
        }
        
        if (fireEffect != null)
        {
            fireEffect.SetActive(true);
        }
        
        if (fireSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(fireSound);
        }
        
        // Play fire animation (frames 4-11)
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        
        animationCoroutine = StartCoroutine(PlayFireAnimation());
        
        yield return new WaitForSeconds(laserLifetime);
        
        isFiring = false;
        
        if (enableDebugLogs)
            Debug.Log("Fire phase completed");
    }
    
    IEnumerator CleanupPhase()
    {
        // Disable collider
        if (laserCollider != null)
        {
            laserCollider.enabled = false;
        }
        
        // Disable visual elements
        spriteRenderer.enabled = false;
        
        if (chargingEffect != null)
        {
            chargingEffect.SetActive(false);
        }
        
        if (fireEffect != null)
        {
            fireEffect.SetActive(false);
        }
        
        // Reset color
        spriteRenderer.color = originalColor;
        
        // Reset position to original local position
        transform.localPosition = originalPosition;
        
        yield return new WaitForSeconds(cooldownTime);
        
        if (enableDebugLogs)
            Debug.Log("Cleanup phase completed");
    }
    
    IEnumerator PlayBuildupAnimation()
{
    while (isCharging)
    {
        for (int i = 0; i < 5; i++)
        {
            if (!isCharging) yield break;
            if (i < laserFrames.Length && laserFrames[i] != null)
            {
                spriteRenderer.sprite = laserFrames[i];
                currentFrame = i;
            }
            yield return new WaitForSeconds(buildupFrameRate);
        }
    }
}

    
    IEnumerator PlayFireAnimation()
    {
        // Play frames 4-11 fast
        for (int i = 4; i < 12; i++)
        {
            if (!isFiring) yield break;
            
            if (i < laserFrames.Length && laserFrames[i] != null)
            {
                spriteRenderer.sprite = laserFrames[i];
                currentFrame = i;
            }
            
            yield return new WaitForSeconds(fireFrameRate);
        }
        
        // Loop fire animation while firing
        while (isFiring)
        {
            for (int i = 4; i < 12; i++)
            {
                if (!isFiring) yield break;
                
                if (i < laserFrames.Length && laserFrames[i] != null)
                {
                    spriteRenderer.sprite = laserFrames[i];
                    currentFrame = i;
                }
                
                yield return new WaitForSeconds(fireFrameRate);
            }
        }
    }
    
    IEnumerator LockOnFlashEffect()
    {
        float flashTimer = 0f;
        while (flashTimer < 0.5f)
        {
            spriteRenderer.color = lockOnColor;
            yield return new WaitForSeconds(lockOnFlashSpeed);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(lockOnFlashSpeed);
            flashTimer += lockOnFlashSpeed * 2f;
        }
    }
    
    void SetSlideStartPosition()
    {
        if (mainCamera == null) return;
        
        // Since laser is child of camera, work with local positions
        float cameraHeight = mainCamera.orthographicSize * 2f;
        
        if (fireFromTop)
        {
            // Start above the camera view (local position)
            float startY = (cameraHeight / 2f) + slideStartOffset;
            slideStartPosition = new Vector3(originalPosition.x, startY, originalPosition.z);
        }
        else
        {
            // Start below the camera view (local position)
            float startY = -(cameraHeight / 2f) - slideStartOffset;
            slideStartPosition = new Vector3(originalPosition.x, startY, originalPosition.z);
        }
        
        // Move to slide start position (local position since it's child of camera)
        transform.localPosition = slideStartPosition;
        
        if (enableDebugLogs)
            Debug.Log($"Slide start local position set to: {slideStartPosition}");
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isFiring)
        {
            Player_Health_Behavior playerHealth = other.GetComponent<Player_Health_Behavior>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
                
                if (enableDebugLogs)
                    Debug.Log("Player hit by laser!");
            }
        }
    }
    
    // Public methods for external control
    public void SetFireDirection(bool fromTop)
    {
        fireFromTop = fromTop;
    }
    
    public void StopLaser()
    {
        if (laserSequence != null)
        {
            StopCoroutine(laserSequence);
        }
        
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        
        isActive = false;
        isCharging = false;
        isFiring = false;
        
        StartCoroutine(CleanupPhase());
    }
    
    public bool IsActive()
    {
        return isActive;
    }
    
    // Context menu methods for testing
    [ContextMenu("Test Laser Fire")]
    public void TestLaserFire()
    {
        ActivateLaser();
    }
    
    [ContextMenu("Stop Laser")]
    public void TestStopLaser()
    {
        StopLaser();
    }
    
    [ContextMenu("Switch Fire Direction")]
    public void TestSwitchDirection()
    {
        SetFireDirection(!fireFromTop);
    }
}