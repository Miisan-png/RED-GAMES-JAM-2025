using UnityEngine;

public class Player_Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jetpackForce = 15f;
    public float gravity = 25f;
    public float maxVerticalSpeed = 15f;
    
    [Header("Jetpack Settings")]
    public float jetpackAcceleration = 20f;
    public float maxJetpackForce = 25f;
    public float jetpackBuildupSpeed = 5f;
    
    [Header("Height Bounds")]
    public bool useHeightBounds = true;
    public float minHeight = -5f;
    public float maxHeight = 5f;
    
    private Rigidbody2D rb;
    private bool isMoving = false;
    private bool isJetpackActive = false;
    private float currentJetpackForce = 0f;
    private float jetpackHoldTime = 0f;
    private Camera mainCamera;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        mainCamera = Camera.main;
        
        if (useHeightBounds && mainCamera != null)
        {
            float screenHeight = mainCamera.orthographicSize * 2f;
            maxHeight = mainCamera.transform.position.y + screenHeight * 0.4f;
            minHeight = mainCamera.transform.position.y - screenHeight * 0.4f;
        }
    }
    
    void Update()
    {
        HandleInput();
        HandleMovement();
        HandleJetpack();
        HandleBounds();
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
            rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
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
}