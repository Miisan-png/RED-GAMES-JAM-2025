using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game_Manager : MonoBehaviour
{
    public static Game_Manager Instance;

    [Header("Game Stats")]
    public int currentDistance = 0;
    public int currentCoins = 0;
    public int boardingPass = 0;

    public int highScore = 0;
    public int totalCoins = 0;

    public Landmark_Manager landmarkManager;

    [Header("UI References")]
    public TextMeshProUGUI coinsUI;
    public TextMeshProUGUI currentDistanceLabel;
    public TextMeshProUGUI bestScoreLabel;

    [Header("Floating Intro UI")]
    public GameObject introFloatUI;

    [Header("Settings")]
    public float distanceMultiplier = 1f;
    public float distanceUpdateInterval = 0.05f;

    private float distanceTimer = 0f;
    private float displayUpdateTimer = 0f;
    private int displayedDistance = 0;
    private bool scoreDisplayActive = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadGameData();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        if (!scoreDisplayActive || !IsGameActive()) return;

        distanceTimer += Time.deltaTime;
        displayUpdateTimer += Time.deltaTime;

        if (distanceTimer >= 1f)
        {
            currentDistance += Mathf.RoundToInt(distanceMultiplier);
            distanceTimer = 0f;

            // Update high score live
            if (currentDistance > highScore)
            {
                highScore = currentDistance;
                UpdateBestScoreLabel();
            }
        }

        if (displayUpdateTimer >= distanceUpdateInterval)
        {
            displayUpdateTimer = 0f;
            if (displayedDistance < currentDistance)
            {
                displayedDistance = Mathf.Min(displayedDistance + 5, currentDistance);
                UpdateDistanceUI();
            }
        }
    }

    bool IsGameActive()
    {
        return FindObjectOfType<Player_Movement>() != null;
    }

    public void EnableScoreDisplay()
    {
        scoreDisplayActive = true;
        displayedDistance = 0;

        UpdateCoinsUI();
        UpdateDistanceUI();
        UpdateBestScoreLabel();

        if (introFloatUI != null)
            introFloatUI.SetActive(true);
    }

    public void AddCoins(int amount)
    {
        currentCoins += amount;
        totalCoins += amount;

        UpdateCoinsUI();

        if (landmarkManager != null)
        {
            landmarkManager.OnCoinsUpdated(amount);
        }
    }

    public void AddBoardingPass(int amount)
    {
        boardingPass += amount;
    }

    public void GameOver()
    {
        if (currentDistance > highScore)
        {
            highScore = currentDistance;
            UpdateBestScoreLabel();
        }

        SaveGameData();
    }

    public void ResetCurrentGame()
    {
        currentDistance = 0;
        currentCoins = 0;
        distanceTimer = 0f;
        displayedDistance = 0;
        scoreDisplayActive = false;

        UpdateCoinsUI();
        UpdateDistanceUI();
        UpdateBestScoreLabel();
    }

    void SaveGameData()
    {
        PlayerPrefs.SetInt("HighScore", highScore);
        PlayerPrefs.SetInt("TotalCoins", totalCoins);
        PlayerPrefs.SetInt("BoardingPass", boardingPass);
        PlayerPrefs.Save();
    }

    void LoadGameData()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        boardingPass = PlayerPrefs.GetInt("BoardingPass", 0);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        coinsUI = GameObject.FindWithTag("CoinsUI")?.GetComponent<TextMeshProUGUI>();
        currentDistanceLabel = GameObject.FindWithTag("ScoreUI")?.GetComponent<TextMeshProUGUI>();
        bestScoreLabel = GameObject.FindWithTag("BestScoreUI")?.GetComponent<TextMeshProUGUI>();

        UpdateCoinsUI();
        UpdateDistanceUI();
        UpdateBestScoreLabel();
    }

    void UpdateCoinsUI()
    {
        if (coinsUI != null)
            coinsUI.text = currentCoins.ToString("D3");
    }

    void UpdateDistanceUI()
    {
        if (currentDistanceLabel != null)
            currentDistanceLabel.text = FormatDistance(displayedDistance);
    }

    void UpdateBestScoreLabel()
    {
        if (bestScoreLabel != null)
            bestScoreLabel.text = "BEST: " + FormatDistance(highScore);
    }

    string FormatDistance(int distance)
    {
        return distance.ToString("0000") + "M";
    }

    // Optional getters
    public int GetCurrentDistance() => currentDistance;
    public int GetCurrentCoins() => currentCoins;
    public int GetBoardingPass() => boardingPass;
    public int GetHighScore() => highScore;
    public int GetTotalCoins() => totalCoins;
}
