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

    [Header("UI Update Settings")]
    [Tooltip("Show the progress bar after this many coins are collected.")]
    public int showBarAfterEvery = 5;

    [Header("Animation Settings")]
    public float juicyEffectDuration = 0.6f;
    public float juicyEffectScale = 1.2f;
    public float progressBarFadeDuration = 0.5f;
    public float progressBarDisplayDuration = 2.0f;
    
    [Header("Flash Settings")]
    public float flashDuration = 0.3f;
    public float flashIntensity = 0.8f;
    
    [Header("Postcard Animation Settings")]
    public float postCardAnimationDuration = 1.0f;
    public float postCardDisplayDuration = 2.0f;
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

        if (landmarks.Length > 0)
        {
            landmarks = landmarks.OrderBy(l => l.coinsRequired).ToArray();
        }
        
        InitializeLandmarks();
        UpdateUI();
    }

    public void OnCoinsUpdated(int coinsAdded)
    {
        coinCounterForBar += coinsAdded;
        
        // Check for unlocks first
        CheckForUnlocks();
        
        // Only show progress bar every 5 coins and if no landmark was just unlocked
        if (coinCounterForBar >= showBarAfterEvery && !isProgressBarShowing && !isPostCardAnimating)
        {
            ShowProgressBarWithFill();
            coinCounterForBar = 0; // Reset counter after showing
        }
    }

    void InitializeLandmarks()
    {
        int totalCoins = Game_Manager.Instance.GetTotalCoins();
        int coinsUsed = 0;
        
        foreach (var landmark in landmarks)
        {
            // Initialize postcard positions
            if (landmark.postCardGameObject != null)
            {
                landmark.originalPosition = landmark.postCardGameObject.transform.position;
                landmark.hiddenPosition = landmark.originalPosition + Vector3.down * 1000f; // Hide below screen
                landmark.postCardGameObject.transform.position = landmark.hiddenPosition;
                landmark.postCardGameObject.SetActive(false);
            }
            
            // Initialize screen flash panel
            if (landmark.screenFlashPanel != null)
            {
                landmark.screenFlashPanel.SetActive(true);
                CanvasGroup canvasGroup = landmark.screenFlashPanel.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = landmark.screenFlashPanel.AddComponent<CanvasGroup>();
                }
                canvasGroup.alpha = 0f; // Start completely transparent
                landmark.screenFlashPanel.SetActive(false);
            }
            
            // Set landmark image state based on cumulative coin system
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

    void UpdateUI()
    {
        int totalCoins = Game_Manager.Instance.GetTotalCoins();
        
        // Find the next landmark to unlock
        Landmark nextLandmark = landmarks.FirstOrDefault(l => !l.isUnlocked);
        
        if (nextLandmark != null)
        {
            // Calculate progress for the current landmark (from 0 to required coins)
            int coinsNeededForCurrentLandmark = nextLandmark.coinsRequired;
            
            // Find how many coins we've collected since the last landmark
            int previousLandmarkIndex = System.Array.IndexOf(landmarks, nextLandmark) - 1;
            int coinsUsedByPreviousLandmarks = 0;
            
            // Sum up coins used by all previous landmarks
            for (int i = 0; i <= previousLandmarkIndex; i++)
            {
                if (i >= 0 && landmarks[i].isUnlocked)
                {
                    coinsUsedByPreviousLandmarks += landmarks[i].coinsRequired;
                }
            }
            
            // Calculate coins available for current landmark
            int coinsForCurrentLandmark = totalCoins - coinsUsedByPreviousLandmarks;
            
            // Calculate progress (0 to 1)
            float progress = (float)coinsForCurrentLandmark / coinsNeededForCurrentLandmark;
            progressBar.SetValue(Mathf.Clamp01(progress), true);
        }
        else
        {
            // All landmarks unlocked
            progressBar.SetValue(1f, false);
        }
    }

    void CheckForUnlocks()
    {
        int totalCoins = Game_Manager.Instance.GetTotalCoins();
        int coinsUsed = 0;
        
        // Check each landmark in order
        foreach (var landmark in landmarks)
        {
            if (!landmark.isUnlocked)
            {
                // Calculate coins available for this landmark
                int coinsAvailableForThisLandmark = totalCoins - coinsUsed;
                
                // Check if we have enough coins for this landmark
                if (coinsAvailableForThisLandmark >= landmark.coinsRequired)
                {
                    UnlockLandmark(landmark);
                    coinsUsed += landmark.coinsRequired; // Add coins used by this landmark
                }
                else
                {
                    // Not enough coins for this landmark, stop checking
                    break;
                }
            }
            else
            {
                // This landmark is already unlocked, add its coin cost to used coins
                coinsUsed += landmark.coinsRequired;
            }
        }
    }

    void UnlockLandmark(Landmark landmark)
    {
        if (isPostCardAnimating) return; // Prevent multiple animations at once
        
        landmark.isUnlocked = true;
        isPostCardAnimating = true;
        
        // Reset coin counter and show progress bar
        coinCounterForBar = 0;
        
        // Create unlock sequence
        Sequence unlockSequence = DOTween.Sequence();
        
        // 1. Flash the screen
        if (landmark.screenFlashPanel != null)
        {
            unlockSequence.AppendCallback(() => FlashScreen(landmark.screenFlashPanel));
        }
        
        // 2. Wait for flash to complete
        unlockSequence.AppendInterval(flashDuration);
        
        // 3. Animate landmark image
        unlockSequence.Append(landmark.landmarkImage.DOColor(Color.white, juicyEffectDuration).SetEase(Ease.OutQuad));
        unlockSequence.Join(landmark.landmarkImage.transform.DOPunchScale(Vector3.one * juicyEffectScale, juicyEffectDuration, 10, 1));
        
        // 4. Show postcard animation
        if (landmark.postCardGameObject != null)
        {
            unlockSequence.AppendCallback(() => ShowPostCard(landmark));
        }
        
        // 5. Show progress bar after postcard animation
        unlockSequence.AppendInterval(postCardAnimationDuration + postCardDisplayDuration);
        unlockSequence.AppendCallback(() => {
            ShowProgressBar();
            isPostCardAnimating = false;
        });
    }

    void FlashScreen(GameObject flashPanel)
    {
        flashPanel.SetActive(true);
        CanvasGroup canvasGroup = flashPanel.GetComponent<CanvasGroup>();
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            
            // Flash effect: 0 -> 1 -> 0
            Sequence flashSequence = DOTween.Sequence();
            flashSequence.Append(canvasGroup.DOFade(1f, flashDuration * 0.2f).SetEase(Ease.OutQuad));
            flashSequence.Append(canvasGroup.DOFade(0f, flashDuration * 0.8f).SetEase(Ease.OutQuad));
            flashSequence.OnComplete(() => {
                flashPanel.SetActive(false);
            });
        }
        else
        {
            // Fallback if no CanvasGroup
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
        
        // Animate postcard from bottom to original position
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
        
        // Kill any existing animation
        if (progressBarFadeTween != null && progressBarFadeTween.IsActive())
        {
            progressBarFadeTween.Kill();
        }
        
        progressBarCanvasGroup.DOKill();

        // Show progress bar and animate the fill
        progressBarFadeTween = DOTween.Sequence()
            .Append(progressBarCanvasGroup.DOFade(1, progressBarFadeDuration))
            .AppendCallback(() => {
                // Update the progress bar with animation when it's visible
                UpdateUI();
            })
            .AppendInterval(progressBarDisplayDuration)
            .Append(progressBarCanvasGroup.DOFade(0, progressBarFadeDuration))
            .OnComplete(() => {
                isProgressBarShowing = false;
            });
    }

    void ShowProgressBar()
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
            .AppendInterval(progressBarDisplayDuration)
            .Append(progressBarCanvasGroup.DOFade(0, progressBarFadeDuration))
            .OnComplete(() => {
                isProgressBarShowing = false;
            });
    }
    
    // Helper method to setup postcard positions in editor
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