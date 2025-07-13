using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LaserManager : MonoBehaviour
{
    [Header("Laser References")]
    public LaserBehavior[] lasers;
    
    [Header("Timing Settings")]
    public float delayBetweenLasers = 2f;
    public float firstLaserDelay = 3f;
    public float laserCycleInterval = 10f;
    
    [Header("Pattern Settings")]
    public bool alternateDirections = true;
    public bool randomOrder = false;
    public int maxSimultaneousLasers = 2;
    
    [Header("Debug")]
    public bool enableDebugLogs = true;
    
    private bool isActive = false;
    private Coroutine laserCycleCoroutine;
    private int currentLaserIndex = 0;
    private List<LaserBehavior> availableLasers = new List<LaserBehavior>();
    
    void Start()
    {
        InitializeLasers();
        
        // Start laser cycle after delay
        if (lasers.Length > 0)
        {
            StartLaserCycle();
        }
    }
    
    void InitializeLasers()
    {
        if (lasers == null) return;
        
        availableLasers.Clear();
        
        for (int i = 0; i < lasers.Length; i++)
        {
            if (lasers[i] != null)
            {
                availableLasers.Add(lasers[i]);
                
                // Set alternating directions if enabled
                if (alternateDirections)
                {
                    lasers[i].SetFireDirection(i % 2 == 0);
                }
            }
        }
        
        if (enableDebugLogs)
            Debug.Log($"Initialized {availableLasers.Count} lasers");
    }
    
    public void StartLaserCycle()
    {
        if (isActive) return;
        
        isActive = true;
        
        if (laserCycleCoroutine != null)
        {
            StopCoroutine(laserCycleCoroutine);
        }
        
        laserCycleCoroutine = StartCoroutine(LaserCycleRoutine());
    }
    
    public void StopLaserCycle()
    {
        isActive = false;
        
        if (laserCycleCoroutine != null)
        {
            StopCoroutine(laserCycleCoroutine);
        }
        
        // Stop all active lasers
        foreach (LaserBehavior laser in availableLasers)
        {
            if (laser != null)
            {
                laser.StopLaser();
            }
        }
    }
    
    IEnumerator LaserCycleRoutine()
    {
        yield return new WaitForSeconds(firstLaserDelay);
        
        while (isActive)
        {
            if (randomOrder)
            {
                yield return StartCoroutine(FireRandomLaser());
            }
            else
            {
                yield return StartCoroutine(FireSequentialLaser());
            }
            
            yield return new WaitForSeconds(laserCycleInterval);
        }
    }
    
    IEnumerator FireRandomLaser()
    {
        List<LaserBehavior> inactiveLasers = new List<LaserBehavior>();
        
        // Find inactive lasers
        foreach (LaserBehavior laser in availableLasers)
        {
            if (laser != null && !laser.IsActive())
            {
                inactiveLasers.Add(laser);
            }
        }
        
        if (inactiveLasers.Count == 0)
        {
            if (enableDebugLogs)
                Debug.Log("No available lasers for random fire");
            yield break;
        }
        
        // Fire random lasers up to the max simultaneous limit
        int lasersToFire = Mathf.Min(maxSimultaneousLasers, inactiveLasers.Count);
        
        for (int i = 0; i < lasersToFire; i++)
        {
            int randomIndex = Random.Range(0, inactiveLasers.Count);
            LaserBehavior selectedLaser = inactiveLasers[randomIndex];
            
            selectedLaser.ActivateLaser();
            inactiveLasers.RemoveAt(randomIndex);
            
            if (enableDebugLogs)
                Debug.Log($"Fired random laser {i + 1}/{lasersToFire}");
            
            if (i < lasersToFire - 1)
            {
                yield return new WaitForSeconds(delayBetweenLasers);
            }
        }
    }
    
    IEnumerator FireSequentialLaser()
    {
        LaserBehavior currentLaser = availableLasers[currentLaserIndex];
        
        if (currentLaser != null && !currentLaser.IsActive())
        {
            currentLaser.ActivateLaser();
            
            if (enableDebugLogs)
                Debug.Log($"Fired sequential laser {currentLaserIndex}");
        }
        
        // Move to next laser
        currentLaserIndex = (currentLaserIndex + 1) % availableLasers.Count;
        
        yield return null;
    }
    
    // Public methods for external control
    public void FireSpecificLaser(int index)
    {
        if (index >= 0 && index < availableLasers.Count)
        {
            LaserBehavior laser = availableLasers[index];
            if (laser != null && !laser.IsActive())
            {
                laser.ActivateLaser();
            }
        }
    }
    
    public void FireAllLasers()
    {
        StartCoroutine(FireAllLasersCoroutine());
    }
    
    IEnumerator FireAllLasersCoroutine()
    {
        foreach (LaserBehavior laser in availableLasers)
        {
            if (laser != null && !laser.IsActive())
            {
                laser.ActivateLaser();
                yield return new WaitForSeconds(delayBetweenLasers);
            }
        }
    }
    
    public void SetLaserDirection(int index, bool fromTop)
    {
        if (index >= 0 && index < availableLasers.Count)
        {
            LaserBehavior laser = availableLasers[index];
            if (laser != null)
            {
                laser.SetFireDirection(fromTop);
            }
        }
    }
    
    public void SetAllLaserDirections(bool fromTop)
    {
        foreach (LaserBehavior laser in availableLasers)
        {
            if (laser != null)
            {
                laser.SetFireDirection(fromTop);
            }
        }
    }
    
    public int GetActiveLaserCount()
    {
        int count = 0;
        foreach (LaserBehavior laser in availableLasers)
        {
            if (laser != null && laser.IsActive())
            {
                count++;
            }
        }
        return count;
    }
    
    // Context menu methods for testing
    [ContextMenu("Fire Random Laser")]
    public void TestFireRandomLaser()
    {
        StartCoroutine(FireRandomLaser());
    }
    
    [ContextMenu("Fire All Lasers")]
    public void TestFireAllLasers()
    {
        FireAllLasers();
    }
    
    [ContextMenu("Stop All Lasers")]
    public void TestStopAllLasers()
    {
        StopLaserCycle();
    }
    
    [ContextMenu("Start Laser Cycle")]
    public void TestStartLaserCycle()
    {
        StartLaserCycle();
    }
}