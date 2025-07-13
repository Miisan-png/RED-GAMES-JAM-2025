using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BP_Manager : MonoBehaviour
{
    [Header("UI References")]
    public RawImage[] boardingPassImages;
    
    [Header("Animation Settings")]
    public float slideInDuration = 0.8f;
    public float slideOutDuration = 0.6f;
    public float slideInDelay = 0.2f;
    public float slideOutDelay = 1.2f;
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
            boardingPassImages[i].transform.localScale = Vector3.zero;
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
        for (int i = 0; i < boardingPassImages.Length; i++)
        {
            int index = i;
            
            boardingPassImages[index].transform.DOKill();
            boardingPassImages[index].DOKill();
            boardingPassImages[index].transform.localScale = Vector3.zero;
            boardingPassImages[index].transform.position = originalPositions[index];
            
            Sequence passSequence = DOTween.Sequence();
            
            passSequence.AppendInterval(index * slideInDelay);
            
            passSequence.AppendCallback(() => {
                boardingPassImages[index].transform.DOScale(1f, slideInDuration)
                    .SetEase(Ease.OutBack);
            });
            
            if (index < passCount)
            {
                passSequence.AppendInterval(slideInDuration * 0.8f);
                
                passSequence.AppendCallback(() => {
                    boardingPassImages[index].DOColor(whiteColor, popDuration * 0.3f)
                        .SetEase(Ease.OutQuint);
                    
                    boardingPassImages[index].transform.DOShakeRotation(popDuration * 0.6f, 15f, 20, 90f)
                        .SetEase(Ease.OutQuad);
                        
                    boardingPassImages[index].transform.DOPunchPosition(Vector3.up * 20f, 0.8f, 10, 0.5f)
                        .SetDelay(popDuration * 0.2f);
                });
                
                passSequence.AppendInterval(1.5f);
            }
            else
            {
                passSequence.AppendInterval(slideOutDelay);
            }
            
            passSequence.AppendCallback(() => HidePass(index));
        }
    }
    
    void HidePass(int index)
    {
        boardingPassImages[index].transform.DOKill();
        boardingPassImages[index].DOKill();
        
        boardingPassImages[index].transform.DOScale(0f, slideOutDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() => {
                boardingPassImages[index].color = darkColor;
                boardingPassImages[index].transform.rotation = Quaternion.identity;
                boardingPassImages[index].transform.position = originalPositions[index];
            });
    }
}