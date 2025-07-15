using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using DG.Tweening;

public class GameOver : MonoBehaviour
{
    [SerializeField] private RawImage homeButton;
    [SerializeField] private RawImage restartButton;

    Vector3 originalScale = new Vector3(1.5f, 1.5f, 1f);


    void Start()
    {
        SetupButtons();
        AnimateMenuIn();
    }

void SetupButtons()
{
    homeButton.transform.localScale = Vector3.zero;
    restartButton.transform.localScale = Vector3.zero;

    // Prevent multiple EventTriggers
    if (!homeButton.GetComponent<EventTrigger>())
        AddTouchHandler(homeButton, OnHomeClick);

    if (!restartButton.GetComponent<EventTrigger>())
        AddTouchHandler(restartButton, OnRestartClick);
}

    void AnimateMenuIn()
    {
        // Set canvas sorting to 200 when this animation starts
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
            canvas = gameObject.AddComponent<Canvas>();

        canvas.overrideSorting = true;
        canvas.sortingOrder = 200;

        // Animate buttons to original scale (1.5)
        homeButton.transform.DOScale(originalScale, 0.3f).SetEase(Ease.OutBack).SetDelay(0.1f);
        restartButton.transform.DOScale(originalScale, 0.3f).SetEase(Ease.OutBack).SetDelay(0.2f);

        homeButton.color = new Color(homeButton.color.r, homeButton.color.g, homeButton.color.b, 0f);
        restartButton.color = new Color(restartButton.color.r, restartButton.color.g, restartButton.color.b, 0f);

        homeButton.DOFade(1f, 0.3f).SetDelay(0.1f);
        restartButton.DOFade(1f, 0.3f).SetDelay(0.2f);
}

    void AddTouchHandler(RawImage button, System.Action onClick)
    {
        EventTrigger eventTrigger = button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((data) =>
        {
            button.transform.DOScale(0.9f, 0.1f);
        });

        EventTrigger.Entry pointerUp = new EventTrigger.Entry();
        pointerUp.eventID = EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((data) =>
        {
            button.transform.DOScale(1f, 0.1f).OnComplete(() => onClick());
        });

        eventTrigger.triggers.Add(pointerDown);
        eventTrigger.triggers.Add(pointerUp);
    }



    void OnHomeClick()
    {
        Debug.Log("Home");
        AnimateOut(() =>
        {
            SceneManager.LoadScene(0);
        });
    }

    void OnRestartClick()
    {
        AnimateOut(() =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        });
        Game_Manager.Instance.ResetCurrentGame(); 

    }

    void AnimateOut(System.Action onComplete)
    {
        homeButton.transform.DOScale(0f, 0.2f).SetEase(Ease.InBack);
        restartButton.transform.DOScale(0f, 0.2f).SetEase(Ease.InBack).SetDelay(0.1f);

        homeButton.DOFade(0f, 0.2f);
        restartButton.DOFade(0f, 0.2f).SetDelay(0.1f).OnComplete(() => onComplete());
    }
}
