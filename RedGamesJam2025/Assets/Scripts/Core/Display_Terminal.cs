using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class Display_Terminal : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI displayText;
    
    [Header("Terminal Settings")]
    public float typewriterSpeed = 0.08f;
    public float linePauseDuration = 1.5f;
    public float cursorBlinkSpeed = 0.5f;
    public bool showCursor = true;
    public bool autoLoop = true;
    public float loopDelay = 3f;
    
    [Header("Terminal Content")]
    public List<string> terminalLines = new List<string>();
    
    [Header("Audio (Optional)")]
    public AudioSource audioSource;
    public AudioClip typeSound;
    
    private bool isTyping = false;
    private bool cursorVisible = true;
    private Coroutine typingCoroutine;
    private Coroutine cursorCoroutine;
    private string currentDisplayText = "";
    
    void Start()
    {
        SetupTerminal();
        StartTerminalDisplay();
    }
    
    void SetupTerminal()
    {
        if (displayText == null)
        {
            displayText = GetComponent<TextMeshProUGUI>();
        }
        
        if (displayText == null)
        {
            Debug.LogError("Display_Terminal: No TextMeshProUGUI component found!");
            return;
        }
        
        // Initialize with default terminal content if empty
        if (terminalLines.Count == 0)
        {
            terminalLines.Add("TOKYO: 34ST");
            terminalLines.Add("TERMINAL 4");
            terminalLines.Add("GATE: A7");
            terminalLines.Add("BOARDING");
            terminalLines.Add("FLIGHT: FS230");
            terminalLines.Add("PARIS: 28TH");
            terminalLines.Add("TERMINAL 2");
            terminalLines.Add("DELAYED");
        }
        
        displayText.text = "";
        
        // Start cursor blinking
        if (showCursor)
        {
            StartCursorBlink();
        }
    }
    
    void StartTerminalDisplay()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TerminalDisplayLoop());
    }
    
    IEnumerator TerminalDisplayLoop()
    {
        while (true)
        {
            // Clear display
            currentDisplayText = "";
            UpdateDisplay();
            
            // Type each line
            foreach (string line in terminalLines)
            {
                yield return StartCoroutine(TypeLine(line));
                yield return new WaitForSeconds(linePauseDuration);
            }
            
            // Wait before looping
            if (autoLoop)
            {
                yield return new WaitForSeconds(loopDelay);
            }
            else
            {
                break;
            }
        }
    }
    
    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        string typedText = "";
        
        for (int i = 0; i <= line.Length; i++)
        {
            typedText = line.Substring(0, i);
            currentDisplayText = typedText;
            UpdateDisplay();
            
            // Play typing sound
            if (audioSource != null && typeSound != null && i < line.Length)
            {
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(typeSound, 0.2f);
            }
            
            yield return new WaitForSeconds(typewriterSpeed);
        }
        
        isTyping = false;
    }
    
    void UpdateDisplay()
    {
        string displayContent = currentDisplayText;
        
        // Add cursor if enabled
        if (showCursor)
        {
            displayContent += (cursorVisible ? "|" : " ");
        }
        
        displayText.text = displayContent;
    }
    
    void StartCursorBlink()
    {
        if (cursorCoroutine != null)
        {
            StopCoroutine(cursorCoroutine);
        }
        cursorCoroutine = StartCoroutine(BlinkCursor());
    }
    
    IEnumerator BlinkCursor()
    {
        while (true)
        {
            cursorVisible = !cursorVisible;
            
            // Only update display if not currently typing
            if (!isTyping)
            {
                UpdateDisplay();
            }
            
            yield return new WaitForSeconds(cursorBlinkSpeed);
        }
    }
    
    // Public methods for external control
    public void TypeSingleLine(string line)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        currentDisplayText = "";
        StartCoroutine(TypeLine(line));
    }
    
    public void AddTerminalLine(string line)
    {
        // Limit line length to prevent overflow
        if (line.Length > 12)
        {
            line = line.Substring(0, 12);
        }
        
        terminalLines.Add(line);
    }
    
    public void ClearTerminalLines()
    {
        terminalLines.Clear();
    }
    
    public void SetTerminalLines(List<string> newLines)
    {
        terminalLines = newLines;
    }
    
    public void ClearDisplay()
    {
        currentDisplayText = "";
        UpdateDisplay();
    }
    
    public void SetTypeSpeed(float speed)
    {
        typewriterSpeed = speed;
    }
    
    public void SetCursorBlinkSpeed(float speed)
    {
        cursorBlinkSpeed = speed;
    }
    
    public void ToggleCursor()
    {
        showCursor = !showCursor;
        
        if (showCursor)
        {
            StartCursorBlink();
        }
        else
        {
            if (cursorCoroutine != null)
            {
                StopCoroutine(cursorCoroutine);
            }
            UpdateDisplay();
        }
    }
    
    public void RestartDisplay()
    {
        StartTerminalDisplay();
    }
    
    public void StopDisplay()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        autoLoop = false;
    }
    
    // Quick setup methods
    public void SetupFlightInfo(string destination, string terminal, string gate, string flight)
    {
        ClearTerminalLines();
        AddTerminalLine($"{destination}: {terminal}");
        AddTerminalLine($"TERMINAL {terminal}");
        AddTerminalLine($"GATE: {gate}");
        AddTerminalLine($"FLIGHT: {flight}");
        RestartDisplay();
    }
    
    public void SetupCustomInfo(params string[] lines)
    {
        ClearTerminalLines();
        foreach (string line in lines)
        {
            AddTerminalLine(line);
        }
        RestartDisplay();
    }
    
    void OnDestroy()
    {
        // Clean up coroutines
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        if (cursorCoroutine != null)
            StopCoroutine(cursorCoroutine);
    }
}