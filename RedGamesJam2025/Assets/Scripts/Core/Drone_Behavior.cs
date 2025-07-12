using UnityEngine;
using DG.Tweening;
using System.Collections;

public class ParallaxDroneBehavior : MonoBehaviour
{
    [Header("Camera Reference")]
    public Camera mainCamera;
    
    [Header("Warning System")]
    public Transform warningIndicator;
    public SpriteRenderer warningRenderer;
    public float warningMoveRange = 2f;
    public float warningDuration = 2f;
    public float lockShakeDuration = 0.8f;
    public Color warningColor = Color.red;
    
    [Header("Drone Movement")]
    public float moveSpeed = 15f;
    public float delayBeforeFirstAttack = 3f;
    public float timeBetweenAttacks = 8f;
    public float droneStartDistance = 20f; // How far outside camera the drone starts
    
    [Header("Warning Position")]
    public bool useLeftEdge = false; // False for right edge (changed default)
    public float edgeOffset = 2f; // Distance from screen edge (increased default)
    public float verticalRandomRange = 3f; // Random vertical position range
    
    [Header("Debug")]
    public int drone_delay_rate = 1;
    
    private bool isAttacking = false;
    private Vector3 targetPosition;
    private Vector3 droneStartPosition;
    private float timer;
    private Color originalWarningColor;
    private Vector3 originalWarningPosition;
    
    void Start()
{
    // Set up camera reference if not assigned
    if (mainCamera == null)
        mainCamera = Camera.main;
    
    // Store original warning indicator color
    if (warningRenderer != null)
        originalWarningColor = warningRenderer.color;
    
    // Initially disable the drone's visual components
    GetComponent<SpriteRenderer>().enabled = false;
    GetComponent<Collider2D>().enabled = false;
    
    // NEW: Set the drone tag for collision detection
    gameObject.tag = "Drone";
    
    // NEW: Ensure the drone has the proper collider setup
    Collider2D droneCollider = GetComponent<Collider2D>();
    if (droneCollider != null)
    {
        droneCollider.isTrigger = true; // Set as trigger for health system
    }
    
    timer = delayBeforeFirstAttack;
    
    // Position warning indicator at camera edge
    PositionWarningIndicator();
}
    void Update()
    {
        PositionWarningIndicator();
        
        timer -= Time.deltaTime;
        if (!isAttacking && timer <= 0f)
        {
            StartCoroutine(DroneAttackSequence());
            timer = timeBetweenAttacks;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Player"))
    {
        Player_Health_Behavior playerHealth = other.GetComponent<Player_Health_Behavior>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(1);
        }
    }
}
    
    void PositionWarningIndicator()
    {
        if (mainCamera == null || warningIndicator == null) return;
        
        // Get camera bounds in world space
        float cameraHeight = mainCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * mainCamera.aspect;
        
        Vector3 cameraPos = mainCamera.transform.position;
        
        // Always position on right edge of camera
        float xPos = cameraPos.x + (cameraWidth / 2f) - edgeOffset;
        
        // Keep Y position centered on camera, or use stored position during attack
        float yPos = cameraPos.y;
        if (isAttacking)
        {
            // During attack, maintain the target Y position
            yPos = targetPosition.y;
        }
        
        // Clamp to camera bounds
        float maxY = cameraPos.y + (cameraHeight / 2f) - 1f;
        float minY = cameraPos.y - (cameraHeight / 2f) + 1f;
        yPos = Mathf.Clamp(yPos, minY, maxY);
        
        warningIndicator.position = new Vector3(xPos, yPos, warningIndicator.position.z);
    }
    
    IEnumerator DroneAttackSequence()
    {
        isAttacking = true;
        
        // Set random target Y position within camera bounds
        Vector3 cameraPos = mainCamera.transform.position;
        float randomY = cameraPos.y + Random.Range(-verticalRandomRange, verticalRandomRange);
        float cameraHeight = mainCamera.orthographicSize * 2f;
        float maxY = cameraPos.y + (cameraHeight / 2f) - 1f;
        float minY = cameraPos.y - (cameraHeight / 2f) + 1f;
        randomY = Mathf.Clamp(randomY, minY, maxY);
        
        targetPosition = new Vector3(warningIndicator.position.x, randomY, warningIndicator.position.z);
        
        // Warning Phase - Flash and move up/down
        yield return StartCoroutine(PlayWarningSequence());
        
        // Lock-on Phase - Shake and prepare drone
        yield return StartCoroutine(LockOnSequence());
        
        // Attack Phase - Enable drone and dash
        yield return StartCoroutine(DroneAttackPhase(targetPosition));
        
        // Reset for next attack
        ResetDrone();
        isAttacking = false;
    }
    
    IEnumerator PlayWarningSequence()
    {
        Vector3 basePosition = warningIndicator.position;
        
        // Create warning animation sequence
        Sequence warnSeq = DOTween.Sequence();
        
        // Move up and down
        warnSeq.Append(warningIndicator.DOMoveY(basePosition.y + warningMoveRange, warningDuration / 4)
            .SetLoops(4, LoopType.Yoyo));
        
        // Flash color
        warnSeq.Join(DOTween.To(() => warningRenderer.color, 
            x => warningRenderer.color = x, warningColor, warningDuration / 8)
            .SetLoops(8, LoopType.Yoyo));
        
        yield return warnSeq.WaitForCompletion();
    }
    
    IEnumerator LockOnSequence()
    {
        // Intense shake to indicate lock-on
        warningIndicator.DOShakePosition(lockShakeDuration, 0.5f, 20, 90, false, true);
        
        // More intense color flash
        DOTween.To(() => warningRenderer.color, 
            x => warningRenderer.color = x, Color.white, 0.1f)
            .SetLoops((int)(lockShakeDuration * 10), LoopType.Yoyo);
        
        yield return new WaitForSeconds(lockShakeDuration);
    }
    
    IEnumerator DroneAttackPhase(Vector3 targetPos)
    {
        // Calculate drone start position (outside camera on RIGHT side, same as warning)
        Vector3 cameraPos = mainCamera.transform.position;
        float cameraWidth = (mainCamera.orthographicSize * 2f) * mainCamera.aspect;
        
        // Start drone on RIGHT side (same side as warning indicator)
        float startX = cameraPos.x + (cameraWidth / 2f) + droneStartDistance;
        
        droneStartPosition = new Vector3(startX, targetPos.y, transform.position.z);
        
        // Position and enable drone
        transform.position = droneStartPosition;
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<Collider2D>().enabled = true;
        
        // Hide warning indicator
        warningRenderer.enabled = false;
        
        // Calculate dash distance and time
        float dashDistance = Vector3.Distance(droneStartPosition, targetPos);
        float dashTime = dashDistance / moveSpeed;
        
        // Dash towards target (moving LEFT across the screen)
        yield return transform.DOMove(targetPos, dashTime)
            .SetEase(Ease.Linear)
            .WaitForCompletion();
        
        // Continue moving past target to exit screen on LEFT side
        Vector3 exitPos = new Vector3(
            cameraPos.x - (cameraWidth / 2f) - droneStartDistance,
            targetPos.y,
            transform.position.z
        );
        
        yield return transform.DOMove(exitPos, 2f)
            .SetEase(Ease.Linear)
            .WaitForCompletion();
    }
    
    void ResetDrone()
    {
        // Hide drone
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        
        // Reset warning indicator
        warningRenderer.enabled = true;
        warningRenderer.color = originalWarningColor;
        
        // Warning indicator will be repositioned in next Update() call
    }
    
    // Public method to trigger attack manually (for testing)
    public void TriggerAttack()
    {
        if (!isAttacking)
        {
            StartCoroutine(DroneAttackSequence());
        }
    }
    
    // Method to change attack side
    public void SwitchAttackSide()
    {
        useLeftEdge = !useLeftEdge;
        if (!isAttacking)
        {
            PositionWarningIndicator();
        }
    }
}