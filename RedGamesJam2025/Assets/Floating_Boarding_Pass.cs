using UnityEngine;
using DG.Tweening;

public class Floating_Boarding_Pass : MonoBehaviour
{
    [Header("Movement")]
    public float floatSpeed = 2f;
    public float floatAmplitude = 0.5f;
    public float rightwardSpeed = 1f;
    
    [Header("Lifecycle")]
    public float fadeOutDelay = 10f;
    public float fadeOutDuration = 2f;
    public float respawnDelay = 15f;
    
    [Header("Collection")]
    public GameObject collectedFX;
    public float collectionAnimDuration = 0.5f;
    public Ease collectionEase = Ease.OutBack;
    
    private Vector3 startPos;
    private Vector3 originalScale;
    private bool isCollected = false;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private Sequence currentSequence;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        startPos = transform.position;
        originalScale = transform.localScale;
        
        StartFloatingMotion();
        StartLifecycle();
    }
    
    void StartFloatingMotion()
    {
        transform.DOMoveY(startPos.y + floatAmplitude, floatSpeed)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
            
        transform.DOMoveX(transform.position.x + rightwardSpeed, 1f)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental);
    }
    
    void StartLifecycle()
    {
        currentSequence = DOTween.Sequence();
        currentSequence.AppendInterval(fadeOutDelay);
        currentSequence.AppendCallback(() => FadeOut());
        currentSequence.AppendInterval(fadeOutDuration + respawnDelay);
        currentSequence.AppendCallback(() => Respawn());
        currentSequence.SetLoops(-1);
    }
    
    void FadeOut()
    {
        if (isCollected) return;
        
        boxCollider.enabled = false;
        spriteRenderer.DOFade(0f, fadeOutDuration).SetEase(Ease.InOutQuad);
        transform.DOScale(originalScale * 0.3f, fadeOutDuration).SetEase(Ease.InBack);
    }
    
    void Respawn()
    {
        if (isCollected) return;
        
        transform.position = new Vector3(startPos.x, startPos.y, startPos.z);
        transform.localScale = originalScale;
        boxCollider.enabled = true;
        
        spriteRenderer.DOFade(1f, 0.8f).SetEase(Ease.OutBack);
        transform.DOScale(originalScale, 0.8f).SetEase(Ease.OutBack);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            CollectItem();
        }
    }
    
    void CollectItem()
    {
        isCollected = true;
        boxCollider.enabled = false;
        
        if (Game_Manager.Instance != null)
        {
            Game_Manager.Instance.AddBoardingPass(1);
        }
        
        if (collectedFX != null)
        {
            Instantiate(collectedFX, transform.position, Quaternion.identity);
        }
        
        currentSequence.Kill();
        transform.DOKill();
        
        Sequence collectSequence = DOTween.Sequence();
        collectSequence.Append(transform.DOScale(originalScale * 1.5f, collectionAnimDuration * 0.3f).SetEase(Ease.OutBack));
        collectSequence.Join(spriteRenderer.DOFade(0f, collectionAnimDuration).SetEase(collectionEase));
        collectSequence.Append(transform.DOScale(Vector3.zero, collectionAnimDuration * 0.7f).SetEase(Ease.InBack));
        collectSequence.OnComplete(() => gameObject.SetActive(false));
    }
}