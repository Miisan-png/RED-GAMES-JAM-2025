using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Boarding_Pass_Manager : MonoBehaviour
{
    [Header("UI References")]
    public RawImage[] boardingPassImages;
    
    [Header("Animation Settings")]
    public float slideInDuration = 0.8f;
    public float slideOutDuration = 0.6f;
    public float slideInDelay = 0.2f;
    public float slideOutDelay = 2f;
    public float popDuration = 0.5f;
    
    [Header("Positions")]
    public float slideDistance = 200f;
    
    private Color darkColor = new Color(0.2117647f, 0.1960784f, 0.2745098f, 1f);
    private Color whiteColor = Color.white;
    private Vector3[] originalPositions;
    private int currentPasses = 0;
    
    void Start()
    {
        SetupBoardingPasses();
    }
    
    void SetupBoardingPasses()
    {
        if (boardingPassImages == null || boardingPassImages.Length == 0) return;
        
        originalPositions = new Vector3[boardingPassImages.Length];
        
        for (int i = 0; i < boardingPassImages.Length; i++)
        {
            originalPositions[i] = boardingPassImages[i].transform.position;
            boardingPassImages[i].transform.position = new Vector3(
                originalPositions[i].x, 
                originalPositions[i].y - slideDistance, 
                originalPositions[i].z
            );
            boardingPassImages[i].color = darkColor;
        }
    }
    
    void Update()
    {
        if (Game_Manager.Instance != null)
        {
            int passes = Game_Manager.Instance.boarding_pass;
            if (passes != currentPasses)
            {
                UpdateBoardingPasses(passes);
                currentPasses = passes;
            }
        }
    }
    
    void UpdateBoardingPasses(int passCount)
    {
        ShowBoardingPasses(passCount);
    }
    
    void ShowBoardingPasses(int passCount)
    {
        Sequence showSequence = DOTween.Sequence();
        
        for (int i = 0; i < boardingPassImages.Length; i++)
        {
            int index = i;
            
            showSequence.AppendCallback(() => {
                boardingPassImages[index].transform.DOMoveY(originalPositions[index].y, slideInDuration)
                    .SetEase(Ease.OutBack);
            });
            
            if (i < passCount)
            {
                showSequence.AppendCallback(() => {
                    boardingPassImages[index].transform.DOScale(1.3f, popDuration * 0.3f)
                        .SetEase(Ease.OutBack)
                        .OnComplete(() => {
                            boardingPassImages[index].transform.DOScale(1f, popDuration * 0.7f)
                                .SetEase(Ease.InBack);
                        });
                    
                    boardingPassImages[index].DOColor(whiteColor, popDuration)
                        .SetEase(Ease.OutQuad);
                });
            }
            
            showSequence.AppendInterval(slideInDelay);
        }
        
        showSequence.AppendInterval(slideOutDelay);
        showSequence.AppendCallback(() => HideBoardingPasses());
    }
    
    void HideBoardingPasses()
    {
        for (int i = 0; i < boardingPassImages.Length; i++)
        {
            int index = i;
            boardingPassImages[index].transform.DOMoveY(originalPositions[index].y - slideDistance, slideOutDuration)
                .SetEase(Ease.InBack)
                .SetDelay(index * 0.1f)
                .OnComplete(() => {
                    boardingPassImages[index].color = darkColor;
                });
        }
    }
}