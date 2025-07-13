using UnityEngine;

public class Player_Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jetpackForce = 15f;

    private Player_Animation_System animator;
    public float gravity = 25f;
    public float maxVerticalSpeed = 15f;
    
    [Header("Starting Position")]
    public float startingXOffset = 0f; // Offset from current position (negative = left, positive = right)
    public bool setStartingPositionOnStart = true;
    
    [Header("Speed Increase Settings")]
    public float speedIncreaseRate = 0.5f;
    public float maxSpeed = 20f;
    public float speedIncreaseInterval = 5f;
    
    [Header("Jetpack Settings")]
    public float jetpackAcceleration = 20f;
    public float maxJetpackForce = 25f;
    public float jetpackBuildupSpeed = 5f;
    
    [Header("Height Bounds")]
    public bool useHeightBounds = true;
    public bool autoCalculateHeightBounds = false; // NEW: Toggle for auto-calculation
    public float minHeight = -5f;
    public float maxHeight = 5f;
    
    private Rigidbody2D rb;
    private bool isMoving = false;
    private bool isJetpackActive = false;
    private float currentJetpackForce = 0f;
    private float jetpackHoldTime = 0f;
    private Camera mainCamera;
    private float currentSpeed;
    private float speedIncreaseTimer = 0f;
    
    // NEW: Health system integration
    private Player_Health_Behavior playerHealth;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        mainCamera = Camera.main;
        currentSpeed = moveSpeed;
        animator = GetComponent<Player_Animation_System>();

        
        // NEW: Get health component
        playerHealth = GetComponent<Player_Health_Behavior>();
        
        // NEW: Set starting position
        if (setStartingPositionOnStart)
        {
            SetStartingPosition();
        }
        
        // Only auto-calculate height bounds if enabled
        if (useHeightBounds && autoCalculateHeightBounds && mainCamera != null)
        {
            float screenHeight = mainCamera.orthographicSize * 2f;
            maxHeight = mainCamera.transform.position.y + screenHeight * 0.4f;
            minHeight = mainCamera.transform.position.y - screenHeight * 0.4f;
        }
    }
    
    void SetStartingPosition()
    {
        Vector3 startPos = transform.position;
        startPos.x += startingXOffset; // Simply add the offset to current position
        transform.position = startPos;
    }
    
    void Update()
    {
        // NEW: Only handle input and movement if alive
        if (playerHealth != null && !playerHealth.IsAlive())
        {
            return; // Don't process movement if dead
        }

        if (isMoving)
        {
            if (rb.linearVelocity.y > 0.1f || rb.linearVelocity.y < -0.1f)
                animator.Play("fly");
            else
                animator.Play("walk");
        }

        
        HandleInput();
        HandleMovement();
        HandleJetpack();
        HandleBounds();
        HandleSpeedIncrease();
    }
    
    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (!isMoving)
            {
                isMoving = true;
            }
            isJetpackActive = true;
            jetpackHoldTime = 0f;
        }
        
        if (Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Space))
        {
            isJetpackActive = false;
            currentJetpackForce = 0f;
            jetpackHoldTime = 0f;
        }
        
        if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space))
        {
            isJetpackActive = true;
            jetpackHoldTime += Time.deltaTime;
        }
    }
    
    void HandleMovement()
    {
        if (isMoving)
        {
            rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);
        }
    }
    
    void HandleJetpack()
    {
        if (isJetpackActive)
        {
            currentJetpackForce = Mathf.Lerp(jetpackForce, maxJetpackForce, jetpackHoldTime * jetpackBuildupSpeed);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y + currentJetpackForce * Time.deltaTime);
        }
        else
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y - gravity * Time.deltaTime);
        }
        
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -maxVerticalSpeed, maxVerticalSpeed));
    }
    
    void HandleBounds()
    {
        if (useHeightBounds)
        {
            Vector3 pos = transform.position;
            pos.y = Mathf.Clamp(pos.y, minHeight, maxHeight);
            transform.position = pos;
            
            if (transform.position.y <= minHeight && rb.linearVelocity.y < 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            }
            if (transform.position.y >= maxHeight && rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            }
        }
    }
    
    void HandleSpeedIncrease()
    {
        if (isMoving)
        {
            speedIncreaseTimer += Time.deltaTime;
            
            if (speedIncreaseTimer >= speedIncreaseInterval)
            {
                currentSpeed = Mathf.Min(currentSpeed + speedIncreaseRate, maxSpeed);
                speedIncreaseTimer = 0f;
            }
        }
    }
    
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }
    
    public void ResetSpeed()
    {
        currentSpeed = moveSpeed;
        speedIncreaseTimer = 0f;
    }
    
    // NEW: Public methods for health system integration
    public void StopMovement()
    {
        isMoving = false;
        isJetpackActive = false;
        currentJetpackForce = 0f;
        jetpackHoldTime = 0f;
    }
    
    public void ResetMovement()
    {
        ResetSpeed();
        StopMovement();
        
        // Reset to starting position
        if (setStartingPositionOnStart)
        {
            SetStartingPosition();
        }
    }
    
    public bool IsMoving()
    {
        return isMoving;
    }
    
    // NEW: Method to manually set starting position (useful for respawning)
    public void SetStartingPosition(float customOffset = float.NaN)
    {
        float offsetToUse = float.IsNaN(customOffset) ? startingXOffset : customOffset;
        
        Vector3 startPos = transform.position;
        startPos.x += offsetToUse; // Simply add offset to current position
        transform.position = startPos;
    }
}