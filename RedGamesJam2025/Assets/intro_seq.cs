using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class QuickIntroFloat : MonoBehaviour
{
    [Header("Main Floating Images")]
    public RawImage image;
    public RawImage imageText;

    [Header("Collect This Image")]
    public RawImage collectThisImage;
    public float collectSlideDuration = 0.5f;
    public float collectFloatAmplitude = 10f;
    public float collectFloatSpeed = 0.6f;
    public float collectStayDuration = 1.8f;
    public float collectExitDistance = 500f;
    public float collectExitDuration = 0.6f;
    public Ease collectExitEase = Ease.InBack;

    [Header("Main Float Settings")]
    public float floatSpeed = 1f;
    public float floatAmplitude = 20f;
    public float exitDelay = 2.5f;         // Delay before first images fly away
    public float exitDuration = 1f;
    public float exitDistance = 1000f;
    public Ease exitEase = Ease.InBack;

    private Vector3 imgStartPos, txtStartPos;
    private Tween imgFloatTween, txtFloatTween;

    void Start()
    {
        collectThisImage.gameObject.SetActive(false);

    }
    public void StartFloating()
    {
        imgStartPos = image.rectTransform.anchoredPosition;
        txtStartPos = imageText.rectTransform.anchoredPosition;

        // Float up/down idle animation
        imgFloatTween = image.rectTransform.DOAnchorPosY(imgStartPos.y + floatAmplitude, floatSpeed)
            .SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        txtFloatTween = imageText.rectTransform.DOAnchorPosY(txtStartPos.y + floatAmplitude, floatSpeed)
            .SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);

        // After delay, play exit and show "collect this"
        Invoke(nameof(PlayExitAnimation), exitDelay);
    }

    void PlayExitAnimation()
    {
        imgFloatTween?.Kill();
        txtFloatTween?.Kill();

        // Slide up off screen
        image.rectTransform.DOAnchorPosY(imgStartPos.y + exitDistance, exitDuration).SetEase(exitEase);
        imageText.rectTransform.DOAnchorPosY(txtStartPos.y + exitDistance, exitDuration).SetEase(exitEase);

        // Trigger "Collect This" to appear after those are gone
        Invoke(nameof(ShowCollectImage), 0.6f); // small delay after exit
    }
void ShowCollectImage()
{
    if (collectThisImage == null) return;

    RectTransform collectRect = collectThisImage.rectTransform;
    Vector2 targetPos = collectRect.anchoredPosition;

    // Set starting position ABOVE target, but do it right before activating
    Vector2 offscreenStart = new Vector2(targetPos.x, targetPos.y + collectExitDistance);
    collectRect.anchoredPosition = offscreenStart;

    // Enable object right before showing
    collectThisImage.gameObject.SetActive(true);

    // Slide down into view
    Sequence collectSeq = DOTween.Sequence();
    collectSeq.Append(collectRect.DOAnchorPos(targetPos, collectSlideDuration).SetEase(Ease.OutBack));

    // Float effect while idle
    collectSeq.AppendCallback(() =>
    {
        Tween floatTween = collectRect.DOAnchorPosY(targetPos.y + collectFloatAmplitude, collectFloatSpeed)
            .SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);

        // After staying for some time, go back up and disable
        DOVirtual.DelayedCall(collectStayDuration, () =>
        {
            floatTween.Kill();

            collectRect.DOAnchorPosY(targetPos.y + collectExitDistance, collectExitDuration)
                .SetEase(collectExitEase)
                .OnComplete(() =>
                {
                    collectThisImage.gameObject.SetActive(false);
                    collectRect.anchoredPosition = targetPos; // Reset to original
                });
        });
    });
}


}
