using UnityEngine;
using DG.Tweening;
using UnityEngine.TextCore.Text;

public class Main_Menu_Logic : MonoBehaviour
{
    public CanvasGroup backgroundPanelCanvasGroup;
    public RectTransform labelTapAnywhere;

    private bool isTransitioning = false;
    private Sequence tapLabelSequence;

    void Start()
    {
        if (labelTapAnywhere != null)
        {
            labelTapAnywhere.localScale = Vector3.one * 0.95f;
            tapLabelSequence = DOTween.Sequence();
            tapLabelSequence.Append(labelTapAnywhere.DOScale(1.05f, 1.5f).SetEase(Ease.InOutSine));
            tapLabelSequence.Append(labelTapAnywhere.DOScale(0.95f, 1.5f).SetEase(Ease.InOutSine));
            tapLabelSequence.SetLoops(-1, LoopType.Yoyo);
        }
    }

    void Update()
    {
        if (isTransitioning) return;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began || Input.GetMouseButtonDown(0))
        {
            StartTransition();
        }
    }

    private void StartTransition()
    {
        isTransitioning = true;

        if (tapLabelSequence != null && tapLabelSequence.IsActive())
        {
            tapLabelSequence.Kill();
        }

        Sequence exitSequence = DOTween.Sequence();

        if (backgroundPanelCanvasGroup != null)
        {
            exitSequence.Join(backgroundPanelCanvasGroup.DOFade(0f, 0.7f).SetEase(Ease.OutQuad));
        }

        if (labelTapAnywhere != null)
        {
            exitSequence.Join(labelTapAnywhere.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack));
        }
    }
}
