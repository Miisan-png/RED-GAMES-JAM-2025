using UnityEngine;
using System.Collections.Generic;

public class Coin_Spawner : MonoBehaviour
{
    public GameObject[] coinSetPrefabs;
    public Transform targetCamera;
    public float spawnDistanceAhead = 15f;
    public float spawnInterval = 5f;
    public float despawnDistanceBehind = 10f;
    public float minSpacing = 3f; 
    public LayerMask coinLayerMask = -1;

    private float nextSpawnX;
    private List<GameObject> activeCoins = new List<GameObject>();

    void Start()
    {
        nextSpawnX = targetCamera.position.x + spawnDistanceAhead;
    }

    void Update()
    {
        if (targetCamera.position.x + spawnDistanceAhead >= nextSpawnX)
        {
            SpawnCoinSet();
            nextSpawnX += spawnInterval;
        }

        DespawnOldCoins();
    }

    void SpawnCoinSet()
    {
        if (coinSetPrefabs.Length == 0) return;

        GameObject prefab = coinSetPrefabs[Random.Range(0, coinSetPrefabs.Length)];
        
        Vector3 spawnPos = FindValidSpawnPosition();
        
        if (spawnPos != Vector3.zero) 
        {
            GameObject instance = Instantiate(prefab, spawnPos, Quaternion.identity);
            activeCoins.Add(instance);
        }
    }

    Vector3 FindValidSpawnPosition()
    {
        float yPos = Random.Range(-1f, 2f);
        Vector3 potentialPos = new Vector3(nextSpawnX, yPos, 0f);
        
        if (!IsPositionOverlapping(potentialPos))
        {
            return potentialPos;
        }
        
        
        for (int attempts = 0; attempts < 10; attempts++)
        {
            float offsetX = Random.Range(-minSpacing, minSpacing);
            float newX = nextSpawnX + offsetX;

            float newY = Random.Range(-1f, 2f);

            Vector3 testPos = new Vector3(newX, newY, 0f);

            if (!IsPositionOverlapping(testPos))
            {
                return testPos;
            }
        }
        
        return Vector3.zero;
    }

    bool IsPositionOverlapping(Vector3 position)
    {
        float checkRadius = minSpacing / 2f;
        
        Collider[] overlapping = Physics.OverlapSphere(position, checkRadius, coinLayerMask);
        
        foreach (Collider col in overlapping)
        {
            if (col.gameObject != gameObject && activeCoins.Contains(col.gameObject))
            {
                return true;
            }
        }
        
        return false;
    }

    void DespawnOldCoins()
    {
        for (int i = activeCoins.Count - 1; i >= 0; i--)
        {
            if (activeCoins[i] == null)
            {
                activeCoins.RemoveAt(i);
                continue;
            }

            if (activeCoins[i].transform.position.x < targetCamera.position.x - despawnDistanceBehind)
            {
                Destroy(activeCoins[i]);
                activeCoins.RemoveAt(i);
            }
        }
    }
}