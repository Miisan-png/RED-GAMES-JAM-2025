using UnityEngine;

[System.Serializable]
public class ParallaxLayer
{
    public Transform layerTransform;
    public float parallaxSpeed = 0.5f;
    public bool infiniteScroll = true;
    public float layerWidth = 20f;
    public bool moveHorizontal = true;
    public bool moveVertical = false;
    public Vector2 customDirection = Vector2.right;
    [HideInInspector]
    public Vector3 startPosition;
    [HideInInspector]
    public Transform cloneTransform;
}

public class Parallax_Node : MonoBehaviour
{
    public Transform cameraTransform;
    public ParallaxLayer[] parallaxLayers;
    public float globalSpeedMultiplier = 1f;
    public bool useCustomDirection = false;
    public Vector2 globalDirection = Vector2.right;

    private Vector3 lastCameraPosition;

    void Start()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }

        lastCameraPosition = cameraTransform.position;

        foreach (ParallaxLayer layer in parallaxLayers)
        {
            if (layer.layerTransform != null)
            {
                layer.startPosition = layer.layerTransform.position;

                if (layer.infiniteScroll && layer.cloneTransform == null)
                {
                    GameObject clone = Instantiate(layer.layerTransform.gameObject, layer.layerTransform.position + Vector3.right * layer.layerWidth, layer.layerTransform.rotation, layer.layerTransform.parent);
                    layer.cloneTransform = clone.transform;
                }
            }
        }
    }

    void Update()
    {
        if (cameraTransform == null) return;

        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        foreach (ParallaxLayer layer in parallaxLayers)
        {
            if (layer.layerTransform == null) continue;

            UpdateParallaxLayer(layer, deltaMovement);
        }

        lastCameraPosition = cameraTransform.position;
    }

    void UpdateParallaxLayer(ParallaxLayer layer, Vector3 deltaMovement)
    {
        Vector3 parallaxMovement = Vector3.zero;

        if (useCustomDirection)
        {
            parallaxMovement = globalDirection * deltaMovement.magnitude * layer.parallaxSpeed * globalSpeedMultiplier;
        }
        else
        {
            if (layer.moveHorizontal)
            {
                parallaxMovement.x = deltaMovement.x * layer.parallaxSpeed * globalSpeedMultiplier;
            }

            if (layer.moveVertical)
            {
                parallaxMovement.y = deltaMovement.y * layer.parallaxSpeed * globalSpeedMultiplier;
            }
        }

        layer.layerTransform.position += parallaxMovement;

        if (layer.cloneTransform != null)
        {
            layer.cloneTransform.position += parallaxMovement;
        }

        if (layer.infiniteScroll && layer.moveHorizontal)
        {
            HandleInfiniteScroll(layer);
        }
    }

    void HandleInfiniteScroll(ParallaxLayer layer)
    {
        float camX = cameraTransform.position.x;
        float leftEdge = layer.layerTransform.position.x - (layer.layerWidth * 0.5f);
        float rightEdge = layer.cloneTransform.position.x - (layer.layerWidth * 0.5f);

        if (camX > rightEdge)
        {
            layer.layerTransform.position += Vector3.right * layer.layerWidth * 2f;
            SwapClones(layer);
        }
        else if (camX < leftEdge)
        {
            layer.cloneTransform.position -= Vector3.right * layer.layerWidth * 2f;
            SwapClones(layer);
        }
    }

    void SwapClones(ParallaxLayer layer)
    {
        Transform temp = layer.layerTransform;
        layer.layerTransform = layer.cloneTransform;
        layer.cloneTransform = temp;
    }
}
