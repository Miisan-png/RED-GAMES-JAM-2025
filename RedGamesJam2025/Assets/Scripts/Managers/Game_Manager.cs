using UnityEngine;
using TMPro;

public class Game_Manager : MonoBehaviour
{
    public static Game_Manager Instance;
    
    [Header("Game Stats")]
    public int currentDistance = 0;
    public int currentCoins = 0;
    public int currentTickets = 0;
    public int highScore = 0;
    public int totalCoins = 0;
    public int totalTickets = 0;
    
    [Header("UI References")]
    public TextMeshProUGUI ticketsUI;
    
    [Header("Settings")]
    public float distanceMultiplier = 1f;
    
    private float distanceTimer = 0f;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadGameData();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Update()
    {
        if (IsGameActive())
        {
            UpdateDistance();
        }
    }
    
    void UpdateDistance()
    {
        distanceTimer += Time.deltaTime;
        if (distanceTimer >= 1f)
        {
            currentDistance += Mathf.RoundToInt(distanceMultiplier);
            distanceTimer = 0f;
        }
    }
    
    bool IsGameActive()
    {
        Player_Movement player = FindObjectOfType<Player_Movement>();
        return player != null;
    }
    
    public void AddCoins(int amount)
    {
        currentCoins += amount;
        totalCoins += amount;
    }
    
    public void AddTickets(int amount)
    {
        currentTickets += amount;
        totalTickets += amount;
        UpdateTicketsUI();
    }
    
    void UpdateTicketsUI()
    {
        if (ticketsUI != null)
        {
            ticketsUI.text = currentTickets.ToString();
        }
    }
    
    public void GameOver()
    {
        if (currentDistance > highScore)
        {
            highScore = currentDistance;
        }
        SaveGameData();
    }
    
    public void ResetCurrentGame()
    {
        currentDistance = 0;
        currentCoins = 0;
        currentTickets = 0;
        distanceTimer = 0f;
        UpdateTicketsUI();
    }
    
    void SaveGameData()
    {
        PlayerPrefs.SetInt("HighScore", highScore);
        PlayerPrefs.SetInt("TotalCoins", totalCoins);
        PlayerPrefs.SetInt("TotalTickets", totalTickets);
        PlayerPrefs.Save();
    }
    
    void LoadGameData()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        totalTickets = PlayerPrefs.GetInt("TotalTickets", 0);
    }
    
    public int GetCurrentDistance()
    {
        return currentDistance;
    }
    
    public int GetCurrentCoins()
    {
        return currentCoins;
    }
    
    public int GetCurrentTickets()
    {
        return currentTickets;
    }
    
    public int GetHighScore()
    {
        return highScore;
    }
    
    public int GetTotalCoins()
    {
        return totalCoins;
    }
    
    public int GetTotalTickets()
    {
        return totalTickets;
    }
}