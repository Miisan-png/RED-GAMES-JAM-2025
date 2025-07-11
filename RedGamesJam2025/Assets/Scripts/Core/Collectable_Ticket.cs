using UnityEngine;

public class Collectable_Ticket : MonoBehaviour
{
    [Header("Ticket Settings")]
    public int ticketValue = 1;
    public float collectDelay = 0.1f;
    
    [Header("Juice Effects")]
    public float scaleEffect = 1.5f;
    public float scaleSpeed = 5f;
    public float fadeSpeed = 3f;
    
    private bool isCollected = false;
    private Vector3 originalScale;
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        originalScale = transform.localScale;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            CollectTicket();
        }
    }
    
    void CollectTicket()
    {
        isCollected = true;
        
        if (Game_Manager.Instance != null)
        {
            Game_Manager.Instance.AddTickets(ticketValue);
        }
        
        StartCoroutine(PlayCollectEffect());
    }
    
    System.Collections.IEnumerator PlayCollectEffect()
    {
        float elapsedTime = 0f;
        Color originalColor = spriteRenderer.color;
        
        while (elapsedTime < collectDelay)
        {
            elapsedTime += Time.deltaTime;
            
            float scaleProgress = elapsedTime / collectDelay;
            float currentScale = Mathf.Lerp(1f, scaleEffect, scaleProgress);
            transform.localScale = originalScale * currentScale;
            
            float alpha = Mathf.Lerp(1f, 0f, scaleProgress * fadeSpeed);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            
            yield return null;
        }
        
        Destroy(gameObject);
    }
}