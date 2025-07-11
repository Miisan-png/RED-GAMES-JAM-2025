using UnityEngine;

[System.Serializable]
public class ParallaxLayer
{
    [Header("Layer Settings")]
    public Transform layerTransform;
    public float parallaxSpeed = 0.5f;
    public bool infiniteScroll = true;
    public float layerWidth = 20f;
    
    [Header("Movement Settings")]
    public bool moveHorizontal = true;
    public bool moveVertical = false;
    public Vector2 customDirection = Vector2.right;
    
    [HideInInspector]
    public Vector3 startPosition;
}

public class Parallax_Node : MonoBehaviour
{
    [Header("Parallax Settings")]
    public Transform cameraTransform;
    public ParallaxLayer[] parallaxLayers;
    
    [Header("Global Settings")]
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
        
        if (layer.infiniteScroll && layer.moveHorizontal)
        {
            HandleInfiniteScroll(layer);
        }
    }
    
    void HandleInfiniteScroll(ParallaxLayer layer)
    {
        float distanceFromStart = layer.layerTransform.position.x - layer.startPosition.x;
        
        if (distanceFromStart > layer.layerWidth)
        {
            Vector3 newPosition = layer.layerTransform.position;
            newPosition.x = layer.startPosition.x;
            layer.layerTransform.position = newPosition;
        }
        else if (distanceFromStart < -layer.layerWidth)
        {
            Vector3 newPosition = layer.layerTransform.position;
            newPosition.x = layer.startPosition.x;
            layer.layerTransform.position = newPosition;
        }
    }
}