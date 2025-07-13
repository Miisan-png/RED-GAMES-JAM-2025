using UnityEngine;

[System.Serializable]
public class ParallaxLayer
{
    public Transform transform;
    public Vector2 parallaxMultiplier;
    public bool infiniteHorizontal;
    public bool infiniteVertical;

    [HideInInspector] public Vector3 startPosition;
    [HideInInspector] public float spriteWidth;
    [HideInInspector] public float spriteHeight;
}

public class Parallax_Node : MonoBehaviour
{
    public Transform cameraTransform;
    public ParallaxLayer[] layers;

    private Vector3 lastCameraPosition;

    void Start()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
        lastCameraPosition = cameraTransform.position;

        foreach (var layer in layers)
        {
            layer.startPosition = layer.transform.position;
            SpriteRenderer spriteRenderer = layer.transform.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                layer.spriteWidth = spriteRenderer.sprite.bounds.size.x * layer.transform.localScale.x;
                layer.spriteHeight = spriteRenderer.sprite.bounds.size.y * layer.transform.localScale.y;
            }
        }
    }

void LateUpdate()
{
    if (cameraTransform == null || layers == null) return;

    Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
    lastCameraPosition = cameraTransform.position;

    foreach (var layer in layers)
    {
        if (layer.transform == null) continue;

        Vector3 parallaxMovement = new Vector3(
            deltaMovement.x * layer.parallaxMultiplier.x,
            deltaMovement.y * layer.parallaxMultiplier.y,
            0
        );

        layer.transform.position += parallaxMovement;

        if (layer.infiniteHorizontal)
        {
            if (layer.spriteWidth > 0.001f && Mathf.Abs(cameraTransform.position.x - layer.transform.position.x) >= layer.spriteWidth)
            {
                float offsetPositionX = (cameraTransform.position.x - layer.transform.position.x) % layer.spriteWidth;
                layer.transform.position = new Vector3(cameraTransform.position.x - offsetPositionX, layer.transform.position.y, layer.transform.position.z);
            }
        }

        if (layer.infiniteVertical)
        {
            if (layer.spriteHeight > 0.001f && Mathf.Abs(cameraTransform.position.y - layer.transform.position.y) >= layer.spriteHeight)
            {
                float offsetPositionY = (cameraTransform.position.y - layer.transform.position.y) % layer.spriteHeight;
                layer.transform.position = new Vector3(layer.transform.position.x, cameraTransform.position.y - offsetPositionY, layer.transform.position.z);
            }
        }
    }
}
}
