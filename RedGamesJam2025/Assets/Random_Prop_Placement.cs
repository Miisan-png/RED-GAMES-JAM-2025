using UnityEngine;

public class Random_Prop_Placement : MonoBehaviour
{
    [Header("Prop Prefabs")]
    public GameObject[] propPrefabs;

    [Header("Spawn Settings")]
    public float minSpacing = 2f;
    public float maxSpacing = 5f;
    public float spawnAheadDistance = 30f;
    public float spacingIncrement = 1f; // Added if collision is detected
    public int maxSpacingAttempts = 10;

    [Header("Tracking")]
    public Transform playerOrCamera;

    private float nextSpawnX;

    void Start()
    {
        if (playerOrCamera == null && Camera.main != null)
            playerOrCamera = Camera.main.transform;

        nextSpawnX = transform.position.x;
    }

    void Update()
    {
        float spawnTriggerX = playerOrCamera.position.x + spawnAheadDistance;

        while (nextSpawnX < spawnTriggerX)
        {
            SpawnNextProp();
        }
    }

    void SpawnNextProp()
    {
        if (propPrefabs.Length == 0) return;

        GameObject prefab = propPrefabs[Random.Range(0, propPrefabs.Length)];
        float spacing = Random.Range(minSpacing, maxSpacing);
        int attempt = 0;

        GameObject spawned = null;
        bool isOverlapping;

        do
        {
            Vector3 spawnPos = new Vector3(nextSpawnX, transform.position.y, 0f);
            spawned = Instantiate(prefab, spawnPos, Quaternion.identity, transform);

            Bounds bounds = GetBounds(spawned);
            Collider2D[] hits = Physics2D.OverlapBoxAll(bounds.center, bounds.size, 0f);

            isOverlapping = false;

            foreach (Collider2D hit in hits)
            {
                if (hit.gameObject != spawned && hit.CompareTag("prop"))
                {
                    isOverlapping = true;
                    break;
                }
            }

            if (isOverlapping)
            {
                Destroy(spawned);
                nextSpawnX += spacingIncrement;
                attempt++;
            }

        } while (isOverlapping && attempt < maxSpacingAttempts);

        if (spawned != null)
        {
            float width = GetBounds(spawned).size.x;
            nextSpawnX += width + spacing;
        }
    }

    Bounds GetBounds(GameObject obj)
    {
        Collider2D collider = obj.GetComponent<Collider2D>();
        if (collider != null)
            return collider.bounds;

        Renderer renderer = obj.GetComponentInChildren<Renderer>();
        if (renderer != null)
            return renderer.bounds;

        return new Bounds(obj.transform.position, Vector3.one);
    }
}
