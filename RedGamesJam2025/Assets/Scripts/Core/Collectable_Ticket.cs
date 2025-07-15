using UnityEngine;
using DG.Tweening;

public class Collectable_Ticket : MonoBehaviour
{
    [Header("Ticket Settings")]
    public int ticketValue = 5;
    public float collectDuration = 0.8f;

    [Header("Juice Effects")]
    public float punchScale = 1.5f;
    public float jumpHeight = 2f;
    public float rotationAmount = 360f;



    public float rare_float = 30f;

    [Header("Particle Effects")]
    public ParticleSystem sparkleCollectedFX;

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
            Game_Manager.Instance.AddCoins(ticketValue);
        }

        if (sparkleCollectedFX != null)
        {
            sparkleCollectedFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        PlayCollectEffect();
    }

    void PlayCollectEffect()
    {
        Sequence collectSequence = DOTween.Sequence();

        collectSequence.Append(transform.DOPunchScale(Vector3.one * punchScale, collectDuration * 0.3f, 10, 1f));
        collectSequence.Join(transform.DORotate(new Vector3(0, 0, rotationAmount), collectDuration * 0.5f, RotateMode.FastBeyond360));
        collectSequence.Join(transform.DOMoveY(transform.position.y + jumpHeight, collectDuration * 0.4f).SetEase(Ease.OutQuad));
        collectSequence.Append(transform.DOMoveY(transform.position.y - jumpHeight * 0.5f, collectDuration * 0.3f).SetEase(Ease.InQuad));
        collectSequence.Join(transform.DOScale(Vector3.zero, collectDuration * 0.3f).SetEase(Ease.InBack));
        collectSequence.Join(spriteRenderer.DOFade(0f, collectDuration * 0.3f));

        collectSequence.OnComplete(() => {
            gameObject.SetActive(false);
        });
    }
}
