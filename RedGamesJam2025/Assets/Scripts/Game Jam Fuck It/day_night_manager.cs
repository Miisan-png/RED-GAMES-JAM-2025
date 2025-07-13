using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class day_night_manager : MonoBehaviour
{
    [Header("Time Settings")]
    public float dayDuration = 60f;
    public float currentTime = 0f;
    [Range(0f, 1f)]
    public float timeOfDay = 0f;
    
    [Header("Background")]
    public SpriteRenderer backgroundSprite;
    public Gradient dayNightGradient;
    
    [Header("Stars")]
    public GameObject starPrefab;
    public Transform starContainer;
    public int maxStars = 50;
    public float starSpawnRadius = 10f;
    public float starPulseDuration = 2f;
    public float starTwinkleDuration = 0.5f;
    
    [Header("Colors")]
    public Color morningColor = new Color(1f, 0.8f, 0.6f);
    public Color dayColor = Color.white;
    public Color eveningColor = new Color(1f, 0.7f, 0.4f);
    public Color nightColor = new Color(0.2f, 0.2f, 0.4f);
    
    private List<GameObject> stars = new List<GameObject>();
    private bool starsActive = false;
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
        if (starContainer == null)
            starContainer = transform;
            
        SetupDayNightGradient();
        SpawnStars();
    }
    
    void Update()
    {
        UpdateTime();
        UpdateBackground();
        UpdateStars();
    }
    
    void UpdateTime()
    {
        currentTime += Time.deltaTime;
        timeOfDay = (currentTime % dayDuration) / dayDuration;
    }
    
    void SetupDayNightGradient()
    {
        if (dayNightGradient == null)
        {
            dayNightGradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[5];
            colorKeys[0] = new GradientColorKey(nightColor, 0f);
            colorKeys[1] = new GradientColorKey(morningColor, 0.25f);
            colorKeys[2] = new GradientColorKey(dayColor, 0.5f);
            colorKeys[3] = new GradientColorKey(eveningColor, 0.75f);
            colorKeys[4] = new GradientColorKey(nightColor, 1f);
            
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1f, 0f);
            alphaKeys[1] = new GradientAlphaKey(1f, 1f);
            
            dayNightGradient.SetKeys(colorKeys, alphaKeys);
        }
    }
    
    void UpdateBackground()
    {
        if (backgroundSprite != null)
        {
            Color currentColor = dayNightGradient.Evaluate(timeOfDay);
            backgroundSprite.color = currentColor;
        }
    }
    
    void SpawnStars()
    {
        if (starPrefab == null || starContainer == null) return;
        
        for (int i = 0; i < maxStars; i++)
        {
            Vector3 randomPos = GetRandomStarPosition();
            GameObject star = Instantiate(starPrefab, randomPos, Quaternion.identity, starContainer);
            
            Vector3 originalScale = star.transform.localScale;
            star.transform.localScale = Vector3.zero;
            
            stars.Add(star);
            StartStarAnimation(star, originalScale);
        }
    }
    
    Vector3 GetRandomStarPosition()
    {
        if (mainCamera != null)
        {
            float screenHeight = mainCamera.orthographicSize * 2f;
            float screenWidth = screenHeight * mainCamera.aspect;
            
            float x = Random.Range(-screenWidth * 0.5f, screenWidth * 0.5f);
            float y = Random.Range(-screenHeight * 0.5f, screenHeight * 0.5f);
            
            return starContainer.position + new Vector3(x, y, 0);
        }
        else
        {
            return starContainer.position + Random.insideUnitSphere * starSpawnRadius;
        }
    }
    
    void StartStarAnimation(GameObject star, Vector3 originalScale)
    {
        star.transform.DOScale(originalScale, Random.Range(0.5f, 1.5f))
            .SetEase(Ease.OutBack)
            .SetDelay(Random.Range(0f, 2f));
        
        star.transform.DOScale(originalScale * Random.Range(0.8f, 1.2f), starPulseDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetDelay(Random.Range(0f, starPulseDuration));
        
        SpriteRenderer starRenderer = star.GetComponent<SpriteRenderer>();
        if (starRenderer != null)
        {
            starRenderer.DOFade(Random.Range(0.3f, 1f), starTwinkleDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(Random.Range(0f, starTwinkleDuration));
                
            Color starColor = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.8f, 1f);
            starRenderer.color = starColor;
        }
    }
    
    void UpdateStars()
    {
        bool shouldShowStars = IsNightTime();
        
        if (shouldShowStars && !starsActive)
        {
            ShowStars();
            starsActive = true;
        }
        else if (!shouldShowStars && starsActive)
        {
            HideStars();
            starsActive = false;
        }
    }
    
    bool IsNightTime()
    {
        return timeOfDay > 0.8f || timeOfDay < 0.2f;
    }
    
    void ShowStars()
    {
        foreach (GameObject star in stars)
        {
            if (star != null)
            {
                star.SetActive(true);
                SpriteRenderer renderer = star.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.DOFade(1f, 1f).SetEase(Ease.InOutSine);
                }
            }
        }
    }
    
    void HideStars()
    {
        foreach (GameObject star in stars)
        {
            if (star != null)
            {
                SpriteRenderer renderer = star.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.DOFade(0f, 1f).SetEase(Ease.InOutSine)
                        .OnComplete(() => star.SetActive(false));
                }
            }
        }
    }
    
    public void SetTimeOfDay(float time)
    {
        timeOfDay = Mathf.Clamp01(time);
        currentTime = timeOfDay * dayDuration;
    }
    
    public void SetDayDuration(float duration)
    {
        dayDuration = duration;
    }
    
    public bool IsDayTime()
    {
        return timeOfDay > 0.25f && timeOfDay < 0.75f;
    }
    
    public bool IsMorning()
    {
        return timeOfDay > 0.2f && timeOfDay < 0.4f;
    }
    
    public bool IsEvening()
    {
        return timeOfDay > 0.6f && timeOfDay < 0.8f;
    }
    
    public string GetTimeOfDayString()
    {
        if (IsMorning()) return "Morning";
        if (IsDayTime()) return "Day";
        if (IsEvening()) return "Evening";
        return "Night";
    }
    
    void OnDestroy()
    {
        foreach (GameObject star in stars)
        {
            if (star != null)
            {
                star.transform.DOKill();
                SpriteRenderer renderer = star.GetComponent<SpriteRenderer>();
                if (renderer != null)
                    renderer.DOKill();
            }
        }
    }
}