using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Main_Menu_Logic : MonoBehaviour
{
    public CanvasGroup backgroundPanelCanvasGroup;
    public RectTransform labelTapAnywhere;
    public GameObject audioPrefab;

    [Header("Intro Float Logic")]
    public QuickIntroFloat quickIntroFloat;
    public float delayBeforeFloat = 1.5f;

    [Header("Team Logo Settings")]
    public RawImage teamLogo;
    public float logoBreathScale = 1.05f;
    public float logoBreathSpeed = 1.2f;

    private Tween logoBreathTween;


    [Header("KL Stamp Settings")]
    public RawImage KLStamp;
    public float stampSlideDuration = 0.4f;
    public float stampStayDuration = 1.5f;
    public float stampFloatAmplitude = 10f;
    public float stampFloatSpeed = 0.6f;

    private Vector2 stampCenterPos;
    private Vector2 stampStartOffscreenPos;
    private bool isTransitioning = false;
    private Tween floatTween;
    private Vector3 originalPosition;

    public float floatDistance = 20f;
    public float floatSpeed = 2f;

    void Start()
    {
        if (labelTapAnywhere != null)
        {
            originalPosition = labelTapAnywhere.anchoredPosition;
            floatTween = labelTapAnywhere.DOAnchorPosY(originalPosition.y + floatDistance, floatSpeed)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

            if (teamLogo != null)
    {
        teamLogo.transform.localScale = Vector3.one; 
        logoBreathTween = teamLogo.transform
            .DOScale(logoBreathScale, logoBreathSpeed)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }


        if (KLStamp != null)
        {
            KLStamp.gameObject.SetActive(false); // hide at start
            stampCenterPos = KLStamp.rectTransform.anchoredPosition;
            stampStartOffscreenPos = stampCenterPos - new Vector2(0f, 500f);
            KLStamp.rectTransform.anchoredPosition = stampStartOffscreenPos;
        }
    }

    void Update()
    {
        if (isTransitioning) return;

        if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) || Input.GetMouseButtonDown(0))
        {
            StartTransition();
        }
    }

    private void StartTransition()
    {
        isTransitioning = true;
        Game_Manager.Instance.EnableScoreDisplay();

        if (floatTween != null && floatTween.IsActive())
        {
            floatTween.Kill();
        }

        Sequence exitSequence = DOTween.Sequence();

        if (backgroundPanelCanvasGroup != null)
        {
            exitSequence.Join(backgroundPanelCanvasGroup.DOFade(0f, 0.7f).SetEase(Ease.OutQuad));
        }

        if (labelTapAnywhere != null)
        {
            exitSequence.Join(labelTapAnywhere.DOAnchorPosY(originalPosition.y + 200f, 0.5f).SetEase(Ease.InBack));
        }

        exitSequence.OnComplete(() =>
        {
            // Activate intro float logic
            Invoke(nameof(ActivateQuickIntro), delayBeforeFloat);
            // Run KL Stamp animation
            PlayKLStampSequence();
        });
    }

    void ActivateQuickIntro()
    {
        if (quickIntroFloat != null)
        {
            quickIntroFloat.StartFloating();
        }
    }

    void PlayKLStampSequence()
    {
        if (KLStamp == null) return;

        KLStamp.gameObject.SetActive(true);
        KLStamp.color = new Color(1f, 1f, 1f, 1f); // ensure visible

        RectTransform stampRect = KLStamp.rectTransform;

        Sequence stampSequence = DOTween.Sequence();

        // Slide up to center
        stampSequence.Append(stampRect.DOAnchorPos(stampCenterPos, stampSlideDuration).SetEase(Ease.OutBack));

        // Float up/down for a while
        stampSequence.AppendCallback(() =>
        {
            floatTween = stampRect.DOAnchorPosY(stampCenterPos.y + stampFloatAmplitude, stampFloatSpeed)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        });

        // Stay floating for a while, then exit
        stampSequence.AppendInterval(stampStayDuration);

        stampSequence.AppendCallback(() =>
        {
            floatTween?.Kill();
        });

        // Slide back down out of view
        stampSequence.Append(stampRect.DOAnchorPos(stampStartOffscreenPos, stampSlideDuration).SetEase(Ease.InBack));

        // Optional: deactivate after done
        stampSequence.OnComplete(() =>
        {
            KLStamp.gameObject.SetActive(false);
        });
    }
}
