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
    public float waitAfterThreePassesDelay = 2f;
    public float flashDuration = 0.5f;

    private StampAnimationController stampAnimationController;
    private int currentCountryIndex = 0;
    private bool hasTriggered = false;
    private int lastPassCount = 0;

    void Start()
    {
        SetupStampManager();
    }

    void SetupStampManager()
    {
        // Get the animation controller
        stampAnimationController = GetComponent<StampAnimationController>();
        if (stampAnimationController == null)
        {
            Debug.LogError("StampAnimationController not found! Please add it to the same GameObject.");
            return;
        }

        // Setup flash panel
        if (flashPanel != null)
        {
            flashPanel.alpha = 0f;
            flashPanel.gameObject.SetActive(false);
        }

        // Setup country nodes
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

        Debug.Log("Stamp_Manager setup complete");
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

            // Reset trigger when boarding passes reset
            if (passes < lastPassCount && passes == 0)
            {
                hasTriggered = false;
            }

            lastPassCount = passes;
        }
    }

    void TriggerStampSequence()
    {
        if (stampAnimationController == null || !stampAnimationController.AreStampsInitialized())
        {
            Debug.LogError("Cannot trigger stamp sequence - animation controller not ready!");
            return;
        }

        Debug.Log("Triggering stamp sequence...");

        Sequence stampSequence = DOTween.Sequence();

        // Wait after collecting 3 boarding passes
        stampSequence.AppendInterval(waitAfterThreePassesDelay);

        // Screen flash
        stampSequence.AppendCallback(() => TriggerScreenFlash());

        // Wait for flash to complete
        stampSequence.AppendInterval(flashDuration);

        // Show stamp
        stampSequence.AppendCallback(() => ShowCurrentStamp());
    }

    void TriggerScreenFlash()
    {
        if (flashPanel == null) 
        {
            Debug.Log("No flash panel - skipping flash and updating country nodes");
            UpdateCountryNodes();
            return;
        }

        Debug.Log("Triggering screen flash");

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

    void ShowCurrentStamp()
    {
        if (stampAnimationController == null)
        {
            Debug.LogError("No stamp animation controller!");
            return;
        }

        if (currentCountryIndex >= stampAnimationController.GetStampCount())
        {
            Debug.LogWarning($"Current country index {currentCountryIndex} exceeds stamp count {stampAnimationController.GetStampCount()}");
            return;
        }

        Debug.Log($"Showing stamp for country index: {currentCountryIndex}");
        stampAnimationController.ShowStamp(currentCountryIndex);
    }

    void UpdateCountryNodes()
    {
        if (countryNodes == null || countryNodes.Length == 0) 
        {
            Debug.Log("No country nodes to update");
            return;
        }

        Debug.Log($"Updating country nodes - current index: {currentCountryIndex}");

        // Disable default node when first country is unlocked
        if (currentCountryIndex == 0 && defaultNode != null)
        {
            defaultNode.SetActive(false);
            Debug.Log("Disabled default node");
        }

        // Disable previous country node (if not the first unlock)
        if (currentCountryIndex > 0 && currentCountryIndex <= countryNodes.Length && countryNodes[currentCountryIndex - 1] != null)
        {
            countryNodes[currentCountryIndex - 1].SetActive(false);
            Debug.Log($"Disabled country node {currentCountryIndex - 1}");
        }

        // Enable new country node
        if (currentCountryIndex < countryNodes.Length && countryNodes[currentCountryIndex] != null)
        {
            countryNodes[currentCountryIndex].SetActive(true);
            Debug.Log($"Enabled country node {currentCountryIndex}");
        }

        // Move to next country index
        currentCountryIndex++;
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

    [ContextMenu("Manual Trigger Stamp")]
    public void TestManualTrigger()
    {
        ManualTriggerStamp();
    }

    public void ResetStampManager()
    {
        Debug.Log("Resetting Stamp Manager");
        
        hasTriggered = false;
        currentCountryIndex = 0;
        lastPassCount = 0;

        // Reset animation controller
        if (stampAnimationController != null)
        {
            stampAnimationController.HideAllStamps();
        }

        // Reset flash panel
        if (flashPanel != null)
        {
            flashPanel.DOKill();
            flashPanel.gameObject.SetActive(false);
            flashPanel.alpha = 0f;
        }

        // Reset country nodes
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

    [ContextMenu("Reset Stamp Manager")]
    public void TestReset()
    {
        ResetStampManager();
    }
}