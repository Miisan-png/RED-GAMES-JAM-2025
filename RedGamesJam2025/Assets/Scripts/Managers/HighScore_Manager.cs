using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HighScore_Manager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI highScoreLabel;
    public TextMeshProUGUI bestScoreLabel;
    
    [Header("Update Settings")]
    public bool updateInRealTime = true;
    [Range(0.01f, 0.2f)]
    public float updateInterval = 0.05f;
    [Range(1, 10)]
    public int countSpeed = 1;
    
    private float updateTimer = 0f;
    private int displayedDistance = 0;
    private int targetDistance = 0;
    
    void Start()
    {
        UpdateHighScoreDisplay();
    }
    
    void Update()
    {
        if (updateInRealTime)
        {
            updateTimer += Time.deltaTime;
            if (updateTimer >= updateInterval)
            {
                UpdateCounterDisplay();
                updateTimer = 0f;
            }
        }
    }
    
    void UpdateCounterDisplay()
    {
        if (Game_Manager.Instance == null) return;
        
        targetDistance = Game_Manager.Instance.GetCurrentDistance();
        
        if (displayedDistance < targetDistance)
        {
            displayedDistance = Mathf.Min(displayedDistance + countSpeed, targetDistance);
        }
        else if (displayedDistance > targetDistance)
        {
            displayedDistance = Mathf.Max(displayedDistance - countSpeed, targetDistance);
        }
        
        if (highScoreLabel != null)
        {
            highScoreLabel.text = FormatDistance(displayedDistance);
        }
        
        UpdateBestScore();
    }
    
    public void UpdateHighScoreDisplay()
    {
        if (Game_Manager.Instance == null) return;
        
        displayedDistance = Game_Manager.Instance.GetCurrentDistance();
        targetDistance = displayedDistance;
        
        if (highScoreLabel != null)
        {
            highScoreLabel.text = FormatDistance(displayedDistance);
        }
        
        UpdateBestScore();
    }
    
    void UpdateBestScore()
    {
        if (bestScoreLabel != null)
        {
            int highScore = Game_Manager.Instance.GetHighScore();
            bestScoreLabel.text = "BEST:" + highScore.ToString();
        }
    }
    
    string FormatDistance(int distance)
    {
        return distance.ToString("0000") + "M";
    }
    
    public void ForceUpdate()
    {
        UpdateHighScoreDisplay();
    }
}