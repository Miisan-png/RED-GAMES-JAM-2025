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
    public float droneStartDistance = 20f;
    
    [Header("Warning Position")]
    public bool useLeftEdge = false;
    public float edgeOffset = 2f;
    public float verticalRandomRange = 3f;
    
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
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        if (warningRenderer != null)
        {
            originalWarningColor = warningRenderer.color;
            warningRenderer.enabled = false;
        }
        
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        
        gameObject.tag = "Drone";
        
        Collider2D droneCollider = GetComponent<Collider2D>();
        if (droneCollider != null)
        {
            droneCollider.isTrigger = true;
        }
        
        timer = delayBeforeFirstAttack;
        
        if (warningIndicator != null)
        {
            warningIndicator.gameObject.SetActive(false);
        }
    }
    
    void Update()
    {
        if (isAttacking)
        {
            PositionWarningIndicator();
        }
        
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
        
        float cameraHeight = mainCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * mainCamera.aspect;
        
        Vector3 cameraPos = mainCamera.transform.position;
        
        float xPos = cameraPos.x + (cameraWidth / 2f) - edgeOffset;
        
        float yPos = cameraPos.y;
        if (isAttacking)
        {
            yPos = targetPosition.y;
        }
        
        float maxY = cameraPos.y + (cameraHeight / 2f) - 1f;
        float minY = cameraPos.y - (cameraHeight / 2f) + 1f;
        yPos = Mathf.Clamp(yPos, minY, maxY);
        
        warningIndicator.position = new Vector3(xPos, yPos, warningIndicator.position.z);
    }
    
    IEnumerator DroneAttackSequence()
    {
        isAttacking = true;
        
        if (warningIndicator != null)
        {
            warningIndicator.gameObject.SetActive(true);
        }
        
        if (warningRenderer != null)
        {
            warningRenderer.enabled = true;
        }
        
        Vector3 cameraPos = mainCamera.transform.position;
        float randomY = cameraPos.y + Random.Range(-verticalRandomRange, verticalRandomRange);
        float cameraHeight = mainCamera.orthographicSize * 2f;
        float maxY = cameraPos.y + (cameraHeight / 2f) - 1f;
        float minY = cameraPos.y - (cameraHeight / 2f) + 1f;
        randomY = Mathf.Clamp(randomY, minY, maxY);
        
        targetPosition = new Vector3(warningIndicator.position.x, randomY, warningIndicator.position.z);
        
        yield return StartCoroutine(PlayWarningSequence());
        
        yield return StartCoroutine(LockOnSequence());
        
        yield return StartCoroutine(DroneAttackPhase(targetPosition));
        
        ResetDrone();
        isAttacking = false;
    }
    
    IEnumerator PlayWarningSequence()
    {
        Vector3 basePosition = warningIndicator.position;
        
        Sequence warnSeq = DOTween.Sequence();
        
        warnSeq.Append(warningIndicator.DOMoveY(basePosition.y + warningMoveRange, warningDuration / 4)
            .SetLoops(4, LoopType.Yoyo));
        
        warnSeq.Join(DOTween.To(() => warningRenderer.color, 
            x => warningRenderer.color = x, warningColor, warningDuration / 8)
            .SetLoops(8, LoopType.Yoyo));
        
        yield return warnSeq.WaitForCompletion();
    }
    
    IEnumerator LockOnSequence()
    {
        warningIndicator.DOShakePosition(lockShakeDuration, 0.5f, 20, 90, false, true);
        
        DOTween.To(() => warningRenderer.color, 
            x => warningRenderer.color = x, Color.white, 0.1f)
            .SetLoops((int)(lockShakeDuration * 10), LoopType.Yoyo);
        
        yield return new WaitForSeconds(lockShakeDuration);
    }
    
    IEnumerator DroneAttackPhase(Vector3 targetPos)
    {
        Vector3 cameraPos = mainCamera.transform.position;
        float cameraWidth = (mainCamera.orthographicSize * 2f) * mainCamera.aspect;
        
        float startX = cameraPos.x + (cameraWidth / 2f) + droneStartDistance;
        
        droneStartPosition = new Vector3(startX, targetPos.y, transform.position.z);
        
        transform.position = droneStartPosition;
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<Collider2D>().enabled = true;
        
        if (warningRenderer != null)
        {
            warningRenderer.enabled = false;
        }
        
        float dashDistance = Vector3.Distance(droneStartPosition, targetPos);
        float dashTime = dashDistance / moveSpeed;
        
        yield return transform.DOMove(targetPos, dashTime)
            .SetEase(Ease.Linear)
            .WaitForCompletion();
        
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
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        
        if (warningRenderer != null)
        {
            warningRenderer.enabled = false;
            warningRenderer.color = originalWarningColor;
        }
        
        if (warningIndicator != null)
        {
            warningIndicator.gameObject.SetActive(false);
        }
    }
    
    public void TriggerAttack()
    {
        if (!isAttacking)
        {
            StartCoroutine(DroneAttackSequence());
        }
    }
    
    public void SwitchAttackSide()
    {
        useLeftEdge = !useLeftEdge;
        if (!isAttacking)
        {
            PositionWarningIndicator();
        }
    }
}