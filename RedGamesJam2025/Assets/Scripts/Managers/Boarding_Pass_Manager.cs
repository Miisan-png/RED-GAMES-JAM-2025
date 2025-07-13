using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Boarding_Pass_Manager : MonoBehaviour
{
    [Header("UI References")]
    public RawImage[] boardingPassImages;
    public CanvasGroup screenFlashPanel;
    
    [Header("Animation Settings")]
    public float slideInDuration = 0.8f;
    public float slideOutDuration = 0.6f;
    public float slideInDelay = 0.2f;
    public float slideOutDelay = 1.2f;
    public float popDuration = 0.5f;
    
    [Header("Positions")]
    public float slideDistance = 200f;
    
    [Header("Map System")]
    public GameObject defaultNode; // The default node that gets disabled
    public GameObject[] mapNodes;
    public Sprite[] countryStamps;
    public RawImage stampPostcard;
    
    [Header("Stamp Animation")]
    public float stampSlideDuration = 0.8f;
    public float stampSlideDistance = 300f; // Distance from bottom
    
    private Color darkColor = new Color(0.2117647f, 0.1960784f, 0.2745098f, 1f);
    private Color whiteColor = Color.white;
    private Vector3[] originalPositions;
    private Vector3 stampOriginalPosition;
    private Vector3 stampBottomPosition;
    private int currentPasses = 0;
    private int mapProgress = 0;
    
    void Start()
    {
        SetupBoardingPasses();
        InitializeMapSystem();
        SetupStampPositions();
    }
    
    void SetupStampPositions()
    {
        if (stampPostcard != null)
        {
            stampOriginalPosition = stampPostcard.transform.position;
            stampBottomPosition = stampOriginalPosition + Vector3.down * stampSlideDistance;
            // Start stamp at bottom position
            stampPostcard.transform.position = stampBottomPosition;
        }
    }
    
    void InitializeMapSystem()
    {
        // Enable default node at start
        if (defaultNode != null)
        {
            defaultNode.SetActive(true);
        }
        
        // Disable all map nodes initially
        for (int i = 0; i < mapNodes.Length; i++)
        {
            mapNodes[i].SetActive(false);
        }
        
        // Set initial stamp
        if (stampPostcard != null && countryStamps.Length > 0)
        {
            stampPostcard.texture = countryStamps[0].texture;
        }
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
                
                if (passes == 3)
                {
                    TriggerMapProgression();
                }
            }
        }
    }
    
    void TriggerMapProgression()
    {
        ScreenFlash();
        DOTween.Sequence()
            .AppendInterval(0.5f)
            .AppendCallback(() => ProgressMap());
    }
    
    void ScreenFlash()
    {
        if (screenFlashPanel != null)
        {
            // Make sure the panel is active and visible
            screenFlashPanel.gameObject.SetActive(true);
            screenFlashPanel.alpha = 0f;
            
            screenFlashPanel.DOFade(1f, 0.1f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => {
                    screenFlashPanel.DOFade(0f, 0.3f)
                        .SetEase(Ease.InQuad)
                        .OnComplete(() => {
                            screenFlashPanel.gameObject.SetActive(false);
                        });
                });
        }
    }
    
    void ProgressMap()
    {
        // Disable default node on first progression
        if (mapProgress == 0 && defaultNode != null)
        {
            defaultNode.SetActive(false);
        }
        
        // Enable next map node
        if (mapProgress < mapNodes.Length)
        {
            mapNodes[mapProgress].SetActive(true);
            
            // Update stamp with smooth animation
            if (stampPostcard != null && mapProgress < countryStamps.Length)
            {
                AnimateStampChange(mapProgress);
            }
            
            mapProgress++;
        }
        
        // Reset boarding passes
        if (Game_Manager.Instance != null)
        {
            Game_Manager.Instance.boarding_pass = 0;
            currentPasses = 0;
        }
    }
    
    void AnimateStampChange(int stampIndex)
    {
        if (stampPostcard == null || stampIndex >= countryStamps.Length) return;
        
        Sequence stampSequence = DOTween.Sequence();
        
        // Slide stamp up from bottom to center
        stampSequence.Append(stampPostcard.transform.DOMove(stampOriginalPosition, stampSlideDuration)
            .SetEase(Ease.OutBack));
        
        // Change texture at center
        stampSequence.AppendCallback(() => {
            stampPostcard.texture = countryStamps[stampIndex].texture;
        });
        
        // Hold at center for a moment
        stampSequence.AppendInterval(0.5f);
        
        // Slide stamp back down to bottom
        stampSequence.Append(stampPostcard.transform.DOMove(stampBottomPosition, stampSlideDuration)
            .SetEase(Ease.InBack));
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