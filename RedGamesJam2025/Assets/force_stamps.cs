using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class StampAnimationController : MonoBehaviour
{
    [Header("Animation Settings")]
    public float stampSlideDuration = 0.8f;
    public float stampPopDuration = 0.4f;
    public float stampSlideDistance = 500f;
    public float stampDisplayTime = 1.5f;
    public float stampHideDuration = 0.6f;

    [Header("Debug")]
    public bool enableDebugLogs = true;

    private Vector3[] stampOriginalPositions;
    private Vector3[] stampStartPositions;
    private RawImage[] stampImages;
    private bool isInitialized = false;

    void Start()
    {
        InitializeStampAnimations();
    }

    public void InitializeStampAnimations()
    {
        // Get stamp images from the Stamp_Manager
        Stamp_Manager stampManager = GetComponent<Stamp_Manager>();
        if (stampManager != null && stampManager.stampImages != null)
        {
            stampImages = stampManager.stampImages;
            SetupStampPositions();
            isInitialized = true;
            
            if (enableDebugLogs)
                Debug.Log($"StampAnimationController initialized with {stampImages.Length} stamps");
        }
        else
        {
            Debug.LogError("StampAnimationController: Could not find Stamp_Manager or stampImages!");
        }
    }

    void SetupStampPositions()
    {
        if (stampImages == null || stampImages.Length == 0) return;

        stampOriginalPositions = new Vector3[stampImages.Length];
        stampStartPositions = new Vector3[stampImages.Length];

        for (int i = 0; i < stampImages.Length; i++)
        {
            if (stampImages[i] != null)
            {
                // Store original position (center of screen)
                stampOriginalPositions[i] = stampImages[i].transform.position;
                
                // Calculate start position (below screen)
                stampStartPositions[i] = stampOriginalPositions[i] + Vector3.down * stampSlideDistance;
                
                // Initially hide all stamps
                stampImages[i].gameObject.SetActive(false);
                
                if (enableDebugLogs)
                    Debug.Log($"Stamp {i} - Original: {stampOriginalPositions[i]}, Start: {stampStartPositions[i]}");
            }
        }
    }

    public void ShowStamp(int stampIndex)
    {
        if (!isInitialized)
        {
            Debug.LogError("StampAnimationController not initialized!");
            return;
        }

        if (stampImages == null || stampIndex >= stampImages.Length || stampIndex < 0)
        {
            Debug.LogError($"Invalid stamp index: {stampIndex}");
            return;
        }

        RawImage currentStamp = stampImages[stampIndex];
        if (currentStamp == null)
        {
            Debug.LogError($"Stamp at index {stampIndex} is null!");
            return;
        }

        if (enableDebugLogs)
            Debug.Log($"Showing stamp {stampIndex}");

        StartCoroutine(AnimateStamp(currentStamp, stampIndex));
    }

    private System.Collections.IEnumerator AnimateStamp(RawImage stamp, int stampIndex)
    {
        // Kill any existing animations
        stamp.transform.DOKill();

        // Reset stamp properties
        stamp.color = Color.white;
        stamp.transform.localScale = Vector3.one;
        stamp.transform.rotation = Quaternion.identity;

        // Position stamp below screen
        stamp.transform.position = stampStartPositions[stampIndex];
        
        // Activate the stamp
        stamp.gameObject.SetActive(true);

        if (enableDebugLogs)
            Debug.Log($"Stamp {stampIndex} activated at position: {stamp.transform.position}");

        // Wait one frame to ensure activation
        yield return null;

        // Create animation sequence
        Sequence stampSequence = DOTween.Sequence();

        // Slide up from bottom to center
        stampSequence.Append(stamp.transform.DOMove(stampOriginalPositions[stampIndex], stampSlideDuration)
            .SetEase(Ease.OutBack, 1.2f));

        // Add bounce effect
        stampSequence.AppendInterval(0.1f);
        stampSequence.Append(stamp.transform.DOPunchScale(Vector3.one * 0.15f, stampPopDuration, 8, 0.2f)
            .SetEase(Ease.OutElastic));

        // Wait at center
        stampSequence.AppendInterval(stampDisplayTime);

        // Hide stamp
        stampSequence.AppendCallback(() => HideStamp(stampIndex));

        // Start the sequence
        stampSequence.Play();
    }

    public void HideStamp(int stampIndex)
    {
        if (!isInitialized || stampImages == null || stampIndex >= stampImages.Length || stampIndex < 0)
            return;

        RawImage currentStamp = stampImages[stampIndex];
        if (currentStamp == null) return;

        if (enableDebugLogs)
            Debug.Log($"Hiding stamp {stampIndex}");

        currentStamp.transform.DOKill();

        // Slide down off screen
        currentStamp.transform.DOMove(stampStartPositions[stampIndex], stampHideDuration)
            .SetEase(Ease.InBack, 1.2f)
            .OnComplete(() =>
            {
                currentStamp.gameObject.SetActive(false);
                currentStamp.transform.rotation = Quaternion.identity;
                currentStamp.transform.position = stampOriginalPositions[stampIndex];
                
                if (enableDebugLogs)
                    Debug.Log($"Stamp {stampIndex} hidden and reset");
            });
    }

    public void HideAllStamps()
    {
        if (!isInitialized || stampImages == null) return;

        for (int i = 0; i < stampImages.Length; i++)
        {
            if (stampImages[i] != null)
            {
                stampImages[i].transform.DOKill();
                stampImages[i].gameObject.SetActive(false);
                stampImages[i].transform.position = stampOriginalPositions[i];
                stampImages[i].transform.rotation = Quaternion.identity;
                stampImages[i].transform.localScale = Vector3.one;
            }
        }
    }

    // Test method to manually show a stamp
    [ContextMenu("Test Show Stamp 0")]
    public void TestShowStamp0()
    {
        ShowStamp(0);
    }

    [ContextMenu("Test Show Stamp 1")]
    public void TestShowStamp1()
    {
        ShowStamp(1);
    }

    [ContextMenu("Test Hide All Stamps")]
    public void TestHideAllStamps()
    {
        HideAllStamps();
    }

    // Public method to check if stamps are properly set up
    public bool AreStampsInitialized()
    {
        return isInitialized && stampImages != null && stampImages.Length > 0;
    }

    // Get stamp count
    public int GetStampCount()
    {
        return stampImages != null ? stampImages.Length : 0;
    }
}