using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;

[System.Serializable]
public class Landmark
{
    [Header("Landmark Settings")]
    public string landmarkName = "Landmark";
    public Image landmarkImage;
    public int coinsRequired;
   public GameObject parallaxBackground;

    
    [Header("Postcard Animation")]
    public GameObject postCardGameObject;
    public Vector3 originalPosition;
    public Vector3 hiddenPosition;
    
    [Header("Screen Flash")]
    public GameObject screenFlashPanel;
    
    [HideInInspector]
    public bool isUnlocked = false;
}

public class Landmark_Manager : MonoBehaviour
{
    [Header("Landmarks")]
    public Landmark[] landmarks;
    public Progress_Bar progressBar;
    public CanvasGroup progressBarCanvasGroup;

    [Header("Parallax Backgrounds")]
    public GameObject defaultParallaxBackground;


    [Header("UI Update Settings")]
    [Tooltip("Show the progress bar after this many coins are collected.")]
    public int showBarAfterEvery = 5;

    [Header("Animation Settings")]
    public float juicyEffectDuration = 0.6f;
    public float juicyEffectScale = 1.2f;
    public float progressBarFadeDuration = 0.5f;
    public float progressBarDisplayDuration = 2.0f;
    
    [Header("Flash Settings")]
    public float flashDuration = 0.8f;
    public float flashIntensity = 0.8f;
    public float flashDelay = 0.0f;
    
    [Header("Postcard Animation Settings")]
    public float postCardAnimationDuration = 1.0f;
    public float postCardDisplayDuration = 2.0f;
    public float postCardDelay = 0.0f;
    public Ease postCardEase = Ease.OutBack;

    private Tween progressBarFadeTween;
    private int coinCounterForBar = 0;
    private bool isProgressBarShowing = false;
    private bool isPostCardAnimating = false;

    void Start()
{
    if (progressBarCanvasGroup != null)
    {
        progressBarCanvasGroup.alpha = 0f;
    }

    foreach (var landmark in landmarks)
    {
        if (landmark.parallaxBackground != null)
            landmark.parallaxBackground.SetActive(false);
    }

    if (defaultParallaxBackground != null)
        defaultParallaxBackground.SetActive(true);

    if (landmarks.Length > 0)
    {
        landmarks = landmarks.OrderBy(l => l.coinsRequired).ToArray();
    }

    InitializeLandmarks();
    UpdateProgressBarValue();
}

    public void OnCoinsUpdated(int coinsAdded)
    {
        coinCounterForBar += coinsAdded;
        
        CheckForUnlocks();
        
        if (coinCounterForBar >= showBarAfterEvery && !isProgressBarShowing && !isPostCardAnimating)
        {
            ShowProgressBarWithFill();
            coinCounterForBar = 0;
        }
    }

    void InitializeLandmarks()
    {
        int totalCoins = Game_Manager.Instance.GetTotalCoins();
        int coinsUsed = 0;
        
        foreach (var landmark in landmarks)
        {
            if (landmark.postCardGameObject != null)
            {
                landmark.originalPosition = landmark.postCardGameObject.transform.position;
                landmark.hiddenPosition = landmark.originalPosition + Vector3.down * 1000f;
                landmark.postCardGameObject.transform.position = landmark.hiddenPosition;
                landmark.postCardGameObject.SetActive(false);
            }
            
            if (landmark.screenFlashPanel != null)
            {
                landmark.screenFlashPanel.SetActive(true);
                CanvasGroup canvasGroup = landmark.screenFlashPanel.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = landmark.screenFlashPanel.AddComponent<CanvasGroup>();
                }
                canvasGroup.alpha = 0f;
                landmark.screenFlashPanel.SetActive(false);
            }
            
            if (!landmark.isUnlocked)
            {
                int coinsAvailableForThisLandmark = totalCoins - coinsUsed;
                
                if (coinsAvailableForThisLandmark >= landmark.coinsRequired)
                {
                    landmark.isUnlocked = true;
                    landmark.landmarkImage.color = Color.white;
                    coinsUsed += landmark.coinsRequired;
                }
                else
                {
                    landmark.landmarkImage.color = Color.black;
                }
            }
            else
            {
                landmark.landmarkImage.color = Color.white;
                coinsUsed += landmark.coinsRequired;
            }
        }
    }

    void UpdateProgressBarValue()
    {
        int totalCoins = Game_Manager.Instance.GetTotalCoins();
        int coinsUsed = 0;
        
        Landmark currentLandmark = null;
        
        foreach (var landmark in landmarks)
        {
            if (landmark.isUnlocked)
            {
                coinsUsed += landmark.coinsRequired;
            }
            else
            {
                currentLandmark = landmark;
                break;
            }
        }
        
        if (currentLandmark != null)
        {
            int coinsForCurrentLandmark = totalCoins - coinsUsed;
            float progress = (float)coinsForCurrentLandmark / currentLandmark.coinsRequired;
            progressBar.SetValue(Mathf.Clamp01(progress), false);
        }
        else
        {
            progressBar.SetValue(1f, false);
        }
    }

    void CheckForUnlocks()
    {
        int totalCoins = Game_Manager.Instance.GetTotalCoins();
        int coinsUsed = 0;
        
        foreach (var landmark in landmarks)
        {
            if (!landmark.isUnlocked)
            {
                int coinsAvailableForThisLandmark = totalCoins - coinsUsed;
                
                if (coinsAvailableForThisLandmark >= landmark.coinsRequired)
                {
                    UnlockLandmark(landmark);
                    coinsUsed += landmark.coinsRequired;
                }
                else
                {
                    break;
                }
            }
            else
            {
                coinsUsed += landmark.coinsRequired;
            }
        }
    }

    void UnlockLandmark(Landmark landmark)
{
    if (isPostCardAnimating) return;

    landmark.isUnlocked = true;
    isPostCardAnimating = true;
    coinCounterForBar = 0;

    Sequence unlockSequence = DOTween.Sequence();

    unlockSequence.Append(landmark.landmarkImage.DOColor(Color.white, juicyEffectDuration).SetEase(Ease.OutQuad));
    unlockSequence.Join(landmark.landmarkImage.transform.DOPunchScale(Vector3.one * juicyEffectScale, juicyEffectDuration, 10, 1));

    unlockSequence.AppendInterval(flashDelay);
    unlockSequence.AppendCallback(() => {
        if (landmark.screenFlashPanel != null)
        {
            FlashScreen(landmark.screenFlashPanel);
        }
    });

    unlockSequence.AppendCallback(() => {
        if (defaultParallaxBackground != null)
            defaultParallaxBackground.SetActive(false);

        foreach (var lm in landmarks)
        {
            if (lm.parallaxBackground != null)
                lm.parallaxBackground.SetActive(false);
        }

        if (landmark.parallaxBackground != null)
            landmark.parallaxBackground.SetActive(true);
    });

    unlockSequence.AppendInterval(postCardDelay);
    unlockSequence.AppendCallback(() => {
        if (landmark.postCardGameObject != null)
        {
            ShowPostCard(landmark);
        }
    });

    unlockSequence.AppendCallback(() => {
        ShowProgressBarForUnlock();
    });

    unlockSequence.AppendInterval(progressBarDisplayDuration + progressBarFadeDuration * 2 + flashDuration + postCardAnimationDuration + postCardDisplayDuration);
    unlockSequence.AppendCallback(() => {
        isPostCardAnimating = false;
    });
}

    void ShowProgressBarForUnlock()
    {
        if (isProgressBarShowing) return;

        isProgressBarShowing = true;
        
        if (progressBarFadeTween != null && progressBarFadeTween.IsActive())
        {
            progressBarFadeTween.Kill();
        }
        
        progressBarCanvasGroup.DOKill();

        progressBarFadeTween = DOTween.Sequence()
            .Append(progressBarCanvasGroup.DOFade(1, progressBarFadeDuration))
            .AppendCallback(() => {
                UpdateProgressBarValue();
            })
            .AppendInterval(progressBarDisplayDuration)
            .Append(progressBarCanvasGroup.DOFade(0, progressBarFadeDuration))
            .OnComplete(() => {
                isProgressBarShowing = false;
            });
    }

    void FlashScreen(GameObject flashPanel)
    {
        flashPanel.SetActive(true);
        CanvasGroup canvasGroup = flashPanel.GetComponent<CanvasGroup>();
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            
            Sequence flashSequence = DOTween.Sequence();
            flashSequence.Append(canvasGroup.DOFade(1f, flashDuration * 0.3f).SetEase(Ease.OutQuad));
            flashSequence.Append(canvasGroup.DOFade(0f, flashDuration * 0.7f).SetEase(Ease.OutQuad));
            flashSequence.OnComplete(() => {
                flashPanel.SetActive(false);
            });
        }
        else
        {
            DOVirtual.DelayedCall(flashDuration, () => {
                flashPanel.SetActive(false);
            });
        }
    }

    void ShowPostCard(Landmark landmark)
    {
        if (landmark.postCardGameObject == null) return;
        
        landmark.postCardGameObject.SetActive(true);
        landmark.postCardGameObject.transform.position = landmark.hiddenPosition;
        
        Sequence postCardSequence = DOTween.Sequence();
        postCardSequence.Append(landmark.postCardGameObject.transform.DOMove(landmark.originalPosition, postCardAnimationDuration).SetEase(postCardEase));
        postCardSequence.AppendInterval(postCardDisplayDuration);
        postCardSequence.Append(landmark.postCardGameObject.transform.DOMove(landmark.hiddenPosition, postCardAnimationDuration * 0.5f).SetEase(Ease.InBack));
        postCardSequence.OnComplete(() => {
            landmark.postCardGameObject.SetActive(false);
        });
    }

    void ShowProgressBarWithFill()
    {
        if (isProgressBarShowing) return;

        isProgressBarShowing = true;
        
        if (progressBarFadeTween != null && progressBarFadeTween.IsActive())
        {
            progressBarFadeTween.Kill();
        }
        
        progressBarCanvasGroup.DOKill();

        progressBarFadeTween = DOTween.Sequence()
            .Append(progressBarCanvasGroup.DOFade(1, progressBarFadeDuration))
            .AppendCallback(() => {
                UpdateProgressBarValue();
            })
            .AppendInterval(progressBarDisplayDuration)
            .Append(progressBarCanvasGroup.DOFade(0, progressBarFadeDuration))
            .OnComplete(() => {
                isProgressBarShowing = false;
            });
    }
    
    [ContextMenu("Setup Postcard Positions")]
    void SetupPostcardPositions()
    {
        foreach (var landmark in landmarks)
        {
            if (landmark.postCardGameObject != null)
            {
                landmark.originalPosition = landmark.postCardGameObject.transform.position;
                landmark.hiddenPosition = landmark.originalPosition + Vector3.down * 1000f;
                Debug.Log($"Setup {landmark.landmarkName}: Original={landmark.originalPosition}, Hidden={landmark.hiddenPosition}");
            }
        }
    }
}