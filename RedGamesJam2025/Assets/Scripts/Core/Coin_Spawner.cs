using UnityEngine;
using System.Collections.Generic;

public class Coin_Spawner : MonoBehaviour
{
    public GameObject[] coinSetPrefabs;
    public Transform targetCamera;
    public float spawnDistanceAhead = 15f;
    public float spawnInterval = 5f;
    public float despawnDistanceBehind = 10f;

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
        Vector3 spawnPos = new Vector3(nextSpawnX, Random.Range(-1f, 2f), 0f);
        GameObject instance = Instantiate(prefab, spawnPos, Quaternion.identity);
        activeCoins.Add(instance);
    }

    void DespawnOldCoins()
    {
        for (int i = activeCoins.Count - 1; i >= 0; i--)
        {
            if (activeCoins[i] == null) continue;

            if (activeCoins[i].transform.position.x < targetCamera.position.x - despawnDistanceBehind)
            {
                Destroy(activeCoins[i]);
                activeCoins.RemoveAt(i);
            }
        }
    }
}
