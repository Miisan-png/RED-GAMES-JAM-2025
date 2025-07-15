using UnityEngine;

public class crow_movement : MonoBehaviour
{
    [Header("Sprite Animation Settings")]
    public Sprite[] animationFrames;
    public float frameRate = 0.1f;

    [Header("Movement Settings")]
    public float minSpeed = 0.5f;
    public float maxSpeed = 1.5f;
    public float minAmplitude = 0.2f;
    public float maxAmplitude = 0.8f;
    public float floatFrequency = 1f;

    [Header("References")]
    public Camera mainCamera;

    private SpriteRenderer spriteRenderer;
    private int currentFrame = 0;
    private float animationTimer = 0f;
    private float moveSpeed;
    private float floatAmplitude;
    private Vector3 direction;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (!spriteRenderer)
        {
            Debug.LogError("Missing SpriteRenderer on crow!");
            enabled = false;
            return;
        }

        if (animationFrames.Length > 0)
        {
            spriteRenderer.sprite = animationFrames[0];
        }

        moveSpeed = Random.Range(minSpeed, maxSpeed);
        floatAmplitude = Random.Range(minAmplitude, maxAmplitude);

        // Spawn off screen relative to current camera position
        SpawnOffScreen();
    }

    void Update()
    {
        AnimateSprite();
        MoveCrow();

        // If off screen (with buffer), re-spawn from opposite side
        if (!IsVisibleWithBuffer(1f))
        {
            SpawnOffScreen();
        }
    }

    void AnimateSprite()
    {
        if (animationFrames.Length == 0) return;

        animationTimer += Time.deltaTime;
        if (animationTimer >= frameRate)
        {
            currentFrame = (currentFrame + 1) % animationFrames.Length;
            spriteRenderer.sprite = animationFrames[currentFrame];
            animationTimer = 0f;
        }
    }

    void MoveCrow()
    {
        float yOffset = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position += (direction * moveSpeed * Time.deltaTime) + new Vector3(0, yOffset * Time.deltaTime, 0);
    }

    void SpawnOffScreen()
    {
        // Get camera bounds
        Vector3 camLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0.5f, mainCamera.nearClipPlane));
        Vector3 camRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 0.5f, mainCamera.nearClipPlane));
        Vector3 camBottom = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0, mainCamera.nearClipPlane));
        Vector3 camTop = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 1, mainCamera.nearClipPlane));

        float ySpawn = Random.Range(camBottom.y, camTop.y);
        bool fromLeft = Random.value > 0.5f;

        if (fromLeft)
        {
            transform.position = new Vector3(camLeft.x - 2f, ySpawn, 0);
            direction = Vector3.right;
            spriteRenderer.flipX = false;
        }
        else
        {
            transform.position = new Vector3(camRight.x + 2f, ySpawn, 0);
            direction = Vector3.left;
            spriteRenderer.flipX = true;
        }

        moveSpeed = Random.Range(minSpeed, maxSpeed);
        floatAmplitude = Random.Range(minAmplitude, maxAmplitude);
    }

    bool IsVisibleWithBuffer(float buffer)
    {
        Vector3 viewPos = mainCamera.WorldToViewportPoint(transform.position);
        return viewPos.x > -buffer && viewPos.x < 1 + buffer &&
               viewPos.y > -buffer && viewPos.y < 1 + buffer;
    }
}
