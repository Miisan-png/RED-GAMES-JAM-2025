using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Stamp_Manager : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup flashPanel;
    public RawImage[] stampImages;

    [Header("Country Nodes")]
    public GameObject defaultNode;
    public GameObject[] countryNodes;

    [Header("Animation Settings")]
    public float waitAfterThreePassesDelay = 2f; // Wait time after collecting 3 passes
    public float flashDuration = 0.5f;
    public float stampSlideDuration = 0.8f;
    public float stampPopDuration = 0.4f;

    [Header("Positions")]
    public float stampSlideDistance = 300f;

    private Vector3[] stampOriginalPositions;
    private Vector3[] stampStartPositions;
    private int currentCountryIndex = 0;
    private bool hasTriggered = false;
    private int lastPassCount = 0;

    void Start()
    {
        SetupStampManager();
    }

    void SetupStampManager()
    {
        if (flashPanel != null)
        {
            flashPanel.alpha = 0f;
            flashPanel.gameObject.SetActive(false);
        }

        if (stampImages != null && stampImages.Length > 0)
        {
            stampOriginalPositions = new Vector3[stampImages.Length];
            stampStartPositions = new Vector3[stampImages.Length];

            for (int i = 0; i < stampImages.Length; i++)
            {
                if (stampImages[i] != null)
                {
                    stampOriginalPositions[i] = stampImages[i].transform.position;
                    stampStartPositions[i] = stampOriginalPositions[i] + Vector3.down * stampSlideDistance;
                    stampImages[i].transform.position = stampStartPositions[i];
                    stampImages[i].transform.localScale = Vector3.zero;
                    stampImages[i].gameObject.SetActive(false);
                }
            }
        }

        // Enable default node initially
        if (defaultNode != null)
        {
            defaultNode.SetActive(true);
        }

        // Disable all country nodes initially
        if (countryNodes != null && countryNodes.Length > 0)
        {
            for (int i = 0; i < countryNodes.Length; i++)
            {
                if (countryNodes[i] != null)
                {
                    countryNodes[i].SetActive(false);
                }
            }
        }
    }

    void Update()
    {
        if (Game_Manager.Instance != null)
        {
            int passes = Game_Manager.Instance.boarding_pass;

            // Check if we've collected 3 boarding passes and haven't triggered yet
            if (passes >= 3 && !hasTriggered)
            {
                TriggerStampSequence();
                hasTriggered = true;
            }

            // Reset trigger when boarding passes reset (allowing collection again)
            if (passes < lastPassCount && passes == 0)
            {
                hasTriggered = false;
            }

            lastPassCount = passes;
        }
    }

    void TriggerStampSequence()
    {
        Sequence stampSequence = DOTween.Sequence();

        // Wait after collecting 3 boarding passes
        stampSequence.AppendInterval(waitAfterThreePassesDelay);

        // Screen flash
        stampSequence.AppendCallback(() => TriggerScreenFlash());

        // Wait for flash to complete
        stampSequence.AppendInterval(flashDuration);

        // Show stamp and update country
        stampSequence.AppendCallback(() => ShowStampAndUpdateCountry());
    }

    void TriggerScreenFlash()
    {
        if (flashPanel == null) return;

        flashPanel.gameObject.SetActive(true);
        flashPanel.alpha = 0f;

        Sequence flashSequence = DOTween.Sequence();

        // Flash in
        flashSequence.Append(flashPanel.DOFade(1f, flashDuration * 0.3f)
            .SetEase(Ease.OutQuad));

        // During peak flash, update the country nodes
        flashSequence.AppendCallback(() => UpdateCountryNodes());

        // Flash out
        flashSequence.Append(flashPanel.DOFade(0f, flashDuration * 0.7f)
            .SetEase(Ease.InQuad));

        // Disable panel when done
        flashSequence.OnComplete(() =>
        {
            flashPanel.gameObject.SetActive(false);
        });
    }

    void ShowStampAndUpdateCountry()
    {
        if (stampImages == null || stampImages.Length == 0) return;

        // Get the stamp for the current country
        int stampIndex = currentCountryIndex;
        if (stampIndex >= stampImages.Length) return;

        RawImage currentStamp = stampImages[stampIndex];
        if (currentStamp == null) return;

        // Enable stamp
        currentStamp.gameObject.SetActive(true);
        currentStamp.transform.position = stampStartPositions[stampIndex];
        currentStamp.transform.localScale = Vector3.zero;

        Sequence stampAnimation = DOTween.Sequence();

        // Slide up from bottom
        stampAnimation.Append(currentStamp.transform.DOMove(stampOriginalPositions[stampIndex], stampSlideDuration)
            .SetEase(Ease.OutBack));

        // Scale up simultaneously
        stampAnimation.Join(currentStamp.transform.DOScale(1f, stampSlideDuration)
            .SetEase(Ease.OutBack));

        // Pop effect
        stampAnimation.AppendInterval(0.1f);
        stampAnimation.Append(currentStamp.transform.DOPunchScale(Vector3.one * 0.2f, stampPopDuration, 6, 0.3f)
            .SetEase(Ease.OutElastic));

        // Shake effect
        stampAnimation.Join(currentStamp.transform.DOShakeRotation(stampPopDuration, 10f, 15, 90f)
            .SetEase(Ease.OutQuad));

        // Hide stamp after delay
        stampAnimation.AppendInterval(2f);
        stampAnimation.AppendCallback(() => HideStamp(stampIndex));
    }

    void UpdateCountryNodes()
    {
        if (countryNodes == null || countryNodes.Length == 0) return;

        // Disable default node when first country is unlocked
        if (currentCountryIndex == 0 && defaultNode != null)
        {
            defaultNode.SetActive(false);
        }

        // Disable previous country node (if not the first unlock)
        if (currentCountryIndex > 0 && currentCountryIndex <= countryNodes.Length && countryNodes[currentCountryIndex - 1] != null)
        {
            countryNodes[currentCountryIndex - 1].SetActive(false);
        }

        // Enable new country node
        if (currentCountryIndex < countryNodes.Length && countryNodes[currentCountryIndex] != null)
        {
            countryNodes[currentCountryIndex].SetActive(true);
        }

        // Move to next country index
        currentCountryIndex++;
    }

    void HideStamp(int stampIndex)
    {
        if (stampImages == null || stampIndex >= stampImages.Length) return;

        RawImage currentStamp = stampImages[stampIndex];
        if (currentStamp == null) return;

        currentStamp.transform.DOKill();

        Sequence hideSequence = DOTween.Sequence();

        // Scale down
        hideSequence.Append(currentStamp.transform.DOScale(0f, 0.5f)
            .SetEase(Ease.InBack));

        // Move down
        hideSequence.Join(currentStamp.transform.DOMove(stampStartPositions[stampIndex], 0.5f)
            .SetEase(Ease.InBack));

        // Disable when done
        hideSequence.OnComplete(() =>
        {
            currentStamp.gameObject.SetActive(false);
            currentStamp.transform.rotation = Quaternion.identity;
        });
    }

    // Public methods for manual control
    public void ManualTriggerStamp()
    {
        if (!hasTriggered)
        {
            TriggerStampSequence();
            hasTriggered = true;
        }
    }

    public void ResetStampManager()
    {
        hasTriggered = false;
        currentCountryIndex = 0;
        lastPassCount = 0;

        if (stampImages != null)
        {
            for (int i = 0; i < stampImages.Length; i++)
            {
                if (stampImages[i] != null)
                {
                    stampImages[i].transform.DOKill();
                    stampImages[i].gameObject.SetActive(false);
                    stampImages[i].transform.position = stampStartPositions[i];
                    stampImages[i].transform.localScale = Vector3.zero;
                    stampImages[i].transform.rotation = Quaternion.identity;
                }
            }
        }

        if (flashPanel != null)
        {
            flashPanel.DOKill();
            flashPanel.gameObject.SetActive(false);
            flashPanel.alpha = 0f;
        }

        if (defaultNode != null)
        {
            defaultNode.SetActive(true);
        }

        if (countryNodes != null && countryNodes.Length > 0)
        {
            for (int i = 0; i < countryNodes.Length; i++)
            {
                if (countryNodes[i] != null)
                {
                    countryNodes[i].SetActive(false);
                }
            }
        }
    }
}