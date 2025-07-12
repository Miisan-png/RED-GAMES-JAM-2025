using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Progress_Bar : MonoBehaviour
{
    [Header("Progress Bar Settings")]
    [Range(0f, 1f)]
    public float value = 0f;
    
    [Header("UI References")]
    public RectTransform fillImage;
    public RectTransform fillArea;
    public Text progressText;
    
    [Header("Animation Settings")]
    public float animationDuration = 0.4f;
    public Ease animationEase = Ease.OutQuad;
    
    [Header("Visual Effects")]
    public bool useColorGradient = true;
    public Color startColor = Color.red;
    public Color endColor = Color.green;
    public bool punchScaleOnChange = true;
    public float punchScaleIntensity = 0.05f;

    private float previousValue = -1f;
    private float maxWidth;
    private Image fillImageComponent;
    private Tween currentAnimation;

    void Start()
    {
        InitializeProgressBar();
        SetValue(value, false);
        previousValue = value;
    }

    void InitializeProgressBar()
    {
        if (fillImage != null)
        {
            fillImage.pivot = new Vector2(0, 0.5f);
            fillImageComponent = fillImage.GetComponent<Image>();
        }

        if (fillArea != null)
        {
            maxWidth = fillArea.rect.width;
        }
        else if (fillImage != null)
        {
            maxWidth = fillImage.rect.width;
        }
        
        if (fillImage != null)
        {
            fillImage.sizeDelta = new Vector2(0, fillImage.sizeDelta.y);
        }
    }

    public void SetValue(float newValue, bool animate = true)
    {
        float targetValue = Mathf.Clamp01(newValue);
        value = targetValue;
        UpdateProgressBar(targetValue, animate);
        previousValue = value;
    }

    private void UpdateProgressBar(float targetValue, bool animate = true)
    {
        if (fillImage == null) return;

        if (currentAnimation != null && currentAnimation.IsActive())
        {
            currentAnimation.Kill();
        }

        float targetWidth = maxWidth * targetValue;
        Vector2 newSize = new Vector2(targetWidth, fillImage.sizeDelta.y);

        if (animate)
        {
            currentAnimation = fillImage.DOSizeDelta(newSize, animationDuration)
                .SetEase(animationEase)
                .OnComplete(() => {
                    currentAnimation = null;
                });

            if (punchScaleOnChange && targetValue > previousValue)
            {
                transform.DOPunchScale(Vector3.one * punchScaleIntensity, animationDuration * 0.5f, 5, 0.5f);
            }
        }
        else
        {
            fillImage.sizeDelta = newSize;
        }

        if (useColorGradient && fillImageComponent != null)
        {
            Color currentColor = Color.Lerp(startColor, endColor, targetValue);
            
            if (animate)
            {
                fillImageComponent.DOColor(currentColor, animationDuration);
            }
            else
            {
                fillImageComponent.color = currentColor;
            }
        }

        if (progressText != null)
        {
            float percentage = targetValue * 100f;
            progressText.text = $"{percentage:F0}%";
        }
    }

    public void ResetProgress(bool animate = true)
    {
        SetValue(0f, animate);
    }

    public void FillComplete(bool animate = true)
    {
        SetValue(1f, animate);
    }

    public void PulseEffect(float intensity = 0.1f, float duration = 0.5f)
    {
        transform.DOPunchScale(Vector3.one * intensity, duration, 5, 0.5f);
    }

    void OnValidate()
    {
        value = Mathf.Clamp01(value);
    }
}